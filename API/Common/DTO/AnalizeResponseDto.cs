using System.Text.Json.Serialization;

namespace API.Common.DTO
{
    public class AnalizeResponseDto
    {
        public string Value { get; set; }
        public byte[] Image { get; set; }
    }
}
