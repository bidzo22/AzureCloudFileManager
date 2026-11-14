using AzureCloudFileManager.Services;
using Microsoft.AspNetCore.Mvc;
using AzureCloudFileManager.Models;

namespace AzureCloudFileManager.Controllers;

public class FilesController : Controller
{
    private readonly BlobStorageService _storage;

    public FilesController(BlobStorageService storage)
    {
        _storage = storage;
    }

    public async Task<IActionResult> Index()
    {
        var files = await _storage.ListAsync();
        return View(files);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            await _storage.UploadAsync(file);
            TempData["Message"] = $"Файл «{file.FileName}» завантажено.";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Download(string name)
    {
        var (content, contentType) = await _storage.DownloadAsync(name);
        return File(content, contentType ?? "application/octet-stream", name);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string name)
    {
        await _storage.DeleteAsync(name);
        TempData["Message"] = $"Файл «{name}» видалено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Sas(string name)
    {
        return View(new BlobSasViewModel { BlobName = name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Sas(BlobSasViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var expiresOn = DateTimeOffset.UtcNow.AddMinutes(model.ExpiresInMinutes);
        model.SasUrl = _storage.GenerateBlobSas(model.BlobName, model.Permission, expiresOn).ToString();
        model.ExpiresOn = expiresOn;

        return View(model);
    }
    
    [HttpGet]
    public IActionResult ContainerSas()
    {
        return View(new ContainerSasViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ContainerSas(ContainerSasViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var expiresOn = DateTimeOffset.UtcNow.AddMinutes(model.ExpiresInMinutes);
        model.SasUrl = _storage.GenerateContainerSas(model.Permission, expiresOn).ToString();
        model.ExpiresOn = expiresOn;

        return View(model);
    }
}