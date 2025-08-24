@echo off
echo ======================================
echo VerdureEmojisAndAction 启动脚本
echo ======================================
echo.

cd /d "%~dp0"

echo 正在构建项目...
dotnet build

if %ERRORLEVEL% neq 0 (
    echo 构建失败！请检查错误信息。
    pause
    exit /b 1
)

echo.
echo 构建成功！正在启动服务...
echo.
echo 控制面板地址: http://localhost:5000
echo 按 Ctrl+C 停止服务
echo.

dotnet run

pause
