using API.Common;
using API.Services.Interfaces;
using NeuralNetwork.Infrastructure;
using NeuralNetwork.Snapshots;
using Newtonsoft.Json;
using System.Drawing;

namespace API.Services.Implementations
{
    public class NeuralNetworkHolder : INeuralNetworkHolder
    {
        private readonly ConvolutionalNeuralNetwork _network;

        public NeuralNetworkHolder()
        {
            string json = string.Empty;
            using (var reader = new StreamReader(@"C:\Users\kvaza\OneDrive\Documents\Projects\DigitRecognition\DigitRecognitionBackend\NetworkTraining\network-state.json"))
            {
                json = reader.ReadToEnd();
            }

            var snapshot = JsonConvert.DeserializeObject<NetworkSnapshot>(json)!;
            _network = new ConvolutionalNeuralNetwork(snapshot);
        }

        public int AnalizeImage(Bitmap bitmap)
        {
            var output = _network.FeedForward(BitmapToFloatArray(bitmap));
            return Array.IndexOf(output, output.Max());
        }

        private static double[][,] BitmapToFloatArray(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            double[][,] data = new double[1][,];
            data[0] = new double[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    data[0][y, x] = 1 - pixelColor.GetBrightness();
                }
            }

            return data;
        }
    }
}
