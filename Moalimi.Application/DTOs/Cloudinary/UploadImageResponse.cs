namespace Moalimi.Application.DTOs.Cloudinary
{
  
    public record UploadImageResponse(
    bool IsSuccess,
    string? Url,
    string? Error
    );
}
