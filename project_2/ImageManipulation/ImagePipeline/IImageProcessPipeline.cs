namespace TVC.ImageServer.ImageManipulation.ImagePipeline
{
    using TVC.ImageServer.ImageManipulation.Models;
    public interface IImageProcessPipeline
    {
        ImageProcessPipeline Construct();
        ImageUnderProcess Process(ImageUnderProcess imageBytes);
        void Clear();
    }
}
