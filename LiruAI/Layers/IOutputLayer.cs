namespace LiruAI.Layers
{
    public interface IOutputLayer : IReadOnlyNeuronLayer
    {
        float[] GetOutput();
        void GetOutput(ref float[] output);
    }
}