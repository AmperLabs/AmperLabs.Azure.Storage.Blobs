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

//await _containerClient.DeleteBlobsAsync();
//await _containerClient.DeleteBlobsAsync("Folder");


var zipFilePath = @"D:\Temp\ZipDownload\archive.zip";
if(!Directory.Exists(Path.GetDirectoryName(zipFilePath)))
    Directory.CreateDirectory(Path.GetDirectoryName(zipFilePath));

using var zip = new FileStream(zipFilePath, FileMode.Create);
await _containerClient.DownloadBlobsToZipStreamAsync(zip);
zip.Close();

var zipFileFolderPath = @"D:\Temp\ZipDownload\folder.zip";
if(!Directory.Exists(Path.GetDirectoryName(zipFileFolderPath)))
    Directory.CreateDirectory(Path.GetDirectoryName(zipFileFolderPath));

using var zipFolder = new FileStream(zipFileFolderPath, FileMode.Create);
await _containerClient.DownloadBlobsToZipStreamAsync(zipFolder, "Folder");
zipFolder.Close();

await _containerClient.DownloadBlobsToZipFileAsync(@"D:\Temp\ZipDownload\archive2.zip");
await _containerClient.DownloadBlobsToZipFileAsync(@"D:\Temp\ZipDownload\folder2.zip", "Folder");
