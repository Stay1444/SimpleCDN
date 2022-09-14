namespace SimpleCDN;

public class Constants
{
    public static string CdnFolder = Environment.GetEnvironmentVariable("CDN_DATA_FOLDER") ?? "cdn";
}