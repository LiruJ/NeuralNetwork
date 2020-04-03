using LiruAI.Neurons;
using System.Collections.Generic;

namespace LiruAI.Layers
{
    public interface IPreviousWeightLayer
    {
        uint Index { get; }
        IReadOnlyNeuralNetwork NeuralNetwork { get; }
        IReadOnlyNeuronLayer NextNeuronLayer { get; }
        IReadOnlyNeuronLayer PreviousNeuronLayer { get; }

        IEnumerable<uint> GetPreviousLayerConnections(IReadOnlyNeuron nextNeuron);
        float GetWeightBetweenNeurons(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron);
    }
}