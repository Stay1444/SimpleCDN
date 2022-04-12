// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using SimpleCDN.Wrapper;Console.WriteLine("SimpleCDN example");

Console.WriteLine("Write the hostname/IP of your SimpleCDN:");
var hostname = Console.ReadLine();

if (string.IsNullOrEmpty(hostname))
{
    Console.WriteLine("No hostname/IP specified. Exiting.");
    return;
}

Console.WriteLine("Write the api key (optional):");
var apiKey = Console.ReadLine();
var apiKeyGuid = apiKey == null ? Guid.Empty : Guid.Parse(apiKey);

var client = new SimpleCdnClient(hostname, apiKeyGuid);
start:

Console.WriteLine("Select an action:");
Console.WriteLine("1. Upload a file");
Console.WriteLine("2. Download a file");
Console.WriteLine("3. Get file metadata");

var action = Console.ReadLine() ?? "";
try
{
    

if (action == "1")
{
    Console.WriteLine("Enter the file path:");
    var filePath = Console.ReadLine() ?? "";
    if (!File.Exists(filePath))
    {
        Console.WriteLine("File not found");
        goto start;
    }
    
    Console.WriteLine("Enter the file name:");
    var fileName = Console.ReadLine() ?? "";
    if (string.IsNullOrEmpty(fileName))
    {
        Console.WriteLine("File name cannot be empty");
        goto start;
    }
    
    Console.WriteLine("Uploading...");
    var id = await client.UploadAsync(fileName, File.OpenRead(filePath));
    Console.WriteLine($"File uploaded with id {id}");
    
    goto start;
}else if (action == "2")
{
    Console.WriteLine("Enter the file id:");
    var id = Console.ReadLine() ?? "";
    if (string.IsNullOrEmpty(id))
    {
        Console.WriteLine("File id cannot be empty");
        goto start;
    }
    
    Console.WriteLine("Enter the directory to save the file:");
    var directory = Console.ReadLine() ?? "";
    if (string.IsNullOrEmpty(directory))
    {
        Console.WriteLine("Directory cannot be empty");
        goto start;
    }
    
    if (!Directory.Exists(directory))
    {
        Console.WriteLine("Directory not found");
        goto start;
    }
    
    Console.WriteLine("Downloading...");

    var (stream, name) = await client.DownloadWithNameAsync(Guid.Parse(id));
    
    Console.WriteLine($"File downloaded with name {name}");
    
    await using var fileStream = File.Create(Path.Combine(directory, name));
    await stream.CopyToAsync(fileStream);
    
    await fileStream.FlushAsync();
    await stream.FlushAsync();
    await stream.DisposeAsync();
    
    goto start;
}else if (action == "3")
{
    Console.WriteLine("Enter the file id:");
    var id = Console.ReadLine() ?? "";
    if (string.IsNullOrEmpty(id))
    {
        Console.WriteLine("File id cannot be empty");
        goto start;
    }
    
    Console.WriteLine("Getting metadata...");
    var metadata = await client.GetMetadataAsync(Guid.Parse(id));
    Console.WriteLine($"File metadata: \n\n{JsonSerializer.Serialize(metadata)}");
    
    goto start;
}

}catch(Exception ex)
{
    Console.WriteLine(ex.Message);
    goto start;
}
