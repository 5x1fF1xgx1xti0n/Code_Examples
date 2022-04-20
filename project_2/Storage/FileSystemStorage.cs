namespace TVC.ImageServer.API.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using TVC.ImageServer.API.ApiModels.Views;

    public class FileSystemStorage : IStorage
    {
        public Uri StructuredUpload(byte[] original, byte[] processed, byte[] thumb, string target)
        {
            var thumbName = target.Split(@"\")[1].Split(".")[0] + "_thumb." + target.Split(".")[1];

            UploadFile(original, @"images\original\" + target.Split(@"\")[1]);
            UploadFile(thumb, @"images\thumbs\" + thumbName);
            return UploadFile(processed, @"images\" + target);
        }

        public Uri UploadFile(byte[] fileBytes, string target)
        {
            var path = Startup.ContentRoot + @"\" + target;
            DirectoryInfo origsDirectory = new DirectoryInfo(Startup.ContentRoot + @"\images\" + target.Split(@"\")[1]);
            if (!origsDirectory.Exists)
            {
                origsDirectory.Create();
            }
            
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(fileBytes, 0, fileBytes.Length);
            }

            return new Uri(@"api\v1\" + target, UriKind.RelativeOrAbsolute);
        }

        public List<UriListItem> GetFiles()
        {
            DirectoryInfo imagesDirectory = new DirectoryInfo(Startup.ContentRoot + @"\images");
            if (!imagesDirectory.Exists)
            {
                imagesDirectory.Create();
            }
            var clients = imagesDirectory.GetDirectories().OrderBy(p => p.CreationTime).ToList();
            
            DirectoryInfo thumbDirectory = new DirectoryInfo(Startup.ContentRoot + @"\images\thumbs");
            if (!thumbDirectory.Exists)
            {
                thumbDirectory.Create();
            }

            List<FileInfo> allFiles = new List<FileInfo>();

            foreach (DirectoryInfo client in clients)
            {
                if (client.Name == "original" || client.Name == "thumbs")
                {
                    continue;
                }

                var files = client.GetFiles();

                foreach (FileInfo f in files)
                {
                    allFiles.Add(f);
                }
            }

            allFiles = allFiles.OrderBy(f => f.CreationTimeUtc).Reverse().ToList();
            
            List<UriListItem> uris = new List<UriListItem>();

            foreach (FileInfo f in allFiles)
            {
                var thumbName = f.Name.Split(".")[0] + "_thumb." + f.Name.Split(".")[1];
                var clientName = f.DirectoryName.Split(@"\").Last();

                var listItem = new UriListItem()
                {
                    Url = new Uri(@"api\v1\images\" + clientName + @"\" + f.Name, UriKind.RelativeOrAbsolute),
                    Thumbnail = new Uri(@"api\v1\images\thumbs\" + thumbName, UriKind.RelativeOrAbsolute)
                };

                uris.Add(listItem);
            }

            return uris;
        }

        public Stream GetImageStream(string path)
        {
            path = Startup.ContentRoot + @"\" + path;
            return new FileStream(path, FileMode.Open);            
        }

        public byte[] GetImageBytes(string path)
        {
            path = Startup.ContentRoot + @"\images\" + path;
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            return new byte[0];
        }

        public bool IsFileExists(string path)
        {
            path = Startup.ContentRoot + @"\images\" + path;
            return File.Exists(path);
        }
    }
}
