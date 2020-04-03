using LiruAI.Neurons;
using System.Collections.Generic;

namespace LiruAI.Layers
{
    public interface IReadOnlyWeightLayer
    {
        uint Index { get; }
        IReadOnlyNeuralNetwork NeuralNetwork { get; }
        IReadOnlyNeuronLayer NextNeuronLayer { get; }
        IReadOnlyNeuronLayer PreviousNeuronLayer { get; }

        IEnumerable<uint> GetNextLayerConnections(IReadOnlyNeuron previousNeuron);
        IEnumerable<uint> GetPreviousLayerConnections(IReadOnlyNeuron nextNeuron);
        float GetWeightBetweenNeurons(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron);
    }
}