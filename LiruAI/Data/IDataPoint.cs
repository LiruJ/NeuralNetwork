namespace LiruAI.Data
{
    public interface IDataPoint
    {
        byte Label { get; }

        void GetFloatData(ref float[] output);
    }
}