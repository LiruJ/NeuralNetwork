using LiruAI.Layers;
using LiruAI.Neurons;

namespace LiruAI.Learning
{
    public interface IReadOnlyNeuronLayerChange
    {
        IReadOnlyNeuronLayer NeuronLayer { get; }
        System.Collections.Generic.IReadOnlyList<float> Delta { get; }

        float GetBias(IReadOnlyNeuron neuron);
    }
}