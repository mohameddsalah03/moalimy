using Microsoft.AspNetCore.Http;
using Moalimi.Application.DTOs.Cloudinary;

namespace Moalimi.Application.Interfaces;

public interface ICloudinaryService
{
    Task<UploadImageResponse> UploadAvatarAsync(IFormFile file, string userId);
    Task<UploadImageResponse> UploadCertificateAsync(IFormFile file, string userId);
}