using System;
using System.Collections.Generic;
using System.IO;
using Be.IO;
using LiruAI.Data;

namespace DigitRecognition.Data
{
    public class DigitDataSet : IDataSet
    {
        #region Constants
        private const int labelMagicNumber = 2049;

        private const int dataMagicNumber = 2051;
        #endregion

        #region Fields
        private List<DigitImage> digits;
        #endregion

        #region Properties
        public int ImageWidth { get; }

        public int ImageHeight { get; }

        public int Count => digits.Count;
        #endregion

        #region Indexers
        public IDataPoint this[int index] => digits[index];
        #endregion

        #region Constructors
        private DigitDataSet(BeBinaryReader labelReader, BeBinaryReader dataReader)
        {
            // Read the data length, ensure they are the same in labels and data.
            int numberOfDatum = labelReader.ReadInt32();
            if (numberOfDatum != dataReader.ReadInt32()) throw new Exception("Data lengths of label and data files do not match.");

            // Initialise the digits list.
            digits = new List<DigitImage>(numberOfDatum);

            // Set the width and height.
            ImageWidth = dataReader.ReadInt32();
            ImageHeight = dataReader.ReadInt32();

            // Read each digit and label, and save them into the list.
            for (int i = 0; i < numberOfDatum; i++)
                digits.Add(new DigitImage(dataReader.ReadBytes(ImageWidth * ImageHeight), labelReader.ReadByte()));
        }
        #endregion

        #region Load Functions
        public static DigitDataSet LoadFromFile(string labelPath, string dataPath)
        {
            // Ensure the files exist.
            if (!File.Exists(labelPath)) throw new FileNotFoundException("The given label file path does not exist.");
            if (!File.Exists(dataPath)) throw new FileNotFoundException("The given data file path does not exist.");

            // Open the files in binary readers.
            BeBinaryReader labelReader = new BeBinaryReader(File.OpenRead(labelPath));
            BeBinaryReader dataReader = new BeBinaryReader(File.OpenRead(dataPath));

            // Ensure the magic numbers are correct.
            int labelVersion = labelReader.ReadInt32();
            int dataVersion = dataReader.ReadInt32();
            if (labelVersion != labelMagicNumber) throw new Exception($"Label file's version number was invalid. Expecting {labelMagicNumber}, got {labelVersion}.");
            if (dataVersion != dataMagicNumber) throw new Exception($"Data file's version number was invalid. Expecting {dataMagicNumber}, got {dataVersion}.");

            // Create the data set from the readers, and return it.
            return new DigitDataSet(labelReader, dataReader);
        }
        #endregion

        #region Shuffle Functions
        public void Shuffle(Random random)
        {
            // Create a new list to store the shuffled digits.
            List<DigitImage> shuffledDigits = new List<DigitImage>(digits.Count);

            // Keep taking random digits from the main array and putting them into the shuffled array until there are none left.
            while (digits.Count > 0)
            {
                // Randomly generate an index.
                int index = random.Next(0, digits.Count);

                // Get the digit.
                DigitImage digit = digits[index];

                // Remove the digit from the main list.
                digits.RemoveAt(index);

                // Add the digit to the shuffled list.
                shuffledDigits.Add(digit);
            }

            // Set the digits to the shuffled digits.
            digits = shuffledDigits;
        }
        #endregion
    }
}
