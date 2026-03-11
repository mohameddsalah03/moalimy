using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moalimi.Application.DTOs.Cloudinary;
using Moalimi.Application.DTOs.Settings;
using Moalimi.Application.Interfaces;

namespace Moalimi.Application.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(
        IOptions<CloudinarySettings> options,
        ILogger<CloudinaryService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret
        );
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<UploadImageResponse> UploadAvatarAsync(IFormFile file, string userId)
    {
        if (_settings.IsSandbox)
        {
            var fakeUrl = $"https://ui-avatars.com/api/?name={userId}&background=random";
            _logger.LogInformation("[SANDBOX] Avatar uploaded: {Url}", fakeUrl);
            return new UploadImageResponse(true, fakeUrl, null);
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = $"hissatak/avatars/{userId}",
                Overwrite = true,
                Transformation = new Transformation()
                    .Width(400).Height(400).Crop("fill").Gravity("face")
            });

            if (result.Error != null)
                return new UploadImageResponse(false, null, result.Error.Message);

            return new UploadImageResponse(true, result.SecureUrl.ToString(), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avatar upload failed");
            return new UploadImageResponse(false, null, ex.Message);
        }
    }

    public async Task<UploadImageResponse> UploadCertificateAsync(IFormFile file, string userId)
    {
        if (_settings.IsSandbox)
        {
            var fakeUrl = $"https://example.com/sandbox-cert-{userId}-{Guid.NewGuid()}.pdf";
            _logger.LogInformation("[SANDBOX] Certificate uploaded: {Url}", fakeUrl);
            return new UploadImageResponse(true, fakeUrl, null);
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _cloudinary.UploadAsync(new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = $"hissatak/certificates/{userId}/{Guid.NewGuid()}"
            });

            if (result.Error != null)
                return new UploadImageResponse(false, null, result.Error.Message);

            return new UploadImageResponse(true, result.SecureUrl.ToString(), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Certificate upload failed");
            return new UploadImageResponse(false, null, ex.Message);
        }
    }
}