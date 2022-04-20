namespace TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings
{
    public class CropSettings
    {
        public CropSettings()
        {
            Top = default;
            Bottom = default;
            Right = default;
            Left = default;
        }

        public CropSettings(float? top, float? bottom, float? right, float? left)
        {
            Top = top ?? default;
            Bottom = bottom ?? default;
            Right = right ?? default;
            Left = left ?? default;
        }

        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Right { get; set; }
        public float Left { get; set; }
    }
}
