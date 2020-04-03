using LiruAI.Layers;
using LiruAI.Neurons;
using System;
using System.Collections.Generic;

namespace LiruAI.Learning
{
    /// <summary> Represents the changes to be made to a <see cref="IReadOnlyWeightLayer"/>. </summary>
    public class WeightLayerChange : IReadOnlyWeightLayerChange
    {
        #region Fields
        /// <summary> A dictionary in which each key is a neuron's index from the next layer, with each value being another dictionary of the neuron's index in the previous layer, with the value of that being the weight change. </summary>
        private readonly Dictionary<uint, Dictionary<uint, float>> nextNeuronConnections;
        #endregion

        #region Properties
        /// <summary> The <see cref="IReadOnlyWeightLayer"/> that these changes are for. </summary>
        public IReadOnlyWeightLayer WeightLayer { get; }
        #endregion

        #region Constructors
        /// <summary> Creates a new <see cref="WeightLayerChange"/> for the given <paramref name="weightLayer"/>. </summary>
        /// <param name="weightLayer"> The <see cref="IReadOnlyWeightLayer"/> that the changes are for. </param>
        public WeightLayerChange(IReadOnlyWeightLayer weightLayer)
        {
            // Set the weight layer.
            WeightLayer = weightLayer ?? throw new ArgumentNullException(nameof(weightLayer));

            // Initialise the changes dictionary.
            nextNeuronConnections = new Dictionary<uint, Dictionary<uint, float>>(weightLayer.NextNeuronLayer.Count);
            for (uint i = 0; i < weightLayer.NextNeuronLayer.Count; i++)
                nextNeuronConnections.Add(i, new Dictionary<uint, float>(weightLayer.PreviousNeuronLayer.Count));
            Reset();
        }
        #endregion

        #region Get Functions
        /// <summary> Gets the change of weight between the given <paramref name="nextNeuron"/> and <paramref name="previousNeuron"/>. </summary>
        /// <param name="previousNeuron"> The neuron in the previous <see cref="NeuronLayer"/>. </param>
        /// <param name="nextNeuron"> The neuron in the next <see cref="NeuronLayer"/>. </param>
        /// <returns> The weight between the given <paramref name="nextNeuron"/> and <paramref name="previousNeuron"/>. </returns>
        public float GetWeightChangeBetweenNeurons(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron)
        {
#if DEBUG
            // Ensure the neurons are for this weight layer.
            ensureValidity(previousNeuron, nextNeuron);
#endif

            // Return the weight.
            return nextNeuronConnections[nextNeuron.Index][previousNeuron.Index];
        }
        #endregion

        #region Add Functions
        /// <summary> Adds the given <paramref name="weightChange"/> to the connection between the given <paramref name="nextNeuron"/> and <paramref name="previousNeuron"/>. </summary>
        /// <param name="previousNeuron"> The neuron in the previous <see cref="NeuronLayer"/>. </param>
        /// <param name="nextNeuron"> The neuron in the next <see cref="NeuronLayer"/>. </param>
        /// <param name="weightChange"> The amount of weight to change. </param>
        public void AddWeightChangeBetweenNeurons(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron, float weightChange)
        {
#if DEBUG
            // Ensure the neurons are for this weight layer.
            ensureValidity(previousNeuron, nextNeuron);
#endif

            // If there is no change to be made, do nothing.
            if (weightChange == 0) return;

            // Add the weight.
            nextNeuronConnections[nextNeuron.Index][previousNeuron.Index] += weightChange;
        }
        #endregion

        #region Reset Functions
        /// <summary> Resets all weight changes to 0. </summary>
        public void Reset()
        {
            for (uint nextNeuronID = 0; nextNeuronID < WeightLayer.NextNeuronLayer.Count; nextNeuronID++)
                foreach (uint previousNeuronID in WeightLayer.GetPreviousLayerConnections(WeightLayer.NextNeuronLayer[nextNeuronID]))
                    nextNeuronConnections[nextNeuronID][previousNeuronID] = 0;
        }
        #endregion

        #region Validity Functions
#if DEBUG
        private void ensureValidity(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron)
        {
            if (previousNeuron.NeuronLayer.NextWeightLayer != WeightLayer) throw new Exception("Neuron mismatch, previous neuron is not in the expected layer.");
            if (nextNeuron.NeuronLayer.PreviousWeightLayer != WeightLayer) throw new Exception("Neuron mismatch, next neuron is not in the expected layer.");
        }
#endif  
        #endregion
    }
}
