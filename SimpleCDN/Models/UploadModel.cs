namespace SimpleCDN.Models;

public class UploadModel
{
    public string? Name { get; set; }
    public bool? Permanent { get; set; }
    public DateTime? Expire { get; set; }
}