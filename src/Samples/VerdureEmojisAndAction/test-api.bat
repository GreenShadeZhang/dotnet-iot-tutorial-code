@echo off
echo ======================================
echo VerdureEmojisAndAction 测试脚本
echo ======================================
echo.

cd /d "%~dp0"

echo 正在启动应用...
echo 请稍候，应用启动后会显示端口号
echo.

start /B dotnet run

echo 等待应用启动...
timeout /t 5 /nobreak > nul

echo.
echo 测试API连接...
echo.

REM 使用PowerShell测试API连接
powershell -Command "try { $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/emotion/test' -Method GET; Write-Host '✅ API连接成功:' $response.message '(' $response.timestamp ')' -ForegroundColor Green } catch { Write-Host '❌ API连接失败:' $_.Exception.Message -ForegroundColor Red }"

echo.
echo 测试获取状态...
powershell -Command "try { $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/emotion/status' -Method GET; Write-Host '✅ 状态获取成功:' $response.status -ForegroundColor Green } catch { Write-Host '❌ 状态获取失败:' $_.Exception.Message -ForegroundColor Red }"

echo.
echo 测试获取可用表情...
powershell -Command "try { $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/emotion/emotions' -Method GET; Write-Host '✅ 表情列表获取成功，共' $response.Count '个表情' -ForegroundColor Green } catch { Write-Host '❌ 表情列表获取失败:' $_.Exception.Message -ForegroundColor Red }"

echo.
echo ======================================
echo 测试完成！
echo.
echo 如果所有测试都成功，请打开浏览器访问:
echo http://localhost:5000
echo.
echo 如果需要停止应用，请按 Ctrl+C
echo ======================================

pause
