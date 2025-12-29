namespace StudyRoomSystem.Server.Contacts;

public interface IFileService
{
    Task<string> Save(IFormFile file);
    Task Delete(string fileName);
}