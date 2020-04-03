using System;

namespace LiruAI.Output
{
    public class NetworkConsoleLogger : INetworkLogger
    {
        #region Properties
        public OutputOptions OutputOptions { get; }
        #endregion

        #region Constructors
        public NetworkConsoleLogger(OutputOptions outputOptions)
        {
            OutputOptions = outputOptions;
        }
        #endregion

        #region Log Functions
        public void Log(string input)
        {
            // If output is disabled, do nothing.
            if (OutputOptions == OutputOptions.None) return;

            Console.Write(input);
        }

        public void Log(int currentEpoch, int maximumEpochs, int currentBatch, int maximumBatches, int dataIndex, int batchSize, float lastBatchPercentage)
        {
            // If output is disabled, do nothing.
            if (OutputOptions == OutputOptions.None) return;

            string input = string.Empty;
            input += '\r';
            if ((OutputOptions & OutputOptions.CurrentEpoch)        == OutputOptions.CurrentEpoch)          input += $"Epoch: {currentEpoch + 1}/{maximumEpochs} ";
            if ((OutputOptions & OutputOptions.CurrentBatch)        == OutputOptions.CurrentBatch)          input += $"Batch: {currentBatch + 1}/{maximumBatches} ";
            if ((OutputOptions & OutputOptions.CurrentData)         == OutputOptions.CurrentData)           input += $"Data: {dataIndex + 1}/{batchSize} ";
            if ((OutputOptions & OutputOptions.LastBatchPercentage) == OutputOptions.LastBatchPercentage)   input += $"Correct: {lastBatchPercentage:P0}  ";
            Console.Write(input);
        }
        #endregion
    }
}
