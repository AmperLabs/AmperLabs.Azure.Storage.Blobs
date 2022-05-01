// See https://aka.ms/new-console-template for more information
using Azure.Storage.Blobs;
using AmperLabs.Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()        
    .Build();

string _connectionString = config.GetValue<string>("StorageConnectionString");
string _containerName = "test";

var _containerClient = new BlobContainerClient(_connectionString, _containerName);  
_containerClient.CreateIfNotExists();


//await _containerClient.UploadDirectoryAsync(@"D:\Temp\StorageTest");

//await _containerClient.DownloadBlobsToAsync(@"D:\Temp\StorageTestDownload");
//await _containerClient.DownloadBlobsToAsync(@"D:\Temp\StorageTestDownload", "Folder");

await _containerClient.DeleteBlobsAsync();
//await _containerClient.DeleteBlobsAsync("Folder");
