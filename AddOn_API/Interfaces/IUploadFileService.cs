namespace AddOn_API.Interfaces
{
    public interface IUploadFileService
    {
         bool IsUpload(List<IFormFile> formFiles);

         string Validation(List<IFormFile> formFiles);
         Task<List<String>> UploadFile(List<IFormFile> formFiles,string path);

         MemoryStream DownloadFile(string fullpart,string filename);
    }
}