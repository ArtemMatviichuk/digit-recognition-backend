using NeuralNetwork.Snapshots;

namespace NeuralNetwork.Infrastructure
{
    public class PoolingLayer
    {
        private readonly int _poolSize;
        private readonly int _stride;

        private double[][,] _input;

        public PoolingLayer(int poolSize, int stride)
        {
            _poolSize = poolSize;
            _stride = stride;
        }

        public PoolingLayer(PoolingLayerSnapshot snapshot)
        {
            _poolSize = snapshot.PoolSize;
            _stride = snapshot.Stride;
        }

        public PoolingLayerSnapshot GetSnapshot()
        {
            return new PoolingLayerSnapshot(_poolSize, _stride);
        }

        public double[][,] ForwardPropagation(double[][,] input)
        {
            _input = input;

            int inputHeight = input[0].GetLength(0);
            int inputWidth = input[0].GetLength(1);
            int outputHeight = (inputHeight - _poolSize) / _stride + 1;
            int outputWidth = (inputWidth - _poolSize) / _stride + 1;

            double[][,] output = new double[input.Length][,];

            for (int f = 0; f < input.Length; f++)
            {
                output[f] = new double[outputHeight, outputWidth];

                for (int i = 0; i < outputHeight; i++)
                {
                    for (int j = 0; j < outputWidth; j++)
                    {
                        output[f][i, j] = MaxPooling(input[f], i * _stride, j * _stride, _poolSize);
                    }
                }
            }

            return output;
        }

        public double[][,] Backpropagation(double[][,] expectedOutputs)
        {
            int inputHeight = _input[0].GetLength(0);
            int inputWidth = _input[0].GetLength(1);
            int outputHeight = (inputHeight - _poolSize) / _stride + 1;
            int outputWidth = (inputWidth - _poolSize) / _stride + 1;

            double[][,] inputGradient = new double[_input.Length][,];

            for (int f = 0; f < _input.Length; f++)
            {
                inputGradient[f] = new double[inputHeight, inputWidth];

                for (int i = 0; i < outputHeight; i++)
                {
                    for (int j = 0; j < outputWidth; j++)
                    {
                        int[] maxIndices = GetMaxIndices(_input[f], i * _stride, j * _stride, _poolSize);

                        inputGradient[f][maxIndices[0], maxIndices[1]] = expectedOutputs[f][i, j];
                    }
                }
            }

            return inputGradient;
        }

        private double MaxPooling(double[,] input, int startRow, int startCol, int poolSize)
        {
            double max = double.MinValue;

            for (int i = 0; i < poolSize; i++)
            {
                for (int j = 0; j < poolSize; j++)
                {
                    max = Math.Max(max, input[startRow + i, startCol + j]);
                }
            }

            return max;
        }

        private int[] GetMaxIndices(double[,] input, int startRow, int startCol, int poolSize)
        {
            int[] maxIndices = { startRow, startCol };
            double maxValue = input[startRow, startCol];

            for (int i = 0; i < poolSize; i++)
            {
                for (int j = 0; j < poolSize; j++)
                {
                    if (input[startRow + i, startCol + j] > maxValue)
                    {
                        maxIndices[0] = startRow + i;
                        maxIndices[1] = startCol + j;
                        maxValue = input[startRow + i, startCol + j];
                    }
                }
            }

            return maxIndices;
        }
    }
}
