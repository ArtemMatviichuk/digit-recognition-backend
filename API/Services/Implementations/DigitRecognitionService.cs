using API.Common.DTO;
using API.Common.Exceptions;
using API.Services.Interfaces;
using System.Drawing;

namespace API.Services.Implementations
{
    public class DigitRecognitionService : IDigitRecognitionService
    {
        private readonly IImageProcessor _imageProcessor;
        private readonly INeuralNetworkHolder _neuralNetwork;

        public DigitRecognitionService(IImageProcessor imageProcessor, INeuralNetworkHolder neuralNetwork)
        {
            _imageProcessor = imageProcessor;
            _neuralNetwork = neuralNetwork;
        }

        public AnalizeResponseDto AnalizeImage(IFormFile image)
        {
            if (image is null || image.Length == 0f)
            {
                throw new ValidationException("File is null");
            }

            var acceptableExtensions = new string[] { ".PNG", ".JPG", ".JPEG" };
            var extension = Path.GetExtension(image.FileName);
            if (!acceptableExtensions.Contains(extension.ToUpper()))
            {
                throw new ValidationException("Unacceptable extension");
            }

            using var stream = image.OpenReadStream();
            using Bitmap bitmap = new Bitmap(stream);

            var cards = _imageProcessor.GetNumberCards(bitmap, 64);
            var result = string.Join("\n", cards.Select(e =>
                string.Join("", e.Select(c => c == null
                    ? " "
                    : _neuralNetwork.AnalizeImage(c).ToString()))));

            return new AnalizeResponseDto()
            {
                Value = result,
                Image = ImageToByte(bitmap),
            };
        }

        private static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
