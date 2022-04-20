namespace TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings
{
    public class SizeSettings
    {
        public SizeSettings()
        {

        }

        public SizeSettings(int? width, int? height)
        {
            Width = width ?? default;
            Height = height ?? default;
        }


        public int Height { get; set; }
        public int Width { get; set; }
    }
}
