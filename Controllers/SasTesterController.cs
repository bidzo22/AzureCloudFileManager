using Azure;
using Azure.Storage.Blobs;
using AzureCloudFileManager.Models;
using Microsoft.AspNetCore.Mvc;


namespace AzureCloudFileManager.Controllers;


public class SasTesterController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new SasTestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SasTestViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            
            var blob = new BlobClient(new Uri(model.SasUrl.Trim()));

            switch (model.Operation)
            {
                case SasTestOperation.Read:
                    var download = await blob.DownloadContentAsync();
                    var bytes = download.Value.Content.ToArray();
                    var contentType = download.Value.Details.ContentType ?? "";
                    model.ResultMessage = $"Прочитано {bytes.Length} байт ({contentType}).";

                    if (contentType.StartsWith("text/") || contentType.Contains("json"))
                    {
                        var text = System.Text.Encoding.UTF8.GetString(bytes);
                        model.ReadPreview = text.Length > 1000 ? text[..1000] + "..." : text;
                    }
                    break;

                case SasTestOperation.Write:
                    await blob.UploadAsync(BinaryData.FromString(model.WriteContent ?? ""), overwrite: true);
                    model.ResultMessage = "Вміст Blob успішно перезаписано.";
                    break;

                case SasTestOperation.Delete:
                    await blob.DeleteAsync();
                    model.ResultMessage = "Blob успішно видалено.";
                    break;
            }

            model.Success = true;
        }
        catch (RequestFailedException ex)
        {
            model.Success = false;
            model.StatusCode = ex.Status;
            model.ErrorCode = ex.ErrorCode;
            model.ResultMessage = ex.Message.Split('\n')[0];
        }
        catch (UriFormatException)
        {
            model.Success = false;
            model.ResultMessage = "Це не схоже на коректне посилання.";
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Container()
    {
        return View(new ContainerSasTestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Container(ContainerSasTestViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var container = new BlobContainerClient(new Uri(model.SasUrl.Trim()));

            await foreach (var item in container.GetBlobsAsync())
            {
                model.Blobs.Add(new ContainerBlobItem
                {
                    Name = item.Name,
                    Size = item.Properties.ContentLength,
                    Url = container.GetBlobClient(item.Name).Uri.ToString()
                });
            }

            model.Success = true;
            model.ResultMessage = $"Знайдено файлів: {model.Blobs.Count}";
        }
        catch (RequestFailedException ex)
        {
            model.Success = false;
            model.StatusCode = ex.Status;
            model.ErrorCode = ex.ErrorCode;
            model.ResultMessage = ex.Message.Split('\n')[0];
        }
        catch (UriFormatException)
        {
            model.Success = false;
            model.ResultMessage = "Це не схоже на коректне посилання.";
        }

        return View(model);
    }
}