namespace TVC.ImageServer.ImageManipulation.Operations
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using TVC.ImageServer.ImageManipulation.Convertation;
    using TVC.ImageServer.ImageManipulation.ImagePipeline;
    using TVC.ImageServer.ImageManipulation.ImageProcessingWrapper;
    using TVC.ImageServer.ImageManipulation.Models;
    using TVC.ImageServer.ImageManipulation.Models.Errors;
    using TVC.ImageServer.ImageManipulation.Models.ForWrapper;
    using TVC.ImageServer.ImageManipulation.Models.ProcessSettings.ImageProcessSettings;

    public class Resize : IPipeOperation<ImageUnderProcess>
    {
        private const float HUNDRED_PERCENTS = 100;

        private readonly ImageProcessPipeline _pipe;
        private readonly ILogger<Resize> _log;

        private SizeSettings _currentSize;
        private SizeSettings _targetSize;
        private FitSettings _fitSettings;
        private byte[] _image;

        public Resize(ImageProcessPipeline pipe, ILoggerFactory loggerFactory)
        {
            _pipe = pipe;
            _log = loggerFactory.CreateLogger<Resize>();
        }

        public static bool IsNeeded(ImageProcessPipeline pipe)
        {
            var resizeSettings = pipe.Settings.ResizeSettings;
            if (resizeSettings == null)
            {
                return false;
            }
            if (resizeSettings.Width == 0 && resizeSettings.Height == 0)
            {
                return false;
            }
            return true;
        }

        public ImageUnderProcess Process(ImageUnderProcess imageModel)
        {
            if (imageModel == null || imageModel.ImageBytes == null)
            {
                throw new ArgumentNullException();
            }

            _targetSize = _pipe.Settings.ResizeSettings;    
            _currentSize = ImageProcessingWrapper.GetSize(imageModel.ImageBytes);
            _fitSettings = _pipe.Settings.FitSettings;
            _image = imageModel.ImageBytes;

            if (IsAspectRatioKept(_currentSize, _targetSize))
            {
                _image = ImageProcessingWrapper.Resize(_image, _targetSize.Width, _targetSize.Height);
                _currentSize = ImageProcessingWrapper.GetSize(_image);
            }
            else
            {
                Fit();
            }

            _log.LogInformation("Resize is successful");

            return new ImageUnderProcess() { ImageBytes = _image, Size = _currentSize };
        }

        private bool IsAspectRatioKept(SizeSettings initial, SizeSettings target)
        {
            float initialAR = (float)initial.Width / initial.Height;
            float targetAR = (float)target.Width / target.Height;
            return initialAR == targetAR;
        }

        private void Fit()
        {
            List<Action> actions = new List<Action>();
            actions.Add(() => Accommodate());
            actions.Add(() => Stretch());
            actions.Add(() => Crop());

            foreach (var func in actions)
            {
                if (_currentSize == _targetSize)
                {
                    return;
                }
                func();
            }
        }

        private void Accommodate()
        {
            var width = _targetSize.Width;
            var height = _targetSize.Height;
            float WAR = (float)width / _currentSize.Width;
            float HAR = (float)height / _currentSize.Height;

            if (WAR > HAR)
            {
                height = (int)(WAR * _currentSize.Height);
            }
            else
            {
                width = (int)(HAR * _currentSize.Width);
            }

            _image = ImageProcessingWrapper.Resize(_image, width, height);
            _currentSize.Width = width;
            _currentSize.Height = height;
        }

        private void Stretch()
        {
            var width = _currentSize.Width;
            var height = _currentSize.Height;
            float maxStretched = (HUNDRED_PERCENTS - _fitSettings.Stretch) / HUNDRED_PERCENTS;
            int toStretch = 0;

            if (_currentSize.Width == _targetSize.Width)
            {
                toStretch = (int)(height * maxStretched);
                height = toStretch < _targetSize.Height ? _targetSize.Height : toStretch;
            }
            else
            {
                toStretch = (int)(width * maxStretched);
                width = toStretch < _targetSize.Width ? _targetSize.Width : toStretch;
            }

            _image = ImageProcessingWrapper.Resize(_image, width, height);
            _currentSize.Width = width;
            _currentSize.Height = height;
        }

        private void Crop()
        {
            CropSettings area = MinimizeCrop();
            Rectangle rectangle = area.ToRectangle(_currentSize);
            _image = ImageProcessingWrapper.Crop(_image, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        private CropSettings MinimizeCrop() => 
            _currentSize.Width == _targetSize.Width 
            ? MinimizeCropByHeight() 
            : MinimizeCropByWidth();

        private CropSettings MinimizeCropByHeight()
        {
            var height = _currentSize.Height;

            var maxCroppedFromOneSide = height * (HUNDRED_PERCENTS - _fitSettings.Crop) / HUNDRED_PERCENTS;
            var maxCroppedFromTwoSides = height * (HUNDRED_PERCENTS - _fitSettings.Crop * 2) / HUNDRED_PERCENTS;

            var singleSideCropCondition = maxCroppedFromOneSide <= _targetSize.Height;
            var twoSideCropCondition = maxCroppedFromTwoSides <= _targetSize.Height;

            if (!twoSideCropCondition)
            {
                throw new ResizeException();
            }                

            var area = new CropSettings();

            area.Top = singleSideCropCondition
                ? (height - _targetSize.Height) * HUNDRED_PERCENTS / height
                : _fitSettings.Crop;

            area.Bottom = singleSideCropCondition
                ? 0
                : (height - _targetSize.Height) * HUNDRED_PERCENTS / height - _fitSettings.Crop;

            return area;
        }

        private CropSettings MinimizeCropByWidth()
        {
            var width = _currentSize.Width;

            var maxCroppedFromOneSide = width * (HUNDRED_PERCENTS - _fitSettings.Crop) / HUNDRED_PERCENTS;
            var maxCroppedFromTwoSides = width * (HUNDRED_PERCENTS - _fitSettings.Crop * 2) / HUNDRED_PERCENTS;

            var singleSideCropCondition = maxCroppedFromOneSide <= _targetSize.Width;
            var twoSideCropCondition = maxCroppedFromTwoSides <= _targetSize.Width;

            if (!twoSideCropCondition)
            {
                throw new ResizeException();
            }

            var area = new CropSettings();

            area.Right = singleSideCropCondition
                ? (width - _targetSize.Width) * HUNDRED_PERCENTS / width
                : _fitSettings.Crop;

            area.Left = singleSideCropCondition
                ? 0
                : (width - _targetSize.Width) * HUNDRED_PERCENTS / width - _fitSettings.Crop;

            return area;
        }
    }
}
