﻿using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AmperLabs.Azure.Storage.Blobs
{
    public static class ContainerClientExtensions
    {
        /// <summary>
        /// Uploads the contents of a local directory to an Azure Blob Storage Container
        /// </summary>
        /// <param name="sourcePath">The path to the local source directory</param>
        /// <param name="prefix">Optional. A virtual path in the container to that the contents are uploaded</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public static async Task UploadDirectoryAsync(this BlobContainerClient containerClient, string sourcePath, string prefix = null)
        {
            if(!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException();

            if(!(await containerClient.ExistsAsync()))
                throw new Exception($"Container '{containerClient.Name}' does not exist.");

            var basepath = Path.GetFullPath(sourcePath);

            foreach(var file in Directory.GetFiles(sourcePath))
            {
                try
                {
                    var localPath = Path.GetFullPath(file);
                    var targetPath = localPath.Replace(basepath, "");
                    
                    if(targetPath.StartsWith("/") || targetPath.StartsWith("\\"))
                        targetPath = targetPath.Substring(1);

                    if(!string.IsNullOrEmpty(prefix))
                        targetPath = Path.Combine(prefix, targetPath);
                    
                    var blobClient = containerClient.GetBlobClient(targetPath);
                    if(await blobClient.ExistsAsync())
                    {
                        Console.WriteLine($"Blob '{blobClient.Name}' already exists.");
                    }
                    else
                    {
                        Console.WriteLine($"Uploading '{localPath}' to '[{containerClient.Name}] {targetPath}'");
                        await blobClient.UploadAsync(localPath);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error while uploading Blob: {ex.Message}");
                }
            }

            foreach(var directory in Directory.GetDirectories(sourcePath))
            {
                var newPrefix = string.IsNullOrEmpty(prefix) ? Path.GetFileName(directory) :  Path.Combine(prefix, Path.GetFileName(directory));
                await containerClient.UploadDirectoryAsync(directory, newPrefix);
            }
        }

        /// <summary>
        /// Downloads the files from an Azure Blob Storage Container to a local directory while preserving file hierarchies
        /// </summary>
        /// <param name="targetPath">The path of the local target directory</param>
        /// <param name="prefix">Optional. If set, only files from the this virtual directory are downloaded</param>
        /// <exception cref="Exception"></exception>
        public static async Task DownloadBlobsToAsync(this BlobContainerClient containerClient, string targetPath, string prefix = null)
        {
            if(!(await containerClient.ExistsAsync()))
                throw new Exception($"Container '{containerClient.Name}' does not exist.");

            if(!string.IsNullOrEmpty(prefix))
                prefix = prefix.Replace("\\", "/");

            foreach(var item in containerClient.GetBlobs(prefix: prefix))
            {
                var storagePath = item.Name;

                var localPath = Path.GetFullPath(Path.Combine(targetPath, storagePath));
                if(!string.IsNullOrEmpty(prefix))
                {
                    localPath = Path.GetFullPath(
                                    Path.Combine(targetPath, storagePath.StartsWith(prefix) ?
                                    storagePath.Substring(prefix.Length + 1) :
                                    storagePath)
                                );
                }

                try
                {
                    Console.WriteLine($"Downloading '[{containerClient.Name}] {storagePath}' to '{localPath}'");

                    var localDirectory = Path.GetDirectoryName(localPath);
                    if(!Directory.Exists(localDirectory))
                    {
                        Console.WriteLine($"Creating missing directory '{localDirectory}'");
                        Directory.CreateDirectory(localDirectory);
                    }

                    var blobClient = containerClient.GetBlobClient(item.Name);
                    await blobClient.DownloadToAsync(localPath);
                }                
                catch(Exception ex)
                {
                    Console.WriteLine($"Downloading '[{containerClient.Name}] {storagePath}' to '{localPath}' failed.");
                }
            }
        }
    
        public static async Task DownloadBlobsToZipAsync(this BlobContainerClient containerClient, string targetFilename, string prefix = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes all files in an Azure Blob Storage Container
        /// </summary>
        /// <param name="prefix">Optional. Deletes only files in this virtual directory</param>
        /// <exception cref="Exception"></exception>
        public static async Task DeleteBlobsAsync(this BlobContainerClient containerClient, string prefix = null)
        {
            if(!(await containerClient.ExistsAsync()))
                throw new Exception($"Container '{containerClient.Name}' does not exist.");

            if(!string.IsNullOrEmpty(prefix))
                prefix = prefix.Replace("\\", "/");

            foreach(var item in containerClient.GetBlobs(prefix: prefix))
            {
                Console.WriteLine($"Deleting Blob '{item.Name}'");
                await containerClient.DeleteBlobAsync(item.Name);
            }   
        }
    }
}
