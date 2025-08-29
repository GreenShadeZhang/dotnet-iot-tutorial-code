using Fswebcam.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 添加日志配置
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// 注册自定义服务
builder.Services.AddSingleton<CameraService>();
builder.Services.AddSingleton<DisplayService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 添加静态文件支持
app.UseDefaultFiles();
app.UseStaticFiles();

// 添加CORS支持以便前端调用
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

app.UseAuthorization();

app.MapControllers();

// 在应用启动时显示信息
var serviceProvider = app.Services;
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("=== Fswebcam.Api 启动 ===");
logger.LogInformation("正在初始化服务...");

try
{
    // 获取服务并检查状态
    var cameraService = serviceProvider.GetRequiredService<CameraService>();
    var displayService = serviceProvider.GetRequiredService<DisplayService>();
    
    logger.LogInformation("服务初始化完成");
    logger.LogInformation("Web控制面板: http://localhost:5000");
    logger.LogInformation("图片存储目录: {0}", cameraService.ImageDirectory);
    logger.LogInformation("显示器状态: {0}", displayService.IsInitialized ? "已初始化" : "未初始化（非Linux平台）");
    logger.LogInformation("API 端点:");
    logger.LogInformation("  POST /api/camera/take-photo - 拍照并显示到屏幕");
    logger.LogInformation("  GET  /api/camera/images - 获取图片列表");
    logger.LogInformation("  POST /api/camera/display-image/{fileName} - 显示指定图片");
    logger.LogInformation("  GET  /api/camera/download/{fileName} - 下载图片");
    logger.LogInformation("  DELETE /api/camera/delete/{fileName} - 删除图片");
    logger.LogInformation("  POST /api/camera/clear-screen - 清除屏幕");
    logger.LogInformation("  POST /api/camera/display-text - 显示文本");
    logger.LogInformation("  GET  /api/camera/status - 获取系统状态");
}
catch (Exception ex)
{
    logger.LogError(ex, "服务初始化失败");
}

app.Run();
