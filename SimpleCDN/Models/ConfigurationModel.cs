namespace SimpleCDN.Models;

public class ConfigurationModel
{
    public string Host { get; set; } = "http://0.0.0.0:85";
    public bool EnableCompression { get; set; } = true;
    public long CompressionThresholdInMb { get; set; } = 10;
    public bool DeleteExpiredFiles { get; set; } = true;
    public Guid[] ApiKeys { get; set; } = new [] { Guid.NewGuid() };
}