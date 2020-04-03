using LiruAI.Neurons;
using System.Collections.Generic;

namespace LiruAI.Layers
{
    public interface INextWeightLayer
    {
        uint Index { get; }
        IReadOnlyNeuralNetwork NeuralNetwork { get; }
        IReadOnlyNeuronLayer NextNeuronLayer { get; }
        IReadOnlyNeuronLayer PreviousNeuronLayer { get; }

        IEnumerable<uint> GetNextLayerConnections(IReadOnlyNeuron previousNeuron);
        float GetWeightBetweenNeurons(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron);
    }
}