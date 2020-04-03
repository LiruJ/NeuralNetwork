using LiruAI.Neurons;

namespace LiruAI.Layers
{
    public interface IReadOnlyNeuronLayer
    {
        IReadOnlyNeuron this[uint index] { get; }

        int Count { get; }

        IReadOnlyNeuralNetwork NeuralNetwork { get; }

        IReadOnlyNeuronLayer NextNeuronLayer { get; }

        IReadOnlyNeuronLayer PreviousNeuronLayer { get; }

        uint Index { get; }
        IPreviousWeightLayer PreviousWeightLayer { get; }
        INextWeightLayer NextWeightLayer { get; }
    }
}