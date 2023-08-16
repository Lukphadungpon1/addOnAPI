using System.Security;
using System.Linq;
using AddOn_API.Interfaces;

namespace AddOn_API.Services
{
    public class UploadFileService : IUploadFileService
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment webHostEnvironment;
        public UploadFileService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.configuration = configuration;

        }

        public bool IsUpload(List<IFormFile> formFiles)
        {
            if (formFiles != null && formFiles.Sum(f => f.Length) > 0)
                return true;
            else
                return false;
        }
        public string Validation(List<IFormFile> formFiles)
        {
            foreach (var formFile in formFiles)
            {
                if(!ValidationExtension(formFile.FileName)){
                    return "Invalid file extension";
                }
                if (!ValidationSize(formFile.Length)){
                    return "The file is too long";
                }
            }
            return null;
        }
        public async Task<List<string>> UploadFile(List<IFormFile> formFiles,string path)
        {
            List<string> listFileName = new List<string>();
            string uploadPath = $"{webHostEnvironment.WebRootPath}/{path}";

            foreach (var formFile in formFiles){
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                string fullPath = uploadPath + fileName;
                using( var stream = File.Create(fullPath)){
                    await formFile.CopyToAsync(stream);
                }
                listFileName.Add(fileName);
            }
            return listFileName;

        }


        public bool ValidationExtension(string fileName)
        {
            string[] permittedExtension = { ".xlsx" };

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (String.IsNullOrEmpty(ext) || !permittedExtension.Contains(ext))
            {
                return false;
            }
            return true;
        }

        public bool ValidationSize(long fileSize) => configuration.GetValue<long>("FileSizeLimit") > fileSize;

        public MemoryStream DownloadFile(string fullpart,string filename)
        {
            string downloadPath = $"{webHostEnvironment.WebRootPath}/{fullpart}/{filename}";
            var memory = new MemoryStream();
            if (System.IO.File.Exists(downloadPath)){
                var net = new System.Net.WebClient();
                var data = net.DownloadData(downloadPath);
                var Content = new System.IO.MemoryStream(data);
                memory = Content;
            }
            memory.Position = 0;
            return memory;

        }
    }
}