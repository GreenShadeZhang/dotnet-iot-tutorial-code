using System.Diagnostics;
using System.Text;

namespace Fswebcam.Api.Services;

/// <summary>
/// 相机拍照服务
/// </summary>
public class CameraService
{
    private readonly ILogger<CameraService> _logger;
    private readonly string _imageDirectory;

    public CameraService(ILogger<CameraService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _imageDirectory = configuration.GetValue<string>("ImageDirectory") ?? "Images";
        
        // 确保图片目录存在
        if (!Directory.Exists(_imageDirectory))
        {
            Directory.CreateDirectory(_imageDirectory);
            _logger.LogInformation($"创建图片目录: {_imageDirectory}");
        }
    }

    /// <summary>
    /// 使用 fswebcam 拍照
    /// </summary>
    /// <param name="fileName">文件名（不含路径）</param>
    /// <returns>完整文件路径</returns>
    public async Task<string> TakePhotoAsync(string? fileName = null)
    {
        try
        {
            // 生成文件名
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            }

            var fullPath = Path.Combine(_imageDirectory, fileName);
            
            // 构建 fswebcam 命令
            var command = "fswebcam";
            var arguments = $"/dev/video0 --no-banner -r 640x480 {fullPath}";

            _logger.LogInformation($"执行拍照命令: {command} {arguments}");

            // 创建进程启动信息
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // 启动进程
            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            // 读取输出
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            // 等待进程完成
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"拍照成功: {fullPath}");
                
                // 验证文件是否存在
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
                else
                {
                    throw new InvalidOperationException($"拍照命令执行成功，但文件不存在: {fullPath}");
                }
            }
            else
            {
                var errorMessage = $"拍照失败，退出代码: {process.ExitCode}, 错误信息: {error}";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "拍照过程中发生异常");
            throw;
        }
    }

    /// <summary>
    /// 获取指定路径的图片
    /// </summary>
    /// <param name="filePath">图片文件路径</param>
    /// <returns>图片字节数组</returns>
    public async Task<byte[]> GetImageBytesAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"图片文件不存在: {filePath}");
        }

        try
        {
            return await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"读取图片文件失败: {filePath}");
            throw;
        }
    }

    /// <summary>
    /// 获取图片目录中的所有图片文件
    /// </summary>
    /// <returns>图片文件信息列表</returns>
    public IEnumerable<FileInfo> GetAllImages()
    {
        try
        {
            var directory = new DirectoryInfo(_imageDirectory);
            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            
            return directory.GetFiles()
                .Where(f => supportedExtensions.Contains(f.Extension.ToLower()))
                .OrderByDescending(f => f.CreationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取图片列表失败");
            return Enumerable.Empty<FileInfo>();
        }
    }

    /// <summary>
    /// 删除指定的图片文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteImage(string fileName)
    {
        try
        {
            var fullPath = Path.Combine(_imageDirectory, fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation($"删除图片: {fullPath}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"删除图片失败: {fileName}");
            return false;
        }
    }

    /// <summary>
    /// 获取图片目录路径
    /// </summary>
    public string ImageDirectory => _imageDirectory;
}
