using System.ComponentModel.DataAnnotations;

namespace AzureCloudFileManager.Models;

public enum ContainerSasOption
{
    [Display(Name = "Read + List")] ReadList,
    [Display(Name = "List")] List,
    [Display(Name = "Read (без List)")] Read
}

public class ContainerSasViewModel
{
    public ContainerSasOption Permission { get; set; } = ContainerSasOption.ReadList;

    [Range(1, 10080, ErrorMessage = "Від 1 хвилини до 7 днів")]
    public int ExpiresInMinutes { get; set; } = 60;

    public string? SasUrl { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
}

public class ContainerSasTestViewModel
{
    [Required(ErrorMessage = "Встав SAS-посилання на контейнер")]
    public string SasUrl { get; set; } = "";

    public bool? Success { get; set; }
    public List<ContainerBlobItem> Blobs { get; set; } = new();
    public int? StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ResultMessage { get; set; }
}

public class ContainerBlobItem
{
    public string Name { get; set; } = "";
    public long? Size { get; set; }
    public string Url { get; set; } = "";   
}