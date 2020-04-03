namespace LiruAI.Learning
{
    public interface IReadOnlyNetworkChange
    {
        IReadOnlyNeuralNetwork NeuralNetwork { get; }

        IReadOnlyNeuronLayerChange GetNeuronLayerChange(uint index);

        IReadOnlyWeightLayerChange GetWeightLayerChange(uint index);
    }
}