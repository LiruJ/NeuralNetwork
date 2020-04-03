using LiruAI.Neurons;

namespace LiruAI.IO
{
    public interface INetworkLoader
    {
        System.Tuple<uint, float, uint>[] GetAllWeightConnections(uint index);
        int GetLayerNeuronCount(uint index);
        float GetNetworkLearningRate();
        int GetNetworkNeuronLayerCount();
        Neuron[] LoadNeuronsFromLayerIndex(Layers.IReadOnlyNeuronLayer neuronLayer, uint index);
    }
}
