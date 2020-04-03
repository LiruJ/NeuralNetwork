using LiruAI.Layers;
using LiruAI.Neurons;
using System;
using System.IO;
using System.Xml;
using static LiruAI.IO.XmlFileConstants;

namespace LiruAI.IO
{
    public class XmlNetworkLoader : INetworkLoader
    {
        #region Fields
        private readonly XmlDocument loadedFile;
        #endregion

        #region Constructors
        public XmlNetworkLoader(string filePath)
        {
            // Ensure the file path was given.
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException("Given filepath cannot be null or empty.");

            // Ensure the file path is valid.
            if (!File.Exists(filePath))
                if (!Path.HasExtension(filePath))
                {
                    // Find the file with the extension.
                    Path.ChangeExtension(filePath, FileExtension);
                    if (!File.Exists(filePath)) throw new FileNotFoundException($"File does not exist with or without {FileExtension} extension.", filePath);
                }

            // Load the xml file.
            loadedFile = new XmlDocument();
            loadedFile.Load(filePath);
        }
        #endregion

        #region Load Functions
        public int GetNetworkNeuronLayerCount()
        {
            // Get the layer nodes.
            XmlNodeList layerNodes = loadedFile.SelectNodes($"{MainNodeName}/{NeuronLayerNodeName}");

            // Return the number of nodes within the layer nodes list.
            return layerNodes.Count;
        }

        public float GetNetworkLearningRate()
        {
            // Get and parse the learning rate.
            if (!(loadedFile.SelectSingleNode(MainNodeName).Attributes.GetNamedItem(LearningRateAttributeName) is XmlNode learningRateAttribute) || !float.TryParse(learningRateAttribute.Value, out float learningRate))
                throw new Exception("Network's learning rate was missing or invalid.");

            // Return the learning rate.
            return learningRate;
        }

        public int GetLayerNeuronCount(uint index)
        {
            // Get the neuron layer node from the index.
            XmlNode neuronLayerNode = loadedFile.SelectSingleNode($"{MainNodeName}/{NeuronLayerNodeName}[@{IndexAttributeName}={index}]");

            // Return the number of children the layer node has.
            return neuronLayerNode.ChildNodes.Count;
        }

        public Neuron[] LoadNeuronsFromLayerIndex(IReadOnlyNeuronLayer neuronLayer, uint index)
        {
            // Get the neuron layer node from the index.
            XmlNode neuronLayerNode = loadedFile.SelectSingleNode($"{MainNodeName}/{NeuronLayerNodeName}[@{IndexAttributeName}={index}]");

            // Get the neuron nodes from the layer node.
            XmlNodeList neuronNodes = neuronLayerNode.SelectNodes(NeuronNodeName);

            // Create an array of neurons to fill the layer.
            Neuron[] neurons = new Neuron[neuronNodes.Count];

            // Create and add each neuron to the array.
            for (uint i = 0; i < neurons.Length; i++)
            {
                // The neuron node.
                XmlNode currentNeuronNode = neuronNodes[(int)i];

                // Parse the bias.
                if (!(currentNeuronNode.Attributes.GetNamedItem(BiasAttributeName) is XmlNode biasAttributeNode) || !float.TryParse(biasAttributeNode.Value, out float bias))
                    throw new Exception($"Neuron's bias attribute was missing or incorrect. Layer index {neuronLayer.Index}, neuron index {i}.");

                // Create the neuron.
                neurons[i] = new Neuron(neuronLayer, bias, i);
            }

            // Return the neuron array.
            return neurons;
        }

        public Tuple<uint, float, uint>[] GetAllWeightConnections(uint index)
        {
            // Get the weight layer node from the index.
            XmlNode weightLayerNode = loadedFile.SelectSingleNode($"{MainNodeName}/{WeightLayerNodeName}[@{IndexAttributeName}={index}]");

            // Get all the connection nodes of the weight layer.
            XmlNodeList connectionNodes = weightLayerNode.SelectNodes(ConnectionNodeName);

            // Create an array to hold the values to return.
            Tuple<uint, float, uint>[] connections = new Tuple<uint, float, uint>[connectionNodes.Count];

            // Parse each connection.
            for (int i = 0; i < connections.Length; i++)
            {
                // The current connection node.
                XmlNode currentConnectionNode = connectionNodes[i];

                // Parse the values
                if (!(currentConnectionNode.Attributes.GetNamedItem(FromIndexAttributeName) is XmlNode fromAttributeNode) || !uint.TryParse(fromAttributeNode.Value, out uint fromIndex))
                    throw new Exception($"Connection's from index was invalid. Weight layer index {index}, connection index {i}.");

                if (!(currentConnectionNode.Attributes.GetNamedItem(ToIndexAttributeName) is XmlNode toAttributeNode) || !uint.TryParse(toAttributeNode.Value, out uint toIndex))
                    throw new Exception($"Connection's to index was invalid. Weight layer index {index}, connection index {i}.");

                if (!(currentConnectionNode.Attributes.GetNamedItem(WeightAttributeName) is XmlNode weightAttributeNode) || !float.TryParse(weightAttributeNode.Value, out float weight))
                    throw new Exception($"Connection's weight was invalid. Weight layer index {index}, connection index {i}.");

                // Set the connection.
                connections[i] = new Tuple<uint, float, uint>(fromIndex, weight, toIndex);
            }

            // Return the connections.
            return connections;
        }
        #endregion
    }
}
