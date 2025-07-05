using MultiServoController;

Console.WriteLine("=== 多舵机机器人控制器演示程序 ===");
Console.WriteLine();
Console.WriteLine("请选择测试模式:");
Console.WriteLine("1. 简单测试模式 (仅测试0x02和0x03两个I2C地址)");
Console.WriteLine("2. 完整控制模式 (多关节控制)");
Console.Write("请输入选择 (1或2): ");

var choice = Console.ReadLine()?.Trim();

if (choice == "1")
{
    // 简单测试模式
    Console.WriteLine("\n=== 简单测试模式 ===");
    try
    {
        using var simpleRobot = new SimpleRobotController();
        
        Console.WriteLine("\n按Enter开始角度扫描测试，或输入'q'退出...");
        var input = Console.ReadLine();
        
        if (input?.ToLower() != "q")
        {
            simpleRobot.PerformAngleSweep();
        }
        
        Console.WriteLine("简单测试完成");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"简单测试运行错误: {ex.Message}");
    }
}
else
{
    // 完整控制模式
    Console.WriteLine("\n=== 完整控制模式 ===");
    try
    {
        using var robot = new RobotController();
        
        Console.WriteLine("机器人控制器初始化完成");
        Console.WriteLine();

        // 显示所有关节信息
        var joints = robot.GetAllJoints();
        Console.WriteLine("关节配置信息:");
        foreach (var joint in joints.Values)
        {
            Console.WriteLine($"ID: {joint.Id:2} | {joint.Name,-8} | 模型角度: {joint.ModelAngleMin,4}° ~ {joint.ModelAngleMax,4}° | " +
                             $"舵机角度: {joint.ServoAngleMin,5:F1}° ~ {joint.ServoAngleMax,5:F1}° | 反向: {joint.IsInverted}");
        }
        Console.WriteLine();

        // 启用所有关节
        Console.WriteLine("启用所有关节...");
        foreach (var jointId in joints.Keys)
        {
            robot.SetJointEnable(jointId, true);
            Thread.Sleep(100);
        }
        Console.WriteLine();

        // 执行预定义动作
        Console.WriteLine("执行初始化动作...");
        robot.PerformAction("初始化");
        Thread.Sleep(2000);

        Console.WriteLine("执行点头动作...");
        robot.PerformAction("点头");
        Thread.Sleep(1000);

        Console.WriteLine("执行挥手动作...");
        robot.PerformAction("挥手");
        Thread.Sleep(1000);

        Console.WriteLine("执行旋转动作...");
        robot.PerformAction("旋转");
        Thread.Sleep(1000);

        // 交互式控制
        Console.WriteLine();
        Console.WriteLine("=== 交互式控制模式 ===");
        Console.WriteLine("输入格式: [关节ID] [模型角度]");
        Console.WriteLine("例如: 2 10 (将头部设置为10度)");
        Console.WriteLine("输入 'exit' 退出程序");
        Console.WriteLine("输入 'action [动作名]' 执行预定义动作");
        Console.WriteLine("可用动作: 初始化, 点头, 挥手, 旋转");
        Console.WriteLine();

        while (true)
        {
            Console.Write("请输入命令: ");
            var input = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(input))
                continue;

            if (input.ToLower() == "exit")
                break;

            if (input.ToLower().StartsWith("action "))
            {
                var actionName = input.Substring(7);
                robot.PerformAction(actionName);
                continue;
            }

            var parts = input.Split(' ');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int jointId) && float.TryParse(parts[1], out float angle))
                {
                    if (joints.ContainsKey(jointId))
                    {
                        var joint = joints[jointId];
                        if (angle >= joint.ModelAngleMin && angle <= joint.ModelAngleMax)
                        {
                            robot.SetJointModelAngle(jointId, angle);
                            
                            // 读取实际角度
                            Thread.Sleep(50);
                            var actualAngle = robot.GetJointModelAngle(jointId);
                            Console.WriteLine($"关节 {joint.Name} 设置完成，实际角度: {actualAngle:F2}°");
                        }
                        else
                        {
                            Console.WriteLine($"角度超出范围！关节 {joint.Name} 的有效范围: {joint.ModelAngleMin}° ~ {joint.ModelAngleMax}°");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"关节 ID {jointId} 不存在！");
                    }
                }
                else
                {
                    Console.WriteLine("输入格式错误！请输入: [关节ID] [角度]");
                }
            }
            else
            {
                Console.WriteLine("输入格式错误！请输入: [关节ID] [角度] 或 'action [动作名]'");
            }
        }

        Console.WriteLine("程序退出");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"完整控制模式运行错误: {ex.Message}");
        Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
    }
}
