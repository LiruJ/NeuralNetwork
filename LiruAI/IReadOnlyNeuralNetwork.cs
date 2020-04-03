using LiruAI.Layers;

namespace LiruAI
{
    public interface IReadOnlyNeuralNetwork
    {
        float LearningRate { get; }
        int NeuronLayerCount { get; }
        int WeightLayerCount { get; }
        int HighestNeuronCount { get; }
        IReadOnlyNeuronLayer InputLayer { get; }
        IReadOnlyNeuronLayer OutputLayer { get; }

        IReadOnlyNeuronLayer GetNeuronLayer(int index);
        IReadOnlyWeightLayer GetWeightLayer(int index);
    }
}