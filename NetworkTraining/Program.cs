using NetworkTraining;
using NeuralNetwork.Helpers;
using NeuralNetwork.Infrastructure;
using NeuralNetwork.Snapshots;
using Newtonsoft.Json;
using System.Drawing;

public static class ExtensinMethods
{
    private static Random rng = new Random();
    public static void Shuffle<T>(this T[] list)
    {
        int n = list.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public class Program
{
    private const string SnapshotPath = @"C:\Users\kvaza\OneDrive\Documents\Projects\DigitRecognition\DigitRecognitionBackend\NetworkTraining\network-state.json";
    private const string ImagesPath = @"C:\Users\kvaza\OneDrive\Documents\Projects\DigitRecognition\DigitRecognitionBackend\NetworkTraining\dataset\test";

    private static void Main(string[] args)
    {
        int[] numbers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        DatasetItem[] data = numbers.SelectMany(n =>
        {
            double[] expected = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            expected[n] = 1;

            var imageFiles = Directory.GetFiles($@"{ImagesPath}/{n}");

            return imageFiles.Select(e => new DatasetItem(e, BitmapToFloatArray(new Bitmap(e)), expected));
        }).ToArray();

        data.Shuffle();

        Console.WriteLine("Створити нову нейронну мережу (1) чи використовувати наявну версiю (2)?");
        bool done = false;
        int res = 0;
        do
        {
            done = int.TryParse(Console.ReadLine(), out res);
            if (!done || (res != 1 && res != 2))
            {
                Console.WriteLine("Введiть цифру 1 або 2");
            }
        } while (!done || (res != 1 && res != 2));

        var neuralNetwork = res == 1 ? CreateNewNetwork() : LoadExistingNetwork();
        if (neuralNetwork == null)
        {
            Console.WriteLine("Не вдалося створити нейронну мережу");
            return;
        }

        var epo = 1; // 20*0.01 50*0.002 50*0.0005
        var learningRate = 0.002d;
        while (false)
        {
            var count = 0;

            for (int i = 0; i < data.Length; i++)
            {
                var result = neuralNetwork.Backpropagation(data[i].ImageData, data[i].Expected, learningRate);
                if (!result)
                {
                    count++;
                }
            }

            Console.WriteLine($"Епоха - {epo++}; кiлькiсть помилок - {count}");

            if (count == 0)
            {
                Console.WriteLine("Помилки вiдсутнi");
                break;
            }

            if (epo == 10)
            {
                break;
            }

            if (epo == 31)
            {
                learningRate = 0.002d;
            } else if (epo == 71)
            {
                learningRate = 0.0005d;
            }
            else if (epo == 121)
            {
                Console.WriteLine("Всi епохи пройденi");
                break;
            }
        }

        //SaveNetwork(neuralNetwork);

        var errorsCount = 0;
        for (int i = 0; i < data.Length; i++)
        {
            var networkOutput = neuralNetwork.FeedForward(data[i].ImageData);

            int target = Array.IndexOf(data[i].Expected, data[i].Expected.Max());
            int recognizedDigit = Array.IndexOf(networkOutput, networkOutput.Max());

            if (target != recognizedDigit)
            {
                errorsCount++;
            }
        }

        Console.WriteLine($"Кiлькiсть правильних вiдповiдей: 195/{data.Length}");
    }

    private static ConvolutionalNeuralNetwork CreateNewNetwork()
    {
        return new ConvolutionalNeuralNetwork(64, [new(3, (3, 1), 2, 2), new(5, (3, 2), 2, 2)], 10);
    }

    private static ConvolutionalNeuralNetwork? LoadExistingNetwork()
    {
        string json = string.Empty;
        using (var reader = new StreamReader(SnapshotPath))
        {
            json = reader.ReadToEnd();
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var snapshot = JsonConvert.DeserializeObject<NetworkSnapshot>(json);
        return new ConvolutionalNeuralNetwork(snapshot!);
    }

    private static void SaveNetwork(ConvolutionalNeuralNetwork neuralNetwork)
    {
        var serialized = JsonConvert.SerializeObject(neuralNetwork.GetNetworkSnapShot(), Formatting.Indented);
        File.WriteAllText(SnapshotPath, serialized);
    }

    private static double[][,] BitmapToFloatArray(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        double[][,] data = [new double[width, height]];
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