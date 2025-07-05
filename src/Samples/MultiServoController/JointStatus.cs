using System.Device.I2c;

namespace MultiServoController
{
    /// <summary>
    /// 关节状态类，包含关节的基本信息和角度映射
    /// </summary>
    public class JointStatus
    {
        public int Id { get; set; }
        public float ServoAngleMin { get; set; }
        public float ServoAngleMax { get; set; }
        public float CurrentServoAngle { get; set; }
        public float ModelAngleMin { get; set; }
        public float ModelAngleMax { get; set; }
        public float CurrentModelAngle { get; set; }
        public bool IsInverted { get; set; } = false;
        public string Name { get; set; } = "";
        public int I2cAddress { get; set; }

        /// <summary>
        /// 将模型角度转换为舵机角度
        /// </summary>
        /// <param name="modelAngle">模型角度</param>
        /// <returns>舵机角度</returns>
        public float ConvertModelAngleToServoAngle(float modelAngle)
        {
            // 限制模型角度在有效范围内
            modelAngle = Math.Max(ModelAngleMin, Math.Min(ModelAngleMax, modelAngle));

            float servoAngle;

            if (IsInverted)
            {
                // 反向映射：模型角度增大时，舵机角度减小
                servoAngle = (modelAngle - ModelAngleMin) / 
                           (ModelAngleMax - ModelAngleMin) * 
                           (ServoAngleMin - ServoAngleMax) + ServoAngleMax;
            }
            else
            {
                // 正向映射：模型角度增大时，舵机角度增大
                servoAngle = (modelAngle - ModelAngleMin) / 
                           (ModelAngleMax - ModelAngleMin) * 
                           (ServoAngleMax - ServoAngleMin) + ServoAngleMin;
            }

            CurrentModelAngle = modelAngle;
            CurrentServoAngle = servoAngle;
            
            return servoAngle;
        }

        /// <summary>
        /// 将舵机角度转换为模型角度
        /// </summary>
        /// <param name="servoAngle">舵机角度</param>
        /// <returns>模型角度</returns>
        public float ConvertServoAngleToModelAngle(float servoAngle)
        {
            float modelAngle;

            if (IsInverted)
            {
                modelAngle = (ServoAngleMax - servoAngle) / 
                           (ServoAngleMax - ServoAngleMin) * 
                           (ModelAngleMax - ModelAngleMin) + ModelAngleMin;
            }
            else
            {
                modelAngle = (servoAngle - ServoAngleMin) / 
                           (ServoAngleMax - ServoAngleMin) * 
                           (ModelAngleMax - ModelAngleMin) + ModelAngleMin;
            }

            CurrentServoAngle = servoAngle;
            CurrentModelAngle = modelAngle;
            
            return modelAngle;
        }
    }
}
