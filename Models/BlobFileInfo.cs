namespace AzureCloudFileManager.Models;

public class BlobFileInfo
{
    public string Name { get; set; } = "";
    public long? Size { get; set; }
    public DateTimeOffset? LastModified { get; set; }
}