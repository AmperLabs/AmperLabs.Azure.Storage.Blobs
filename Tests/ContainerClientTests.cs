using Xunit;
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;
using AmperLabs.Azure.Storage.Blobs;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    public class ContainerClientTests
    {
        private readonly string _connectionString;

        public ContainerClientTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<ContainerClientTests>()
                .AddEnvironmentVariables()
                .Build();

            _connectionString = config.GetValue<string>("StorageConnectionString");
        }

        private BlobContainerClient CreateContainerClient(string prefix = "test")
        {
            var containerName = prefix.ToLower() + "-" + Guid.NewGuid().ToString("n");
            var containerClient = new BlobContainerClient(_connectionString, containerName);
            containerClient.CreateIfNotExists();

            return containerClient;
        }

        private string CreateLocalFileStructure(List<string> filepaths)
        {
            var basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
            
            Directory.CreateDirectory(basePath);

            foreach(var filepath in filepaths)
            {
                var path = Path.Combine(basePath, filepath);
                var filename = Path.GetFileName(path);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllText(path, $"I am a testfile named '{filename}'.");
            }

            return basePath;
        }

        [Fact]
        public void T01_CanCreateLocalFileStructure()
        {
            var paths = new List<string>
            {
                "File1.txt",
                "File2.txt",
                "File3.txt",
                @"Folder\File4.txt",
                @"Folder\File5.txt",
                @"Folder\Subfolder\File6.txt",
                @"AnotherFolder\File7.txt",
            };

            var localPath = CreateLocalFileStructure(paths);

            localPath.Should().NotBeNullOrEmpty();

            Directory.Exists(localPath).Should().BeTrue();
            
            foreach(var path in paths)
            {
                var p = Path.Combine(localPath, path);
                File.Exists(p).Should().BeTrue(p);
            }

            Console.WriteLine($"Created file structure at {localPath}");
            Debug.WriteLine($"Created file structure at {localPath}");

            Directory.Delete(localPath, true);
            Directory.Exists(localPath).Should().BeFalse();
        }

        [Fact]
        public async Task T02_CanUploadDirectoryToContainerRoot()
        {
            var paths = new List<string>
            {
                "File1.txt",
                "File2.txt",
                "File3.txt",
                @"Folder\File4.txt",
                @"Folder\File5.txt",
                @"Folder\Subfolder\File6.txt",
                @"AnotherFolder\File7.txt",
            };

            var localPath = CreateLocalFileStructure(paths);

            var containerClient = CreateContainerClient("T02");

            await containerClient.UploadDirectoryAsync(localPath);
            
            foreach(var filename in paths)
            {
                var standardizedFilename = filename.Contains("\\") ?
                                            filename.Replace("\\", "/") :
                                            filename;

                var blob = containerClient.GetBlobClient(standardizedFilename);

                blob.Should().NotBeNull();

                var exists = await blob.ExistsAsync();
                exists.Value.Should().BeTrue(standardizedFilename);
            }

            Directory.Delete(localPath, true);
            await containerClient.DeleteAsync();
        }

        [Fact]
        public async Task T03_CanUploadDirectoryToVirtualDirectory()
        {
            var paths = new List<string>
            {
                "File1.txt",
                "File2.txt",
                "File3.txt",
                @"Folder\File4.txt",
                @"Folder\File5.txt",
                @"Folder\Subfolder\File6.txt",
                @"AnotherFolder\File7.txt",
            };

            var localPath = CreateLocalFileStructure(paths);

            var containerClient = CreateContainerClient("T03");

            await containerClient.UploadDirectoryAsync(localPath, "my");
            
            foreach(var filename in paths)
            {
                var standardizedFilename = filename.Contains("\\") ?
                                            filename.Replace("\\", "/") :
                                            filename;

                var blob = containerClient.GetBlobClient("my/" + standardizedFilename);

                blob.Should().NotBeNull();

                var exists = await blob.ExistsAsync();
                exists.Value.Should().BeTrue(standardizedFilename);
            }

            Directory.Delete(localPath, true);
            await containerClient.DeleteAsync();
        }
    
        [Fact]
        public async Task T04_CanDeleteAllBlobsFromContainer()
        {
            var paths = new List<string>
            {
                "File1.txt",
                "File2.txt",
                "File3.txt",
                @"Folder\File4.txt",
                @"Folder\File5.txt",
                @"Folder\Subfolder\File6.txt",
                @"AnotherFolder\File7.txt",
            };

            var localPath = CreateLocalFileStructure(paths);

            var containerClient = CreateContainerClient("T04");

            await containerClient.UploadDirectoryAsync(localPath);

            foreach(var filename in paths)
            {
                var standardizedFilename = filename.Contains("\\") ?
                                            filename.Replace("\\", "/") :
                                            filename;

                var blob = containerClient.GetBlobClient(standardizedFilename);

                blob.Should().NotBeNull();

                var exists = await blob.ExistsAsync();
                exists.Value.Should().BeTrue(standardizedFilename);
            }

            await containerClient.DeleteBlobsAsync();

            foreach(var filename in paths)
            {
                var standardizedFilename = filename.Contains("\\") ?
                                            filename.Replace("\\", "/") :
                                            filename;

                var blob = containerClient.GetBlobClient(standardizedFilename);

                blob.Should().NotBeNull();

                var exists = await blob.ExistsAsync();
                exists.Value.Should().BeFalse(standardizedFilename);
            }

            Directory.Delete(localPath, true);
            await containerClient.DeleteAsync();
        }

        [Fact]
        public async Task T05_CanDeleteBlobsInVirtualDirectoryFromContainer()
        {
            var paths = new List<string>
            {
                "File1.txt",
                "File2.txt",
                "File3.txt",
                @"Folder\File4.txt",
                @"Folder\File5.txt",
                @"Folder\Subfolder\File6.txt",
                @"AnotherFolder\File7.txt",
            };

            var localPath = CreateLocalFileStructure(paths);

            var containerClient = CreateContainerClient("T05");

            await containerClient.UploadDirectoryAsync(localPath);

            foreach(var filename in paths)
            {
                var standardizedFilename = filename.Contains("\\") ?
                                            filename.Replace("\\", "/") :
                                            filename;

                var blob = containerClient.GetBlobClient(standardizedFilename);

                blob.Should().NotBeNull();

                var exists = await blob.ExistsAsync();
                exists.Value.Should().BeTrue(standardizedFilename);
            }

            await containerClient.DeleteBlobsAsync("Folder");

            foreach(var filename in paths)
            {
                var standardizedFilename = filename.Contains("\\") ?
                                            filename.Replace("\\", "/") :
                                            filename;

                var blob = containerClient.GetBlobClient(standardizedFilename);

                blob.Should().NotBeNull();

                var exists = await blob.ExistsAsync();

                if(standardizedFilename.StartsWith("Folder"))
                    exists.Value.Should().BeFalse(standardizedFilename);
                else
                    exists.Value.Should().BeTrue(standardizedFilename);
            }

            Directory.Delete(localPath, true);
            await containerClient.DeleteAsync();
        }
    }
}