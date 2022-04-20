namespace TVC.ImageServer.ImageManipulation.Operations
{
    using Microsoft.Extensions.Logging;
    using System;
    using TVC.ImageServer.ImageManipulation.Convertation;
    using TVC.ImageServer.ImageManipulation.ImagePipeline;
    using TVC.ImageServer.ImageManipulation.ImageProcessingWrapper;
    using TVC.ImageServer.ImageManipulation.Models;

    public class Crop : IPipeOperation<ImageUnderProcess>
    {
        private readonly ImageProcessPipeline _pipe;
        private readonly ILogger<Crop> _log;

        public Crop(ImageProcessPipeline pipe, ILoggerFactory loggerFactory)
        {
            _pipe = pipe;
            _log = loggerFactory.CreateLogger<Crop>();
        }

        public static bool IsNeeded(ImageProcessPipeline pipe)
        {
            var cropSettings = pipe.Settings.CropSettings;

            if (cropSettings == null)
            {
                return false;
            }
            if (cropSettings.Top == 0 && cropSettings.Bottom == 0 && cropSettings.Right == 0 && cropSettings.Left == 0)
            {
                return false;
            }
            return true;
        }

        public ImageUnderProcess Process(ImageUnderProcess imageModel)
        {
            if (imageModel == null || imageModel.ImageBytes == null)
            {
                throw new ArgumentNullException();
            }

            var rect = _pipe.Settings.CropSettings.ToRectangle(imageModel.Size);

            byte[] imageBytes = ImageProcessingWrapper.Crop(imageModel.ImageBytes, rect.X, rect.Y, rect.Width, rect.Height);
            var size = ImageProcessingWrapper.GetSize(imageBytes);

            _log.LogInformation("Crop is successful");

            return new ImageUnderProcess() { ImageBytes = imageBytes, Size = size };
        }
    }
}
