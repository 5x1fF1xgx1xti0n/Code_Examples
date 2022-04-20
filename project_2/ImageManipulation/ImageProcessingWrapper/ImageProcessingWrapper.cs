namespace TVC.ImageServer.ImageManipulation.ImageProcessingWrapper
{
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;
    using System.IO;
    using TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings;

    public static class ImageProcessingWrapper
    {
        public static byte[] Resize(byte[] imageBytes, int width, int height)
        {
            var image = Image.Load(imageBytes);
            image.Mutate(ctx => ctx.Resize(width, height));
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, PngFormat.Instance);
                imageBytes = stream.ToArray();
            }

            return imageBytes;
        }

        public static byte[] Crop(byte[] imageBytes, int x, int y, int width, int height)
        {
            var image = Image.Load(imageBytes);
            var rect = new Rectangle(x, y, width, height);
            image.Mutate(ctx => ctx.Crop(rect));
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, PngFormat.Instance);
                imageBytes = stream.ToArray();
            }

            return imageBytes;
        }

        public static byte[] Stretch(byte[] imageBytes, int width, int height)
        {
            var image = Image.Load(imageBytes);

            ResizeOptions options = new ResizeOptions()
            {
                Mode = ResizeMode.Stretch,
                Position = AnchorPositionMode.Center,
                Size = new Size(width, height)
            };

            image.Mutate(ctx => ctx.Resize(options));

            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, PngFormat.Instance);
                imageBytes = stream.ToArray();
            }

            return imageBytes;
        }

        public static byte[] DrawImage(byte[] backgroundBytes, byte[] imageBytes, int x, int y)
        {
            Point position = new Point(x, y);
            var background = Image.Load(backgroundBytes);
            var image = Image.Load(imageBytes);

            background.Mutate(ctx => ctx.DrawImage(image, position, 1));

            using (MemoryStream stream = new MemoryStream())
            {
                background.Save(stream, PngFormat.Instance);
                return stream.ToArray();
            }
        }

        public static byte[] DrawText(byte[] backgroundBytes, string text, int x, int y)
        {
            var font = new Font(SystemFonts.CreateFont("Arial", 150, FontStyle.Bold), 150);
            var fontColor = new Rgba32(200, 200, 200);
            var background = Image.Load(backgroundBytes);
            Point position = new Point(x, y);

            background.Mutate(ctx => ctx.DrawText(text, font, fontColor, position));

            using (MemoryStream stream = new MemoryStream())
            {
                background.Save(stream, PngFormat.Instance);
                return stream.ToArray();
            }
        }

        public static byte[] CreateBlank(SizeSettings model)
        {
            var image = new Image<Rgba32>(model.Width, model.Height);
            byte[] imageBytes;
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, PngFormat.Instance);
                imageBytes = stream.ToArray();
            }
            return imageBytes;
        }

        public static SizeSettings GetSize(byte[] imageBytes)
        {
            var image = Image.Load(imageBytes);
            return new SizeSettings(image.Width, image.Height);
        }
    }
}
