using LiruAI.IO;
using LiruAI.Learning;
using LiruAI.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiruAI.Layers
{
    /// <summary> Represents a layer within a network that contains <see cref="Neuron"/>s. </summary>
    public class NeuronLayer : IReadOnlyNeuronLayer, IInputLayer, IOutputLayer, ILinkableNeuronLayer
    {
        #region Fields
        /// <summary> The <see cref="Neuron"/>s. </summary>
        private readonly Neuron[] neurons;
        #endregion

        #region Properties
        /// <summary> The index of this <see cref="NeuronLayer"/> within the containing <see cref="NeuralNetwork"/>. </summary>
        public uint Index { get; }

        /// <summary> The amount of <see cref="Neuron"/>s in this <see cref="NeuronLayer"/>. </summary>
        public int Count => neurons.Length;

        /// <summary> The <see cref="IReadOnlyNeuralNetwork"/> that this <see cref="NeuronLayer"/> belongs to. </summary>
        public IReadOnlyNeuralNetwork NeuralNetwork { get; }

        /// <summary> The previous <see cref="IReadOnlyNeuronLayer"/> linked to this <see cref="NeuronLayer"/>. </summary>
        public IReadOnlyNeuronLayer PreviousNeuronLayer => PreviousWeightLayer.PreviousNeuronLayer;

        /// <summary> The next <see cref="IReadOnlyNeuronLayer"/> linked to this <see cref="NeuronLayer"/>. </summary>
        public IReadOnlyNeuronLayer NextNeuronLayer => NextWeightLayer.NextNeuronLayer;

        /// <summary> The previous <see cref="IPreviousWeightLayer"/> linked to this <see cref="NeuronLayer"/>. </summary>
        public IPreviousWeightLayer PreviousWeightLayer { get; set; }

        /// <summary> The next <see cref="IPreviousWeightLayer"/> linked to this <see cref="NeuronLayer"/>. </summary>
        public INextWeightLayer NextWeightLayer { get; set; }
        #endregion

        #region Indexers
        /// <summary> Gets the <see cref="IReadOnlyNeuron"/> with the given <paramref name="index"/>. </summary>
        /// <param name="index"> The index of the <see cref="IReadOnlyNeuron"/> to get. </param>
        /// <returns> The <see cref="IReadOnlyNeuron"/> at the given <paramref name="index"/>. </returns>
        public IReadOnlyNeuron this[uint index] => neurons[index];
        #endregion

        #region Constructors
        /// <summary> Creates a new <see cref="NeuronLayer"/> belonging to the given <paramref name="neuralNetwork"/> with the given <paramref name="index"/> and <see cref="Neuron"/> <paramref name="count"/>. </summary>
        /// <param name="neuralNetwork"> The <see cref="IReadOnlyNeuralNetwork"/> that this <see cref="NeuronLayer"/> belongs to. </param>
        /// <param name="index"> The index of this <see cref="NeuronLayer"/> within the <see cref="NeuralNetwork"/>. </param>
        /// <param name="count"> The number of <see cref="Neuron"/>s in this <see cref="NeuronLayer"/>. </param>
        /// <param name="random"> The <see cref="Random"/> object used to randomise the starting biases of the <see cref="Neuron"/>s. </param>
        public NeuronLayer(IReadOnlyNeuralNetwork neuralNetwork, uint index, int count, Random random) : this(neuralNetwork, index)
        {
            // Ensure the neuron count is not zero.
            if (count <= 0) throw new ArgumentException("Cannot make neuron layer with no or negative neurons.");

            // Initialise the neurons.
            neurons = randomiseNeurons(this, count, random);
        }

        /// <summary> Creates an empty <see cref="NeuronLayer"/> belonging to the given <paramref name="neuralNetwork"/> with the given <paramref name="index"/> and <see cref="Neuron"/> <paramref name="count"/>. </summary>
        /// <param name="neuralNetwork"> The <see cref="IReadOnlyNeuralNetwork"/> that this <see cref="NeuronLayer"/> belongs to. </param>
        /// <param name="index"> The index of this <see cref="NeuronLayer"/> within the <see cref="NeuralNetwork"/>. </param>
        /// <param name="count"> The number of <see cref="Neuron"/>s in this <see cref="NeuronLayer"/>. </param>
        private NeuronLayer(IReadOnlyNeuralNetwork neuralNetwork, uint index, int count) : this(neuralNetwork, index)
        {
            // Create the neurons array with null values.
            neurons = new Neuron[count];
        }

        /// <summary> Base constructor that sets the <paramref name="neuralNetwork"/> and <paramref name="index"/>. </summary>
        /// <param name="neuralNetwork"> The <see cref="IReadOnlyNeuralNetwork"/> that this <see cref="NeuronLayer"/> belongs to. </param>
        /// <param name="index"> The index of this <see cref="NeuronLayer"/> within the <see cref="NeuralNetwork"/>. </param>
        private NeuronLayer(IReadOnlyNeuralNetwork neuralNetwork, uint index)
        {
            // Set the neural network.
            NeuralNetwork = neuralNetwork ?? throw new ArgumentNullException(nameof(neuralNetwork));

            // Set the index.
            Index = index;
        }
        #endregion

        #region Initialisation Functions
        /// <summary> Creates an array of <see cref="Neuron"/>s with random biases. </summary>
        /// <param name="neuronLayer"> The <see cref="IReadOnlyNeuronLayer"/> that will contain the <see cref="Neuron"/>s. </param>
        /// <param name="count"> The amount of <see cref="Neuron"/>s to create. </param>
        /// <param name="random"> The <see cref="Random"/> object used to randomise the biases. </param>
        /// <returns> An array of <see cref="Neuron"/>s with random biases. </returns>
        private static Neuron[] randomiseNeurons(IReadOnlyNeuronLayer neuronLayer, int count, Random random)
        {
            Neuron[] newNeurons = new Neuron[count];
            for (uint i = 0; i < count; i++) newNeurons[i] = new Neuron(neuronLayer, random, i);
            return newNeurons;
        }
        #endregion

        #region Process Functions
        /// <summary> Causes each <see cref="Neuron"/> in this <see cref="NeuronLayer"/> to calculate its output, using <see cref="Neuron.CalculateOutput"/>. </summary>
        public void Pulse()
        {
            foreach (Neuron neuron in neurons) neuron.CalculateOutput();
        }
        #endregion

        #region Input Functions
        /// <summary> Sets the outputs of the <see cref="Neuron"/> in this <see cref="NeuronLayer"/> to the given <paramref name="input"/> values. </summary>
        /// <param name="input"> The values to set the outputs to. </param>
        public void SetInput(IReadOnlyCollection<float> input)
        {
            // Ensure the input is not null and the correct length.
            if (input is null || input.Count != Count) throw new ArgumentException("Given input is null or not the correct length.");

            // Set the output of each neuron to the value within the matching index of the input.
            for (int i = 0; i < input.Count; i++) neurons[i].Output = input.ElementAt(i);
        }
        #endregion

        #region Output Functions
        /// <summary> Gets the outputs from each <see cref="Neuron"/> in this <see cref="NeuronLayer"/> and puts them in a new array. </summary>
        /// <returns> An array of outputs from each <see cref="Neuron"/> in this <see cref="NeuronLayer"/>. </returns>
        public float[] GetOutput()
        {
            // Create a new array to hold the output values.
            float[] output = new float[Count];

            // Get the output.
            GetOutput(ref output);

            // Return the output.
            return output;
        }

        /// <summary> Gets the outputs from each <see cref="Neuron"/> in this <see cref="NeuronLayer"/> and puts them in the given <paramref name="output"/>. </summary>
        /// <param name="output"> The array to fill with the output from the output <see cref="neurons"/>. </param>
        public void GetOutput(ref float[] output)
        {
            // If the given array is of the wrong size, resize it.
            if (output.Length != Count) Array.Resize(ref output, Count);

            // Set the output values based on the neurons with the matching indices.
            for (int i = 0; i < Count; i++) output[i] = neurons[i].Output;
        }
        #endregion

        #region Learning Functions
        /// <summary> Calculates the changes this <see cref="NeuronLayer"/> wishes to make to its <see cref="neurons"/>. </summary>
        /// <param name="neuronLayerChange"> The changes to make to the <see cref="NeuronLayer"/>. </param>
        /// <param name="weightLayerChange"> The changes to make to the <see cref="WeightLayer"/>. </param>
        /// <param name="deltaOrError"> The delta or error of the next <see cref="NeuronLayer"/>, or calculated from the output of the <see cref="NeuralNetwork"/>. </param>
        public void CalculateChanges(NeuronLayerChange neuronLayerChange, WeightLayerChange weightLayerChange, IReadOnlyList<float> deltaOrError)
        {
#if DEBUG
            if (neuronLayerChange.NeuronLayer != this) throw new Exception("Neuron layer mismatch.");
            if (weightLayerChange.WeightLayer != PreviousWeightLayer) throw new Exception("Weight layer mismatch.");
#endif

            // If this is the output layer, calculate based on the error.
            if (NextWeightLayer is null)
                for (uint i = 0; i < Count; i++)
                    neuronLayerChange.SetDelta(this[i], neurons[i].CalculateChanges(neuronLayerChange, weightLayerChange, deltaOrError[(int)i]));
            // Otherwise; if this is a hidden layer, calculate based on the delta of the next layer.
            else
                for (uint i = 0; i < Count; i++)
                    neuronLayerChange.SetDelta(this[i], neurons[i].CalculateErrorThenChanges(neuronLayerChange, weightLayerChange, deltaOrError));
        }

        /// <summary> Applies all bias changes from the given <paramref name="layerChange"/> using the given <paramref name="learningRate"/>. </summary>
        /// <param name="layerChange"> The changes to make. </param>
        /// <param name="learningRate"> The scaled learning rate. </param>
        public void ApplyChanges(IReadOnlyNeuronLayerChange layerChange, float learningRate)
        {
            // Ensure this is the layer that the change is intended for.
            if (this != layerChange.NeuronLayer) throw new Exception("Layer mismatch.");

            // Change each neuron's bias.
            for (uint i = 0; i < Count; i++) neurons[i].ApplyChanges(layerChange.GetBias(this[i]), learningRate);
        }
        #endregion

        #region File Functions
        /// <summary> Creates and loads a new <see cref="NeuronLayer"/> from the given <see cref="INetworkLoader"/>. </summary>
        /// <param name="neuralNetwork"> The <see cref="IReadOnlyNeuralNetwork"/> that this <see cref="NeuronLayer"/> is for. </param>
        /// <param name="networkLoader"> The <see cref="INetworkLoader"/> used to load the network. </param>
        /// <param name="index"> The index of the <see cref="NeuronLayer"/> within the <see cref="NeuralNetwork"/>. </param>
        /// <returns> The created and loaded <see cref="NeuronLayer"/>. </returns>
        public static NeuronLayer Load(IReadOnlyNeuralNetwork neuralNetwork, INetworkLoader networkLoader, uint index)
        {
            // The amount of neurons in the layer.
            int neuronCount = networkLoader.GetLayerNeuronCount(index);

            // Create a new empty layer.
            NeuronLayer neuronLayer = new NeuronLayer(neuralNetwork, index, neuronCount);

            // Set the neurons of the layer to the loaded neurons from the loader.
            Neuron[] newNeurons = networkLoader.LoadNeuronsFromLayerIndex(neuronLayer, index);
            for (int i = 0; i < neuronCount; i++)
                neuronLayer.neurons[i] = newNeurons[i];

            // Return the filled neuron layer.
            return neuronLayer;
        }
        #endregion

        #region String Functions
        public override string ToString()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            string output = string.Empty;
            if (PreviousNeuronLayer is null) stringBuilder.Append("input ");
            else if (NextNeuronLayer is null) stringBuilder.Append("output ");

            stringBuilder.Append($"layer with index {Index} and {Count} neurons.");
            stringBuilder[0] = char.ToUpper(stringBuilder[0]);

            return stringBuilder.ToString();
        }
        #endregion
    }
}