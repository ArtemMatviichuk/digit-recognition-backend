using NeuralNetwork.Snapshots;

namespace NeuralNetwork.Infrastructure
{
    public class ConvolutionalLayer
    {
        private readonly int _inputSize;
        private readonly int _filtersCount;
        private readonly (int xy, int z) _filtersShape;
        private readonly int _outputSize;

        private readonly double[][,,] _filters;
        private readonly double[] _biases;

        private double[][,] _input;

        public ConvolutionalLayer(int inputSize, int filtersCount, (int xy, int z) filtersShape)
        {
            _inputSize = inputSize;
            _filtersCount = filtersCount;
            _filtersShape = filtersShape;

            _outputSize = inputSize - _filtersShape.xy + 1;

            _biases = new double[_filtersCount];

            _filters = new double[_filtersCount][,,];

            Random rand = new Random();
            for (int i = 0; i < _filtersCount; i++)
            {
                _filters[i] = new double[_filtersShape.xy, _filtersShape.xy, _filtersShape.z];

                for (int x = 0; x < _filtersShape.xy; x++)
                {
                    for (int y = 0; y < _filtersShape.xy; y++)
                    {
                        for (int z = 0; z < _filtersShape.z; z++)
                        {
                            _filters[i][x, y, z] = rand.NextDouble() - 0.5d;
                        }
                    }
                }

                _biases[i] = rand.NextDouble() - 0.5d;
            }
        }

        public ConvolutionalLayer(ConvolutionalLayerSnapshot snapshot)
        {
            _inputSize = snapshot.InputSize;
            _filtersCount = snapshot.Filters.Length;
            _filtersShape = (snapshot.Filters[0].GetLength(0), snapshot.Filters[0].GetLength(2));
            _filters = snapshot.Filters;
            _biases = snapshot.Biases;
            _outputSize = _inputSize - _filtersShape.xy + 1;
        }

        public ConvolutionalLayerSnapshot GetSnapshot()
        {
            return new ConvolutionalLayerSnapshot(_inputSize, _filters, _biases);
        }

        public double[][,] ForwardPropagation(double[][,] input)
        {
            _input = input;

            var output = new double[_filtersCount][,];

            for (int f = 0; f < _filtersCount; f++)
            {
                output[f] = new double[_outputSize, _outputSize];

                for (int i = 0; i < _outputSize; i++)
                {
                    for (int j = 0; j < _outputSize; j++)
                    {
                        output[f][i, j] = CalculateOutput(f, i, j, _input);
                    }
                }
            }

            return NormalizeOutput(output);
        }

        public double[][,] Backpropagation(double[][,] delta, double learningRate)
        {
            var filterGradients = new double[_filtersCount][,,];
            var biasGradients = new double[_filtersCount];

            for (int f = 0; f < _filtersCount; f++)
            {
                filterGradients[f] = new double[_filtersShape.xy, _filtersShape.xy, _filtersShape.z];

                for (int i = 0; i < _outputSize; i++)
                {
                    for (int j = 0; j < _outputSize; j++)
                    {
                        biasGradients[f] += delta[f][i, j];
                        UpdateGradients(filterGradients[f], i, j, delta[f][i, j]);
                    }
                }
            }

            var inputGradients = new double[_filtersCount][,];
            for (int f = 0; f < _filtersCount; f++)
            {
                inputGradients[f] = new double[_inputSize, _inputSize];

                for (int i = 0; i < _outputSize; i++)
                {
                    for (int j = 0; j < _outputSize; j++)
                    {
                        for (int x = 0; x < _filtersShape.xy; x++)
                        {
                            for (int y = 0; y < _filtersShape.xy; y++)
                            {
                                for (int z = 0; z < _filtersShape.z; z++)
                                {
                                    inputGradients[f][x + i, y + j] += delta[f][i, j] * _filters[f][x, y, z];
                                }
                            }
                        }
                    }
                }
            }

            UpdateWeights(filterGradients, biasGradients, learningRate);

            return inputGradients;
        }

        private double CalculateOutput(int filter, int xOffset, int yOffset, double[][,] input)
        {
            double sum = 0;
            for (int i = 0; i < input.Length; i++)
            {
                for (int x = 0; x < _filtersShape.xy; x++)
                {
                    for (int y = 0; y < _filtersShape.xy; y++)
                    {
                        for (int z = 0; z < _filtersShape.z; z++)
                        {
                            sum += _filters[filter][x, y, z] * input[i][x + xOffset, y + yOffset];
                        }
                    }
                }
            }

            return Math.Max(0, sum + _biases[filter]);
        }

        private void UpdateGradients(double[,,] filterGradient, int xOffset, int yOffset, double delta)
        {
            for (int i = 0; i < _input.Length; i++)
            {
                for (int x = 0; x < _filtersShape.xy; x++)
                {
                    for (int y = 0; y < _filtersShape.xy; y++)
                    {
                        for (int z = 0; z < _filtersShape.z; z++)
                        {
                            filterGradient[x, y, z] += delta * _input[i][x + xOffset, y + yOffset];
                        }
                    }
                }
            }
        }

        private void UpdateWeights(double[][,,] filterGradients, double[] biasGradients, double learningRate)
        {
            for (int f = 0; f < _filtersCount; f++)
            {
                for (int x = 0; x < _filtersShape.xy; x++)
                {
                    for (int y = 0; y < _filtersShape.xy; y++)
                    {
                        for (int z = 0; z < _filtersShape.z; z++)
                        {
                            _filters[f][x, y, z] -= learningRate * filterGradients[f][x, y, z];
                        }
                    }

                    _biases[f] -= learningRate * biasGradients[f];
                }
            }
        }

        private static double[][,] NormalizeOutput(double[][,] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                double max = double.MinValue;

                for (int j = 0; j < output[0].GetLength(0); j++)
                {
                    for (int k = 0; k < output[0].GetLength(1); k++)
                    {
                        max = Math.Max(max, output[i][j, k]);
                    }
                }

                if (max != 0)
                {
                    for (int j = 0; j < output[0].GetLength(0); j++)
                    {
                        for (int k = 0; k < output[0].GetLength(1); k++)
                        {
                            output[i][j, k] /= max;
                        }
                    }
                }
            }

            return output;
        }
    }
}
