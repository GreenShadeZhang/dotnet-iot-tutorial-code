using Fswebcam.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fswebcam.Api.Controllers;

/// <summary>
/// 相机控制API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CameraController : ControllerBase
{
    private readonly ILogger<CameraController> _logger;
    private readonly CameraService _cameraService;
    private readonly DisplayService _displayService;

    public CameraController(
        ILogger<CameraController> logger,
        CameraService cameraService,
        DisplayService displayService)
    {
        _logger = logger;
        _cameraService = cameraService;
        _displayService = displayService;
    }

    /// <summary>
    /// 拍照并显示到屏幕
    /// </summary>
    /// <param name="fileName">可选的文件名</param>
    /// <returns>拍照结果</returns>
    [HttpPost("take-photo")]
    public async Task<IActionResult> TakePhoto([FromQuery] string? fileName = null)
    {
        try
        {
            _logger.LogInformation("收到拍照请求");

            // 显示拍照状态
            await _displayService.DisplayStatusAsync("拍照中...", true);

            // 拍照
            var imagePath = await _cameraService.TakePhotoAsync(fileName);
            
            _logger.LogInformation($"拍照完成: {imagePath}");

            // 显示拍摄的图片到屏幕
            await _displayService.DisplayImageAsync(imagePath);

            return Ok(new
            {
                Success = true,
                Message = "拍照成功",
                ImagePath = imagePath,
                FileName = Path.GetFileName(imagePath),
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "拍照失败");

            // 显示错误状态
            await _displayService.DisplayStatusAsync("拍照失败", false);

            return StatusCode(500, new
            {
                Success = false,
                Message = "拍照失败",
                Error = ex.Message,
                Timestamp = DateTime.Now
            });
        }
    }

    /// <summary>
    /// 获取图片列表
    /// </summary>
    /// <returns>图片文件列表</returns>
    [HttpGet("images")]
    public IActionResult GetImages()
    {
        try
        {
            var images = _cameraService.GetAllImages()
                .Select(img => new
                {
                    FileName = img.Name,
                    Size = img.Length,
                    CreatedTime = img.CreationTime,
                    ModifiedTime = img.LastWriteTime
                })
                .ToList();

            return Ok(new
            {
                Success = true,
                Count = images.Count,
                Images = images
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取图片列表失败");
            return StatusCode(500, new
            {
                Success = false,
                Message = "获取图片列表失败",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 显示指定图片到屏幕
    /// </summary>
    /// <param name="fileName">图片文件名</param>
    /// <returns>显示结果</returns>
    [HttpPost("display-image/{fileName}")]
    public async Task<IActionResult> DisplayImage(string fileName)
    {
        try
        {
            var imagePath = Path.Combine(_cameraService.ImageDirectory, fileName);
            
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "图片文件不存在",
                    FileName = fileName
                });
            }

            await _displayService.DisplayImageAsync(imagePath);

            return Ok(new
            {
                Success = true,
                Message = "图片显示成功",
                FileName = fileName,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"显示图片失败: {fileName}");
            return StatusCode(500, new
            {
                Success = false,
                Message = "显示图片失败",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 下载图片文件
    /// </summary>
    /// <param name="fileName">图片文件名</param>
    /// <returns>图片文件</returns>
    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadImage(string fileName)
    {
        try
        {
            var imagePath = Path.Combine(_cameraService.ImageDirectory, fileName);
            
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            var imageBytes = await _cameraService.GetImageBytesAsync(imagePath);
            var contentType = GetContentType(fileName);

            return File(imageBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"下载图片失败: {fileName}");
            return StatusCode(500, "下载图片失败");
        }
    }

    /// <summary>
    /// 删除图片文件
    /// </summary>
    /// <param name="fileName">图片文件名</param>
    /// <returns>删除结果</returns>
    [HttpDelete("delete/{fileName}")]
    public IActionResult DeleteImage(string fileName)
    {
        try
        {
            var success = _cameraService.DeleteImage(fileName);
            
            if (success)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "图片删除成功",
                    FileName = fileName
                });
            }
            else
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "图片文件不存在",
                    FileName = fileName
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"删除图片失败: {fileName}");
            return StatusCode(500, new
            {
                Success = false,
                Message = "删除图片失败",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 清除屏幕
    /// </summary>
    /// <returns>清屏结果</returns>
    [HttpPost("clear-screen")]
    public IActionResult ClearScreen()
    {
        try
        {
            _displayService.ClearScreen();
            
            return Ok(new
            {
                Success = true,
                Message = "屏幕已清除",
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清屏失败");
            return StatusCode(500, new
            {
                Success = false,
                Message = "清屏失败",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 显示文本到屏幕
    /// </summary>
    /// <param name="text">要显示的文本</param>
    /// <param name="fontSize">字体大小</param>
    /// <returns>显示结果</returns>
    [HttpPost("display-text")]
    public async Task<IActionResult> DisplayText([FromQuery] string text, [FromQuery] int fontSize = 24)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "文本不能为空"
                });
            }

            await _displayService.DisplayTextAsync(text, fontSize);

            return Ok(new
            {
                Success = true,
                Message = "文本显示成功",
                Text = text,
                FontSize = fontSize,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"显示文本失败: {text}");
            return StatusCode(500, new
            {
                Success = false,
                Message = "显示文本失败",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 获取相机状态
    /// </summary>
    /// <returns>相机和显示器状态</returns>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        try
        {
            var imageCount = _cameraService.GetAllImages().Count();
            
            return Ok(new
            {
                Success = true,
                CameraReady = true, // 假设相机总是准备好的
                DisplayReady = _displayService.IsInitialized,
                ImageDirectory = _cameraService.ImageDirectory,
                ImageCount = imageCount,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取状态失败");
            return StatusCode(500, new
            {
                Success = false,
                Message = "获取状态失败",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 根据文件扩展名获取内容类型
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }
}
