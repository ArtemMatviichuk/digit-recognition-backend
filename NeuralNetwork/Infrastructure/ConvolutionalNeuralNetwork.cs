using NeuralNetwork.Helpers;
using NeuralNetwork.Snapshots;
using System.ComponentModel.DataAnnotations;

namespace NeuralNetwork.Infrastructure
{
    public class ConvolutionalNeuralNetwork
    {
        public ConvolutionalPoolingLayer[] CPLayers { get; set; }
        public DenseLayer DenseLayer { get; set; }

        public ConvolutionalNeuralNetwork(
            int inputSide,
            [Required, MinLength(1)] CPLayerConfiguration[] cpLayersConfigurations,
            int outputSize)
        {
            CPLayers = new ConvolutionalPoolingLayer[cpLayersConfigurations.Length];
            
            CPLayers[0] = new ConvolutionalPoolingLayer(inputSide, cpLayersConfigurations[0]);
            for (int i = 1; i < cpLayersConfigurations.Length; i++)
            {
                CPLayers[i] = new ConvolutionalPoolingLayer(CPLayers[i - 1].Side, cpLayersConfigurations[i]);
            }

            int lastLayersFiltersCount = CPLayers[^1].FiltersCount;
            int lastLayersSideSquare = (int)Math.Pow(CPLayers[^1].Side, 2);
            DenseLayer = new DenseLayer(lastLayersFiltersCount * lastLayersSideSquare, outputSize);
        }

        public ConvolutionalNeuralNetwork(NetworkSnapshot snapshot)
        {
            CPLayers = snapshot.CPLayers.Select(e => new ConvolutionalPoolingLayer(e)).ToArray();
            DenseLayer = new DenseLayer(snapshot.DenseLayer);
        }

        public NetworkSnapshot GetNetworkSnapShot()
        {
            return new NetworkSnapshot()
            {
                CPLayers = CPLayers.Select(e => e.GetSnapshot()).ToArray(),
                DenseLayer = DenseLayer.GetSnapshot(),
            };
        }

        public double[] FeedForward(double[][,] input)
        {
            var cpLayersOutput = CPLayers[0].ForwardPropagation(input);
            for (int i = 1; i < CPLayers.Length; i++)
            {
                cpLayersOutput = CPLayers[i].ForwardPropagation(cpLayersOutput);
            }

            var denseOut = DenseLayer.ForwardPropagation(FlattenArray(cpLayersOutput));

            return denseOut;
        }

        public bool Backpropagation(double[][,] input, double[] expected, double learningRate)
        {
            var networkOutput = FeedForward(input);

            var outputGradient = CalculateOutputGradient(expected, networkOutput);
            var denseBack = DenseLayer.Backpropagation(outputGradient, learningRate);

            var cpLayersBack = CPLayers[^1]
                .Backpropagation(
                    UnflattenArray(denseBack, CPLayers[^1].FiltersCount),
                    learningRate);

            for (int i = CPLayers.Length - 2; i >= 0; i--)
            {
                cpLayersBack = CPLayers[i].Backpropagation(cpLayersBack, learningRate);
            }

            int target = Array.IndexOf(expected, expected.Max());
            int recognizedDigit = Array.IndexOf(networkOutput, networkOutput.Max());

            return target == recognizedDigit;
        }

        private static double[] FlattenArray(double[][,] input)
        {
            var output = new double[input.Length * input[0].GetLength(0) * input[0].GetLength(1)];

            for (int i = 0; i < input[0].GetLength(0); i++)
            {
                for (int j = 0; j < input[0].GetLength(1); j++)
                {
                    for (int f = 0; f < input.Length; f++)
                    {
                        output[f + (i * input[0].GetLength(1) + j) * input.Length] = input[f][i, j];
                    }
                }
            }

            return output;
        }

        private static double[][,] UnflattenArray(double[] input, int count)
        {
            int size = (int)Math.Sqrt(input.Length / count);
            var output = new double[count][,];
            for (int i = 0; i < count; i++)
            {
                output[i] = new double[size, size];
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int f = 0; f < count; f++)
                    {
                        output[f][i, j] = input[f + (i * size + j) * count];
                    }
                }
            }

            return output;
        }

        private static double[] CalculateOutputGradient(double[] expectedOutput, double[] actualOutput)
        {
            double[] gradient = new double[expectedOutput.Length];
            for (int i = 0; i < expectedOutput.Length; i++)
            {
                gradient[i] = 2 * (actualOutput[i] - expectedOutput[i]);
            }

            return gradient;
        }
    }
}
