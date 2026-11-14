using System.ComponentModel.DataAnnotations;

namespace AzureCloudFileManager.Models;

public enum SasPermissionOption
{
    [Display(Name = "Read")] Read,
    [Display(Name = "Write")] Write,
    [Display(Name = "Read + Write")] ReadWrite,
    [Display(Name = "Read + Delete")] ReadDelete
}

public class BlobSasViewModel
{
    [Required]
    public string BlobName { get; set; } = "";

    public SasPermissionOption Permission { get; set; } = SasPermissionOption.Read;

    [Range(1, 10080, ErrorMessage = "Від 1 хвилини до 7 днів")]
    public int ExpiresInMinutes { get; set; } = 60;

    public string? SasUrl { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
}