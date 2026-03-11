using Microsoft.AspNetCore.Mvc;
using Moalimi.Application.Interfaces;

namespace Moalimi.API.Controller;

[ApiController]
[Route("api/upload")]
public class CloudinaryController : ControllerBase
{
    private readonly ICloudinaryService _service;

    public CloudinaryController(ICloudinaryService service) => _service = service;

    /// <summary>رفع صورة بروفايل</summary>
    [HttpPost("avatar/{userId}")]
    public async Task<IActionResult> UploadAvatar(string userId, IFormFile file)
    {
        var result = await _service.UploadAvatarAsync(file, userId);
        return Ok(result);
    }

    /// <summary>رفع شهادة</summary>
    [HttpPost("certificate/{userId}")]
    public async Task<IActionResult> UploadCertificate(string userId, IFormFile file)
    {
        var result = await _service.UploadCertificateAsync(file, userId);
        return Ok(result);
    }
}