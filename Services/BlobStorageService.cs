using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureCloudFileManager.Models;
using Azure.Storage.Sas;

namespace AzureCloudFileManager.Services;

public class BlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Не задано AzureStorage:ConnectionString");
        var containerName = config["AzureStorage:ContainerName"] ?? "files";

        _container = new BlobContainerClient(connectionString, containerName);
        _container.CreateIfNotExists();
    }

    public async Task<List<BlobFileInfo>> ListAsync()
    {
        var result = new List<BlobFileInfo>();
        await foreach (var item in _container.GetBlobsAsync())
        {
            result.Add(new BlobFileInfo
            {
                Name = item.Name,
                Size = item.Properties.ContentLength,
                LastModified = item.Properties.LastModified
            });
        }
        return result;
    }

    public async Task UploadAsync(IFormFile file)
    {
        var blob = _container.GetBlobClient(file.FileName);
        using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        });
    }

    public async Task<(Stream Content, string ContentType)> DownloadAsync(string name)
    {
        var blob = _container.GetBlobClient(name);
        var response = await blob.DownloadStreamingAsync();
        return (response.Value.Content, response.Value.Details.ContentType);
    }

    public async Task DeleteAsync(string name)
    {
        await _container.GetBlobClient(name).DeleteIfExistsAsync();
    }

    
    public Uri GenerateBlobSas(string blobName, SasPermissionOption option, DateTimeOffset expiresOn)
    {
        var blob = _container.GetBlobClient(blobName);

        if (!blob.CanGenerateSasUri)
            throw new InvalidOperationException("Клієнт не може генерувати SAS (немає ключа акаунта).");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b",                               
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), 
            ExpiresOn = expiresOn
        };

        var permissions = option switch
        {
            SasPermissionOption.Read => BlobSasPermissions.Read,
            SasPermissionOption.Write => BlobSasPermissions.Write,
            SasPermissionOption.ReadWrite => BlobSasPermissions.Read | BlobSasPermissions.Write,
            SasPermissionOption.ReadDelete => BlobSasPermissions.Read | BlobSasPermissions.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(option))
        };
        sasBuilder.SetPermissions(permissions);

        return blob.GenerateSasUri(sasBuilder);
    }
    
    public Uri GenerateContainerSas(ContainerSasOption option, DateTimeOffset expiresOn)
    {
        if (!_container.CanGenerateSasUri)
            throw new InvalidOperationException("Клієнт не може генерувати SAS (немає ключа акаунта).");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            Resource = "c",                                   
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = expiresOn
        };

        var permissions = option switch
        {
            ContainerSasOption.ReadList => BlobContainerSasPermissions.Read | BlobContainerSasPermissions.List,
            ContainerSasOption.List => BlobContainerSasPermissions.List,
            ContainerSasOption.Read => BlobContainerSasPermissions.Read,
            _ => throw new ArgumentOutOfRangeException(nameof(option))
        };
        sasBuilder.SetPermissions(permissions);

        return _container.GenerateSasUri(sasBuilder);
    }
}