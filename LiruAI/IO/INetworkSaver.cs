namespace LiruAI.IO
{
    public interface INetworkSaver
    {
        void CreateWeightLayer(uint index);
        void Save();
        void SaveLearningRate(float learningRate);
        void SaveNeuronLayer(Layers.IReadOnlyNeuronLayer neuronLayer);
        void SaveWeight(uint index, uint previousNeuronID, uint nextNeuronID, float weight);
    }
}