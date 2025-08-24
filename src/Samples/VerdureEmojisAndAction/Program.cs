using VerdureEmojisAndAction.Services;

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
builder.Services.AddSingleton<DisplayService>();
builder.Services.AddSingleton<RobotActionService>();
builder.Services.AddSingleton<EmotionActionService>();

// 注册后台服务
builder.Services.AddHostedService<TimeDisplayService>();

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

// 在应用启动时初始化机器人
var serviceProvider = app.Services;
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("=== VerdureEmojisAndAction 启动 ===");
logger.LogInformation("正在初始化服务...");

try
{
    // 获取服务并进行初始化
    var emotionService = serviceProvider.GetRequiredService<EmotionActionService>();
    
    // 在后台初始化机器人位置
    _ = Task.Run(async () =>
    {
        try
        {
            await Task.Delay(2000); // 等待服务完全启动
            await emotionService.InitializeRobotAsync();
            logger.LogInformation("机器人初始化完成");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "机器人初始化失败");
        }
    });
    
    logger.LogInformation("服务初始化完成");
    logger.LogInformation("Web控制面板: http://localhost:5000");
    logger.LogInformation("API 端点:");
    logger.LogInformation("  POST /api/emotion/play - 播放指定表情和动作");
    logger.LogInformation("  POST /api/emotion/play-emotion/{type} - 仅播放表情");
    logger.LogInformation("  POST /api/emotion/play-action/{type} - 仅播放动作");
    logger.LogInformation("  POST /api/emotion/play-random - 随机播放");
    logger.LogInformation("  POST /api/emotion/stop - 停止播放");
    logger.LogInformation("  GET  /api/emotion/status - 获取状态");
    logger.LogInformation("  GET  /api/emotion/emotions - 获取可用表情");
    logger.LogInformation("  POST /api/emotion/initialize - 初始化机器人");
    logger.LogInformation("  POST /api/emotion/demo - 运行演示程序");
}
catch (Exception ex)
{
    logger.LogError(ex, "服务初始化失败");
}

app.Run();
