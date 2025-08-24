namespace VerdureEmojisAndAction.Models;

/// <summary>
/// 表情类型枚举
/// </summary>
public enum EmotionType
{
    Anger,
    Happy,
    Random
}

/// <summary>
/// 表情配置
/// </summary>
public class EmotionConfig
{
    public EmotionType Type { get; set; }
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
    public EmotionType? EmotionType { get; set; }
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
    public EmotionType? CurrentEmotion { get; set; }
    public DateTime StartTime { get; set; }
    public int CurrentLoop { get; set; }
    public string? ErrorMessage { get; set; }
}
