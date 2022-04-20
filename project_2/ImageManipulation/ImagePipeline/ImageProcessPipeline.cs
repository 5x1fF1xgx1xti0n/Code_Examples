namespace TVC.ImageServer.ImageManipulation.ImagePipeline
{
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using TVC.ImageServer.ImageManipulation.Models;
    using TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings;
    using TVC.ImageServer.ImageManipulation.Operations;

    public class ImageProcessPipeline : IImageProcessPipeline
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ImageProcessPipeline> _log;
        protected readonly List<IPipeOperation<ImageUnderProcess>> _operations = new List<IPipeOperation<ImageUnderProcess>>();
        protected readonly ImageProcessSettings _settings;

        public ImageProcessSettings Settings { get { return _settings; } }

        public ImageProcessPipeline(ImageProcessSettings settings, ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _loggerFactory = loggerFactory;
            _log = _loggerFactory.CreateLogger<ImageProcessPipeline>();
        }

        public ImageProcessPipeline Construct()
        {
            if (Settings == null)
            {
                return this;
            }

            if (Crop.IsNeeded(this))
            {
                _operations.Add(new Crop(this, _loggerFactory));
            }
            if (Resize.IsNeeded(this))
            {
                _operations.Add(new Resize(this, _loggerFactory));
            }

            return this;
        }

        public ImageUnderProcess Process(ImageUnderProcess imageBytes)
        {
            foreach (var operation in _operations)
            {
                imageBytes = operation.Process(imageBytes);
            }
            
            return imageBytes;
        }

        public void Clear()
        {
            _operations.Clear();
        }
    }
}
