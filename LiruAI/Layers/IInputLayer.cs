using System.Collections.Generic;

namespace LiruAI.Layers
{
    public interface IInputLayer : IReadOnlyNeuronLayer
    {
        void SetInput(IReadOnlyCollection<float> input);
    }
}