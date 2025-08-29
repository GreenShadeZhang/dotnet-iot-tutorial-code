#!/bin/bash

# Fswebcam.Api 树莓派部署脚本

set -e

echo "=== Fswebcam.Api 树莓派部署脚本 ==="

# 检查是否为 root 用户
if [[ $EUID -eq 0 ]]; then
   echo "请勿以 root 用户运行此脚本"
   exit 1
fi

# 检查系统
if [[ "$OSTYPE" != "linux-gnu"* ]]; then
    echo "此脚本只能在 Linux 系统上运行"
    exit 1
fi

echo "1. 检查系统依赖..."

# 检查并安装必要的系统包
if ! command -v fswebcam &> /dev/null; then
    echo "安装 fswebcam..."
    sudo apt update
    sudo apt install -y fswebcam
fi

# 检查 .NET 运行时
if ! command -v dotnet &> /dev/null; then
    echo "安装 .NET 9.0 运行时..."
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --runtime aspnetcore
    
    # 添加到 PATH
    if ! grep -q 'export PATH="$PATH:$HOME/.dotnet"' ~/.bashrc; then
        echo 'export PATH="$PATH:$HOME/.dotnet"' >> ~/.bashrc
        export PATH="$PATH:$HOME/.dotnet"
    fi
fi

echo "2. 配置硬件..."

# 检查 SPI 是否启用
if ! ls /dev/spi* &> /dev/null; then
    echo "启用 SPI..."
    if ! grep -q "dtparam=spi=on" /boot/firmware/config.txt; then
        echo "dtparam=spi=on" | sudo tee -a /boot/firmware/config.txt
        echo "⚠ SPI 已启用，需要重启系统生效"
        NEED_REBOOT=true
    fi
fi

# 检查用户权限
echo "配置用户权限..."
sudo usermod -a -G spi,gpio,video $USER

echo "3. 构建项目..."

# 还原依赖
dotnet restore

# 构建项目
dotnet build -c Release

echo "4. 创建服务..."

# 创建 systemd 服务文件
SERVICE_FILE="/etc/systemd/system/fswebcam-api.service"
PROJECT_PATH=$(pwd)

sudo tee $SERVICE_FILE > /dev/null <<EOF
[Unit]
Description=Fswebcam API Service
After=network.target

[Service]
Type=notify
User=$USER
Group=$USER
WorkingDirectory=$PROJECT_PATH
ExecStart=$HOME/.dotnet/dotnet run --project $PROJECT_PATH
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=fswebcam-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
EOF

echo "5. 配置防火墙..."

# 允许端口 5000
if command -v ufw &> /dev/null; then
    sudo ufw allow 5000/tcp
fi

echo "6. 启动服务..."

# 重新加载 systemd
sudo systemctl daemon-reload

# 启用并启动服务
sudo systemctl enable fswebcam-api.service
sudo systemctl start fswebcam-api.service

echo "7. 部署完成！"
echo ""
echo "服务状态："
sudo systemctl status fswebcam-api.service --no-pager
echo ""
echo "查看日志："
echo "  sudo journalctl -u fswebcam-api.service -f"
echo ""
echo "访问地址："
echo "  http://$(hostname -I | awk '{print $1}'):5000"
echo "  http://localhost:5000"
echo ""
echo "服务管理："
echo "  启动: sudo systemctl start fswebcam-api.service"
echo "  停止: sudo systemctl stop fswebcam-api.service"
echo "  重启: sudo systemctl restart fswebcam-api.service"
echo "  状态: sudo systemctl status fswebcam-api.service"
echo ""

if [[ "$NEED_REBOOT" == "true" ]]; then
    echo "⚠ 需要重启系统以启用 SPI 接口"
    echo "运行：sudo reboot"
fi
