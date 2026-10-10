using System;
using System.Globalization;

[Serializable]
public sealed class ServerEventSnapshot
{
    public string id,title,type,description,startAt,endAt;
    public int version;
    public bool disabled;
    public ServerEventReward[] rewards;
    public ServerEventConfig config;
}

[Serializable]
public sealed class ServerEventReward
{
    public string currency,itemId;
    public int amount,tier,count;
}

[Serializable]
public sealed class ServerEventConfig
{
    public string[] eligibleModes;
}

// Presentation-only countdown helper. The server's serverTime is the authority;
// never derive event eligibility from the device clock.
public static class EventCountdown
{
    public static DateTimeOffset ParseUtc(string value)=>DateTimeOffset.Parse(value,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal);
    public static TimeSpan Remaining(ServerEventSnapshot eventData,DateTimeOffset serverNow)
    {
        if(eventData==null||eventData.disabled)return TimeSpan.Zero;
        var end=ParseUtc(eventData.endAt);return end>serverNow?end-serverNow:TimeSpan.Zero;
    }
    public static string Format(TimeSpan remaining)
    {
        if(remaining<=TimeSpan.Zero)return "ENDED";
        if(remaining.TotalDays>=1)return $"{(int)remaining.TotalDays}d {remaining.Hours:00}h";
        return $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
    }
}
