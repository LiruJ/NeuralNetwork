using LiruAI.Layers;
using LiruAI.Neurons;
using System;
using System.Collections.Generic;

namespace LiruAI.Learning
{
    /// <summary> Represents a change to be made to a neuron layer. </summary>
    public class NeuronLayerChange : IReadOnlyNeuronLayerChange
    {
        #region Fields
        /// <summary> The changes to bias for each <see cref="Neuron"/> in the represented <see cref="NeuronLayer"/>. </summary>
        private readonly float[] biasChanges;

        /// <summary> The calculated delta from each <see cref="Neuron"/> in the represented <see cref="NeuronLayer"/>. </summary>
        private readonly float[] delta;
        #endregion

        #region Properties
        /// <summary> The represented <see cref="IReadOnlyNeuronLayer"/>. </summary>
        public IReadOnlyNeuronLayer NeuronLayer { get; }

        /// <summary> The deltas for each <see cref="Neuron"/> in the represented <see cref="NeuronLayer"/>. </summary>
        public IReadOnlyList<float> Delta => delta;
        #endregion

        #region Constructors
        /// <summary> Creates a new <see cref="NeuronLayerChange"/> for the given <paramref name="neuronLayer"/>. </summary>
        /// <param name="neuronLayer"> The <see cref="IReadOnlyNeuronLayer"/> whose changes are being stored. </param>
        public NeuronLayerChange(IReadOnlyNeuronLayer neuronLayer)
        {
            // Set the neuron layer.
            NeuronLayer = neuronLayer ?? throw new ArgumentNullException(nameof(neuronLayer));

            // Initialise the bias changes and deltas.
            biasChanges = new float[neuronLayer.Count];
            delta = new float[neuronLayer.Count];
        }
        #endregion

        #region Get Functions
        /// <summary> Gets the change to bias for the given <paramref name="neuron"/>. </summary>
        /// <param name="neuron"> The <see cref="IReadOnlyNeuron"/> whose bias change is desired. </param>
        /// <returns> The change to bias for the given <paramref name="neuron"/>. </returns>
        public float GetBias(IReadOnlyNeuron neuron)
        {
#if DEBUG
            // Ensure this is the correct layer for the given neuron.
            if (neuron.NeuronLayer != NeuronLayer) throw new Exception("Layer mismatch.");
#endif

            // Return the bias change.
            return biasChanges[neuron.Index];
        }
        #endregion

        #region Add Functions
        /// <summary> Adds the given <paramref name="biasChange"/> to the bias for the given <paramref name="neuron"/>. </summary>
        /// <param name="neuron"> The <see cref="IReadOnlyNeuron"/> who bias is being changed. </param>
        /// <param name="biasChange"> The change to the bias. </param>
        public void AddBias(IReadOnlyNeuron neuron, float biasChange)
        {
#if DEBUG
            // Ensure this is the correct layer for the given neuron.
            if (neuron.NeuronLayer != NeuronLayer) throw new Exception("Layer mismatch.");
#endif

            // Add the bias change.
            biasChanges[neuron.Index] += biasChange;
        }

        /// <summary> Sets the delta for the given <paramref name="neuron"/> to the given <paramref name="newDelta"/>. </summary>
        /// <param name="neuron"> The <see cref="IReadOnlyNeuron"/> whose delta is being set. </param>
        /// <param name="newDelta"> The new value of the delta. </param>
        public void SetDelta(IReadOnlyNeuron neuron, float newDelta)
        {
#if DEBUG
            // Ensure this is the correct layer for the given neuron.
            if (neuron.NeuronLayer != NeuronLayer) throw new Exception("Layer mismatch.");
#endif

            // Set the delta.
            delta[neuron.Index] = newDelta;
        }
        #endregion

        #region Reset Functions
        /// <summary> Resets every bias and delta in this <see cref="NeuronLayerChange"/> to 0. </summary>
        public void Reset()
        {
            for (int i = 0; i < biasChanges.Length; i++)
            {
                delta[i] = 0;
                biasChanges[i] = 0;
            }
        }
        #endregion
    }
}
