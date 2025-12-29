using Ardalis.GuardClauses;
using StudyRoomSystem.Server.Contacts;

namespace StudyRoomSystem.Server.Services;

public class FileService : IFileService
{
    public async Task<string> Save(IFormFile file)
    {
        Guard.Against.NegativeOrZero(file.Length);

        var fileId = Ulid.NewUlid().ToGuid();
        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{fileId}{fileExt}";
        if (Directory.Exists("files") is false)
            Directory.CreateDirectory("files");
        await using var fs = new FileStream(Path.Combine("files",fileName), FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(fs);
        
        return fileName;
    }
    
    
    
    public Task Delete(string fileName)
    {
        if (Directory.Exists("files") is false)
            Directory.CreateDirectory("files");
        File.Delete(Path.Combine("files",fileName));
        return Task.CompletedTask;
    }
}