using System.Drawing;

namespace API.Services.Interfaces
{
    public interface IImageProcessor
    {
        IEnumerable<IEnumerable<Bitmap?>> GetNumberCards(Bitmap image, int resizedSize);
    }
}
