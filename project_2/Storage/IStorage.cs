namespace TVC.ImageServer.API.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TVC.ImageServer.API.ApiModels.Views;

    public interface IStorage
    {
        Uri StructuredUpload(byte[] original, byte[] processed, byte[] thumb, string target);
        Uri UploadFile(byte[] fileBytes, string target);
        List<UriListItem> GetFiles();
        Stream GetImageStream(string path);
        byte[] GetImageBytes(string path);
        bool IsFileExists(string path);
    }
}
