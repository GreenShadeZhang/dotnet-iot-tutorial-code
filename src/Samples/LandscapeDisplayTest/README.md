# ST7789Display 1.47寸屏幕横屏显示问题修复

## 问题分析

### 用户反馈的问题
- 1.47寸屏幕横屏模式上部分有几十像素的空白区域
- 这个空白区域正好对应之前添加的34像素偏移

### 根本原因
1. **偏移量错误应用**: 在横屏模式下错误地应用了Y轴偏移，导致显示内容向下偏移34像素
2. **坐标系统混乱**: 竖屏和横屏模式应该使用不同的偏移策略
3. **MADCTL配置**: 需要正确的MADCTL值来实现真正的横屏显示

## 修复方案

### 核心修改

#### 1. 偏移量策略优化
```csharp
case DisplayType.Display147Inch:
    if (isLandscape)
    {
        // 横屏模式：320x172 - 不需要任何偏移
        _width = 320;
        _height = 172;
        _xOffset = 0;   
        _yOffset = 0;
    }
    else
    {
        // 竖屏模式：172x320 - 只需要X偏移
        _width = 172;
        _height = 320;
        _xOffset = 34;  
        _yOffset = 0;
    }
    break;
```

#### 2. SetAddressWindow地址窗口优化
```csharp
case DisplayType.Display147Inch:
    if (_width == 320 && _height == 172)
    {
        // 横屏模式 - 直接使用坐标，无偏移
        SendCommand(0x2A);
        SendData((byte)(x0 >> 8));
        SendData((byte)(x0 & 0xff));
        SendData((byte)((x1 - 1) >> 8));
        SendData((byte)((x1 - 1) & 0xff));
        // Y坐标也无偏移
    }
    else
    {
        // 竖屏模式 - X坐标加34偏移
        SendData((byte)((x0 + 34) & 0xff));
        // ...
    }
    break;
```

#### 3. MADCTL正确配置
```csharp
SendCommand(0x36);    // MADCTL: Memory Data Access Control
if (isLandscape)
{
    SendData(0x60);   // 横屏模式：MY=0, MX=1, MV=1
}
else
{
    SendData(0x00);   // 竖屏模式：MY=0, MX=0, MV=0
}
```

## 测试验证

### 修复前的问题
- 横屏模式上方有34像素空白区域
- 实际显示区域只有 320x138 (172-34)

### 修复后的效果  
- 横屏模式完全填满 320x172 区域
- 无任何空白区域
- 显示内容完整

### 测试程序说明
运行 `LandscapeDisplayTest` 来验证修复效果：

```bash
cd src/Samples/LandscapeDisplayTest
dotnet run
```

测试步骤：
1. 先测试竖屏模式 (172x320) - 验证竖屏功能正常
2. 再测试横屏模式 (320x172) - 验证无空白区域
3. 循环显示不同颜色，确认整个屏幕区域都被正确填充

## 技术细节

### 偏移量说明
- **竖屏模式**: 需要X偏移34像素，因为1.47寸屏幕在竖屏时有硬件限制
- **横屏模式**: 不需要任何偏移，因为旋转后的坐标系统已经对齐

### MADCTL位定义
- `MY` (bit 7): Page Address Order
- `MX` (bit 6): Column Address Order  
- `MV` (bit 5): Page/Column Order
- `0x60 = 0110 0000`: MX=1, MV=1 实现90度旋转

### 关键洞察
1.47寸ST7789屏幕的34像素偏移只在特定方向需要，不是所有情况都需要。横屏模式通过MADCTL旋转后，坐标系统已经重新映射，无需额外偏移。

## 使用方法

### 无空白区域的横屏显示
```csharp
var display = new ST7789Display(
    settings, gpio, true,
    dcPin: 25, resetPin: 27,
    displayType: DisplayType.Display147Inch,
    isLandscape: true  // 关键：横屏模式
);

// 现在填充整个屏幕，无空白区域
display.FillScreen(0xF800); // 红色填满整个320x172区域
```

### 兼容性保证
- 竖屏模式 (`isLandscape: false`) 行为完全不变
- 原有代码无需修改
- 向后完全兼容
