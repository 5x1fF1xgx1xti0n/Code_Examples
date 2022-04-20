namespace TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings
{
    public class ImageProcessSettings
    {
        public ImageProcessSettings()
        {
            ResizeSettings = new SizeSettings();
            CropSettings = new CropSettings();
            FitSettings = new FitSettings();
        }
        public ImageProcessSettings(SizeSettings resizeSettings, CropSettings cropSettings, FitSettings fitSettings)
        {
            ResizeSettings = resizeSettings;
            CropSettings = cropSettings;
            FitSettings = fitSettings;
        }

        public SizeSettings ResizeSettings { get; set; }
        public CropSettings CropSettings { get; set; }
        public FitSettings FitSettings { get; set; }
    }
}
