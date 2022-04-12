using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SimpleCDN.Models;
using SimpleCDN.Services;

namespace SimpleCDN.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : Controller
{
    private ConfigurationModel _configuration;
    private FileProvider _fileProvider;
    public ApiController(ConfigurationModel configuration, FileProvider fileProvider)
    {
        _configuration = configuration;
        _fileProvider = fileProvider;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.ContainsKey("Authorization"))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
        
        if (authHeader.Contains("Bearer"))
        {
            authHeader = authHeader.Replace("Bearer ", "");
        }
        if (!Guid.TryParse(authHeader, out var token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        if (_configuration.ApiKeys.Contains(token))
        {
            base.OnActionExecuting(context);
        }
        else
        {
            context.Result = new UnauthorizedResult();
        }
    }


    [HttpPost("upload")]
    public async Task<IActionResult> EndUpload(IFormCollection data)
    {
        if (!data.ContainsKey("metadata"))
        {
            return BadRequest("Missing metadata");
        }
        
        var metadataString = data["metadata"].ToString();

        var model = JsonSerializer.Deserialize<UploadModel?>(metadataString, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
        
        if (model == null)
        {
            Console.WriteLine("Failed to deserialize metadata");
            return BadRequest("Invalid request");
        }

        if (model.Permanent is null)
        {
            Console.WriteLine("Permanent is null");
            return BadRequest("Permanent must be set to true or false");
        }

        if (!model.Permanent.Value && model.Expire is null)
        {
            Console.WriteLine("Expire is null");
            return BadRequest("Expire must be set if permanent is false");
        }
        
        if (model.Permanent.Value && model.Expire.HasValue && model.Expire.Value < DateTime.UtcNow)
        {
            Console.WriteLine("Expire is in the past");
            return BadRequest("Expire must be in the future");
        }
        
        var id = Guid.NewGuid();
        
        var result = await _fileProvider.CreateAsync(model, data.Files[0].OpenReadStream());

        return Ok(new
        {
            id = result
        });
    }
}