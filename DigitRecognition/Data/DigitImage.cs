using LiruAI.Data;
using System;
using System.Collections.Generic;

namespace DigitRecognition.Data
{
    public struct DigitImage : IDataPoint
    {
        #region Fields
        private readonly byte[] data;
        #endregion

        #region Properties
        public IReadOnlyCollection<byte> Data => data;

        public byte Label { get; }
        #endregion

        #region Constructors
        public DigitImage(byte[] data, byte label)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            Label = label;
        }
        #endregion

        #region Get Functions
        public void GetFloatData(ref float[] output)
        {
            // Create a new array for the data.
            if (data.Length != output.Length) Array.Resize(ref output, output.Length);

            // Add each data point to the array as a float from 0 to 1.
            for (int i = 0; i < output.Length; i++) output[i] = data[i] / (float)byte.MaxValue;
        }
        #endregion
    }
}
