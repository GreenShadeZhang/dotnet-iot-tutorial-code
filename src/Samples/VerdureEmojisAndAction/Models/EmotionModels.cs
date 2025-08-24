namespace VerdureEmojisAndAction.Models;

/// <summary>
/// 表情类型常量
/// </summary>
public static class EmotionTypes
{
    public const string Anger = "Anger";
    public const string Happy = "Happy";
    public const string Random = "Random";
    
    public static readonly string[] All = { Anger, Happy, Random };
    
    public static bool IsValid(string? emotionType)
    {
        return !string.IsNullOrEmpty(emotionType) && 
               All.Contains(emotionType, StringComparer.OrdinalIgnoreCase);
    }
    
    public static string GetRandomEmotion()
    {
        var availableEmotions = new[] { Anger, Happy };
        var random = new Random();
        return availableEmotions[random.Next(availableEmotions.Length)];
    }
}

/// <summary>
/// 表情配置
/// </summary>
public class EmotionConfig
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LottieFile { get; set; } = string.Empty;
    public Dictionary<int, float> ActionAngles { get; set; } = new();
    public int Duration { get; set; } = 3000; // 毫秒
}

/// <summary>
/// 播放请求
/// </summary>
public class PlayRequest
{
    public string? EmotionType { get; set; }
    public bool IncludeAction { get; set; } = true;
    public bool IncludeEmotion { get; set; } = true;
    public int Loops { get; set; } = 1;
    public int Fps { get; set; } = 30;
}

/// <summary>
/// 播放状态
/// </summary>
public enum PlaybackStatus
{
    Stopped,
    Playing,
    Paused,
    Cancelled
}

/// <summary>
/// 播放状态信息
/// </summary>
public class PlaybackState
{
    public PlaybackStatus Status { get; set; }
    public string? CurrentEmotion { get; set; }
    public DateTime StartTime { get; set; }
    public int CurrentLoop { get; set; }
    public string? ErrorMessage { get; set; }
}
