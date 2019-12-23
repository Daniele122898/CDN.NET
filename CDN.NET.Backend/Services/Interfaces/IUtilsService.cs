namespace CDN.NET.Backend.Services.Interfaces
{
    public interface IUtilsService
    {
        string GenerateFilePath(string fileId, string fileExtension);
    }
}