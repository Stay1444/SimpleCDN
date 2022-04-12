# SimpleCDN

A very basic "CDN" that i created to easily upload pictures or other files from my discord bot or other similar projects.

## Features

- [x] File Compression
- [x] API for uploading files
- [x] C# Wrapper for the API
- [x] File Statistics
  - Download Count
  - Date and Time of the last download
  - Etc
- [x] Temporal Files
- [x] No need for external services (like a database)

## Building
SimpleCDN does not require any external dependencies.
You can build it using 
`dotnet build`

## Running
The first time you run the project it will create a configuration file `config.json` in the working directory.
You can edit this file to change the settings.

## Configuration
The default configuration looks like this:

```json
{
  "Host": "http://localhost:85",
  "EnableCompression": true,
  "CompressionThresholdInMb": 10,
  "DeleteExpiredFiles": true,
  "ApiKeys": [
    "6a88ddf8-3f6c-4eb2-ac98-0cb816ba79da"
  ]
}
```

| Key | Default Value           | Accepted           | Description                                                                           |
| --- |-------------------------|--------------------|---------------------------------------------------------------------------------------|
| `Host` | `http://localhost:1444` | URI or IP Endpoint | The host of the server.                                                               |
| `EnableCompression` | `true` | `true` / `false`    | Enable file compression.                                                              |
| `CompressionThresholdInMb` | `10` | `Number`        | The compression threshold in megabytes. Any file larger than this will be compressed. |
| `DeleteExpiredFiles` | `true` | `true` / `false`    | Delete files that are older than the expiration date provided.                        |
| `ApiKeys` | `[]` | `[]` / `[String]` | The API keys that are allowed to access the API (uploading files).                    |
