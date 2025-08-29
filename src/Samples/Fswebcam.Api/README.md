# Fswebcam.Api - 树莓派相机控制API

这是一个基于 ASP.NET Core 的 Web API 项目，用于在树莓派上控制相机拍照并将图片显示到 2.4 寸屏幕上。

## 功能特性

- 🔧 **Web 控制面板**：通过网页界面控制相机和屏幕
- 📷 **相机拍照**：使用 fswebcam 命令进行拍照
- 🖥️ **屏幕显示**：将拍摄的图片显示到 2.4 寸 ST7789 屏幕
- 📸 **图片管理**：查看、下载、删除拍摄的图片
- 💬 **文本显示**：在屏幕上显示自定义文本
- 🔄 **实时状态**：显示相机和屏幕的状态信息

## 系统要求

### 硬件要求
- 树莓派（推荐 4B 或更新版本）
- USB 摄像头或树莓派摄像头模块
- 2.4 寸 ST7789 SPI 显示屏

### 软件要求
- .NET 9.0 Runtime
- fswebcam 工具
- Linux 操作系统（推荐 Raspberry Pi OS）

## 安装步骤

### 1. 安装系统依赖

```bash
# 更新系统
sudo apt update && sudo apt upgrade -y

# 安装 fswebcam
sudo apt install fswebcam -y

# 安装 .NET 9.0 Runtime
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --runtime aspnetcore
```

### 2. 硬件连接

#### 2.4寸屏幕连接（SPI0.0）：
- VCC -> 3.3V
- GND -> GND
- DIN -> GPIO 10 (MOSI)
- CLK -> GPIO 11 (SCLK)
- CS -> GPIO 8 (CE0)
- DC -> GPIO 25
- RST -> GPIO 27

#### USB 摄像头：
- 连接到任意 USB 端口
- 确保设备路径为 `/dev/video0`

### 3. 启用 SPI

```bash
# 编辑配置文件
sudo nano /boot/firmware/config.txt

# 添加或取消注释以下行：
dtparam=spi=on

# 重启系统
sudo reboot
```

### 4. 验证硬件

```bash
# 检查 SPI 设备
ls /dev/spi*

# 检查摄像头
ls /dev/video*

# 测试拍照命令
fswebcam /dev/video0 --no-banner -r 640x480 test.jpg
```

## 使用方法

### 1. 构建和运行项目

```bash
# 克隆项目
git clone <repository-url>
cd dotnet-iot-tutorial-code/src/Samples/Fswebcam.Api

# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行项目
dotnet run
```

### 2. 访问 Web 界面

打开浏览器访问：
- 本地访问：`http://localhost:5000`
- 局域网访问：`http://<树莓派IP地址>:5000`

### 3. 使用控制面板

1. **立即拍照**：点击"立即拍照并显示"按钮
2. **自定义拍照**：输入文件名后点击"自定义拍照"
3. **显示图片**：在图片列表中点击"显示"按钮
4. **清除屏幕**：点击"清除屏幕"按钮
5. **显示文本**：输入文本后点击"显示文本"按钮

## API 接口

### 相机控制
- `POST /api/camera/take-photo` - 拍照并显示
- `GET /api/camera/images` - 获取图片列表
- `POST /api/camera/display-image/{fileName}` - 显示指定图片
- `GET /api/camera/download/{fileName}` - 下载图片
- `DELETE /api/camera/delete/{fileName}` - 删除图片

### 屏幕控制
- `POST /api/camera/clear-screen` - 清除屏幕
- `POST /api/camera/display-text` - 显示文本

### 系统状态
- `GET /api/camera/status` - 获取系统状态

## 配置说明

### appsettings.json

```json
{
  "ImageDirectory": "Images"  // 图片存储目录
}
```

### 环境变量

- `ASPNETCORE_URLS`：设置监听地址，如 `http://0.0.0.0:5000`
- `ImageDirectory`：覆盖图片存储目录

## 故障排除

### 1. 相机问题

```bash
# 检查摄像头权限
sudo chmod 666 /dev/video0

# 检查摄像头是否被占用
sudo lsof /dev/video0

# 重新插拔 USB 摄像头
```

### 2. 屏幕问题

```bash
# 检查 SPI 是否启用
ls /dev/spi*

# 检查 GPIO 权限
sudo usermod -a -G spi,gpio $USER

# 重新登录生效
```

### 3. 权限问题

```bash
# 添加用户到相关组
sudo usermod -a -G video,spi,gpio $USER

# 重新登录或重启
sudo reboot
```

### 4. 网络访问问题

```bash
# 检查防火墙设置
sudo ufw status

# 允许端口 5000
sudo ufw allow 5000

# 或者关闭防火墙（仅测试环境）
sudo ufw disable
```

## 开发说明

### 项目结构

```
Fswebcam.Api/
├── Controllers/
│   └── CameraController.cs     # API 控制器
├── Services/
│   ├── CameraService.cs        # 相机服务
│   └── DisplayService.cs       # 显示服务
├── wwwroot/
│   └── index.html              # Web 控制面板
├── Program.cs                  # 主程序
├── appsettings.json           # 配置文件
└── Fswebcam.Api.csproj        # 项目文件
```

### 依赖项目

- `Verdure.Iot.Device` - ST7789 显示器驱动
- `System.Device.Gpio` - GPIO 控制
- `SixLabors.ImageSharp` - 图像处理

### 扩展建议

1. **安全性**：添加身份验证和授权
2. **多摄像头**：支持多个摄像头设备
3. **图像处理**：添加滤镜和特效
4. **定时拍照**：支持定时自动拍照
5. **云存储**：支持图片上传到云端

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request！
