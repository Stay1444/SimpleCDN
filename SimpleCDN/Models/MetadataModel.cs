using System;

namespace SimpleCDN.Models;

public class MetadataModel
{
    public MetadataModel(Guid id, string name, DateTime created, DateTime lastDownload, ulong downloadCount, bool permanent, DateTime? expiration, long size, bool isCompressed)
    {
        Id = id;
        Name = name;
        Created = created;
        LastDownload = lastDownload;
        DownloadCount = downloadCount;
        Permanent = permanent;
        Expiration = expiration;
        Size = size;
        IsCompressed = isCompressed;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string MimeType { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastDownload { get; set; }
    public ulong DownloadCount { get; set; }
    public bool Permanent { get; set; }
    public DateTime? Expiration { get; set; }
    public long Size { get; set; }
    public bool IsCompressed { get; set; }
}