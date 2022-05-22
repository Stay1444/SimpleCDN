using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using SimpleCDN.Models;

namespace SimpleCDN.Services;

public class FileProvider
{
    private ConfigurationModel _configuration;
    public FileProvider(ConfigurationModel model)
    {
        this._configuration = model;   
    }
    
    public bool Exists(Guid handle)
    {
        return File.Exists(Path.Combine(Constants.CdnFolder, handle.ToString())) && File.Exists(Path.Combine(Constants.CdnFolder, handle.ToString() + ".meta"));
    }

    public async Task<MetadataModel> GetMetadataAsync(Guid handle)
    {
        if (!Exists(handle))
        {
            throw new FileNotFoundException("File not found", handle.ToString());
        }


        await using var stream = File.OpenRead(Path.Combine(Constants.CdnFolder, handle.ToString() + ".meta"));
        var metadataObject = await JsonSerializer.DeserializeAsync<MetadataModel>(stream) ?? throw new FileNotFoundException("File not found", handle.ToString());
        
        if (metadataObject.Permanent == false && metadataObject.Expiration is not null && metadataObject.Expiration < DateTime.UtcNow)
        {
            if (_configuration.DeleteExpiredFiles)
            {
                Delete(handle);
            }
            
            throw new FileNotFoundException("File expired", handle.ToString());
        }
        
        return metadataObject;
    }

    public async Task<Guid> CreateAsync(UploadModel model, Stream data)
    {
        var handle = Guid.NewGuid();

        while(Exists(handle))
        {
            handle = Guid.NewGuid();
        }
        
        var metadata = new MetadataModel(handle, model.Name ?? "Unnamed", DateTime.UtcNow, DateTime.MinValue, 0, model.Permanent ?? true,
            model.Expire, data.Length, false);
        
        new FileExtensionContentTypeProvider().TryGetContentType(metadata.Name, out var mime);
        
        metadata.MimeType = mime ?? "application/octet-stream";
        
        

        Stream fileStream;
        
        if (_configuration.EnableCompression && data.Length > _configuration.CompressionThresholdInMb * 1024 * 1024)
        {
            fileStream = new GZipStream(File.Create(Path.Combine(Constants.CdnFolder, handle.ToString())), CompressionLevel.SmallestSize);
            metadata.IsCompressed = true;
        }
        else
        {
            fileStream = File.Create(Path.Combine(Constants.CdnFolder, handle.ToString()));
        }
        
        
        await data.CopyToAsync(fileStream);
        
        data.Close();
        await data.DisposeAsync();
        
        var metadataJson = JsonSerializer.Serialize(metadata);
        
        await using var metaStream = File.Create(Path.Combine(Constants.CdnFolder, handle.ToString() + ".meta"));

        await metaStream.WriteAsync(Encoding.UTF8.GetBytes(metadataJson));
        
        await metaStream.FlushAsync();

        await fileStream.FlushAsync();
        await fileStream.DisposeAsync();
        
        return handle;
    }
    
    public async Task<Stream> GetAsync(Guid handle)
    {
        if (!Exists(handle))
        {
            throw new FileNotFoundException("File not found", handle.ToString());
        }

        var metadata = await GetMetadataAsync(handle);
        Stream stream;
        if (!metadata.IsCompressed)
        {
            stream = File.OpenRead(Path.Combine(Constants.CdnFolder, handle.ToString()));
        }
        else
        {
            stream = new GZipStream(File.OpenRead(Path.Combine(Constants.CdnFolder, handle.ToString())), CompressionMode.Decompress);
        }

        return stream;
    }

    public async Task UpdateMetadataAsync(Guid id, MetadataModel metadataModel)
    {
        if (!Exists(id))
        {
            throw new FileNotFoundException("File not found", id.ToString());
        }
        
        var metadataJson = JsonSerializer.Serialize(metadataModel);

        await File.WriteAllTextAsync(Path.Combine(Constants.CdnFolder, id.ToString() + ".meta"), metadataJson);
    }
    
    public void Delete(Guid id)
    {
        if (!Exists(id))
        {
            throw new FileNotFoundException("File not found", id.ToString());
        }

        File.Delete(Path.Combine(Constants.CdnFolder, id.ToString()));
        File.Delete(Path.Combine(Constants.CdnFolder, id.ToString() + ".meta"));
        
    }
    
    public async Task<MetadataModel[]> GetAllMetadatasAsync()
    {
        var files = Directory.GetFiles(Constants.CdnFolder, "*.meta");
        var result = new List<MetadataModel>();
        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);

            if (Exists(Guid.Parse(fileName)))
            {
                result.Add(await GetMetadataAsync(Guid.Parse(fileName)));
            }
        }
        
        return result.ToArray();
    }
}