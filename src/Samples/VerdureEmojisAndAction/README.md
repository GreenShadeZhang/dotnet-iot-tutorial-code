# VerdureEmojisAndAction - 机器人情感表达和动作控制系统

## 🎯 项目概述

VerdureEmojisAndAction 是一个集成的机器人情感表达和动作控制系统，支持：

- **双屏显示控制**: 2.4寸屏幕显示Lottie动画表情，1.47寸屏幕显示实时时间
- **机器人动作控制**: 通过I2C控制多个舵机关节实现情感动作
- **分离式架构**: 表情播放和动作播放逻辑分离，支持独立或同步执行
- **RESTful API**: 提供完整的Web API接口便于其他系统集成
- **实时监控**: 后台定时任务和状态监控

## 🏗️ 系统架构

### 核心服务模块

1. **DisplayService** - 双屏显示服务
   - 管理2.4寸和1.47寸屏幕
   - Lottie动画渲染
   - 时间显示

2. **RobotActionService** - 机器人动作控制服务
   - I2C舵机控制
   - 关节角度管理
   - 预定义动作序列

3. **EmotionActionService** - 情感动作整合服务
   - 统一管理表情和动作
   - 支持同步/异步执行
   - 播放状态管理

4. **TimeDisplayService** - 时间显示后台服务
   - 每秒更新1.47寸屏幕时间
   - 后台持续运行

### 支持的表情类型

- **Neutral (平静)**: 轻柔自然的动作，耳朵和脖子的温和摆动  
- **Happy (快乐)**: 活跃的耳朵活动，欢快的手臂摆动
- **Sad (悲伤)**: 耳朵下垂，手臂下垂，缓慢的沮丧动作
- **Angry (愤怒)**: 耳朵竖起，手臂张开威胁，激烈的头部晃动
- **Surprised (惊讶)**: 突然的震惊姿态，快速的左右张望
- **Confused (困惑)**: 不对称的耳朵动作，思考性的头部摆动

## 🚀 快速开始

### 1. 环境要求

- .NET 9.0
- Linux系统 (树莓派等)
- I2C总线支持
- SPI总线支持

### 2. 硬件连接

- **2.4寸屏幕**: SPI0 (CS0)
- **1.47寸屏幕**: SPI0 (CS1)
- **舵机控制器**: I2C总线，地址0x01-0x06

### 3. 运行项目

```bash
cd src/Samples/VerdureEmojisAndAction
dotnet run
```

### 4. 访问控制面板

打开浏览器访问: `http://localhost:5000`

## 📡 API 接口

### 播放控制

#### 播放指定表情和动作
```http
POST /api/emotion/play
Content-Type: application/json

{
  "emotionType": "Happy",
  "includeAction": true,
  "includeEmotion": true,
  "loops": 1,
  "fps": 30
}
```

#### 仅播放表情
```http
POST /api/emotion/play-emotion/Happy?loops=1&fps=30
```

#### 仅播放动作
```http
POST /api/emotion/play-action/Angry
```

#### 随机播放
```http
POST /api/emotion/play-random?includeAction=true&includeEmotion=true&loops=1&fps=30
```

#### 停止播放
```http
POST /api/emotion/stop
```

### 状态和控制

#### 获取播放状态
```http
GET /api/emotion/status
```

#### 获取可用表情
```http
GET /api/emotion/emotions
```

#### 初始化机器人
```http
POST /api/emotion/initialize
```

#### 运行演示程序
```http
POST /api/emotion/demo
```

## 🎪 演示程序

演示程序包含以下场景：

1. **初始化机器人位置**
2. **单独表情播放** - 展示纯动画效果
3. **单独动作播放** - 展示纯机械动作
4. **同步播放** - 表情和动作协调配合
5. **随机播放** - 随机选择情感类型

## 🔧 配置说明

### 关节配置

```csharp
// 关节ID映射
2  - 头部      (I2C: 0x01)
4  - 左臂展开  (I2C: 0x02)
6  - 左臂旋转  (I2C: 0x03)
8  - 右臂展开  (I2C: 0x04)
10 - 右臂旋转  (I2C: 0x05)
12 - 底部旋转  (I2C: 0x06)
```

### 屏幕配置

```csharp
// 屏幕尺寸
2.4寸屏幕: 320x240 (表情显示)
1.47寸屏幕: 320x172 (时间显示，横屏)
```

## 🔌 扩展集成

### 语音服务集成示例

```csharp
public class VoiceService
{
    private readonly EmotionActionService _emotionService;
    
    public async Task ProcessVoiceCommand(string command)
    {
        var emotionType = AnalyzeEmotion(command);
        await _emotionService.PlayEmotionWithActionAsync(new PlayRequest
        {
            EmotionType = emotionType,
            IncludeAction = true,
            IncludeEmotion = true
        });
    }
}
```

### 外部系统调用示例

```csharp
// HTTP客户端调用
using var client = new HttpClient();
var request = new
{
    emotionType = "Happy",
    includeAction = true,
    includeEmotion = true,
    loops = 2
};

var response = await client.PostAsJsonAsync(
    "http://robot-ip:5000/api/emotion/play", 
    request);
```

## 🛠️ 开发指南

### 添加新表情

1. 将Lottie文件放入项目根目录
2. 在 `EmotionType` 枚举中添加新类型
3. 在 `InitializeEmotionConfigs()` 中配置表情参数
4. 在 `RobotActionService` 中实现对应动作

### 自定义动作序列

```csharp
private async Task PerformCustomActionAsync(CancellationToken cancellationToken)
{
    // 设置关节角度
    await SetMultipleJointAnglesAsync(new Dictionary<int, float>
    {
        { 2, 10 },   // 头部角度
        { 4, 20 },   // 左臂角度
        // ...
    }, cancellationToken);
    
    // 等待动作完成
    await Task.Delay(1000, cancellationToken);
}
```

## 📊 系统监控

- **播放状态**: 实时监控当前播放状态
- **错误处理**: 完整的异常捕获和日志记录
- **性能优化**: 帧率控制和内存管理
- **取消支持**: 支持随时停止播放

## 🚨 注意事项

1. **硬件兼容性**: 确保硬件连接正确
2. **权限设置**: Linux系统需要GPIO和I2C权限
3. **资源管理**: 正确释放显示和I2C资源
4. **并发控制**: 避免同时执行多个播放任务

## 📞 技术支持

本项目专为嵌入式机器人应用设计，如有技术问题请查看日志输出或联系开发团队。
