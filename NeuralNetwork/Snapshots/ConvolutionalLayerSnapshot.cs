namespace NeuralNetwork.Snapshots
{
    public class ConvolutionalLayerSnapshot
    {
        public ConvolutionalLayerSnapshot(int inputSize, double[][,,] filters, double[] biases)
        {
            InputSize = inputSize;
            Filters = filters;
            Biases = biases;
        }

        public int InputSize { get; set; }
        public double[][,,] Filters { get; set; }
        public double[] Biases { get; set; }
    }
}
