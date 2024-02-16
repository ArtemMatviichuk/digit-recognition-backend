namespace NeuralNetwork.Snapshots
{
    public class PoolingLayerSnapshot
    {
        public int PoolSize { get; set; }
        public int Stride { get; set; }

        public PoolingLayerSnapshot(int poolSize, int stride)
        {
            PoolSize = poolSize;
            Stride = stride;
        }
    }
}
