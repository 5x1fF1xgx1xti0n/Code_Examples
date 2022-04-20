namespace TVC.ImageServer.ImageManipulation.Convertation
{
    using TVC.ImageServer.ImageManipulation.Models.ForWrapper;
    using TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings;

    public static class Converter
    {
        private const float HUNDRED_PERCENTS = 100;

        public static Rectangle ToRectangle(this CropSettings crop, SizeSettings size)
        {
            int x = (int)(size.Width * crop.Left / HUNDRED_PERCENTS);
            int y = (int)(size.Height * crop.Top / HUNDRED_PERCENTS);
            int width = (int)(size.Width * (HUNDRED_PERCENTS - crop.Right - crop.Left) / HUNDRED_PERCENTS);
            int height = (int)(size.Height * (HUNDRED_PERCENTS - crop.Bottom - crop.Top) / HUNDRED_PERCENTS);

            return new Rectangle(x, y, width, height);
        }
    }
}
