using System;
using System.IO;

namespace CDN.NET.Wrapper.Models
{
    public class UploadFileInfo
    {
        public string PathToFile { get; private set; }
        public bool HasPath => !string.IsNullOrWhiteSpace(this.PathToFile);
        public FileStream FileStream { get; private set; }
        public bool HasFileStream => FileStream != null && this.FileStream.CanRead;
        public string Name { get; set; }
        public bool IsPublic { get; set; } = true;
        public int? AlbumId { get; set; }

        public UploadFileInfo(string pathToFile)
        {
            if (!File.Exists(pathToFile))
            {
                throw new FileNotFoundException($"File could not be found: {pathToFile}");
            }

            this.PathToFile = pathToFile;
        }

        public UploadFileInfo(FileStream fileStream)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            if (!fileStream.CanRead)
            {
                throw new ArgumentException("Cannot read from file stream");
            }

            this.FileStream = fileStream;
        }
    }
}