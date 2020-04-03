namespace LiruAI.Layers
{
    public interface ILinkableNeuronLayer : IReadOnlyNeuronLayer
    {
        new IPreviousWeightLayer PreviousWeightLayer { get; set; }
        new INextWeightLayer NextWeightLayer { get; set; }
    }
}