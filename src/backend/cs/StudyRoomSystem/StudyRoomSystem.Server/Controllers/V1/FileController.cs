using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/file")]
[ApiVersion("1.0")]
public class FileController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest();

        var fileId = Ulid.NewUlid().ToGuid();
        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{fileId}{fileExt}";
        var filePath = $"{HttpContext.Request.Path}/{fileName}";
        if (Directory.Exists("files") is false)
            Directory.CreateDirectory("files");
        await using var fs = new FileStream($"files/{fileName}", FileMode.Create,FileAccess.Write);
        await file.CopyToAsync(fs);

        return Ok(new { url = filePath });
    }

    [HttpGet("{file:file}")]
    public async Task<IActionResult> Get(string file)
    {
        var fileId = Guid.TryParse(file.Split('.')[0], out var id) ? id : Guid.Empty;
        if (fileId == Guid.Empty)
            return BadRequest(new { message = "请求文件格式正不正确" });

        // var filePath = $"{Environment.CurrentDirectory}/files/{file}";
        var filePath = Path.Combine(Environment.CurrentDirectory, "files", file);
        if (System.IO.File.Exists(filePath) is false)
            return NotFound(new { message = "文件不存在" });

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath,out var contentType))
        {
            contentType = "application/octet-stream";
        }
        
        return PhysicalFile(filePath, contentType);
    }
}