# AmperLabs.Azure.Storage.Blobs
This project provides extension methods for the [BlobContainerClass](https://docs.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient?view=azure-dotnet) in the Microsoft [Azure SDK for .NET (v12)](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs-readme?view=azure-dotnet).

## Table of Contents
* [General Info](#general-information)
* [Technologies Used](#technologies-used)
* [Features](#features)
* [Setup](#setup)
* [Usage](#usage)
* [Project Status](#project-status)
* [License](#license)


## General Information
This project was created to make life easier when dealing with uploading/downloading/deleting file structures in an Azure Storage Blob Container. Especially uploading/downloading a complete directory structures to/from a container or virtual directory (aka prefix) in a container involves too much boiler plate code.


## Technologies Used
- netstandard2.0
- [Azure SDK for .NET (v12)](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs-readme?view=azure-dotnet)

## Features
- UploadDirectoryAsync
- DownloadBlobsToAsync
- DeleteBlobsAsync


## Setup
Install from nuget.org:
```
dotnet add package AmperLabs.Azure.Storage.Blobs
```


## Usage
Assume the following file structure under a local directory `.\Temp\StorageTest`:
```
.\Temp\StorageTest
│   File1.txt
│   File2.txt
│   File3.txt
│
├───AnotherFolder
│       AnotherFile.txt
│
└───Folder
    │   FileA.txt
    │   FileB.txt
    │
    └───SubFolder
            FileX.txt
```
then
```cs
using Azure.Storage.Blobs;
using AmperLabs.Azure.Storage.Blobs;

string _connectionString = "<storageConnectionString>";
string _containerName = "<containerName>";

# Create a new BlobContainerClient
var _containerClient = new BlobContainerClient(_connectionString, _containerName);
# Create the container if neccessary
_containerClient.CreateIfNotExists();

# Upload contents from a local directory directly to the container
await _containerClient.UploadDirectoryAsync(@".\Temp\StorageTest");

# This will upload the contents to a virtual folder named 'my' in the container
await _containerClient.UploadDirectoryAsync(@"D:\Temp\StorageTest", "my");

# Download the Blobs from the container to a local directory 
# Local directories get created as needed
await _containerClient.DownloadBlobsToAsync(@".\Temp\StorageTestDownload");

# Download the Blobs from a virtual directory named 'Folder' in the container to a local directory 
# Local directories get created as needed
await _containerClient.DownloadBlobsToAsync(@".\Temp\StorageTestDownload", "Folder");

# Delete all Blobs in the container
await _containerClient.DeleteBlobsAsync();

# Delete all Blobs in the virtual directory named 'Folder' the container
await _containerClient.DeleteBlobsAsync();
```


## Project Status
Project is: _in progress_

## Gitpod
[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/AmperLabs/AmperLabs.Azure.Storage.Blobs)

## License
This project is open source and available under the

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
