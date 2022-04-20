namespace TVC.ImageServer.API.Storage
{
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using TVC.ImageServer.API.ApiModels.Views;

    public class AzureAccessor : IStorage
    {        
        private readonly CloudStorageAccount _account;
        private readonly CloudBlobContainer _blobContainer;
        private readonly ILogger _log;

        public AzureAccessor(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger<AzureAccessor>();
            _account = Authenticate(Startup.ConnectionString);
            _blobContainer = GetBlobContainer(_account);
        }

        public Uri StructuredUpload(byte[] original, byte[] processed, byte[] thumb, string target)
        {
            var thumbName = target.Split(@"\")[1].Split(".")[0] + "_thumb." + target.Split(".")[1];

            UploadFile(original, @"images\original\" + target.Split(@"\")[1]);
            UploadFile(thumb, @"images\thumbs\" + thumbName);
            return UploadFile(processed, @"images\" + target);
        }

        public Uri UploadFile(byte[] fileBytes, string target)
        {
            string[] directory = target.Split(@"\");
            var blobDirectory = GetDirectory(_blobContainer, directory);

            string name = directory[directory.Length - 1];
            return CreateBlob(blobDirectory, fileBytes, name);
        }

        public List<UriListItem> GetFiles()
        {
            CloudBlobDirectory clientsDirectory = _blobContainer.GetDirectoryReference("images");
            CloudBlobDirectory thumbDirectory = _blobContainer.GetDirectoryReference("images/thumbs");

            var thumbs = thumbDirectory.ListBlobs()
                .OfType<CloudBlockBlob>()
                .OrderByDescending(b => b.Properties.LastModified)
                .ToList();

            var clients = clientsDirectory.ListBlobs()
                .OfType<CloudBlobDirectory>()
                .ToList();

            List<UriListItem> uris = new List<UriListItem>();
            List<CloudBlockBlob> blobList = new List<CloudBlockBlob>();
            foreach (var dir in clients)
            {
                if (dir.Prefix.EndsWith("original/") || dir.Prefix.EndsWith("thumbs/"))
                {
                    continue;
                }

                var blobs = dir.ListBlobs()
                    .OfType<CloudBlockBlob>()
                    .ToList();

                blobList.AddRange(blobs);
            }

            foreach (var item in thumbs)
            {

                var clientImageNameArray = item.Name.Split("/");
                var clientImageName = clientImageNameArray[clientImageNameArray.Length - 1];
                var client = blobList.Where(b => b.Name.EndsWith(clientImageName)).ToList()[0];
                var clientName = client.Name.Split("/")[1];

                var listItem = new UriListItem()
                {
                    Url = new Uri(@"api\v1\images\" + clientName + @"\" + clientImageName, UriKind.RelativeOrAbsolute),
                    Thumbnail = new Uri(@"api\v1\images\thumbs\" + clientImageName, UriKind.RelativeOrAbsolute)
                };

                uris.Add(listItem);
            }

            return uris;
        }

        public Stream GetImageStream(string path)
        {
            Stream stream = new MemoryStream();
            var item = _blobContainer.GetBlobReference(path);
            
            item.DownloadToStream(stream);
            return item.OpenRead(); 
        }

        public byte[] GetImageBytes(string path)
        {
            var item = _blobContainer.GetBlobReference(path);
            byte[] data = { };
            item.DownloadToByteArray(data, 0);
            return data;
        }

        public bool IsFileExists(string path)
        {
            var item = _blobContainer.GetBlobReference(path);
            return item.Exists();
        }

        private CloudStorageAccount Authenticate(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            CloudStorageAccount.TryParse(storageConnectionString, out storageAccount);
            _log.LogInformation("Authentication is successful");
            return storageAccount;
        }

        private CloudBlobContainer GetBlobContainer(CloudStorageAccount storageAccount)
        {
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

            if (cloudBlobContainer.Exists())
            {
                return cloudBlobContainer;
            }

            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob,
            };
            cloudBlobContainer.Create();
            cloudBlobContainer.SetPermissions(permissions);
            _log.LogInformation("New blob container is created");

            return cloudBlobContainer;
        }

        private CloudBlobDirectory GetDirectory(CloudBlobContainer container, string[] directory)
        {
            var currentDirectory = container.GetDirectoryReference(directory[0]);

            for (int i = 1; i < directory.Length - 1; ++i)
            {
                currentDirectory = currentDirectory.GetDirectoryReference(directory[i]);
            }

            return currentDirectory;
        }

        private Uri CreateBlob(CloudBlobDirectory directory, byte[] fileBytes, string name)
        {
            CloudBlockBlob cloudBlockBlob = directory.GetBlockBlobReference(name);
            cloudBlockBlob.UploadFromByteArray(fileBytes, 0, fileBytes.Length);
            _log.LogInformation("Blob is successfuly created");
            var uri = new Uri(@"api\v1\" + directory.Prefix + name, UriKind.RelativeOrAbsolute);
            return uri;
        }
    }
}
