using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using SimpleCDN;
using SimpleCDN.Models;
using SimpleCDN.Services;
if (args.Contains("--new-guid"))
{
    Console.WriteLine(Guid.NewGuid());

    return;
}

if (!File.Exists("config.json"))
{
    File.WriteAllText("config.json", JsonSerializer.Serialize(new ConfigurationModel(), new JsonSerializerOptions()
    {
        WriteIndented = true
    }));
    Console.WriteLine("Config file created");
    return;
}

var config = JsonSerializer.Deserialize<ConfigurationModel>(File.ReadAllText("config.json"));
if (config is null)
{
    Console.WriteLine("Config file is invalid");
    return;
}
if (!Directory.Exists(Constants.CdnFolder))
{
    Directory.CreateDirectory(Constants.CdnFolder);
}

var fileProvider = new FileProvider(config);

var webBuilder = WebApplication.CreateBuilder();
webBuilder.WebHost.UseKestrel(x =>
{
    x.Limits.MaxRequestBodySize = long.MaxValue;
});

// Periodically loop through all files and check for their due date. 
// Check every 60 minutes.
_ = Task.Run(async () =>
{
    while (true)
    {
        var _ = await fileProvider.GetAllMetadatasAsync();
        await Task.Delay(TimeSpan.FromMinutes(60));        
    }
    // ReSharper disable once FunctionNeverReturns
});

webBuilder.Services.AddControllers();
webBuilder.WebHost.UseUrls(config.Host);
webBuilder.Services.AddSingleton(config);
webBuilder.Services.AddSingleton(fileProvider);
webBuilder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = long.MaxValue;
});
var webApp = webBuilder.Build();
webApp.MapControllers();
webApp.Run();