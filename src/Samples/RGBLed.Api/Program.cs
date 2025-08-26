using RGBLed.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 注册RGB LED服务为单例
builder.Services.AddSingleton<RgbLedService>();

// 添加CORS支持
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 启用CORS
app.UseCors();

// 启用静态文件服务
app.UseStaticFiles();

// 设置默认文件
app.UseDefaultFiles();

app.UseAuthorization();

app.MapControllers();

// 在应用启动时初始化LED服务
var ledService = app.Services.GetRequiredService<RgbLedService>();

app.Run();
