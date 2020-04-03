using System;

namespace LiruAI.Data
{
    public interface IDataSet
    {
        IDataPoint this[int index] { get; }

        int Count { get; }

        void Shuffle(Random random);
    }
}