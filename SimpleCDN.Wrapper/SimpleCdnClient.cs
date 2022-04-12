using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleCDN.Wrapper;

public class SimpleCdnClient
{
    private HttpClient _httpClient;
    public string Host { get; }
    public SimpleCdnClient(string host, Guid? apikey = null)
    {
        this.Host = host;
        this._httpClient = new HttpClient();
        this._httpClient.BaseAddress = new Uri(host);
        this._httpClient.DefaultRequestHeaders.Accept.Clear();
        if (apikey is not null)
        {
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + apikey.Value.ToString());
        }
    }

    public async Task<Stream> DownloadAsync(Guid id)
    {
        var response = await this._httpClient.GetAsync(id.ToString());
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync();
        }
        throw new Exception(response.ReasonPhrase);
    }

    public async Task<(Stream stream, string name)> DownloadWithNameAsync(Guid id)
    {
        var metadata = await this.GetMetadataAsync(id);
        
        if (metadata is null) throw new Exception("File not found");

        var stream = await DownloadAsync(id);
        
        return (stream, metadata.Name);
    }
    
    public async Task<Metadata?> GetMetadataAsync(Guid id)
    {
        var response = await this._httpClient.GetAsync(id.ToString() + "/metadata");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Metadata>(json, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
        }
        
        throw new Exception(response.ReasonPhrase);
    }

    public async Task<Guid> UploadAsync(string name, Stream stream, bool permanent = true, DateTime? expire = null)
    {
        var uploadModel = new
        {
            name,
            permanent,
            expire
        };
        
        var formMultipartContent = new MultipartFormDataContent();
        formMultipartContent.Add(new StringContent(JsonSerializer.Serialize(uploadModel)), "metadata");
        formMultipartContent.Add(new StreamContent(stream), "file", name);
        
        var response = await this._httpClient.PostAsync("api/upload", formMultipartContent);
        
        if (response.IsSuccessStatusCode)
        {
            var snowflake = JsonSerializer.Deserialize<SnowflakeObject>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            if (snowflake is null) throw new Exception("Invalid response");
            await stream.DisposeAsync();
            return snowflake.Id;
        }
        await stream.DisposeAsync();
        throw new Exception(response.ReasonPhrase);
    }
}