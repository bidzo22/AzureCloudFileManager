using System.ComponentModel.DataAnnotations;

namespace AzureCloudFileManager.Models;

public enum SasTestOperation
{
    [Display(Name = "Read (прочитати)")] Read,
    [Display(Name = "Write (записати)")] Write,
    [Display(Name = "Delete (видалити)")] Delete
}

public class SasTestViewModel
{
    [Required(ErrorMessage = "Встав SAS-посилання")]
    public string SasUrl { get; set; } = "";

    public SasTestOperation Operation { get; set; } = SasTestOperation.Read;

    public string? WriteContent { get; set; } = "Hello from SAS!";

    
    public bool? Success { get; set; }
    public string? ResultMessage { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ReadPreview { get; set; }
}