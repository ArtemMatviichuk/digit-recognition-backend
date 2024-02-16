using NeuralNetwork.Snapshots;

namespace NeuralNetwork.Infrastructure
{
    public class DenseLayer
    {
        private readonly int _inputSize;
        private readonly int _outputSize;

        private double[,] _weights;
        private double[] _biases;

        private double[] _inputs;

        public DenseLayer(int inputSize, int outputSize)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;

            _weights = new double[outputSize, inputSize];
            _biases = new double[outputSize];

            Random random = new Random();
            for (int i = 0; i < _weights.GetLength(0); i++)
            {
                for (int j = 0; j < _weights.GetLength(1); j++)
                {
                    _weights[i, j] = random.NextDouble() - 0.5;
                }
            }

            for (int i = 0; i < _biases.Length; i++)
            {
                _biases[i] = random.NextDouble() - 0.5;
            }
        }

        public DenseLayer(DenseLayerSnapshot snapshot)
        {
            _inputSize = snapshot.InputSize;
            _outputSize = snapshot.OutputSize;
            _weights = snapshot.Weights;
            _biases = snapshot.Biases;
        }

        public DenseLayerSnapshot GetSnapshot()
        {
            return new DenseLayerSnapshot(_inputSize, _outputSize, _weights, _biases);
        }

        public double[] ForwardPropagation(double[] input)
        {
            _inputs = input;
            var outputs = new double[_outputSize];

            for (int i = 0; i < _outputSize; i++)
            {
                outputs[i] = _biases[i];
                for (int j = 0; j < _inputSize; j++)
                {
                    outputs[i] += _weights[i, j] * _inputs[j];
                }
            }

            return Activate(outputs);
        }

        public double[] Backpropagation(double[] outputGradients, double learningRate)
        {
            double[] inputGradients = new double[_inputSize];

            for (int i = 0; i < _outputSize; i++)
            {
                for (int j = 0; j < _inputSize; j++)
                {
                    inputGradients[j] += outputGradients[i] * _weights[i, j];
                    _weights[i, j] -= learningRate * outputGradients[i] * _inputs[j];
                }

                _biases[i] -= learningRate * outputGradients[i];
            }

            return inputGradients;
        }

        private static double[] Activate(double[] outputs)
        {
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i] = Math.Exp(outputs[i]);
            }

            double sum = outputs.Sum();
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i] = outputs[i] / sum;
            }

            return outputs;
        }
    }
}
