using System.Data;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Api.V1;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/file")]
[ApiVersion("1.0")]
public class FileController : ControllerBase
{
    // TODO 之后要限制上传文件的类型和大小
    [HttpPost]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<UploadFileResponseOk>(StatusCodes.Status200OK)]
    [EndpointSummary("上传文件")]
    [EndpointDescription("使用该接口上传文件")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(new ProblemDetails() { Title = "文件流为空" });

        var fileId = Ulid.NewUlid().ToGuid();
        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{fileId}{fileExt}";
        var filePath = $"{HttpContext.Request.Path}/{fileName}";
        if (Directory.Exists("files") is false)
            Directory.CreateDirectory("files");
        await using var fs = new FileStream($"files/{fileName}", FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(fs);

        return Ok(new UploadFileResponseOk() { Url = filePath });
    }

    [HttpGet("{file:file}")]
    [EndpointSummary("获取指定路径下的文件")]
    [EndpointDescription("目前文件的路径为guid格式，文件也只能按照id查找")]
    public async Task<IActionResult> Get(string file)
    {
        var fileId = Guid.TryParse(file.Split('.')[0], out var id) ? id : Guid.Empty;
        if (fileId == Guid.Empty)
            throw new BadHttpRequestException("请求文件格式不正确");

        // var filePath = $"{Environment.CurrentDirectory}/files/{file}";
        var filePath = Path.Combine(Environment.CurrentDirectory, "files", file);
        if (System.IO.File.Exists(filePath) is false)
            throw new NotFoundException("文件不存在");

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(filePath, contentType);
    }


    [HttpDelete("{file:file}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("删除指定路径下的文件")]
    public async Task<IActionResult> Delete(string file)
    {
        throw new NotImplementedException();
    }
}