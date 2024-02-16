namespace NeuralNetwork.Helpers
{
    public class CPLayerConfiguration(int filtersCount, (int xy, int z) filtersShape, int poolSize, int stride)
    {
        public int FiltersCount { get; set; } = filtersCount;
        public (int xy, int z) FiltersShape { get; set; } = filtersShape;
        public int PoolSize { get; set; } = poolSize;
        public int Stride { get; set; } = stride;
    }
}
