namespace NetworkTraining
{
    public class DatasetItem(string fileName, double[][,] imageData, double[] output)
    {
        public string FileName { get; set; } = fileName;
        public double[][,] ImageData { get; set; } = imageData;
        public double[] Expected { get; set; } = output;
    }
}
