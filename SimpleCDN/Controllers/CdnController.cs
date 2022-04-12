using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleCDN.Services;

namespace SimpleCDN.Controllers;

public class CdnController : Controller
{
    private FileProvider _fileProvider;
    
    public CdnController(FileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    [HttpGet("/{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var metadata = await _fileProvider.GetMetadataAsync(id);

            metadata.LastDownload = DateTime.UtcNow;
            metadata.DownloadCount++;
            
            await _fileProvider.UpdateMetadataAsync(id, metadata);
            
            var stream = await _fileProvider.GetAsync(id);
            
            return File(stream, metadata.MimeType, metadata.Name);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
    
    [HttpGet("/{id}/metadata")]
    public async Task<IActionResult> GetMetadata(Guid id)
    {
        try
        {
            var metadata = await _fileProvider.GetMetadataAsync(id);
            
            return Ok(metadata);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
}

