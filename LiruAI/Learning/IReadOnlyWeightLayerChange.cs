using LiruAI.Layers;
using LiruAI.Neurons;

namespace LiruAI.Learning
{
    public interface IReadOnlyWeightLayerChange
    {
        IReadOnlyWeightLayer WeightLayer { get; }

        float GetWeightChangeBetweenNeurons(IReadOnlyNeuron previousNeuron, IReadOnlyNeuron nextNeuron);
    }
}