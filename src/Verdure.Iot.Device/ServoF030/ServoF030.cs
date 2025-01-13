using System.Device.I2c;

namespace Verdure.Iot.Device.ServoF030
{
    public class ServoF030 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private byte[] i2cTxData = new byte[8];
        private byte[] i2cRxData = new byte[8]; // 假设接收数据长度为5

        public ServoF030(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public void SetJointEnable(JointStatus joint, bool enable)
        {
            i2cTxData[0] = 0xff;
            i2cTxData[1] = enable ? (byte)1 : (byte)0;

            if (TransmitAndReceiveI2cPacket(joint.Id))
            {
                joint.Angle = BitConverter.ToSingle(i2cRxData, 1);
            }
            else
            {
                // 处理传输失败的情况
                Console.WriteLine($"Failed to communicate with joint {joint.Id}");
            }
        }

        public void UpdateServoAngle(JointStatus joint, float angleSetPoint)
        {
            if (angleSetPoint >= joint.AngleMin && angleSetPoint <= joint.AngleMax)
            {
                byte[] angleBytes = BitConverter.GetBytes(angleSetPoint);

                i2cTxData[0] = 0x01;
                Array.Copy(angleBytes, 0, i2cTxData, 1, angleBytes.Length);

                if (TransmitAndReceiveI2cPacket(joint.Id))
                {
                    joint.Angle = BitConverter.ToSingle(i2cRxData, 1);
                }
                else
                {
                    joint.Angle = 0;
                }
            }
        }

        public void GetServoAngle(JointStatus joint)
        {
            i2cTxData[0] = 0x11;

            if (TransmitAndReceiveI2cPacket(joint.Id))
            {
                joint.Angle = BitConverter.ToSingle(i2cRxData, 1);
            }
            else
            {
                joint.Angle = 0;
            }
        }

        private bool TransmitAndReceiveI2cPacket(int jointId)
        {
            int retryCount = 2;
            bool success = false;

            Console.WriteLine($"adress:{_i2cDevice.ConnectionSettings.DeviceAddress}");


            // 发送数据
            while (retryCount > 0)
            {
                try
                {
                    _i2cDevice.Write(i2cTxData);
                    Console.WriteLine($"I2C write to joint {jointId} success");
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Message);
                    retryCount--;
                }
            }

            if (!success)
            {
                JointsConnectionStatusChange(jointId, false);
                Console.WriteLine($"I2C write to joint {jointId} failed");
                return false;
            }

            JointsConnectionStatusChange(jointId, true);

            // 接收数据
            retryCount = 2;
            success = false;
            while (retryCount > 0)
            {
                try
                {
                    _i2cDevice.Read(i2cRxData);
                    Console.WriteLine("read ok");
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Message);
                    retryCount--;
                }
            }

            if (!success)
            {
                JointsConnectionStatusChange(jointId, false);
                Console.WriteLine($"I2C read from joint {jointId} failed");
                return false;
            }

            JointsConnectionStatusChange(jointId, true);
            return true;
        }

        private void JointsConnectionStatusChange(int jointId, bool status)
        {
            // 更新关节连接状态的逻辑
            Console.WriteLine($"Joint {jointId} connection status: {(status ? "Connected" : "Disconnected")}");
        }

        public void Dispose()
        {
            _i2cDevice?.Dispose();
        }
    }

    public class JointStatus
    {
        public int Id { get; set; }
        public float Angle { get; set; }
        public float AngleMin { get; set; }
        public float AngleMax { get; set; }
    }
}
