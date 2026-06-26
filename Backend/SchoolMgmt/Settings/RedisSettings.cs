namespace SchoolMgmt.Settings;

public class RedisSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int DefaultTtlMinutes { get; set; } = 30;
}