namespace NeuralNetwork.Snapshots
{
    public class NetworkSnapshot
    {
        public ConvolutionalPoolingLayerSnapshot[] CPLayers { get; set; }
        public DenseLayerSnapshot DenseLayer { get; set; }
    }
}
