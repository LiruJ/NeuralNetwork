namespace LiruAI.Learning
{
    /// <summary> Represents a change to be made to an entire neural network. </summary>
    public class NetworkChange : IReadOnlyNetworkChange
    {
        #region Fields
        /// <summary> The changes to neuron layers. </summary>
        private readonly NeuronLayerChange[] neuronLayerChanges;

        /// <summary> The changes to weight layers. </summary>
        private readonly WeightLayerChange[] weightLayerChanges;
        #endregion

        #region Properties
        /// <summary> The <see cref="IReadOnlyNeuralNetwork"/> whose changes are being stored. </summary>
        public IReadOnlyNeuralNetwork NeuralNetwork { get; }
        #endregion

        #region Constructors
        /// <summary> Creates a new <see cref="NetworkChange"/> for the given <paramref name="neuralNetwork"/>. </summary>
        /// <param name="neuralNetwork"> The <see cref="IReadOnlyNeuralNetwork"/> whose changes are being stored. </param>
        public NetworkChange(IReadOnlyNeuralNetwork neuralNetwork)
        {
            // Set the neural network.
            NeuralNetwork = neuralNetwork;

            // Initialise the neuron and weight changes.
            neuronLayerChanges = new NeuronLayerChange[neuralNetwork.NeuronLayerCount - 1];
            weightLayerChanges = new WeightLayerChange[neuralNetwork.WeightLayerCount];
            for (int i = 0; i < neuronLayerChanges.Length; i++) neuronLayerChanges[i] = new NeuronLayerChange(neuralNetwork.GetNeuronLayer(i + 1));
            for (int i = 0; i < weightLayerChanges.Length; i++) weightLayerChanges[i] = new WeightLayerChange(neuralNetwork.GetWeightLayer(i));
        }
        #endregion

        #region Get Functions
        /// <summary> The the <see cref="NeuronLayerChange"/> for the given <paramref name="index"/>. </summary>
        /// <param name="index"> The index of the <see cref="NeuronLayerChange"/> to get. </param>
        /// <returns> The <see cref="NeuronLayerChange"/> at the given <paramref name="index"/>. </returns>
        public NeuronLayerChange GetNeuronLayerChange(uint index) => neuronLayerChanges[index - 1];

        /// <summary> The the <see cref="IReadOnlyNeuronLayerChange"/> for the given <paramref name="index"/>. </summary>
        /// <param name="index"> The index of the <see cref="IReadOnlyNeuronLayerChange"/> to get. </param>
        /// <returns> The <see cref="IReadOnlyNeuronLayerChange"/> at the given <paramref name="index"/>. </returns>
        IReadOnlyNeuronLayerChange IReadOnlyNetworkChange.GetNeuronLayerChange(uint index) => GetNeuronLayerChange(index);

        /// <summary> The the <see cref="WeightLayerChange"/> for the given <paramref name="index"/>. </summary>
        /// <param name="index"> The index of the <see cref="WeightLayerChange"/> to get. </param>
        /// <returns> The <see cref="WeightLayerChange"/> at the given <paramref name="index"/>. </returns>
        public WeightLayerChange GetWeightLayerChange(uint index) => weightLayerChanges[index];

        /// <summary> The the <see cref="IReadOnlyWeightLayerChange"/> for the given <paramref name="index"/>. </summary>
        /// <param name="index"> The index of the <see cref="IReadOnlyWeightLayerChange"/> to get. </param>
        /// <returns> The <see cref="IReadOnlyWeightLayerChange"/> at the given <paramref name="index"/>. </returns>
        IReadOnlyWeightLayerChange IReadOnlyNetworkChange.GetWeightLayerChange(uint index) => GetWeightLayerChange(index);
        #endregion

        #region Reset Functions
        /// <summary> Resets all weight and neuron changes to 0. </summary>
        public void Reset()
        {
            for (int i = 0; i < neuronLayerChanges.Length; i++) neuronLayerChanges[i].Reset();
            for (int i = 0; i < weightLayerChanges.Length; i++) weightLayerChanges[i].Reset();
        }
        #endregion
    }
}