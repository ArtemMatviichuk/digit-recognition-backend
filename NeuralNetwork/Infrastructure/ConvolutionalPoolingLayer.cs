using NeuralNetwork.Helpers;
using NeuralNetwork.Snapshots;

namespace NeuralNetwork.Infrastructure
{
    public class ConvolutionalPoolingLayer
    {
        public int Side { get; private set; }
        public int FiltersCount { get; private set; }

        public ConvolutionalLayer Convolutional { get; set; }
        public PoolingLayer Pooling { get; set; }

        public ConvolutionalPoolingLayer(int shape, CPLayerConfiguration conf)
        {
            Convolutional = new ConvolutionalLayer(shape, conf.FiltersCount, conf.FiltersShape);
            Pooling = new PoolingLayer(conf.PoolSize, conf.Stride);

            FiltersCount = conf.FiltersCount;
            Side = (shape - conf.FiltersShape.xy + 1 - conf.PoolSize) / conf.Stride + 1;
        }

        public ConvolutionalPoolingLayer(ConvolutionalPoolingLayerSnapshot snapshot)
        {
            Convolutional = new ConvolutionalLayer(snapshot.Convolutional);
            Pooling = new PoolingLayer(snapshot.Pooling);

            FiltersCount = snapshot.Convolutional.Filters.Length;
            Side = (snapshot.Convolutional.InputSize - snapshot.Convolutional.Filters[0].GetLength(0) + 1 - snapshot.Pooling.PoolSize) / snapshot.Pooling.Stride + 1;
        }

        public ConvolutionalPoolingLayerSnapshot GetSnapshot()
        {
            return new ConvolutionalPoolingLayerSnapshot()
            {
                Convolutional = Convolutional.GetSnapshot(),
                Pooling = Pooling.GetSnapshot(),
            };
        }

        public double[][,] ForwardPropagation(double[][,] input)
        {
            var conv = Convolutional.ForwardPropagation(input);
            return Pooling.ForwardPropagation(conv);
        }

        public double[][,] Backpropagation(double[][,] input, double learningRate)
        {
            var pool = Pooling.Backpropagation(input);
            return Convolutional.Backpropagation(pool, learningRate);
        }
    }
}
