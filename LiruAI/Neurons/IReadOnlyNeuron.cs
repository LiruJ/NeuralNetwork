using LiruAI.Layers;

namespace LiruAI.Neurons
{
    public interface IReadOnlyNeuron
    {
        float Bias { get; }
        float Output { get; }
        
        IReadOnlyNeuronLayer NeuronLayer { get; }

        uint Index { get; }
    }
}