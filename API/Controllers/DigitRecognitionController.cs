using API.Common.DTO;
using API.Common.Exceptions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DigitRecognitionController : ControllerBase
    {
        private readonly IDigitRecognitionService _digitRecognitionService;

        public DigitRecognitionController(IDigitRecognitionService digitRecognitionService)
        {
            _digitRecognitionService = digitRecognitionService;
        }

        [HttpPost("Analize")]
        public IActionResult AnalizeImage([FromForm] ValueDto<IFormFile> dto)
        {
            var response = InterceptErrors(() => _digitRecognitionService.AnalizeImage(dto.Value));

            return ProcessResponse(response);
        }

        private ServiceResponse<T> InterceptErrors<T>(Func<T> method)
        {
            var result = new ServiceResponse<T>();

            try
            {
                result.Data = method.Invoke();
            }
            catch (CustomException ex)
            {
                result.Success = false;
                result.StatusCode = ex.ErrorCode;
                result.ErrorMessage = GetFullErrorMessage(ex);
            }

            return result;
        }

        private IActionResult ProcessResponse<T>(ServiceResponse<T> response)
        {
            if (!response.Success)
            {
                object? responseBody = response.StatusCode == 500
                    ? response.ErrorMessage
                    : new { message = response.ErrorMessage };

                return StatusCode(response.StatusCode, responseBody);
            }

            return Ok(JsonConvert.SerializeObject(response.Data, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            }));
        }


        private string GetFullErrorMessage(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            if (exception.InnerException != null)
            {
                return $"{exception.Message}:\n{GetFullErrorMessage(exception.InnerException)}";
            }

            return exception.Message;
        }
    }
}