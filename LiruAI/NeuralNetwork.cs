using LiruAI.Data;
using LiruAI.IO;
using LiruAI.Layers;
using LiruAI.Learning;
using LiruAI.Maths;
using LiruAI.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiruAI
{
    /// <summary> Represents a neural network that can take process input, as well as learn. </summary>
    public class NeuralNetwork : IReadOnlyNeuralNetwork
    {
        #region Constants
        /// <summary> The learning rate used when none is given. </summary>
        public const float DefaultLearningRate = 0.1f;

        private static readonly IErrorFunction defaultErrorFunction = new SquaredError();
        #endregion

        #region Fields
        /// <summary> The <see cref="NeuronLayer"/>s of the network. </summary>
        private readonly NeuronLayer[] neuronLayers;

        /// <summary> The <see cref="WeightLayer"/>s connecting together the <see cref="NeuronLayer"/>s of the network. </summary>
        private readonly WeightLayer[] weightLayers;

        private readonly IErrorFunction errorFunction = defaultErrorFunction;
        #endregion

        #region Properties
        /// <summary> The input layer, allowing for input to be easily set. </summary>
        private IInputLayer inputLayer => neuronLayers[0];

        /// <summary> The output layer, allowing for output to be easily gotten. </summary>
        private IOutputLayer outputLayer => neuronLayers[neuronLayers.Length - 1];

        /// <summary> The very first <see cref="IReadOnlyNeuronLayer"/> of the network. </summary>
        public IReadOnlyNeuronLayer InputLayer => inputLayer;

        /// <summary> The very last <see cref="IReadOnlyNeuronLayer"/> of the network. </summary>
        public IReadOnlyNeuronLayer OutputLayer => outputLayer;

        /// <summary> The learning rate of the network. </summary>
        public float LearningRate { get; set; }

        /// <summary> The number of <see cref="NeuronLayer"/>s within the network. </summary>
        public int NeuronLayerCount => neuronLayers.Length;

        /// <summary> The number of <see cref="WeightLayer"/>s within the network. </summary>
        public int WeightLayerCount => NeuronLayerCount - 1;

        /// <summary> The highest amount of neurons that any layer in this network has. </summary>
        public int HighestNeuronCount { get; private set; }
        #endregion

        #region Constructors
        /// <summary> Creates a new network with completely random values, using the values in the given <paramref name="layerSizes"/> to create the <see cref="neuronLayers"/>. </summary>
        /// <param name="random"> The <see cref="Random"/> object used to randomise the weights and biases. </param>
        /// <param name="layerSizes"> An array where each index associates to a <see cref="NeuronLayer"/>, with its value being the amount of <see cref="Neurons.Neuron"/>s to create. </param>
        /// <param name="learningRate"> The learning rate to use, defaults to <see cref="DefaultLearningRate"/> if not given. </param>
        public NeuralNetwork(Random random, int[] layerSizes, float learningRate = DefaultLearningRate)
        {
            // Ensure the number of given sizes allows for an input, output, and hidden layer.
            if (layerSizes is null || layerSizes.Length < 3) throw new ArgumentException("Neural network requires at least 3 layers to function.");

            // Create and initialise the layers.
            neuronLayers = new NeuronLayer[layerSizes.Length];
            weightLayers = new WeightLayer[layerSizes.Length - 1];
            initialiseLayers(random, layerSizes);

            // Set the learning rate.
            LearningRate = learningRate;

            // Set the highest neuron count.
            HighestNeuronCount = calculateHighestNeuronCount(this);
        }

        /// <summary> Creates an empty network with the given <paramref name="learningRate"/>, <paramref name="neuronLayers"/>, and <paramref name="weightLayers"/>. </summary>
        /// <param name="learningRate"> The learning rate to use. </param>
        /// <param name="neuronLayers"> The <see cref="NeuronLayer"/>s of the new network. </param>
        /// <param name="weightLayers"> The <see cref="WeightLayer"/>s of the new network. </param>
        private NeuralNetwork(float learningRate, NeuronLayer[] neuronLayers, WeightLayer[] weightLayers)
        {
            // Ensure the given layers are valid.
            if (neuronLayers is null || neuronLayers.Length == 0) throw new ArgumentException("Given neuron layers were null or empty");
            if (weightLayers is null || weightLayers.Length != neuronLayers.Length - 1) throw new Exception("Given weight layers were null or not of the correct length.");

            // Set the learning rate.
            LearningRate = learningRate;

            // Set the layers.
            this.neuronLayers = neuronLayers;
            this.weightLayers = weightLayers;
        }

        /// <summary> Creates an empty network with the given <paramref name="learningRate"/> and <paramref name="neuronLayerCount"/>. </summary>
        /// <param name="learningRate"> The learning rate to use. </param>
        /// <param name="neuronLayerCount"> The number of <see cref="NeuronLayer"/>s to create. </param>
        private NeuralNetwork(float learningRate, int neuronLayerCount) : this(learningRate, new NeuronLayer[neuronLayerCount], new WeightLayer[neuronLayerCount - 1]) { }
        #endregion

        #region Initialisation Functions
        /// <summary> Calculates and returns the highest number of <see cref="Neurons.Neuron"/>s in the given <paramref name="neuralNetwork"/>'s <see cref="neuronLayers"/>. </summary>
        /// <param name="neuralNetwork"> The <see cref="NeuralNetwork"/> whose <see cref="NeuronLayer"/>s to calculate from. </param>
        /// <returns> The highest number of <see cref="Neurons.Neuron"/>s in the given <paramref name="neuralNetwork"/>'s <see cref="neuronLayers"/>. </returns>
        private static int calculateHighestNeuronCount(IReadOnlyNeuralNetwork neuralNetwork)
        {
            int highestNeuronCount = 0;
            for (int l = 0; l < neuralNetwork.NeuronLayerCount; l++)
                if (neuralNetwork.GetNeuronLayer(l).Count > highestNeuronCount)
                    highestNeuronCount = neuralNetwork.GetNeuronLayer(l).Count;

            return highestNeuronCount;
        }

        /// <summary> Creates and initialises each <see cref="NeuronLayer"/> in the <see cref="neuronLayers"/>. </summary>
        /// <param name="random"> The <see cref="Random"/> object used to randomise the weights and biases of the network. </param>
        /// <param name="layerSizes"> The desired sizes of each <see cref="NeuronLayer"/>. </param>
        private void initialiseLayers(Random random, int[] layerSizes)
        {
            // Create the neuron layers.
            for (uint i = 0; i < layerSizes.Length; i++)
                neuronLayers[i] = new NeuronLayer(this, i, layerSizes[i], random);

            // Create the weight layers. Note that the input and output layers only have one weight layer.
            for (uint i = 0; i < layerSizes.Length - 1; i++)
            {
                // Create the weight layer.
                weightLayers[i] = new WeightLayer(this, i, neuronLayers[i], neuronLayers[i + 1]);

                // Link the layers.
                weightLayers[i].LinkAllNeurons(random);
            }
        }
        #endregion

        #region Get Functions
        /// <summary> Gets the <see cref="IReadOnlyNeuronLayer"/> associated with the given <paramref name="index"/>. </summary>
        /// <param name="index"> The index of the <see cref="IReadOnlyNeuronLayer"/> to get. </param>
        /// <returns> The <see cref="IReadOnlyNeuronLayer"/> associated with the given <paramref name="index"/>. </returns>
        public IReadOnlyNeuronLayer GetNeuronLayer(int index) => neuronLayers[index];

        /// <summary> Gets the <see cref="IReadOnlyWeightLayer"/> associated with the given <paramref name="index"/>. </summary>
        /// <param name="index"> The index of the <see cref="IReadOnlyWeightLayer"/> to get. </param>
        /// <returns> The <see cref="IReadOnlyWeightLayer"/> associated with the given <paramref name="index"/>. </returns>
        public IReadOnlyWeightLayer GetWeightLayer(int index) => weightLayers[index];
        #endregion

        #region Process Functions
        /// <summary> Processes the given <paramref name="input"/> through the <see cref="NeuralNetwork"/>, and returning the result from the <see cref="OutputLayer"/>. </summary>
        /// <param name="input"> A collection of floats to input. Must be equal in length to the amount of input neurons in the <see cref="InputLayer"/>. </param>
        /// <returns> An array of floats from the <see cref="OutputLayer"/>  after the <paramref name="input"/> has been processed. </returns>
        public float[] ProcessInput(IReadOnlyCollection<float> input)
        {
            // Create an array to hold the output.
            float[] output = new float[outputLayer.Count];

            // Get the output.
            ProcessInput(input, ref output);

            // Return the output.
            return output;
        }

        /// <summary> Processes the given <paramref name="input"/> through the <see cref="NeuralNetwork"/>, and filling the <paramref name="output"/> array with the result from the <see cref="OutputLayer"/>. </summary>
        /// <param name="input"> A collection of floats to input. Must be equal in length to the amount of input neurons in the <see cref="InputLayer"/>. </param>
        /// <param name="output"> The array to fill with output from the <see cref="OutputLayer"/> after the <paramref name="input"/> has been processed. Must be equal in length to the amount of output neurons in the <see cref="OutputLayer"/>. </param>
        public void ProcessInput(IReadOnlyCollection<float> input, ref float[] output)
        {
            // Give the input to the input layer.
            inputLayer.SetInput(input);

            // Pulse each non-input layer.
            for (int i = 1; i < neuronLayers.Length; i++) neuronLayers[i].Pulse();

            // Set the output.
            outputLayer.GetOutput(ref output);
        }

        /// <summary> Calculates the <paramref name="error"/> of the <see cref="NeuralNetwork"/> based on the given <paramref name="desiredOutput"/> and the outputs in the <see cref="OutputLayer"/>. </summary>
        /// <param name="desiredOutput"> The desired output of the <see cref="NeuralNetwork"/>. </param>
        /// <param name="error"> The array to fill with the errors. Must be equal in length to the amount of output neurons in the <see cref="OutputLayer"/>. </param>
        public void CalculateError(IReadOnlyCollection<float> desiredOutput, ref float[] error) => CalculateError(desiredOutput, outputLayer.GetOutput(), ref error);

        /// <summary> Calculates the <paramref name="error"/> of the <see cref="NeuralNetwork"/> based on the given <paramref name="desiredOutput"/> and the given <paramref name="output"/>. </summary>
        /// <param name="desiredOutput"></param>
        /// <param name="output"></param>
        /// <param name="error"></param>
        public void CalculateError(IReadOnlyCollection<float> desiredOutput, IReadOnlyCollection<float> output, ref float[] error) => error.ApplyVectorisedFunction((e) => errorFunction.ErrorDerivative(output.ElementAt(e), desiredOutput.ElementAt(e)));
        #endregion

        #region Output Functions
        /// <summary> Fills the given <paramref name="output"/> array with the outputs from the <see cref="OutputLayer"/>. </summary>
        /// <param name="output"> The array to fill with the outputs. </param>
        public void GetOutput(ref float[] output) => outputLayer.GetOutput(ref output);

        /// <summary> Creates and returns a new array of floats with the values from the <see cref="OutputLayer"/>. </summary>
        /// <returns> The output from the <see cref="OutputLayer"/>. </returns>
        public float[] GetOutput() => outputLayer.GetOutput();

        /// <summary> Gets the index of the <see cref="Neurons.Neuron"/> in the <see cref="OutputLayer"/> with the highest output. </summary>
        /// <returns> The index of the <see cref="Neurons.Neuron"/> in the <see cref="OutputLayer"/> with the highest output. </returns>
        public int GetIndexOfHighestOutput()
        {
            // Find the index of the highest output.
            int currentIndex = -1;
            float currentHighest = -1;
            for (uint n = 0; n < outputLayer.Count; n++)
                if (outputLayer[n].Output > currentHighest)
                {
                    currentHighest = outputLayer[n].Output;
                    currentIndex = (int)n;
                }

            // Return the index.
            return currentIndex;
        }

        /// <summary> Creates an array of indices of <see cref="Neurons.Neuron"/>s in the <see cref="OutputLayer"/> sorted by output. </summary>
        /// <returns> A new array of indices of <see cref="Neurons.Neuron"/>s in the <see cref="OutputLayer"/> sorted by output. </returns>
        public int[] GetSortedIndices()
        {
            // Create a new indices array, sort it, then return it.
            int[] indices = new int[OutputLayer.Count];
            GetSortedIndices(ref indices);
            return indices;
        }

        /// <summary> Fills the given <paramref name="indices"/> with the indices of <see cref="Neurons.Neuron"/>s in the <see cref="OutputLayer"/> sorted by output. </summary>
        /// <param name="indices"> The array to fill with the sorted indices. </param>
        public void GetSortedIndices(ref int[] indices)
        {
            // Get the output.
            float[] output = outputLayer.GetOutput();

            // Sort the arrays.
            GetSortedOutputIndices(ref indices, ref output);
        }

        /// <summary> Fills the given <paramref name="indices"/> and <paramref name="output"/> arrays with the outputs of the <see cref="Neurons.Neuron"/>s in the <see cref="OutputLayer"/> and their associated indices. </summary>
        /// <param name="indices"> The array to fill with the sorted indices. </param>
        /// <param name="output"> The array to fill with the sorted outputs. </param>
        public void GetSortedOutputIndices(ref int[] indices, ref float[] output)
        {
            // Ensure the given arrays are of the correct length.
            if (output is null || indices.Length != outputLayer.Count) throw new ArgumentException($"Given {nameof(indices)} array was not of the correct length or was null. Must be the same length as the output layer.");
            if (output is null || output.Length != outputLayer.Count) throw new ArgumentException($"Given {nameof(output)} array was not of the correct length or was null. Must be the same length as the output layer.");

            // Reset the indices and fill the output array.
            indices.ApplyVectorisedFunction((i) => i);
            outputLayer.GetOutput(ref output);

            // Sort the arrays.
            Array.Sort(output, indices);
            Array.Reverse(output);
            Array.Reverse(indices);
        }

        /// <summary> Creates and returns an array of indices of <see cref="Neurons.Neuron"/>s in the <see cref="OutputLayer"/> sorted by output, whose values will also be sorted and placed into the given <paramref name="output"/>.  </summary>
        /// <param name="output"> The array to fill with the sorted outputs. </param>
        /// <returns> A new array of indices of <see cref="Neurons.Neuron"/>s in the <see cref="OutputLayer"/> sorted by output. </returns>
        public int[] GetSortedOutputIndices(ref float[] output)
        {
            // Create a new indices array, sort it, then return it.
            int[] indices = new int[OutputLayer.Count];
            GetSortedOutputIndices(ref indices, ref output);
            return indices;
        }
        #endregion

        #region Learning Functions
        /// <summary> Using the given <paramref name="trainingSet"/>, trains the neural network with the given <paramref name="batchSize"/> over the given <paramref name="epochs"/>. </summary>
        /// <param name="random"> The random number generator used to shuffle the <paramref name="trainingSet"/>. </param>
        /// <param name="trainingSet"> The <see cref="IDataSet"/> against which to train. </param>
        /// <param name="epochs"> The number of times to go over the <paramref name="trainingSet"/>. </param>
        /// <param name="batchSize"> The number of <see cref="IDataPoint"/>s to process before applying learning. </param>
        /// <param name="logger"> <see cref="INetworkLogger"/> used to log the progress of the training. If this is <c>null</c>, no logging will happen. </param>
        /// <param name="testSet"> Another <see cref="IDataSet"/>, with <see cref="IDataPoint"/>s that are not in the <paramref name="trainingSet"/>. If this is given, the network will be tested against the <paramref name="testSet"/> after every epoch. </param>
        public void LearnOverData(Random random, IDataSet trainingSet, int epochs, int batchSize, INetworkLogger logger = null, IDataSet testSet = null)
        {
            // Create the various required arrays once, then just fill them with data. This cuts down on memory allocations, dramatically speeding up the training.
            float[] input = new float[inputLayer.Count];
            float[] output = new float[outputLayer.Count];
            float[] desiredOutput = new float[outputLayer.Count];
            float[] error = new float[outputLayer.Count];

            // Calculate how many batches are required with the given batch size.
            int numberOfBatches = trainingSet.Count / batchSize;

            // Create the change to be made to the network.
            NetworkChange networkChange = new NetworkChange(this);

            // Do the given amount of epochs.
            for (int currentEpoch = 0; currentEpoch < epochs; currentEpoch++)
            {
                // For each epoch, shuffle the training data randomly.
                trainingSet.Shuffle(random);

                // The percentage of data correctly guessed per batch.
                float lastBatchPercentage = 0.0f;

                // Do the given amount of batches.
                for (int batch = 0; batch < numberOfBatches; batch++)
                {
                    // How many data were correctly guessed in this batch.
                    int batchGuesses = 0;

                    // Do the calculated number of training data in the batch.
                    for (int dataIndex = 0; dataIndex < batchSize; dataIndex++)
                    {
                        // Get the next data and set the input array to its data.
                        IDataPoint currentData = trainingSet[batch * batchSize + dataIndex];
                        currentData.GetFloatData(ref input);

                        // Process the data through the network, saving its output.
                        ProcessInput(input, ref output);

                        // Get the output that the network guessed with the most confidence.
                        int bestGuess = output.GetIndexOfHighestValue();

                        // Create the desired output.
                        desiredOutput.ApplyVectorisedFunction((int g) => g == currentData.Label ? 1 : 0);

                        // If the guess was correct, track it.
                        if (bestGuess == currentData.Label) batchGuesses++;

                        // Calculate the error.
                        CalculateError(desiredOutput, output, ref error);

                        // Learn from the output compared to the desired output, save the results in the previous network change.
                        CalculateChanges(networkChange, error);

                        // If a logger has been given, log the state of the network as of this iteration.
                        logger?.Log(currentEpoch, epochs, batch, numberOfBatches, dataIndex, batchSize, lastBatchPercentage);
                    }

                    // Calculate how many of the data in the batch were correctly guessed.
                    lastBatchPercentage = (float)batchGuesses / batchSize;

                    // Apply the changes.
                    ApplyChanges(networkChange, batchSize);
                    networkChange.Reset();
                }

                // If a logger was given and should log output, log the output of the epoch.
                if (logger != null && (logger.OutputOptions & OutputOptions.WhenFinished) == OutputOptions.WhenFinished)
                {
                    // Log that the epoch finished.
                    logger.Log($"\nEpoch {currentEpoch} finished");

                    // If test data was given, test against it and log the output.
                    if (trainingSet != null) logger.Log($", correctly guessed {PercentageOfCorrectTestData(random, testSet, ref input, ref output):P2} of the test data");

                    // Log the full stop and line end.
                    logger.Log(".\n");
                }
            }

            // If a logger was given, log that the training finished.
            logger?.Log("Training finished.");
        }

        /// <summary> Calculates the changes that must be made to the network based on the given <paramref name="error"/>. Adds the changes to the given <paramref name="networkChange"/>. </summary>
        /// <param name="networkChange"> The changes to be made to the network, any changes calculated here are added to this parameter. </param>
        /// <param name="error"> The error of the network's last output. </param>
        /// <remarks> Note that this uses the state of the <see cref="NeuralNetwork"/>, so this should be called immediately after the input is processed. </remarks>
        public void CalculateChanges(NetworkChange networkChange, IReadOnlyList<float> error)
        {
            // Ensure the given array is of the correct length.
            if (error.Count != outputLayer.Count) throw new ArgumentException("Given error collection does not match the number of output neurons in the network.");

            // Set the current delta to the given error.
            IReadOnlyList<float> currentDelta = error;

            // Go over each non-input layer and calculate the required changes.
            for (uint i = (uint)(neuronLayers.Length - 1); i >= 1; i--)
            {
                // Calculate the changes to this layer, saving the delta.
                neuronLayers[i].CalculateChanges(networkChange.GetNeuronLayerChange(i), networkChange.GetWeightLayerChange(i - 1), currentDelta);

                // Set the current delta to the delta of the current layer that was just calculated.
                if (i < neuronLayers.Length - 1) currentDelta = networkChange.GetNeuronLayerChange(i).Delta;
            }
        }

        /// <summary> Applies the given <paramref name="networkChange"/> to the network, using the given <paramref name="batchSize"/> to scale the <see cref="LearningRate"/>. </summary>
        /// <param name="networkChange"> The change to make. </param>
        /// <param name="batchSize"> The number of batches that were processed before the learning was applied. Simply divides the <see cref="LearningRate"/> by this. </param>
        public void ApplyChanges(IReadOnlyNetworkChange networkChange, int batchSize)
        {
#if DEBUG
            // Ensure this is the intended network.
            if (this != networkChange.NeuralNetwork) throw new Exception("Network mismatch.");
#endif

            // Apply the bias changes to each neuron layer, except the input layer.
            for (uint i = 1; i < neuronLayers.Length; i++) neuronLayers[i].ApplyChanges(networkChange.GetNeuronLayerChange(i), LearningRate / batchSize);

            // Apply the weight changes to each weight layer.
            for (uint i = 0; i < weightLayers.Length; i++) weightLayers[i].ApplyChanges(networkChange.GetWeightLayerChange(i), LearningRate / batchSize);
        }
        #endregion

        #region Test Functions
        /// <summary> Using the supplied <paramref name="testSet"/>, tests this <see cref="NeuralNetwork"/> on each data point and returns the percentage of correct guesses. </summary>
        /// <param name="random"> The random number generator used to shuffle the <paramref name="testSet"/>. </param>
        /// <param name="testSet"> The <see cref="IDataSet"/> on which to test this <see cref="NeuralNetwork"/>. </param>
        /// <returns> A number between <c>0</c> and <c>1</c> representing a percentage of correct guesses. </returns>
        public float PercentageOfCorrectTestData(Random random, IDataSet testSet)
        {
            // Create the arrays.
            float[] input = new float[inputLayer.Count];
            float[] output = new float[outputLayer.Count];

            // Test the data using the given arrays.
            return PercentageOfCorrectTestData(random, testSet, ref input, ref output);
        }

        /// <summary> Using the supplied <paramref name="testSet"/>, tests this <see cref="NeuralNetwork"/> on each data point and returns the percentage of correct guesses. </summary>
        /// <param name="random"> The random number generator used to shuffle the <paramref name="testSet"/>. </param>
        /// <param name="testSet"> The <see cref="IDataSet"/> on which to test this <see cref="NeuralNetwork"/>. </param>
        /// <param name="input"> The inputs. Must be equal in length to the amount of input neurons in the <see cref="InputLayer"/>. </param>
        /// <param name="output"> The outputs. Must be equal in length to the amount of output neurons in the <see cref="OutputLayer"/>. </param>
        /// <returns> A number between <c>0</c> and <c>1</c> representing a percentage of correct guesses. </returns>
        public float PercentageOfCorrectTestData(Random random, IDataSet testSet, ref float[] input, ref float[] output)
        {
            // Shuffle the test data.
            testSet.Shuffle(random);

            // Track how many were correctly guessed.
            int correctGuesses = 0;

            // Go over each data in the test set.
            for (int i = 0; i < testSet.Count; i++)
            {
                // Get the input.
                IDataPoint currentData = testSet[i];
                currentData.GetFloatData(ref input);

                // Run the input through the network, and save the output.
                ProcessInput(input, ref output);

                // Get the digit that the network guessed with the most confidence.
                int bestGuess = output.GetIndexOfHighestValue();

                // If the guess was correct, track it.
                if (bestGuess == currentData.Label) correctGuesses++;
            }

            // Calculate and return the percentage of correctly guessed data.
            return correctGuesses / (float)testSet.Count;
        }
        #endregion

        #region IO Functions
        /// <summary> Saves the network using the given <paramref name="networkSaver"/>. </summary>
        /// <param name="networkSaver"> The <see cref="INetworkSaver"/> used to save the network. </param>
        public void Save(INetworkSaver networkSaver)
        {
            // Save the learning rate.
            networkSaver.SaveLearningRate(LearningRate);

            // Save each neuron layer.
            for (int i = 0; i < neuronLayers.Length; i++)
                networkSaver.SaveNeuronLayer(neuronLayers[i]);

            // Save each weight layer.
            for (uint i = 0; i < weightLayers.Length; i++)
            {
                // Create the weight layer entry.
                networkSaver.CreateWeightLayer(i);

                // Get the previous neuron layer.
                IReadOnlyNeuronLayer previousLayer = weightLayers[i].PreviousNeuronLayer;

                // Go over each neuron in the weight layer's previous neuron layer.
                for (uint previousNeuronID = 0; previousNeuronID < previousLayer.Count; previousNeuronID++)
                    foreach (uint nextNeuronID in weightLayers[i].GetNextLayerConnections(previousLayer[previousNeuronID]))
                        networkSaver.SaveWeight(i, previousNeuronID, nextNeuronID, weightLayers[i].GetWeightBetweenNeurons(previousLayer[previousNeuronID], weightLayers[i].NextNeuronLayer[nextNeuronID]));
            }

            // Save the network to file.
            networkSaver.Save();
        }

        /// <summary> Creates, loads, and returns a <see cref="NeuralNetwork"/> using the given <paramref name="networkLoader"/>. </summary>
        /// <param name="networkLoader"> The <see cref="INetworkLoader"/> used to load the network. </param>
        /// <returns> The loaded <see cref="NeuralNetwork"/>. </returns>
        public static NeuralNetwork Load(INetworkLoader networkLoader)
        {
            // Get the number of neuron layers in the file.
            int neuronLayerCount = networkLoader.GetNetworkNeuronLayerCount();

            // Create an empty neural network.
            NeuralNetwork neuralNetwork = new NeuralNetwork(networkLoader.GetNetworkLearningRate(), neuronLayerCount);

            // Create the neuron layers for the neural network.
            for (uint i = 0; i < neuronLayerCount; i++)
                neuralNetwork.neuronLayers[i] = NeuronLayer.Load(neuralNetwork, networkLoader, i);

            // Create the weight layers for the neural network.
            for (uint i = 0; i < neuronLayerCount - 1; i++)
                neuralNetwork.weightLayers[i] = WeightLayer.Load(neuralNetwork, networkLoader, i, neuralNetwork.neuronLayers[i], neuralNetwork.neuronLayers[i + 1]);

            // Set the highest neuron count.
            neuralNetwork.HighestNeuronCount = calculateHighestNeuronCount(neuralNetwork);

            // Return the neural network.
            return neuralNetwork;
        }
        #endregion
    }
}