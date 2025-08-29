#!/bin/bash

# Fswebcam.Api 测试脚本

echo "=== Fswebcam.Api 项目测试 ==="

# 检查当前目录
if [[ ! -f "Fswebcam.Api.csproj" ]]; then
    echo "错误：请在 Fswebcam.Api 项目目录中运行此脚本"
    exit 1
fi

echo "1. 还原 NuGet 包..."
dotnet restore

if [[ $? -ne 0 ]]; then
    echo "错误：NuGet 包还原失败"
    exit 1
fi

echo "2. 构建项目..."
dotnet build

if [[ $? -ne 0 ]]; then
    echo "错误：项目构建失败"
    exit 1
fi

echo "3. 检查硬件依赖..."

# 检查是否为 Linux 系统
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    echo "✓ Linux 系统检测通过"
    
    # 检查 fswebcam
    if command -v fswebcam &> /dev/null; then
        echo "✓ fswebcam 已安装"
    else
        echo "⚠ fswebcam 未安装，请运行：sudo apt install fswebcam"
    fi
    
    # 检查 SPI 设备
    if ls /dev/spi* &> /dev/null; then
        echo "✓ SPI 设备可用"
    else
        echo "⚠ SPI 设备不可用，请检查 SPI 是否启用"
    fi
    
    # 检查摄像头
    if ls /dev/video* &> /dev/null; then
        echo "✓ 摄像头设备可用"
    else
        echo "⚠ 摄像头设备不可用，请检查摄像头连接"
    fi
    
else
    echo "⚠ 非 Linux 系统，硬件功能将被禁用"
fi

echo "4. 检查项目文件..."

# 检查必要文件
files=(
    "Controllers/CameraController.cs"
    "Services/CameraService.cs"
    "Services/DisplayService.cs"
    "wwwroot/index.html"
    "README.md"
    "Images"
)

for file in "${files[@]}"; do
    if [[ -e "$file" ]]; then
        echo "✓ $file 存在"
    else
        echo "✗ $file 缺失"
    fi
done

echo "5. 测试完成！"
echo ""
echo "启动项目："
echo "  dotnet run"
echo ""
echo "访问地址："
echo "  http://localhost:5000"
echo ""
echo "API 测试："
echo "  curl http://localhost:5000/api/camera/status"
