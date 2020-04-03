using System;
using System.Collections.Generic;
using System.Text;

namespace LiruAI.Output
{
    [Flags]
    public enum OutputOptions : byte
    {
        None                = 0b0000_0000,
        All                 = 0b1111_1111,
        WhenFinished        = 0b1000_0000,
        CurrentEpoch        = 0b0100_0000,
        CurrentBatch        = 0b0010_0000,
        CurrentData         = 0b0001_0000,
        LastBatchPercentage = 0b0000_1000
    }
}
