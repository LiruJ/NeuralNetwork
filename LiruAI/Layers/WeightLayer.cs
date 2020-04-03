using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiruAI.IO;
using LiruAI.Learning;
using LiruAI.Neurons;

namespace LiruAI.Layers
{
    /// <summary> Represents a layer of weights connecting together two <see cref="NeuronLayer"/>s. </summary>
    public class WeightLayer : IReadOnlyWeightLayer, IPreviousWeightLayer, INextWeightLayer
    {
        #region Fields
        /// <summary> Each ID within this set is a neuron within the <see cref="PreviousNeuronLayer"/>, with the value being the dictionary of connections it has, with each ID in that being of the <see cref="NextNeuronLayer"/>'s neurons. </summary>
        private readonly Dictionary<uint, Dictionary<uint, weightConnection>> previousNeuronConnectionsByID;

        /// <summary> Each ID within this set is a neuron within the <see cref="NextNeuronLayer"/>, with the value being the dictionary of connections it has, with each ID in that being of the <see cref="PreviousNeuronLayer"/>'s neurons. </summary>
        private readonly Dictionary<uint, Dictionary<uint, weightConnection>> nextNeuronConnectionsByID;
        #endregion

        #region Properties
        /// <summary> The index of the <see cref="WeightLayer"/>, where an index of 0 is between the <see cref="NeuronLayer"/>s at indices 0 and 1. </summary>
        public uint Index { get; }

        /// <summary> The <see cref="IReadOnlyNeuralNetwork"/> that this <see cref="WeightLayer"/> is part of. </summary>
        public IReadOnlyNeuralNetwork NeuralNetwork { get; }

        /// <summary> The previously linked <see cref="IReadOnlyNeuronLayer"/>. </summary>
        public IReadOnlyNeuronLayer PreviousNeuronLayer { get; }

        /// <summary> The next linked <see cref="IReadOnlyNeuronLayer"/>. </summary>
        public IReadOnlyNeuronLayer NextNeuronLayer { get; }
        #endregion

        #region Constructors
        /// <summary> Creates a new <see cref="WeightLayer"/> for the given <paramref name="neuralNetwork"/> with the given <paramref name="index"/> and linked to the given <paramref name="nextNeuronLayer"/> and <paramref name="previousNeuronLayer"/>. </summary>
        /// <param name="neuralNetwork"> The <see cref="IReadOnlyNeuralNetwork"/> this <see cref="WeightLayer"/> is for. </param>
        /// <param name="index"> The index of this new <see cref="WeightLayer"/>. </param>
        /// <param name="previousNeuronLayer"> The previous <see cref="NeuronLayer"/> that this <see cref="WeightLayer"/> is linked to. </param>
        /// <param name="nextNeuronLayer"> The next <see cref="NeuronLayer"/> that this <see cref="WeightLayer"/> is linked to. </param>
        public WeightLayer(IReadOnlyNeuralNetwork neuralNetwork, uint index, ILinkableNeuronLayer previousNeuronLayer, ILinkableNeuronLayer nextNeuronLayer)
        {
            // Set the index.
            Index = index;

            // Set references.
            NeuralNetwork = neuralNetwork;
            PreviousNeuronLayer = previousNeuronLayer;
            NextNeuronLayer = nextNeuronLayer;

            // Link the previous and next layers to this layer.
            previousNeuronLayer.NextWeightLayer = this;
            nextNeuronLayer.PreviousWeightLayer = this;

            // Initialise dictionaries.
            previousNeuronConnectionsByID = new Dictionary<uint, Dictionary<uint, weightConnection>>(previousNeuronLayer.Count);
            nextNeuronConnectionsByID = new Dictionary<uint, Dictionary<uint, weightConnection>>(nextNeuronLayer.Count);
        }
        #endregion

        #region Get Functions
        /// <summary> Gets the weight between the given <paramref name="previousNeuron"/> and <paramref name="nextNeuron"/>. </summary>
        /// <param name="previousNeuron"> The <see cref="IReadOnlyNeuron"/> in the <see cref="PreviousNeuronLayer"/>. </param>
        /// <param name="nextNeuron"> The <see cref="IReadOnlyNeuron"/> in the <see cref="NextNeuronLayer"/>. </param>
        /// <returns> The weight between the given <paramref name="previousNeuron"/> and <paramref name="nextNeuron"/>. </returns>
        public float GetWeightBetweenNeurons(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron)
        {
#if DEBUG
            ensureValid(previousNeuron, nextNeuron);
#endif

            return previousNeuronConnectionsByID[previousNeuron.Index][nextNeuron.Index].Weight;
        }

        /// <summary> Gets an <see cref="IEnumerable"/> for the given <paramref name="nextNeuron"/>'s connections to the <see cref="PreviousNeuronLayer"/>. </summary>
        /// <param name="nextNeuron"></param>
        /// <returns></returns>
        public IEnumerable<uint> GetPreviousLayerConnections(IReadOnlyNeuron nextNeuron) => nextNeuronConnectionsByID[nextNeuron.Index].Keys.AsEnumerable();

        /// <summary> Gets an <see cref="IEnumerable"/> for the given <paramref name="previousNeuron"/>'s connections to the <see cref="NextNeuronLayer"/>. </summary>
        /// <param name="previousNeuron"></param>
        /// <returns></returns>
        public IEnumerable<uint> GetNextLayerConnections(IReadOnlyNeuron previousNeuron) => previousNeuronConnectionsByID[previousNeuron.Index].Keys.AsEnumerable();
        #endregion

        #region Link Functions
        /// <summary> Links the given <paramref name="previousNeuron"/> to the <paramref name="nextNeuron"/> with the given <paramref name="weight"/>. </summary>
        /// <param name="previousNeuron"> The <see cref="IReadOnlyNeuron"/> in the <see cref="PreviousNeuronLayer"/>. </param>
        /// <param name="nextNeuron"> The <see cref="IReadOnlyNeuron"/> in the <see cref="NextNeuronLayer"/>. </param>
        /// <param name="weight"> The weight to set. </param>
        public void LinkNeurons(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron, float weight)
        {
#if DEBUG
            ensureValid(previousNeuron, nextNeuron);
#endif

            // Create the connection.
            weightConnection weightConnection = new weightConnection(previousNeuron.Index, nextNeuron.Index, weight);

            // Add the previous connection using the previous neuron.
            if (previousNeuronConnectionsByID.TryGetValue(previousNeuron.Index, out Dictionary<uint, weightConnection> previousNeuronConnections)) previousNeuronConnections.Add(nextNeuron.Index, weightConnection);
            else previousNeuronConnectionsByID.Add(previousNeuron.Index, new Dictionary<uint, weightConnection>() { { nextNeuron.Index, weightConnection } });

            // Add the next connection using the next neuron.
            if (nextNeuronConnectionsByID.TryGetValue(nextNeuron.Index, out Dictionary<uint, weightConnection> nextNeuronConnections)) nextNeuronConnections.Add(previousNeuron.Index, weightConnection);
            else nextNeuronConnectionsByID.Add(nextNeuron.Index, new Dictionary<uint, weightConnection>() { { previousNeuron.Index, weightConnection } });
        }

        /// <summary> Links together all neurons in the previous layer to those in the next layer, and vice versa. </summary>
        /// <param name="random"> The <see cref="Random"/> object used to randomise the weights. </param>
        public void LinkAllNeurons(Random random)
        {
            // For each neuron in the previous layer.
            for (uint i = 0; i < PreviousNeuronLayer.Count; i++)
                // For each neuron in the next layer.
                for (uint j = 0; j < NextNeuronLayer.Count; j++)
                    // Link the neurons together.
                    //LinkNeurons(PreviousNeuronLayer[i], NextNeuronLayer[j], (float)(random.NextDouble() * 2.0f) - 1.0f);
                    LinkNeurons(PreviousNeuronLayer[i], NextNeuronLayer[j], (float)random.NextDouble() - 0.5f);
        }
        #endregion

        #region Learning Functions
        /// <summary> Apply all changes in the given <paramref name="weightLayerChange"/> to this <see cref="WeightLayer"/>. </summary>
        /// <param name="weightLayerChange"> The changes to make. </param>
        /// <param name="learningRate"> The learning rate applied to the changes. </param>
        public void ApplyChanges(IReadOnlyWeightLayerChange weightLayerChange, float learningRate)
        {
#if DEBUG
            // Ensure this is the intended weight layer.
            if (weightLayerChange.WeightLayer != this) throw new Exception("Weight layer mismatch.");
#endif

            // Apply each weight change.
            for (uint previousNeuronID = 0; previousNeuronID < PreviousNeuronLayer.Count; previousNeuronID++)
                foreach (uint nextNeuronID in GetNextLayerConnections(PreviousNeuronLayer[previousNeuronID]))
                {
                    // Get the weight change to make.
                    float weightChange = weightLayerChange.GetWeightChangeBetweenNeurons(PreviousNeuronLayer[previousNeuronID], NextNeuronLayer[nextNeuronID]) * learningRate;

                    // If the weight needs to change, change it.
                    if (weightChange != 0) previousNeuronConnectionsByID[previousNeuronID][nextNeuronID].Weight -= weightChange;
                }
        }
        #endregion

        #region Debug Functions
#if DEBUG
        private void ensureValid(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron)
        {
            // Ensure the given neurons are not null.
            if (previousNeuron is null) throw new ArgumentNullException(nameof(previousNeuron));
            if (nextNeuron is null) throw new ArgumentNullException(nameof(nextNeuron));

            // Ensure that the layers of the given neurons match.
            if (previousNeuron.NeuronLayer != PreviousNeuronLayer) throw new Exception("Given previous neuron was not part of the previous layer of this weight layer.");
            if (nextNeuron.NeuronLayer != NextNeuronLayer) throw new Exception("Given next neuron was not part of the next layer of this weight layer.");
        }
#endif
        #endregion

        #region Load Functions
        /// <summary> Creates, loads, links up, and returns a <see cref="WeightLayer"/> from the given <paramref name="networkLoader"/>. </summary>
        /// <param name="neuralNetwork"> The neural network for which this layer is created. </param>
        /// <param name="networkLoader"> The <see cref="INetworkLoader"/> used to load this <see cref="WeightLayer"/>. </param>
        /// <param name="index"> The index of this new <see cref="WeightLayer"/>. </param>
        /// <param name="previousNeuronLayer"> The previous <see cref="NeuronLayer"/> that this <see cref="WeightLayer"/> is linked to. </param>
        /// <param name="nextNeuronLayer"> The next <see cref="NeuronLayer"/> that this <see cref="WeightLayer"/> is linked to. </param>
        /// <returns> A new <see cref="WeightLayer"/> loaded from the given parameters. </returns>
        public static WeightLayer Load(IReadOnlyNeuralNetwork neuralNetwork, INetworkLoader networkLoader, uint index, ILinkableNeuronLayer previousNeuronLayer, ILinkableNeuronLayer nextNeuronLayer)
        {
            // Create the weight layer to return.
            WeightLayer weightLayer = new WeightLayer(neuralNetwork, index, previousNeuronLayer, nextNeuronLayer);

            // Get all defined connections from the file.
            Tuple<uint, float, uint>[] connections = networkLoader.GetAllWeightConnections(index);

            // Link the layers together based on the given connections.
            for (int i = 0; i < connections.Length; i++)
                weightLayer.LinkNeurons(previousNeuronLayer[connections[i].Item1], nextNeuronLayer[connections[i].Item3], connections[i].Item2);

            // Return the weight layer.
            return weightLayer;
        }
        #endregion

        #region Classes
        /// <summary> Represents a weighted connection between two neurons. </summary>
        /// <remarks> Using a class means that C# uses a reference, so the dictionaries do not need to be updated as it updates the reference. </remarks>
        private class weightConnection
        {
            #region Properties
            public uint InputNeuronID { get; }

            public uint OutputNeuronID { get; }

            public float Weight { get; set; }
            #endregion

            #region Constructors
            public weightConnection(uint inputNeuronID, uint outputNeuronID, float weight)
            {
                this.InputNeuronID = inputNeuronID;
                this.OutputNeuronID = outputNeuronID;
                this.Weight = weight;
            }
            #endregion

            #region String Functions
            public override string ToString() => $"{InputNeuronID} <=({Weight})=> {OutputNeuronID}.";
            #endregion
        }
        #endregion
    }
}
