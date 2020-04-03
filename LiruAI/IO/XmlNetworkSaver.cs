using LiruAI.Layers;
using System;
using System.IO;
using System.Xml;
using static LiruAI.IO.XmlFileConstants;

namespace LiruAI.IO
{
    public class XmlNetworkSaver : INetworkSaver
    {
        #region Fields
        private readonly XmlDocument saveFile;

        private readonly string filePath;
        #endregion

        #region Constructors
        public XmlNetworkSaver(string filePath, bool overwrite = false)
        {
            // Ensure the file path was given.
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException("Given filepath cannot be null or empty.");
            this.filePath = filePath;

            // Force the extension onto the file path.
            if (!Path.HasExtension(filePath)) Path.ChangeExtension(filePath, FileExtension);

            // If a file already exists with the name, handle overwriting it.
            if (File.Exists(filePath))
            {
                if (overwrite) File.Delete(filePath);
                else throw new Exception("File already exists and overwrite is false.");
            }

            // Load the xml file.
            saveFile = new XmlDocument();

            // Create the main node.
            saveFile.AppendChild(saveFile.CreateElement(MainNodeName));
        }
        #endregion

        #region Save Functions
        public void Save() => saveFile.Save(filePath);

        public void SaveLearningRate(float learningRate)
        {
            XmlAttribute learningRateAttribute = createAttributeWithValue(saveFile, LearningRateAttributeName, learningRate);

            saveFile.SelectSingleNode(MainNodeName).Attributes.Append(learningRateAttribute);
        }

        public void SaveNeuronLayer(IReadOnlyNeuronLayer neuronLayer)
        {
            // Create a new neuron layer node.
            XmlNode neuronLayerNode = saveFile.CreateElement(NeuronLayerNodeName);

            // Add the index attribute.
            neuronLayerNode.Attributes.Append(createAttributeWithValue(saveFile, IndexAttributeName, neuronLayer.Index));

            // Add each neuron to the neuron layer.
            for (uint i = 0; i < neuronLayer.Count; i++)
            {
                // Create a new neuron node.
                XmlNode neuronNode = saveFile.CreateElement(NeuronNodeName);

                // Create the bias and index attributes.
                neuronNode.Attributes.Append(createAttributeWithValue(saveFile, IndexAttributeName, neuronLayer[i].Index));
                neuronNode.Attributes.Append(createAttributeWithValue(saveFile, BiasAttributeName, neuronLayer[i].Bias));

                // Append the neuron node to the neuron layer node.
                neuronLayerNode.AppendChild(neuronNode);
            }

            // Append the neuron layer to the main node.
            saveFile.SelectSingleNode(MainNodeName).AppendChild(neuronLayerNode);
        }

        public void CreateWeightLayer(uint index)
        {
            // Create a new weight layer node.
            XmlNode weightLayerNode = saveFile.CreateElement(WeightLayerNodeName);

            // Add the index attribute.
            weightLayerNode.Attributes.Append(createAttributeWithValue(saveFile, IndexAttributeName, index));

            // Append the neuron layer to the main node.
            saveFile.SelectSingleNode(MainNodeName).AppendChild(weightLayerNode);
        }

        public void SaveWeight(uint index, uint previousNeuronID, uint nextNeuronID, float weight)
        {
            // Get the weight layer node from the index.
            XmlNode weightLayerNode = saveFile.SelectSingleNode($"{MainNodeName}/{WeightLayerNodeName}[@{IndexAttributeName}={index}]");

            // Create a new element to hold the connection.
            XmlNode connectionNode = saveFile.CreateElement(ConnectionNodeName);

            // Add the attributes to the connection.
            connectionNode.Attributes.Append(createAttributeWithValue(saveFile, WeightAttributeName, weight));
            connectionNode.Attributes.Append(createAttributeWithValue(saveFile, FromIndexAttributeName, previousNeuronID));
            connectionNode.Attributes.Append(createAttributeWithValue(saveFile, ToIndexAttributeName, nextNeuronID));

            // Append the connection node to the weight layer node.
            weightLayerNode.AppendChild(connectionNode);
        }
        #endregion

        #region Helper Functions
        private static XmlAttribute createAttributeWithValue(XmlDocument document, string name, object value)
        {
            XmlAttribute attribute = document.CreateAttribute(name);
            attribute.Value = value.ToString();
            
            return attribute;
        }
        #endregion
    }
}
