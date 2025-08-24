# 🔧 VerdureEmojisAndAction API连接诊断指南

## 📋 已修复的问题

### 1. 前端API基础URL问题
- **问题**: 前端使用硬编码的 `http://localhost:5000`
- **修复**: 改为自动检测 `window.location.origin`

### 2. 请求参数格式问题
- **问题**: 前端发送的参数格式与后端期望不匹配
- **修复**: 
  - 确保EmotionType枚举值正确传递
  - 改进错误处理和日志记录
  - 添加参数验证

### 3. 错误处理和日志
- **问题**: 前端无法正确处理API错误响应
- **修复**: 
  - 增强前端错误处理逻辑
  - 添加详细的日志记录
  - 支持非JSON响应处理

### 4. 连接测试
- **新增**: 添加 `/api/emotion/test` 端点用于测试连接
- **新增**: 前端测试连接按钮

## 🚀 快速测试步骤

### 1. 启动应用
```bash
cd VerdureEmojisAndAction
dotnet run
```

### 2. 使用测试脚本
运行 `test-api.bat` 自动测试所有API端点

### 3. 手动测试API

#### 测试连接
```bash
curl http://localhost:5000/api/emotion/test
```

#### 测试状态
```bash
curl http://localhost:5000/api/emotion/status
```

#### 测试播放
```bash
curl -X POST http://localhost:5000/api/emotion/play \
  -H "Content-Type: application/json" \
  -d '{"emotionType":"Happy","includeAction":true,"includeEmotion":true,"loops":1,"fps":30}'
```

### 4. 浏览器测试
打开 `http://localhost:5000` 使用Web控制面板

## 🔍 常见问题排查

### 问题1: 前端显示"网络错误"
**可能原因**:
- 应用未启动
- 端口被占用
- CORS配置问题

**解决方案**:
1. 确认应用正在运行
2. 检查控制台输出的端口号
3. 使用测试脚本验证API

### 问题2: API返回404
**可能原因**:
- 路由配置错误
- 控制器未正确注册

**解决方案**:
1. 检查控制器路由: `[Route("api/[controller]")]`
2. 确认应用启动日志中显示的端点

### 问题3: 表情播放失败
**可能原因**:
- Lottie文件未找到
- 硬件未连接(非Linux环境)

**解决方案**:
1. 确认 `anger.mp4.lottie.json` 和 `happy.mp4.lottie.json` 存在
2. 查看应用日志获取详细错误信息

## 📊 API端点列表

| 方法 | 端点 | 描述 |
|------|------|------|
| GET | `/api/emotion/test` | 测试连接 |
| GET | `/api/emotion/status` | 获取播放状态 |
| GET | `/api/emotion/emotions` | 获取可用表情 |
| POST | `/api/emotion/play` | 播放表情和动作 |
| POST | `/api/emotion/play-random` | 随机播放 |
| POST | `/api/emotion/stop` | 停止播放 |
| POST | `/api/emotion/initialize` | 初始化机器人 |
| POST | `/api/emotion/demo` | 运行演示程序 |

## 🔧 调试技巧

1. **查看浏览器控制台**: F12 -> Console 查看前端错误
2. **查看应用日志**: 控制台输出包含详细的API调用日志
3. **使用测试端点**: 先测试 `/test` 端点确认基础连接
4. **逐步测试**: 从简单的GET请求开始，再测试复杂的POST请求

现在您的API连接问题应该已经解决了！🎉
