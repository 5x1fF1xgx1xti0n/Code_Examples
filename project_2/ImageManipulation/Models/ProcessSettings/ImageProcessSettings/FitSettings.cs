namespace TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings
{
    public class FitSettings
    {
        public FitSettings()
        {
            Stretch = default;
            Crop = default;
        }

        public FitSettings(float? stretch, float? crop)
        {
            Stretch = stretch ?? default;
            Crop = crop ?? default;
        }

        public float Stretch { get; set; }
        public float Crop { get; set; }
    }
}
