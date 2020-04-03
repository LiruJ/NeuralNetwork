namespace LiruAI.Output
{
    public interface INetworkLogger
    {
        OutputOptions OutputOptions { get; }

        void Log(int currentEpoch, int maximumEpochs, int currentBatch, int maximumBatches, int dataIndex, int batchSize, float lastBatchPercentage);
        void Log(string input);
    }
}