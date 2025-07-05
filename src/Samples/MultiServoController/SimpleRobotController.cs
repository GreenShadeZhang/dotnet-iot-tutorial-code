using System.Device.I2c;

namespace MultiServoController
{
    /// <summary>
    /// 简化的双舵机测试控制器
    /// </summary>
    public class SimpleRobotController : IDisposable
    {
        private readonly I2cDevice _i2cDevice1; // 地址 0x02
        private readonly I2cDevice _i2cDevice2; // 地址 0x03
        private readonly byte[] _i2cTxData = new byte[5];
        private readonly byte[] _i2cRxData = new byte[5];

        public SimpleRobotController()
        {
            try
            {
                _i2cDevice1 = I2cDevice.Create(new I2cConnectionSettings(1, 0x02));
                Console.WriteLine("I2C设备1 (0x02) 初始化成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"I2C设备1 (0x02) 初始化失败: {ex.Message}");
            }

            try
            {
                _i2cDevice2 = I2cDevice.Create(new I2cConnectionSettings(1, 0x03));
                Console.WriteLine("I2C设备2 (0x03) 初始化成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"I2C设备2 (0x03) 初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启用舵机
        /// </summary>
        public bool EnableServos()
        {
            Console.WriteLine("启用舵机...");
            
            // 准备启用命令
            _i2cTxData[0] = 0xff;
            _i2cTxData[1] = 0x01;
            _i2cTxData[2] = 0x00;
            _i2cTxData[3] = 0x00;
            _i2cTxData[4] = 0x00;

            bool success1 = SendCommand(_i2cDevice1, "设备1");
            bool success2 = SendCommand(_i2cDevice2, "设备2");

            return success1 && success2;
        }

        /// <summary>
        /// 设置舵机角度
        /// </summary>
        public bool SetServoAngle(float angle)
        {
            Console.WriteLine($"设置舵机角度: {angle}°");
            
            // 准备角度命令
            byte[] angleBytes = BitConverter.GetBytes(angle);
            _i2cTxData[0] = 0x01;
            Array.Copy(angleBytes, 0, _i2cTxData, 1, angleBytes.Length);

            bool success1 = SendCommand(_i2cDevice1, "设备1");
            bool success2 = SendCommand(_i2cDevice2, "设备2");

            return success1 && success2;
        }

        /// <summary>
        /// 执行角度扫描测试
        /// </summary>
        public void PerformAngleSweep()
        {
            Console.WriteLine("开始角度扫描测试...");
            
            // 启用舵机
            if (!EnableServos())
            {
                Console.WriteLine("舵机启用失败，停止测试");
                return;
            }

            Thread.Sleep(1000);

            // 正向扫描 0° -> 180°
            Console.WriteLine("正向扫描 0° -> 180°");
            for (int i = 0; i <= 180; i += 10)
            {
                SetServoAngle(i);
                Thread.Sleep(200);
            }

            Thread.Sleep(1000);

            // 反向扫描 180° -> 0°
            Console.WriteLine("反向扫描 180° -> 0°");
            for (int i = 180; i >= 0; i -= 10)
            {
                SetServoAngle(i);
                Thread.Sleep(200);
            }

            Console.WriteLine("角度扫描测试完成");
        }

        /// <summary>
        /// 发送I2C命令
        /// </summary>
        private bool SendCommand(I2cDevice device, string deviceName)
        {
            if (device == null)
            {
                Console.WriteLine($"{deviceName} 不可用");
                return false;
            }

            int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    device.WriteRead(_i2cTxData, _i2cRxData);
                    Console.WriteLine($"{deviceName} I2C通信成功");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{deviceName} I2C通信失败: {ex.Message}");
                    retryCount--;
                    if (retryCount > 0)
                    {
                        Thread.Sleep(50);
                    }
                }
            }

            Console.WriteLine($"{deviceName} I2C通信最终失败");
            return false;
        }

        public void Dispose()
        {
            _i2cDevice1?.Dispose();
            _i2cDevice2?.Dispose();
            Console.WriteLine("简单机器人控制器已释放资源");
        }
    }
}
