using LiruAI.Layers;
using LiruAI.Learning;
using LiruAI.Maths;
using System;
using System.Collections.Generic;

namespace LiruAI.Neurons
{
    /// <summary> Represents a neuron within a neuron layer, with a bias and an output. </summary>
    public class Neuron : IReadOnlyNeuron
    {
        #region Constants
        /// <summary> The default <see cref="IActivationFunction"/> to use if none was given. </summary>
        private static readonly IActivationFunction defaultActivationFunction = new TanH();
        #endregion

        #region Fields
        /// <summary> The <see cref="IActivationFunction"/> that this <see cref="Neuron"/> applies to its weighted sums. </summary>
        private readonly IActivationFunction activationFunction;
        #endregion

        #region Accessors
        /// <summary> Quick access to the previous <see cref="IReadOnlyNeuronLayer"/>. </summary>
        private IReadOnlyNeuronLayer previousNeuronLayer => NeuronLayer.PreviousNeuronLayer;

        /// <summary> Quick access to the next <see cref="IReadOnlyNeuronLayer"/>. </summary>
        private IReadOnlyNeuronLayer nextNeuronLayer => NeuronLayer.NextNeuronLayer;
        #endregion

        #region Structure Properties
        /// <summary> The <see cref="IReadOnlyNeuronLayer"/> that this <see cref="Neuron"/> belongs to. </summary>
        public IReadOnlyNeuronLayer NeuronLayer { get; }

        /// <summary> The index of this <see cref="Neuron"/>. </summary>
        public uint Index { get; }
        #endregion

        #region Calculation Properties
        /// <summary> The bias applied to the weighted sum of this <see cref="Neuron"/>. </summary>
        public float Bias { get; private set; }

        /// <summary> The output from the last pulse of this <see cref="Neuron"/>. </summary>
        public float Output { get; set; }
        #endregion

        #region Constructors
        /// <summary> Creates a new <see cref="Neuron"/> contained by the given <paramref name="neuronLayer"/> with the given <paramref name="id"/> and <paramref name="bias"/>. </summary>
        /// <param name="neuronLayer"> The containing <see cref="IReadOnlyNeuronLayer"/>. </param>
        /// <param name="bias"> The new <see cref="Bias"/> of this <see cref="Neuron"/>. </param>
        /// <param name="id"> The index of this <see cref="Neuron"/> within the containing <see cref="NeuronLayer"/>. </param>
        public Neuron(IReadOnlyNeuronLayer neuronLayer, float bias, uint id) : this(neuronLayer, defaultActivationFunction, bias, id) { }

        /// <summary> Creates a new <see cref="Neuron"/> contained by the given <paramref name="neuronLayer"/> with the given <paramref name="id"/> and randomised <see cref="Bias"/>. </summary>
        /// <param name="neuronLayer"> The containing <see cref="IReadOnlyNeuronLayer"/>. </param>
        /// <param name="random"> The <see cref="Random"/> object used to randomise the <see cref="Bias"/>. </param>
        /// <param name="id"> The index of this <see cref="Neuron"/> within the containing <see cref="NeuronLayer"/>. </param>
        public Neuron(IReadOnlyNeuronLayer neuronLayer, Random random, uint id) : this(neuronLayer, defaultActivationFunction, random, id) { }

        /// <summary> Creates a new <see cref="Neuron"/> contained by the given <paramref name="neuronLayer"/> with the given <paramref name="id"/>, <paramref name="activationFunction"/>, and randomised <paramref name="Bias"/>. </summary>
        /// <param name="neuronLayer"> The containing <see cref="IReadOnlyNeuronLayer"/>. </param>
        /// <param name="activationFunction"> The <see cref="IActivationFunction"/> to use. </param>
        /// <param name="random"> The <see cref="Random"/> object used to randomise the <see cref="Bias"/>. </param>
        /// <param name="id"> The index of this <see cref="Neuron"/> within the containing <see cref="NeuronLayer"/>. </param>
        //public Neuron(IReadOnlyNeuronLayer neuronLayer, IActivationFunction activationFunction, Random random, uint id) : this(neuronLayer, activationFunction, (float)(random.NextDouble() * 2) - 1, id) { }
        public Neuron(IReadOnlyNeuronLayer neuronLayer, IActivationFunction activationFunction, Random random, uint id) : this(neuronLayer, activationFunction, (float)random.NextDouble() - 0.5f, id) { }

        /// <summary> Creates a new <see cref="Neuron"/> contained by the given <paramref name="neuronLayer"/> with the given <paramref name="id"/>, <paramref name="activationFunction"/>, and <paramref name="bias"/>. </summary>
        /// <param name="neuronLayer"> The containing <see cref="IReadOnlyNeuronLayer"/>. </param>
        /// <param name="activationFunction"> The <see cref="IActivationFunction"/> to use. </param>
        /// <param name="bias"> The new <see cref="Bias"/> of this <see cref="Neuron"/>. </param>
        /// <param name="id"> The index of this <see cref="Neuron"/> within the containing <see cref="NeuronLayer"/>. </param>
        public Neuron(IReadOnlyNeuronLayer neuronLayer, IActivationFunction activationFunction, float bias, uint id)
        {
            // Set the containing layer.
            NeuronLayer = neuronLayer ?? throw new ArgumentNullException(nameof(neuronLayer));

            // Set the activation function.
            this.activationFunction = activationFunction ?? throw new ArgumentNullException(nameof(activationFunction));

            // Set the bias.
            Bias = bias;

            // Set the ID.
            Index = id;
        }
        #endregion

        #region Proccess Functions
        public void CalculateOutput()
        {
            // Start with the bias.
            float output = Bias;

            // Add each linked neuron's output multiplied by its associated weight.
            foreach (uint previousLayerNeuronID in NeuronLayer.PreviousWeightLayer.GetPreviousLayerConnections(this))
                output += previousNeuronLayer[previousLayerNeuronID].Output * NeuronLayer.PreviousWeightLayer.GetWeightBetweenNeurons(previousNeuronLayer[previousLayerNeuronID], this);

            // Apply the activation and save the output.
            Output = activationFunction.Activation(output);
        }
        #endregion

        #region Learning Functions
        /// <summary> Calculates and adds the changes to be made to this <see cref="Neuron"/> from the given <paramref name="previousDelta"/>. </summary>
        /// <param name="neuronLayerChange"> The changes to make to the containing <see cref="NeuronLayer"/>. </param>
        /// <param name="weightLayerChange"> The changes to make to the previous <see cref="WeightLayer"/>. </param>
        /// <param name="previousDelta"> The calculated deltas from the previous <see cref="NeuronLayer"/> of this <see cref="Neuron"/>. </param>
        /// <returns> The calculated delta of this <see cref="Neuron"/>. </returns>
        public float CalculateErrorThenChanges(NeuronLayerChange neuronLayerChange, WeightLayerChange weightLayerChange, IReadOnlyList<float> previousDelta)
        {
            // Calculate the summed error using the connections from this neuron to the next layer.
            float summedError = 0;
            foreach (uint nextLayerNeuronID in NeuronLayer.NextWeightLayer.GetNextLayerConnections(this))
                summedError += NeuronLayer.NextWeightLayer.GetWeightBetweenNeurons(this, nextNeuronLayer[nextLayerNeuronID]) * previousDelta[(int)nextLayerNeuronID];

            // Finish the delta calculations with the summed error.
            return CalculateChanges(neuronLayerChange, weightLayerChange, summedError);
        }

        /// <summary> Calculates and adds the changes to be made to this <see cref="Neuron"/> from the given <paramref name="error"/>. </summary>
        /// <param name="neuronLayerChange"> The changes to make to the containing <see cref="NeuronLayer"/>. </param>
        /// <param name="weightLayerChange"> The changes to make to the previous <see cref="WeightLayer"/>. </param>
        /// <param name="error"> The previously calculated delta. </param>
        /// <returns> The calculated delta of this <see cref="Neuron"/>. </returns>
        public float CalculateChanges(NeuronLayerChange neuronLayerChange, WeightLayerChange weightLayerChange, float error)
        {
            // Calculate the delta.
            float delta = activationFunction.ActivationDerivative(Output) * error;

            float p = activationFunction.ActivationDerivative(Output);
            if (p < 0) throw new Exception();

            // Add the changes to be made.
            addChanges(neuronLayerChange, weightLayerChange, delta);

            // Return the calculated delta.
            return delta;
        }

        /// <summary> Calculates and adds the changes to make to this <see cref="Neuron"/> to the given <paramref name="neuronLayerChange"/> and <paramref name="weightLayerChange"/> using the given <paramref name="delta"/>. </summary>
        /// <param name="neuronLayerChange"> The changes to make to the containing <see cref="NeuronLayer"/>. </param>
        /// <param name="weightLayerChange"> The changes to make to the previous <see cref="WeightLayer"/>. </param>
        /// <param name="delta"> The calculated delta, used to calculate the desired change. </param>
        private void addChanges(NeuronLayerChange neuronLayerChange, WeightLayerChange weightLayerChange, float delta)
        {
            // Calculate the change in bias based on the delta.
            neuronLayerChange.AddBias(this, delta);

            // Calculate the changes to the input weights of this neuron.
            // The change to make is based on the output of the neuron to which the weight is connected. "Neurons that fire together, wire together".
            foreach (uint previousLayerNeuronID in NeuronLayer.PreviousWeightLayer.GetPreviousLayerConnections(this))
                weightLayerChange.AddWeightChangeBetweenNeurons(previousNeuronLayer[previousLayerNeuronID], this, previousNeuronLayer[previousLayerNeuronID].Output * delta);
        }

        /// <summary> Applies the given <paramref name="biasChange"/> to this <see cref="Neuron"/>'s <see cref="Bias"/>. </summary>
        /// <param name="biasChange"> The change in <see cref="Bias"/> to make. </param>
        /// <param name="learningRate"> The scaled learning rate. </param>
        public void ApplyChanges(float biasChange, float learningRate) => Bias -= biasChange * learningRate;
        #endregion

        #region String Functions
        public override string ToString() => $"Neuron with index of {Index} in layer with index of {NeuronLayer.Index}. Output: {Output}.";
        #endregion
    }
}
