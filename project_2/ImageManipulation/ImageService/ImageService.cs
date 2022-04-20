namespace TVC.ImageServer.ImageManipulation.ImageService
{
    using TVC.ImageServer.ImageManipulation.ImagePipeline;
    using TVC.ImageServer.ImageManipulation.Models;
    using TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings;
    using TVC.ImageServer.ImageManipulation.ImageProcessingWrapper;
    using TVC.ImageServer.ImageManipulation.Operations;
    using Microsoft.Extensions.Logging;

    public class ImageService : IImageService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ImageService> _log;

        public ImageService(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _log = _loggerFactory.CreateLogger<ImageService>();
        }

        public byte[] ProcessImage(byte[] image, ImageProcessSettings imageProcessSettings)
        {
            var pipeRunner = new ImageUnderProcess() { ImageBytes = image, Size = ImageProcessingWrapper.GetSize(image) };
            var pipeline = new ImageProcessPipeline(imageProcessSettings, _loggerFactory).Construct();

            pipeRunner = pipeline.Process(pipeRunner);
            pipeline.Clear();

            _log.LogInformation("Process is finished");

            return pipeRunner.ImageBytes;
        }

        public byte[] MergeImages(byte[] background, byte[] left, byte[] right, string text)
        {
            var mergeModel = new MergeModel()
            {
                Background = background,
                Left = left,
                Right = right,
                Text = text
            };

            var merge = new Merge();
            byte[] bytes = merge.Process(mergeModel).ImageBytes;

            _log.LogInformation("Merge is finished");

            return bytes;
        }

        public byte[] MakeThumb(byte[] image, SizeSettings thumbSize)
        {

            byte[] bytes = ImageProcessingWrapper.Resize(image, thumbSize.Width, thumbSize.Height);

            _log.LogInformation("Thumb is made");

            return bytes;
        }
    }
}
