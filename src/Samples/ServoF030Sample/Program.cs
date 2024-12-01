using System.Device.I2c;
using Verdure.Iot.Device.ServoF030;

var angle = new JointStatus()
{
    Id = 2
};
using I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x02));
using var driver = new ServoF030(i2c);
driver.SetJointEnable(angle, true);
Console.WriteLine($"Angle: {angle.Angle}");
driver.UpdateServoAngle(angle, 90);
Console.WriteLine($"Angle: {angle.Angle}");
driver.GetServoAngle(angle);
Console.WriteLine($"Angle: {angle.Angle}");
Console.ReadLine();