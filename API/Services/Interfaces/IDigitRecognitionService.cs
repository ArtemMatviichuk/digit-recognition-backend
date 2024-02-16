using API.Common.DTO;

namespace API.Services.Interfaces
{
    public interface IDigitRecognitionService
    {
        AnalizeResponseDto AnalizeImage(IFormFile value);
    }
}
