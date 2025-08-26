# RGB LED 控制器 API

这是一个基于 .NET IoT 库的 RGB LED 控制器项目，支持硬件按钮控制和Web API控制。

## 功能特性

### LED 效果
- ❌ **关闭**: 关闭所有LED
- 💡 **静态**: 显示固定颜色
- ⚡ **闪烁**: LED闪烁效果
- 🫁 **呼吸**: 渐亮渐暗的呼吸效果
- 🌈 **彩虹**: 循环显示彩虹色彩

### 颜色支持
支持以下预定义颜色：
- 红色 (Red)
- 绿色 (Green) 
- 蓝色 (Blue)
- 白色 (White)
- 黄色 (Yellow)
- 青色 (Cyan)
- 洋红 (Magenta)
- 橙色 (Orange)
- 紫色 (Purple)

### 控制方式
1. **硬件按钮控制**
2. **Web API 控制**
3. **Web 页面控制**

## 硬件连接

### 🔌 树莓派40针引脚图
```
        树莓派 GPIO 引脚图 (从上往下看)
    +3V3  (1) (2)  +5V     ← 电源引脚
   GPIO2  (3) (4)  +5V     ← I2C SDA
   GPIO3  (5) (6)  GND     ← I2C SCL
   GPIO4  (7) (8)  GPIO14  ← 按钮3连接这里
     GND  (9) (10) GPIO15
  GPIO17 (11) (12) GPIO18  ← 红色LED连接这里 ⭐
  GPIO27 (13) (14) GND     ← 公共接地线 ⭐
  GPIO22 (15) (16) GPIO23
    +3V3 (17) (18) GPIO24
  GPIO10 (19) (20) GND     ← 公共接地线 ⭐
   GPIO9 (21) (22) GPIO25
  GPIO11 (23) (24) GPIO8
     GND (25) (26) GPIO7
   GPIO0 (27) (28) GPIO1
   GPIO5 (29) (30) GND
   GPIO6 (31) (32) GPIO12
  GPIO13 (33) (34) GND
  GPIO19 (35) (36) GPIO16  ← 绿色LED连接这里 ⭐
  GPIO26 (37) (38) GPIO20  ← 蓝色LED连接这里 ⭐
     GND (39) (40) GPIO21

⭐ = 本项目使用的引脚
```

### 🎯 详细接线说明

#### LED 模块连接 (共阴极RGB LED)
```
树莓派引脚        →  RGB LED模块
GPIO18 (12号脚)   →  红色LED正极 (R+)
GPIO19 (35号脚)   →  绿色LED正极 (G+)  
GPIO20 (38号脚)   →  蓝色LED正极 (B+)
GND (任意GND脚)   →  LED共阴极 (-)

推荐使用的GND引脚: 6号脚、9号脚、14号脚、20号脚、25号脚等
```

#### 按钮连接 (带内部上拉电阻)
```
树莓派引脚        →  按钮          功能
GPIO2 (3号脚)     →  按钮1 一端    颜色切换
GPIO3 (5号脚)     →  按钮2 一端    效果切换  
GPIO4 (7号脚)     →  按钮3 一端    亮度调节
GND (任意GND脚)   →  所有按钮另一端 公共接地

推荐使用 6号脚(GND) 连接所有按钮的另一端
```

### 🔧 实际接线步骤

#### 第一步: 准备材料
- 树莓派 (任意型号，带40针GPIO)
- 共阴极RGB LED模块 或 分立的红绿蓝LED + 电阻
- 3个按钮开关
- 杜邦线若干 (公对母或公对公)
- 面包板 (可选，方便接线)

#### 第二步: LED模块接线
```
🔴 红色LED线:
   树莓派 12号脚 (GPIO18) → LED模块 R+ 引脚

🟢 绿色LED线:  
   树莓派 35号脚 (GPIO19) → LED模块 G+ 引脚

🔵 蓝色LED线:
   树莓派 38号脚 (GPIO20) → LED模块 B+ 引脚

⚫ 接地线:
   树莓派 6号脚 (GND) → LED模块 GND 或 - 引脚
```

#### 第三步: 按钮接线
```
🟡 按钮1 (颜色切换):
   一端 → 树莓派 3号脚 (GPIO2)
   另一端 → 树莓派 6号脚 (GND)

🟡 按钮2 (效果切换):
   一端 → 树莓派 5号脚 (GPIO3)  
   另一端 → 树莓派 6号脚 (GND)

🟡 按钮3 (亮度调节):
   一端 → 树莓派 7号脚 (GPIO4)
   另一端 → 树莓派 6号脚 (GND)
```

### 📋 接线检查清单
- [ ] 红色LED → GPIO18 (12号脚)
- [ ] 绿色LED → GPIO19 (35号脚)  
- [ ] 蓝色LED → GPIO20 (38号脚)
- [ ] LED地线 → GND (6号脚)
- [ ] 按钮1 → GPIO2 (3号脚) + GND (6号脚)
- [ ] 按钮2 → GPIO3 (5号脚) + GND (6号脚)
- [ ] 按钮3 → GPIO4 (7号脚) + GND (6号脚)

### ⚠️ 注意事项
1. **断电接线**: 接线时确保树莓派已关机断电
2. **极性检查**: LED模块要确认是共阴极类型
3. **电阻**: 如使用分立LED，需要串联限流电阻(约220Ω-1kΩ)
4. **按钮**: 代码已配置内部上拉电阻，外部无需额外电阻

### 🎨 LED模块类型说明

#### 如果你有4针RGB LED模块:
```
模块标识     →  连接位置
R+ 或 RED   →  GPIO18 (12号脚)
G+ 或 GREEN →  GPIO19 (35号脚)  
B+ 或 BLUE  →  GPIO20 (38号脚)
GND 或 -    →  GND (6号脚)
```

#### 如果你有分立的LED:
每个LED需要串联一个220Ω电阻:
```
GPIO18 → 220Ω电阻 → 红色LED正极 → LED负极 → GND
GPIO19 → 220Ω电阻 → 绿色LED正极 → LED负极 → GND  
GPIO20 → 220Ω电阻 → 蓝色LED正极 → LED负极 → GND
```

## 按钮功能

| 按钮 | 功能 | 说明 |
|------|------|------|
| 按钮1 | 颜色切换 | 循环切换9种预设颜色 |
| 按钮2 | 效果切换 | 循环切换5种LED效果 |
| 按钮3 | 亮度调节 | 循环调节亮度：25% → 50% → 75% → 100% |

## API 接口

### 获取状态
```http
GET /api/led/status
```

### 设置效果
```http
POST /api/led/effect
Content-Type: application/json

{
  "effect": "Breathe",
  "color": {
    "red": 255,
    "green": 0,
    "blue": 0
  },
  "brightness": 100,
  "speed": 1000
}
```

### 快速控制
```http
POST /api/led/off                    # 关闭LED
POST /api/led/color                  # 设置静态颜色
POST /api/led/blink                  # 设置闪烁效果
POST /api/led/breathe                # 设置呼吸效果
POST /api/led/rainbow                # 设置彩虹效果
```

### 获取信息
```http
GET /api/led/colors                  # 获取预定义颜色
GET /api/led/effects                 # 获取可用效果
```

## 运行项目

### 1. 构建和运行
```bash
cd src/Samples/RGBLed.Api
dotnet build
dotnet run
```

### 2. 访问Web界面
打开浏览器访问: `http://localhost:5000`

### 3. API文档
访问: `http://localhost:5000/swagger` (开发环境)

## Web 界面功能

### 状态面板
- LED预览显示当前颜色和效果
- 实时显示当前效果、亮度、速度
- 自动刷新状态

### 效果控制
- 一键切换各种LED效果
- 直观的按钮界面

### 颜色选择
- 9种预定义颜色快速选择
- 颜色按钮实时预览

### 参数调节
- 亮度滑块：1-100%
- 速度滑块：100-5000ms
- 实时显示参数值

### 键盘快捷键
| 按键 | 功能 |
|------|------|
| 0 | 关闭LED |
| 1 | 静态效果 |
| 2 | 闪烁效果 |
| 3 | 呼吸效果 |
| 4 | 彩虹效果 |
| R | 选择红色 |
| G | 选择绿色 |
| B | 选择蓝色 |
| W | 选择白色 |
| 空格 | 刷新状态 |

## 项目结构

```
RGBLed.Api/
├── Controllers/
│   └── LedController.cs          # LED控制API
├── Models/
│   └── LedEffect.cs              # 数据模型
├── Services/
│   └── RgbLedService.cs          # LED硬件控制服务
├── wwwroot/
│   ├── index.html                # Web控制界面
│   └── script.js                 # 前端JavaScript
├── Program.cs                    # 应用程序入口
├── RGBLed.Api.csproj            # 项目配置
└── RGBLed.Api.http              # API测试文件
```

## 技术栈

- **.NET 9.0**: 主框架
- **ASP.NET Core**: Web API
- **System.Device.Gpio**: GPIO控制
- **PWM**: LED亮度控制
- **HTML5/CSS3/JavaScript**: Web界面

## 故障排除

### 权限问题
如果遇到GPIO权限问题，可以：
```bash
sudo chmod 666 /dev/gpiomem
# 或者使用sudo运行程序
sudo dotnet run
```

### PWM不可用
项目会自动检测PWM可用性，如果PWM不可用会回退到GPIO开关模式。

### 按钮无响应
检查按钮连接和上拉电阻配置。

## 扩展功能

### 添加新颜色
在 `LedEffect.cs` 的 `LedColor` 类中添加新的静态颜色属性。

### 添加新效果
1. 在 `LedEffect` 枚举中添加新效果
2. 在 `RgbLedService` 中实现效果逻辑
3. 在Web界面中添加对应按钮

### 自定义GPIO引脚
在服务注册时传入自定义引脚号：
```csharp
builder.Services.AddSingleton(provider => 
    new RgbLedService(redPin: 12, greenPin: 13, bluePin: 14));
```

## 许可证

此项目使用 MIT 许可证。
