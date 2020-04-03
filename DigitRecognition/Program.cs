using DigitRecognition.Data;
using LiruAI;
using LiruAI.IO;
using LiruAI.Maths;
using LiruAI.Output;
using System;
using System.Diagnostics;

namespace DigitRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            IActivationFunction activationFunction = new TanH();

            for (float x = -2; x < 2; x += 0.1f)
            {
                Debug.Write($"{activationFunction.ActivationDerivative((float)Math.Tanh(x))},");
                //Debug.Write($"{Math.Tanh(x)},");
            }

            // Create the random object.
            Random random = new Random();

            // Load the data set.
            DigitDataSet digitDataSet = DigitDataSet.LoadFromFile("Files/labels.idx1-ubyte", "Files/images.idx3-ubyte");
            DigitDataSet testSet = DigitDataSet.LoadFromFile("Files/t10k-labels.idx1-ubyte", "Files/t10k-images.idx3-ubyte");

            // Take input.
            Console.WriteLine("Choose the size of a batch.");
            int batchSize = int.Parse(Console.ReadLine());

            Console.WriteLine("Choose number of epochs.");
            int epochs = int.Parse(Console.ReadLine());

            // Create the neural network.
            NeuralNetwork neuralNetwork = new NeuralNetwork(random, new int[] { digitDataSet.ImageWidth * digitDataSet.ImageHeight, 30, 10 }, 3.0f); //Network.NeuralNetwork.Load(new XmlNetworkLoader("DigitRecognition.xml"));

            neuralNetwork.LearnOverData(random, digitDataSet, epochs, batchSize, new NetworkConsoleLogger(OutputOptions.All ^ OutputOptions.CurrentData), testSet);
            //neuralNetwork.LearnOverData(random, digitDataSet, epochs, batchSize, new NetworkConsoleLogger(OutputOptions.None ^ OutputOptions.WhenFinished), testSet);

            neuralNetwork.Save(new XmlNetworkSaver("DigitRecognition.xml", true));

            Console.ReadKey();
        }
    }
}
