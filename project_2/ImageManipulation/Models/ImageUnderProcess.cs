namespace TVC.ImageServer.ImageManipulation.Models
{
    using TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings;
    public class ImageUnderProcess
    {
        public byte[] ImageBytes { get; set; }
        public SizeSettings Size { get; set; }
    }
}
