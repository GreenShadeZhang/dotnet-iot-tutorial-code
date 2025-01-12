using System.Device.I2c;
using UnitsNet;

try
{
    while (true)
    {
        using I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x02));

        using I2cDevice i2c8 = I2cDevice.Create(new I2cConnectionSettings(1, 0x03));

        byte[] writeBuffer = new byte[5] { 0xff, 0x01, 0x00, 0x00, 0x00 };
        byte[] receiveData = new byte[5];

        i2c.WriteRead(writeBuffer, receiveData);

        i2c8.WriteRead(writeBuffer, receiveData);

        byte[] angleBytes = BitConverter.GetBytes(90);

        writeBuffer[0] = 0x01;

        Array.Copy(angleBytes, 0, writeBuffer, 1, angleBytes.Length);

        i2c.WriteRead(writeBuffer, receiveData);
        i2c8.WriteRead(writeBuffer, receiveData);
        Thread.Sleep(20);
        Console.WriteLine($"I2C 2 8 设备连接成功--{DateTime.Now.ToString("s")}");
        foreach (var data in receiveData)
        {
            Console.Write($"{data}, ");
        }
        //Console.WriteLine();
        //Thread.Sleep(500);      
    }
}
catch (Exception ex)
{
    Console.WriteLine($"I2C 设备连接失败: {ex.Message}");
}

Console.ReadLine();