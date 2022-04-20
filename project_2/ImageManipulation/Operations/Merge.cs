namespace TVC.ImageServer.ImageManipulation.Operations
{
    using TVC.ImageServer.ImageManipulation.Models;
    using TVC.ImageServer.ImageManipulation.ImageProcessingWrapper;
    using System;

    public class Merge
    {
        public ImageUnderProcess Process(MergeModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var left = ImageProcessingWrapper.Resize(model.Left, 600, 600);
            var right = ImageProcessingWrapper.Resize(model.Right, 600, 600);
            var background = ImageProcessingWrapper.Resize(model.Background, 1800, 1200);

            var result = background;

            result = ImageProcessingWrapper.DrawImage(result, left, 150, 300);
            result = ImageProcessingWrapper.DrawImage(result, right, 1050, 300);
            result = ImageProcessingWrapper.DrawText(result, model.Text, 825, 525);

            return new ImageUnderProcess()
            {
                ImageBytes = result,
                Size = ImageProcessingWrapper.GetSize(result)
            };
        }
    }
}
