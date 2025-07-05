using System.Device.I2c;

namespace MultiServoController
{
    /// <summary>
    /// 多舵机控制器类
    /// </summary>
    public class RobotController : IDisposable
    {
        private readonly Dictionary<int, I2cDevice> _i2cDevices;
        private readonly Dictionary<int, JointStatus> _joints;
        private readonly byte[] _i2cTxData = new byte[5];
        private readonly byte[] _i2cRxData = new byte[5];

        public RobotController()
        {
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
            // 根据您提供的角度配置和BSP参考，恢复正确的I2C地址映射
            _joints[2] = new JointStatus
            {
                Id = 2,
                Name = "头部",
                ServoAngleMin = 70,     // 参考Bsp配置
                ServoAngleMax = 95,
                ModelAngleMin = -15,
                ModelAngleMax = 15,
                IsInverted = true,
                I2cAddress = 0x01       // 头部对应ID2，I2C地址0x01
            };

            _joints[4] = new JointStatus
            {
                Id = 4,
                Name = "左臂展开",
                ServoAngleMin = -9,     // 参考Bsp配置
                ServoAngleMax = 3,
                ModelAngleMin = 0,
                ModelAngleMax = 30,
                IsInverted = false,
                I2cAddress = 0x02       // 左臂展开对应ID4，I2C地址0x02
            };

            _joints[6] = new JointStatus
            {
                Id = 6,
                Name = "左臂旋转",
                ServoAngleMin = -16,    // 参考Bsp配置
                ServoAngleMax = 117,
                ModelAngleMin = 0,
                ModelAngleMax = 180,
                IsInverted = false,
                I2cAddress = 0x03      // 左臂旋转对应ID6，I2C地址0x03
            };

            _joints[8] = new JointStatus
            {
                Id = 8,
                Name = "右臂展开",
                ServoAngleMin = 133,    // 参考Bsp配置
                ServoAngleMax = 141,
                ModelAngleMin = 0,
                ModelAngleMax = 30,
                IsInverted = true,
                I2cAddress = 0x04       // 右臂展开对应ID8，I2C地址0x04
            };

            _joints[10] = new JointStatus
            {
                Id = 10,
                Name = "右臂旋转",
                ServoAngleMin = 15,     // 参考Bsp配置
                ServoAngleMax = 150,
                ModelAngleMin = 0,
                ModelAngleMax = 180,
                IsInverted = true,
                I2cAddress = 0x05       // 右臂旋转对应ID10，I2C地址0x05
            };

            _joints[12] = new JointStatus
            {
                Id = 12,
                Name = "底部旋转",
                ServoAngleMin = 0,      // 参考Bsp配置
                ServoAngleMax = 180,
                ModelAngleMin = -90,
                ModelAngleMax = 90,
                IsInverted = false,
                I2cAddress = 0x06       // 底部旋转对应ID12，I2C地址0x06
            };
        }

        /// <summary>
        /// 初始化I2C设备
        /// </summary>
        private void InitializeI2cDevices()
        {
            // 创建所有需要的I2C设备连接
            var uniqueAddresses = _joints.Values.Select(j => j.I2cAddress).Distinct().OrderBy(x => x).ToList();
            
            foreach (var address in uniqueAddresses)
            {
                try
                {
                    var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, address));
                    _i2cDevices[address] = i2cDevice;
                    Console.WriteLine($"初始化I2C设备地址 0x{address:X2} 成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"初始化I2C设备地址 0x{address:X2} 失败: {ex.Message}");
                }
            }

            Console.WriteLine();
            // 显示关节与I2C设备的映射
            foreach (var joint in _joints.Values.OrderBy(j => j.Id))
            {
                var deviceStatus = _i2cDevices.ContainsKey(joint.I2cAddress) ? "可用" : "不可用";
                Console.WriteLine($"关节 {joint.Name} (ID: {joint.Id}) → I2C地址 0x{joint.I2cAddress:X2} ({deviceStatus})");
            }
        }

        /// <summary>
        /// 启用或禁用指定关节
        /// </summary>
        /// <param name="jointId">关节ID</param>
        /// <param name="enable">是否启用</param>
        public bool SetJointEnable(int jointId, bool enable)
        {
            if (!_joints.ContainsKey(jointId))
            {
                Console.WriteLine($"关节 ID {jointId} 不存在");
                return false;
            }

            var joint = _joints[jointId];
            if (!_i2cDevices.ContainsKey(joint.I2cAddress))
            {
                Console.WriteLine($"关节 {joint.Name} 对应的I2C设备地址 0x{joint.I2cAddress:X2} 不可用");
                return false;
            }

            var device = _i2cDevices[joint.I2cAddress];

            _i2cTxData[0] = 0xff;
            _i2cTxData[1] = enable ? (byte)1 : (byte)0;
            _i2cTxData[2] = 0x00;
            _i2cTxData[3] = 0x00;
            _i2cTxData[4] = 0x00;

            return TransmitAndReceiveI2cPacket(device, joint);
        }

        /// <summary>
        /// 设置关节的模型角度（自动转换为舵机角度）
        /// </summary>
        /// <param name="jointId">关节ID</param>
        /// <param name="modelAngle">模型角度</param>
        public bool SetJointModelAngle(int jointId, float modelAngle)
        {
            if (!_joints.ContainsKey(jointId))
            {
                Console.WriteLine($"关节 ID {jointId} 不存在");
                return false;
            }

            var joint = _joints[jointId];
            if (!_i2cDevices.ContainsKey(joint.I2cAddress))
            {
                Console.WriteLine($"关节 {joint.Name} 对应的I2C设备地址 0x{joint.I2cAddress:X2} 不可用");
                return false;
            }

            var device = _i2cDevices[joint.I2cAddress];

            // 转换模型角度为舵机角度
            float servoAngle = joint.ConvertModelAngleToServoAngle(modelAngle);

            Console.WriteLine($"关节 {joint.Name}: 模型角度 {modelAngle}° → 舵机角度 {servoAngle:F2}°");

            // 发送舵机角度
            byte[] angleBytes = BitConverter.GetBytes(servoAngle);
            _i2cTxData[0] = 0x01;
            Array.Copy(angleBytes, 0, _i2cTxData, 1, angleBytes.Length);

            return TransmitAndReceiveI2cPacket(device, joint);
        }

        /// <summary>
        /// 同时设置多个关节的模型角度
        /// </summary>
        /// <param name="jointAngles">关节ID和对应的模型角度字典</param>
        public void SetMultipleJointAngles(Dictionary<int, float> jointAngles)
        {
            Console.WriteLine($"开始设置 {jointAngles.Count} 个关节角度...");
            
            foreach (var kvp in jointAngles)
            {
                SetJointModelAngle(kvp.Key, kvp.Value);
                Thread.Sleep(10); // 小延时，避免I2C总线冲突
            }
            
            Console.WriteLine("多关节角度设置完成");
        }

        /// <summary>
        /// 获取关节当前角度
        /// </summary>
        /// <param name="jointId">关节ID</param>
        /// <returns>当前模型角度</returns>
        public float GetJointModelAngle(int jointId)
        {
            if (!_joints.ContainsKey(jointId))
            {
                Console.WriteLine($"关节 ID {jointId} 不存在");
                return 0;
            }

            var joint = _joints[jointId];
            if (!_i2cDevices.ContainsKey(joint.I2cAddress))
            {
                Console.WriteLine($"关节 {joint.Name} 对应的I2C设备地址 0x{joint.I2cAddress:X2} 不可用");
                return 0;
            }

            var device = _i2cDevices[joint.I2cAddress];

            _i2cTxData[0] = 0x11; // 读取角度命令
            _i2cTxData[1] = 0x00;
            _i2cTxData[2] = 0x00;
            _i2cTxData[3] = 0x00;
            _i2cTxData[4] = 0x00;

            if (TransmitAndReceiveI2cPacket(device, joint))
            {
                float servoAngle = BitConverter.ToSingle(_i2cRxData, 1);
                return joint.ConvertServoAngleToModelAngle(servoAngle);
            }

            return 0;
        }

        /// <summary>
        /// 获取所有关节信息
        /// </summary>
        /// <returns>关节信息字典</returns>
        public Dictionary<int, JointStatus> GetAllJoints()
        {
            return new Dictionary<int, JointStatus>(_joints);
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
                    //Console.WriteLine($"关节 {joint.Name} I2C通信成功");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"关节 {joint.Name} I2C通信失败: {ex.Message}");
                    retryCount--;
                    Thread.Sleep(10);
                }
            }

            Console.WriteLine($"关节 {joint.Name} I2C通信最终失败");
            return false;
        }

        /// <summary>
        /// 执行预定义的动作序列
        /// </summary>
        public void PerformAction(string actionName)
        {
            Console.WriteLine($"执行动作: {actionName}");

            switch (actionName.ToLower())
            {
                case "初始化":
                    SetMultipleJointAngles(new Dictionary<int, float>
                    {
                        { 2, 0 },    // 头部居中
                        { 4, 0 },    // 左臂收起
                        { 6, 90 },   // 左臂中位
                        { 8, 0 },    // 右臂收起
                        { 10, 90 },  // 右臂中位
                        { 12, 0 }    // 底部居中
                    });
                    break;

                case "挥手":
                    // 右臂挥手动作
                    for (int i = 0; i < 3; i++)
                    {
                        SetJointModelAngle(10, 45);  // 右臂向前
                        Thread.Sleep(500);
                        SetJointModelAngle(10, 135); // 右臂向后
                        Thread.Sleep(500);
                    }
                    SetJointModelAngle(10, 90); // 回到中位
                    break;

                case "点头":
                    // 头部点头动作
                    for (int i = 0; i < 3; i++)
                    {
                        SetJointModelAngle(2, -10);  // 头部向下
                        Thread.Sleep(300);
                        SetJointModelAngle(2, 10);   // 头部向上
                        Thread.Sleep(300);
                    }
                    SetJointModelAngle(2, 0);    // 回到中位
                    break;

                case "旋转":
                    // 底部旋转一圈
                    for (int angle = -90; angle <= 90; angle += 10)
                    {
                        SetJointModelAngle(12, angle);
                        Thread.Sleep(100);
                    }
                    for (int angle = 90; angle >= -90; angle -= 10)
                    {
                        SetJointModelAngle(12, angle);
                        Thread.Sleep(100);
                    }
                    SetJointModelAngle(12, 0); // 回到中位
                    break;

                case "循环测试":
                    // 执行所有关节的循环往复测试
                    PerformAllJointsCycleTest(2, 100);
                    break;

                case "同步测试":
                    // 执行多关节同步循环测试
                    PerformSynchronizedCycleTest(new int[] { 2, 4, 6, 8, 10, 12 }, 2, 150);
                    break;

                case "波浪测试":
                    // 执行波浪式循环测试
                    PerformWaveCycleTest(3, 200);
                    break;

                case "头部测试":
                    // 测试头部关节
                    PerformJointCycleTest(2, 3, 100);
                    break;

                case "手臂测试":
                    // 测试手臂关节
                    PerformSynchronizedCycleTest(new int[] { 4, 6, 8, 10 }, 2, 200);
                    break;

                default:
                    Console.WriteLine($"未知动作: {actionName}");
                    break;
            }
        }

        /// <summary>
        /// 执行单个关节的循环往复测试
        /// </summary>
        /// <param name="jointId">关节ID</param>
        /// <param name="cycles">循环次数</param>
        /// <param name="stepDelay">每步延时（毫秒）</param>
        public void PerformJointCycleTest(int jointId, int cycles = 3, int stepDelay = 100)
        {
            if (!_joints.ContainsKey(jointId))
            {
                Console.WriteLine($"关节 ID {jointId} 不存在");
                return;
            }

            var joint = _joints[jointId];
            if (!_i2cDevices.ContainsKey(joint.I2cAddress))
            {
                Console.WriteLine($"关节 {joint.Name} 对应的I2C设备不可用");
                return;
            }

            Console.WriteLine($"开始关节 {joint.Name} (ID: {jointId}) 循环往复测试，共 {cycles} 个循环");
            Console.WriteLine($"角度范围: {joint.ModelAngleMin}° ~ {joint.ModelAngleMax}°");

            for (int cycle = 1; cycle <= cycles; cycle++)
            {
                Console.WriteLine($"第 {cycle}/{cycles} 个循环...");
                
                // 从最小角度到最大角度
                float angleRange = joint.ModelAngleMax - joint.ModelAngleMin;
                int steps = Math.Max(10, (int)(angleRange / 5)); // 至少10步，每5度一步
                
                for (int step = 0; step <= steps; step++)
                {
                    float angle = joint.ModelAngleMin + (angleRange * step / steps);
                    SetJointModelAngle(jointId, angle);
                    Thread.Sleep(stepDelay);
                }
                
                // 从最大角度回到最小角度
                for (int step = steps - 1; step >= 0; step--)
                {
                    float angle = joint.ModelAngleMin + (angleRange * step / steps);
                    SetJointModelAngle(jointId, angle);
                    Thread.Sleep(stepDelay);
                }
                
                Thread.Sleep(500); // 循环间隔
            }
            
            // 回到中位
            float centerAngle = (joint.ModelAngleMin + joint.ModelAngleMax) / 2;
            SetJointModelAngle(jointId, centerAngle);
            Console.WriteLine($"关节 {joint.Name} 循环测试完成，回到中位: {centerAngle:F1}°");
        }

        /// <summary>
        /// 执行所有可用关节的循环往复测试
        /// </summary>
        /// <param name="cycles">每个关节的循环次数</param>
        /// <param name="stepDelay">每步延时（毫秒）</param>
        public void PerformAllJointsCycleTest(int cycles = 2, int stepDelay = 150)
        {
            Console.WriteLine("=== 开始所有关节循环往复测试 ===");
            
            var availableJoints = _joints.Values
                .Where(j => _i2cDevices.ContainsKey(j.I2cAddress))
                .OrderBy(j => j.Id)
                .ToList();
            
            if (!availableJoints.Any())
            {
                Console.WriteLine("没有可用的关节设备");
                return;
            }
            
            Console.WriteLine($"发现 {availableJoints.Count} 个可用关节");
            
            foreach (var joint in availableJoints)
            {
                Console.WriteLine();
                PerformJointCycleTest(joint.Id, cycles, stepDelay);
                Thread.Sleep(1000); // 关节间隔
            }
            
            Console.WriteLine("\n=== 所有关节循环测试完成 ===");
        }

        /// <summary>
        /// 执行同步多关节循环测试
        /// </summary>
        /// <param name="jointIds">要测试的关节ID列表</param>
        /// <param name="cycles">循环次数</param>
        /// <param name="stepDelay">每步延时（毫秒）</param>
        public void PerformSynchronizedCycleTest(int[] jointIds, int cycles = 2, int stepDelay = 200)
        {
            var testJoints = jointIds
                .Where(id => _joints.ContainsKey(id) && _i2cDevices.ContainsKey(_joints[id].I2cAddress))
                .Select(id => _joints[id])
                .ToList();
            
            if (!testJoints.Any())
            {
                Console.WriteLine("没有可用的测试关节");
                return;
            }
            
            Console.WriteLine($"=== 开始同步多关节循环测试 ===");
            Console.WriteLine($"测试关节: {string.Join(", ", testJoints.Select(j => $"{j.Name}(ID:{j.Id})"))}");
            
            for (int cycle = 1; cycle <= cycles; cycle++)
            {
                Console.WriteLine($"\n第 {cycle}/{cycles} 个同步循环...");
                
                // 计算每个关节的步数（取最大值确保同步）
                int maxSteps = testJoints.Max(j => Math.Max(10, (int)((j.ModelAngleMax - j.ModelAngleMin) / 5)));
                
                // 正向同步运动
                Console.WriteLine("正向同步运动...");
                for (int step = 0; step <= maxSteps; step++)
                {
                    var angleCommands = new Dictionary<int, float>();
                    
                    foreach (var joint in testJoints)
                    {
                        float progress = (float)step / maxSteps;
                        float angle = joint.ModelAngleMin + (joint.ModelAngleMax - joint.ModelAngleMin) * progress;
                        angleCommands[joint.Id] = angle;
                    }
                    
                    SetMultipleJointAngles(angleCommands);
                    Thread.Sleep(stepDelay);
                }
                
                Thread.Sleep(300);
                
                // 反向同步运动
                Console.WriteLine("反向同步运动...");
                for (int step = maxSteps - 1; step >= 0; step--)
                {
                    var angleCommands = new Dictionary<int, float>();
                    
                    foreach (var joint in testJoints)
                    {
                        float progress = (float)step / maxSteps;
                        float angle = joint.ModelAngleMin + (joint.ModelAngleMax - joint.ModelAngleMin) * progress;
                        angleCommands[joint.Id] = angle;
                    }
                    
                    SetMultipleJointAngles(angleCommands);
                    Thread.Sleep(stepDelay);
                }
                
                Thread.Sleep(500); // 循环间隔
            }
            
            // 所有关节回到中位
            Console.WriteLine("所有关节回到中位...");
            var centerCommands = new Dictionary<int, float>();
            foreach (var joint in testJoints)
            {
                float centerAngle = (joint.ModelAngleMin + joint.ModelAngleMax) / 2;
                centerCommands[joint.Id] = centerAngle;
            }
            SetMultipleJointAngles(centerCommands);
            
            Console.WriteLine("=== 同步多关节循环测试完成 ===");
        }

        /// <summary>
        /// 执行波浪式循环测试（关节依次运动）
        /// </summary>
        /// <param name="cycles">波浪循环次数</param>
        /// <param name="waveDelay">波浪延时（毫秒）</param>
        public void PerformWaveCycleTest(int cycles = 3, int waveDelay = 300)
        {
            var availableJoints = _joints.Values
                .Where(j => _i2cDevices.ContainsKey(j.I2cAddress))
                .OrderBy(j => j.Id)
                .ToList();
            
            if (availableJoints.Count < 2)
            {
                Console.WriteLine("需要至少2个可用关节才能执行波浪测试");
                return;
            }
            
            Console.WriteLine("=== 开始波浪式循环测试 ===");
            Console.WriteLine($"参与关节: {string.Join(", ", availableJoints.Select(j => j.Name))}");
            
            for (int cycle = 1; cycle <= cycles; cycle++)
            {
                Console.WriteLine($"\n第 {cycle}/{cycles} 个波浪循环...");
                
                // 正向波浪
                Console.WriteLine("正向波浪...");
                foreach (var joint in availableJoints)
                {
                    SetJointModelAngle(joint.Id, joint.ModelAngleMax);
                    Thread.Sleep(waveDelay);
                    SetJointModelAngle(joint.Id, (joint.ModelAngleMin + joint.ModelAngleMax) / 2);
                }
                
                Thread.Sleep(500);
                
                // 反向波浪
                Console.WriteLine("反向波浪...");
                foreach (var joint in availableJoints.AsEnumerable().Reverse())
                {
                    SetJointModelAngle(joint.Id, joint.ModelAngleMin);
                    Thread.Sleep(waveDelay);
                    SetJointModelAngle(joint.Id, (joint.ModelAngleMin + joint.ModelAngleMax) / 2);
                }
                
                Thread.Sleep(1000); // 循环间隔
            }
            
            Console.WriteLine("=== 波浪式循环测试完成 ===");
        }

        public void Dispose()
        {
            foreach (var device in _i2cDevices.Values)
            {
                device?.Dispose();
            }
            _i2cDevices.Clear();
            Console.WriteLine("机器人控制器已释放资源");
        }
    }
}
