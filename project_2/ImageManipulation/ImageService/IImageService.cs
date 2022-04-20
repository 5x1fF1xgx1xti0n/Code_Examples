namespace TVC.ImageServer.ImageManipulation.ImageService
{
    using TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings;

    public interface IImageService
    {
        byte[] ProcessImage(byte[] image, ImageProcessSettings imageProcessSettings);
        byte[] MergeImages(byte[] background, byte[] left, byte[] right, string text);
        byte[] MakeThumb(byte[] image, SizeSettings thumbSize);
    }
}
