namespace NeuralNetwork.Snapshots
{
    public class DenseLayerSnapshot
    {
        public int InputSize { get; set; }
        public int OutputSize { get; set; }
        public double[,] Weights { get; set; }
        public double[] Biases { get; set; }

        public DenseLayerSnapshot(int inputSize, int outputSize, double[,] weights, double[] biases)
        {
            InputSize = inputSize;
            OutputSize = outputSize;
            Weights = weights;
            Biases = biases;
        }
    }
}
