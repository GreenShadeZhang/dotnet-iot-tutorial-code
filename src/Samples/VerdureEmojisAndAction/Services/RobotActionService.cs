using System.Device.I2c;
using VerdureEmojisAndAction.Models;

namespace VerdureEmojisAndAction.Services;

/// <summary>
/// 机器人动作控制服务
/// </summary>
public class RobotActionService : IDisposable
{
    private readonly Dictionary<int, I2cDevice> _i2cDevices;
    private readonly Dictionary<int, JointStatus> _joints;
    private readonly byte[] _i2cTxData = new byte[5];
    private readonly byte[] _i2cRxData = new byte[5];
    private readonly ILogger<RobotActionService> _logger;

    public RobotActionService(ILogger<RobotActionService> logger)
    {
        _logger = logger;
        _i2cDevices = new Dictionary<int, I2cDevice>();
        _joints = new Dictionary<int, JointStatus>();
        
        InitializeJoints();
        InitializeI2cDevices();
    }

    /// <summary>
    /// 初始化关节配置
    /// </summary>
    private void InitializeJoints()
    {
        _joints[2] = new JointStatus
        {
            Id = 2,
            Name = "头部",
            ServoAngleMin = 70,
            ServoAngleMax = 95,
            ModelAngleMin = -15,
            ModelAngleMax = 15,
            IsInverted = true,
            I2cAddress = 0x01
        };

        _joints[4] = new JointStatus
        {
            Id = 4,
            Name = "左臂展开",
            ServoAngleMin = 30,
            ServoAngleMax = 90,
            ModelAngleMin = 0,
            ModelAngleMax = 30,
            IsInverted = false,
            I2cAddress = 0x02
        };

        _joints[6] = new JointStatus
        {
            Id = 6,
            Name = "左臂旋转",
            ServoAngleMin = 0,
            ServoAngleMax = 180,
            ModelAngleMin = 0,
            ModelAngleMax = 180,
            IsInverted = false,
            I2cAddress = 0x03
        };

        _joints[8] = new JointStatus
        {
            Id = 8,
            Name = "右臂展开",
            ServoAngleMin = 120,
            ServoAngleMax = 180,
            ModelAngleMin = 0,
            ModelAngleMax = 30,
            IsInverted = true,
            I2cAddress = 0x04
        };

        _joints[10] = new JointStatus
        {
            Id = 10,
            Name = "右臂旋转",
            ServoAngleMin = 0,
            ServoAngleMax = 180,
            ModelAngleMin = 0,
            ModelAngleMax = 180,
            IsInverted = true,
            I2cAddress = 0x05
        };

        _joints[12] = new JointStatus
        {
            Id = 12,
            Name = "底部旋转",
            ServoAngleMin = 0,
            ServoAngleMax = 180,
            ModelAngleMin = -90,
            ModelAngleMax = 90,
            IsInverted = false,
            I2cAddress = 0x06
        };
    }

    /// <summary>
    /// 初始化I2C设备
    /// </summary>
    private void InitializeI2cDevices()
    {
        var uniqueAddresses = _joints.Values.Select(j => j.I2cAddress).Distinct().OrderBy(x => x).ToList();
        
        foreach (var address in uniqueAddresses)
        {
            try
            {
                var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, address));
                _i2cDevices[address] = i2cDevice;
                _logger.LogInformation($"初始化I2C设备地址 0x{address:X2} 成功");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"初始化I2C设备地址 0x{address:X2} 失败: {ex.Message}");
            }
        }

        // 显示关节与I2C设备的映射
        foreach (var joint in _joints.Values.OrderBy(j => j.Id))
        {
            var deviceStatus = _i2cDevices.ContainsKey(joint.I2cAddress) ? "可用" : "不可用";
            _logger.LogInformation($"关节 {joint.Name} (ID: {joint.Id}) → I2C地址 0x{joint.I2cAddress:X2} ({deviceStatus})");
        }
    }

    /// <summary>
    /// 启用或禁用指定关节
    /// </summary>
    /// <param name="jointId">关节ID</param>
    /// <param name="enable">是否启用</param>
    public async Task<bool> SetJointEnableAsync(int jointId, bool enable, CancellationToken cancellationToken = default)
    {
        if (!_joints.ContainsKey(jointId))
        {
            _logger.LogWarning($"关节 ID {jointId} 不存在");
            return false;
        }

        var joint = _joints[jointId];
        if (!_i2cDevices.ContainsKey(joint.I2cAddress))
        {
            _logger.LogWarning($"关节 {joint.Name} 对应的I2C设备地址 0x{joint.I2cAddress:X2} 不可用");
            return false;
        }

        var device = _i2cDevices[joint.I2cAddress];

        _i2cTxData[0] = 0xff;
        _i2cTxData[1] = enable ? (byte)1 : (byte)0;
        _i2cTxData[2] = 0x00;
        _i2cTxData[3] = 0x00;
        _i2cTxData[4] = 0x00;

        return await Task.Run(() => TransmitAndReceiveI2cPacket(device, joint), cancellationToken);
    }

    /// <summary>
    /// 设置关节的模型角度（自动转换为舵机角度）
    /// </summary>
    /// <param name="jointId">关节ID</param>
    /// <param name="modelAngle">模型角度</param>
    public async Task<bool> SetJointModelAngleAsync(int jointId, float modelAngle, CancellationToken cancellationToken = default)
    {
        if (!_joints.ContainsKey(jointId))
        {
            _logger.LogWarning($"关节 ID {jointId} 不存在");
            return false;
        }

        var joint = _joints[jointId];
        if (!_i2cDevices.ContainsKey(joint.I2cAddress))
        {
            _logger.LogWarning($"关节 {joint.Name} 对应的I2C设备地址 0x{joint.I2cAddress:X2} 不可用");
            return false;
        }

        var device = _i2cDevices[joint.I2cAddress];

        // 转换模型角度为舵机角度
        float servoAngle = joint.ConvertModelAngleToServoAngle(modelAngle);

        _logger.LogDebug($"关节 {joint.Name}: 模型角度 {modelAngle}° → 舵机角度 {servoAngle:F2}°");

        // 发送舵机角度
        byte[] angleBytes = BitConverter.GetBytes(servoAngle);
        _i2cTxData[0] = 0x01;
        Array.Copy(angleBytes, 0, _i2cTxData, 1, angleBytes.Length);

        return await Task.Run(() => TransmitAndReceiveI2cPacket(device, joint), cancellationToken);
    }

    /// <summary>
    /// 同时设置多个关节的模型角度
    /// </summary>
    /// <param name="jointAngles">关节ID和对应的模型角度字典</param>
    public async Task SetMultipleJointAnglesAsync(Dictionary<int, float> jointAngles, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug($"开始设置 {jointAngles.Count} 个关节角度...");
        
        foreach (var kvp in jointAngles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SetJointModelAngleAsync(kvp.Key, kvp.Value, cancellationToken);
            await Task.Delay(10, cancellationToken); // 小延时，避免I2C总线冲突
        }
        
        _logger.LogDebug("多关节角度设置完成");
    }

    /// <summary>
    /// 执行预定义的动作序列
    /// </summary>
    public async Task PerformActionAsync(EmotionType emotionType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"执行动作: {emotionType}");

        switch (emotionType)
        {
            case EmotionType.Anger:
                await PerformAngerActionAsync(cancellationToken);
                break;

            case EmotionType.Happy:
                await PerformHappyActionAsync(cancellationToken);
                break;

            default:
                _logger.LogWarning($"未知动作类型: {emotionType}");
                break;
        }
    }

    /// <summary>
    /// 执行愤怒动作
    /// </summary>
    private async Task PerformAngerActionAsync(CancellationToken cancellationToken)
    {
        // 愤怒动作：手臂张开，头部晃动
        await SetMultipleJointAnglesAsync(new Dictionary<int, float>
        {
            { 4, 25 },    // 左臂张开
            { 8, 25 },    // 右臂张开
            { 6, 45 },    // 左臂前伸
            { 10, 135 },  // 右臂前伸
        }, cancellationToken);

        await Task.Delay(300, cancellationToken);

        // 头部左右晃动
        for (int i = 0; i < 3; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SetJointModelAngleAsync(2, -12, cancellationToken);
            await Task.Delay(200, cancellationToken);
            await SetJointModelAngleAsync(2, 12, cancellationToken);
            await Task.Delay(200, cancellationToken);
        }

        // 回到中位
        await SetMultipleJointAnglesAsync(new Dictionary<int, float>
        {
            { 2, 0 },    // 头部居中
            { 4, 0 },    // 左臂收起
            { 6, 90 },   // 左臂中位
            { 8, 0 },    // 右臂收起
            { 10, 90 },  // 右臂中位
        }, cancellationToken);
    }

    /// <summary>
    /// 执行快乐动作
    /// </summary>
    private async Task PerformHappyActionAsync(CancellationToken cancellationToken)
    {
        // 快乐动作：挥手，点头
        // 右臂挥手动作
        for (int i = 0; i < 3; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SetJointModelAngleAsync(10, 45, cancellationToken);  // 右臂向前
            await Task.Delay(300, cancellationToken);
            await SetJointModelAngleAsync(10, 135, cancellationToken); // 右臂向后
            await Task.Delay(300, cancellationToken);
        }

        // 头部点头动作
        for (int i = 0; i < 2; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SetJointModelAngleAsync(2, -8, cancellationToken);  // 头部向下
            await Task.Delay(250, cancellationToken);
            await SetJointModelAngleAsync(2, 8, cancellationToken);   // 头部向上
            await Task.Delay(250, cancellationToken);
        }

        // 回到中位
        await SetMultipleJointAnglesAsync(new Dictionary<int, float>
        {
            { 2, 0 },    // 头部居中
            { 10, 90 },  // 右臂中位
        }, cancellationToken);
    }

    /// <summary>
    /// 初始化动作 - 所有关节回到初始位置
    /// </summary>
    public async Task InitializePositionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("执行初始化动作");

        // 启用所有关节
        foreach (var jointId in _joints.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SetJointEnableAsync(jointId, true, cancellationToken);
            await Task.Delay(100, cancellationToken);
        }

        // 设置初始位置
        await SetMultipleJointAnglesAsync(new Dictionary<int, float>
        {
            { 2, 0 },    // 头部居中
            { 4, 0 },    // 左臂收起
            { 6, 90 },   // 左臂中位
            { 8, 0 },    // 右臂收起
            { 10, 90 },  // 右臂中位
            { 12, 0 }    // 底部居中
        }, cancellationToken);
    }

    /// <summary>
    /// I2C通信底层实现
    /// </summary>
    private bool TransmitAndReceiveI2cPacket(I2cDevice device, JointStatus joint)
    {
        int retryCount = 3;
        
        while (retryCount > 0)
        {
            try
            {
                device.WriteRead(_i2cTxData, _i2cRxData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"关节 {joint.Name} I2C通信失败: {ex.Message}");
                retryCount--;
                Thread.Sleep(10);
            }
        }

        _logger.LogError($"关节 {joint.Name} I2C通信最终失败");
        return false;
    }

    /// <summary>
    /// 获取所有关节信息
    /// </summary>
    /// <returns>关节信息字典</returns>
    public Dictionary<int, JointStatus> GetAllJoints()
    {
        return new Dictionary<int, JointStatus>(_joints);
    }

    public void Dispose()
    {
        foreach (var device in _i2cDevices.Values)
        {
            device?.Dispose();
        }
        _i2cDevices.Clear();
        _logger.LogInformation("机器人动作控制服务已释放资源");
    }
}
