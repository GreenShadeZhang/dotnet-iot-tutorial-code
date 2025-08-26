using Microsoft.AspNetCore.Mvc;
using RGBLed.Api.Models;
using RGBLed.Api.Services;

namespace RGBLed.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedController : ControllerBase
{
    private readonly RgbLedService _ledService;
    private readonly ILogger<LedController> _logger;

    public LedController(RgbLedService ledService, ILogger<LedController> logger)
    {
        _ledService = ledService;
        _logger = logger;
    }

    /// <summary>
    /// 获取LED状态
    /// </summary>
    [HttpGet("status")]
    public ActionResult<LedStatus> GetStatus()
    {
        try
        {
            var status = _ledService.GetStatus();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取LED状态失败");
            return StatusCode(500, "获取LED状态失败");
        }
    }

    /// <summary>
    /// 设置LED效果
    /// </summary>
    [HttpPost("effect")]
    public async Task<IActionResult> SetEffect([FromBody] LedControlRequest request)
    {
        try
        {
            await _ledService.SetEffect(request);
            _logger.LogInformation("LED效果设置成功: {Effect}, 颜色: R:{Red} G:{Green} B:{Blue}", 
                request.Effect, request.Color.R, request.Color.G, request.Color.B);
            return Ok(new { message = "LED效果设置成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置LED效果失败");
            return StatusCode(500, "设置LED效果失败");
        }
    }

    /// <summary>
    /// 关闭LED
    /// </summary>
    [HttpPost("off")]
    public async Task<IActionResult> TurnOff()
    {
        try
        {
            await _ledService.SetEffect(new LedControlRequest { Effect = LedEffect.Off });
            return Ok(new { message = "LED已关闭" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关闭LED失败");
            return StatusCode(500, "关闭LED失败");
        }
    }

    /// <summary>
    /// 设置静态颜色
    /// </summary>
    [HttpPost("color")]
    public async Task<IActionResult> SetColor([FromBody] LedColor color, [FromQuery] int brightness = 100)
    {
        try
        {
            var request = new LedControlRequest
            {
                Effect = LedEffect.Static,
                Color = color,
                Brightness = brightness
            };
            await _ledService.SetEffect(request);
            return Ok(new { message = "颜色设置成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置颜色失败");
            return StatusCode(500, "设置颜色失败");
        }
    }

    /// <summary>
    /// 设置闪烁效果
    /// </summary>
    [HttpPost("blink")]
    public async Task<IActionResult> SetBlink([FromBody] LedColor color, [FromQuery] int speed = 1000, [FromQuery] int brightness = 100)
    {
        try
        {
            var request = new LedControlRequest
            {
                Effect = LedEffect.Blink,
                Color = color,
                Speed = speed,
                Brightness = brightness
            };
            await _ledService.SetEffect(request);
            return Ok(new { message = "闪烁效果设置成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置闪烁效果失败");
            return StatusCode(500, "设置闪烁效果失败");
        }
    }

    /// <summary>
    /// 设置呼吸灯效果
    /// </summary>
    [HttpPost("breathe")]
    public async Task<IActionResult> SetBreathe([FromBody] LedColor color, [FromQuery] int speed = 2000, [FromQuery] int brightness = 100)
    {
        try
        {
            var request = new LedControlRequest
            {
                Effect = LedEffect.Breathe,
                Color = color,
                Speed = speed,
                Brightness = brightness
            };
            await _ledService.SetEffect(request);
            return Ok(new { message = "呼吸灯效果设置成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置呼吸灯效果失败");
            return StatusCode(500, "设置呼吸灯效果失败");
        }
    }

    /// <summary>
    /// 设置彩虹效果
    /// </summary>
    [HttpPost("rainbow")]
    public async Task<IActionResult> SetRainbow([FromQuery] int speed = 3600, [FromQuery] int brightness = 100)
    {
        try
        {
            var request = new LedControlRequest
            {
                Effect = LedEffect.Rainbow,
                Color = LedColor.White, // 彩虹效果不使用固定颜色
                Speed = speed,
                Brightness = brightness
            };
            await _ledService.SetEffect(request);
            return Ok(new { message = "彩虹效果设置成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置彩虹效果失败");
            return StatusCode(500, "设置彩虹效果失败");
        }
    }

    /// <summary>
    /// 获取预定义颜色
    /// </summary>
    [HttpGet("colors")]
    public ActionResult<object> GetPredefinedColors()
    {
        var colors = new
        {
            Red = LedColor.Red,
            Green = LedColor.Green,
            Blue = LedColor.Blue,
            White = LedColor.White,
            Yellow = LedColor.Yellow,
            Cyan = LedColor.Cyan,
            Magenta = LedColor.Magenta,
            Orange = LedColor.Orange,
            Purple = LedColor.Purple,
            Black = LedColor.Black
        };
        return Ok(colors);
    }

    /// <summary>
    /// 获取可用效果
    /// </summary>
    [HttpGet("effects")]
    public ActionResult<string[]> GetAvailableEffects()
    {
        var effects = Enum.GetNames<LedEffect>();
        return Ok(effects);
    }
}
