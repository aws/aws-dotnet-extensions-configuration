namespace AppConfigParameterStore;

public class Settings
{
    public string StringValue { get; set; } = string.Empty;
    public int IntegerValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public bool BooleanValue { get; set; }
    public TimeSpan TimeSpanValue { get; set; }
    public DatabaseSettings Database { get; set; } = new();
    public ApiSettings Api { get; set; } = new();
    public FeatureSettings Features { get; set; } = new();
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; }
    public bool EnableRetries { get; set; }
}

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}

public class FeatureSettings
{
    public bool EnableLogging { get; set; }
    public bool EnableMetrics { get; set; }
    public string LogLevel { get; set; } = "Information";
}
