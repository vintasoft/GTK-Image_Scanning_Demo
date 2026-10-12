using System;
using System.Collections.Generic;
using System.IO;
using Vintasoft.Primitives;
using Vintasoft.Twain;
using Vintasoft.Twain.ImageEncoders;

namespace Vintasoft.UniversalImageScanner
{
    /// <summary>
    /// The universal TWAIN image scanner.
    /// </summary>
    public class UniversalTwainImageScanner : IUniversalImageScanner
    {

        Device _twainDevice;
        List<AcquiredImage> _acquiredImages;



        public UniversalTwainImageScanner(Device twainDevice)
        {
            _twainDevice = twainDevice;
            _twainDevice.TransferMode = TransferMode.Native;

            _acquiredImages = new List<AcquiredImage>();
        }



        /// <summary>
        /// Gets the name of image scanner.
        /// </summary>
        public string Name
        {
            get { return _twainDevice.Info.ProductName; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether image scanner should show the UI.
        /// </summary>
        public bool ShowUI
        {
            get { return _twainDevice.ShowUI; }
            set { _twainDevice.ShowUI = value; }
        }

        /// <summary>
        /// Gets a value indicating whether image scanner has the flated.
        /// </summary>
        public bool HasFlatbed
        {
            get { return _twainDevice.HasFlatbed; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether flatbed is enabled.
        /// </summary>
        public bool IsFlatbedEnabled
        {
            get { return !_twainDevice.DocumentFeeder.Enabled; }
            set
            {
                try
                {
                    _twainDevice.DocumentFeeder.Enabled = !value;
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether image scanner has the automatic document feeder.
        /// </summary>
        public bool HasFeeder
        {
            get { return _twainDevice.HasFeeder; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the automatic document feeder is enabled.
        /// </summary>
        public bool IsFeederEnabled
        {
            get { return _twainDevice.DocumentFeeder.Enabled; }
            set
            {
                try
                {
                    _twainDevice.DocumentFeeder.Enabled = value;
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the automatic document feeder supports duplex scanning.
        /// </summary>
        public bool HasDuplex
        {
            get { return _twainDevice.DocumentFeeder.DuplexMode != DuplexMode.None; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the duplex scanning is enabled.
        /// </summary>
        public bool IsDuplexEnabled
        {
            get { return _twainDevice.DocumentFeeder.DuplexEnabled; }
            set { _twainDevice.DocumentFeeder.DuplexEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the pixel type for scanned images.
        /// </summary>
        public ImagePixelType ImagePixelType
        {
            get
            {
                PixelType pixelType = _twainDevice.PixelType;
                switch (pixelType)
                {
                    case PixelType.BW:
                        return ImagePixelType.BlackWhite;
                    case PixelType.Gray:
                        return ImagePixelType.Grayscale;
                }
                return ImagePixelType.RGB;
            }
            set
            {
                switch (value)
                {
                    case ImagePixelType.BlackWhite:
                        _twainDevice.PixelType = PixelType.BW;
                        break;
                    case ImagePixelType.Grayscale:
                        _twainDevice.PixelType = PixelType.Gray;
                        break;
                    case ImagePixelType.RGB:
                        _twainDevice.PixelType = PixelType.RGB;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal resolution for scanned images.
        /// </summary>
        public int ScanXResolution
        {
            get { return (int)_twainDevice.Resolution.XResolution; }
            set { _twainDevice.Resolution = new Resolution(value, ScanYResolution); }
        }

        /// <summary>
        /// Gets or sets the vertical resolution for scanned images.
        /// </summary>
        public int ScanYResolution
        {
            get { return (int)_twainDevice.Resolution.YResolution; }
            set { _twainDevice.Resolution = new Resolution(ScanXResolution, value); }
        }

        /// <summary>
        /// Gets the count of scanned images.
        /// </summary>
        public int ScannedImageCount
        {
            get { return _acquiredImages.Count; }
        }



        /// <summary>
        /// Opens the image scanner.
        /// </summary>
        public void Open()
        {
            _twainDevice.Open();
        }

        /// <summary>
        /// Returns the image layout, in inches, of image scanner.
        /// </summary>
        /// <returns>The image layout, in inches</returns>
        public VintasoftRect GetImageLayout()
        {
            _twainDevice.UnitOfMeasure = UnitOfMeasure.Inches;
            return _twainDevice.ImageLayout.Get();
        }

        /// <summary>
        /// Sets the image layout for image scanner.
        /// </summary>
        /// <param name="imageLayout">The image layout in inches.</param>
        public void SetImageLayout(VintasoftRect imageLayout)
        {
            _twainDevice.UnitOfMeasure = UnitOfMeasure.Inches;
            _twainDevice.ImageLayout.Set((float)imageLayout.Left, (float)imageLayout.Top, (float)imageLayout.Right, (float)imageLayout.Bottom);
        }

        /// <summary>
        /// Acquires one image from image scanner.
        /// </summary>
        /// <returns>True - image is acquired; False - image is not acquired.</returns>
        public bool AcquireImageSync()
        {
            while (true)
            {
                AcquireModalState acquiredModalState = _twainDevice.AcquireModal();
                if (acquiredModalState == AcquireModalState.ImageAcquired)
                {
                    _acquiredImages.Add(_twainDevice.AcquiredImage);
                    return true;
                }
                else
                {
                    if (acquiredModalState == AcquireModalState.None)
                        return false;
                }
            }
        }

        /// <summary>
        /// Saves image to a file.
        /// </summary>
        /// <param name="imageIndex">The index of image in image buffer.</param>
        /// <param name="filename">The name of file, where image must be saved.</param>
        public void SaveImage(int imageIndex, string filename)
        {
            if (imageIndex < 0 || imageIndex >= _acquiredImages.Count)
                throw new ArgumentOutOfRangeException();
            if (filename == null)
                throw new ArgumentNullException();

            TwainImageEncoderSettings encoderSettings = null;
            switch (Path.GetExtension(filename).ToLowerInvariant())
            {
                case ".jpg":
                    encoderSettings = new TwainJpegEncoderSettings();
                    break;
                case ".tif":
                    encoderSettings = new TwainTiffEncoderSettings();
                    ((TwainTiffEncoderSettings)encoderSettings).TiffMultiPage = true;
                    break;
                case ".pdf":
                    encoderSettings = new TwainPdfEncoderSettings();
                    ((TwainPdfEncoderSettings)encoderSettings).PdfMultiPage = true;
                    break;
                case ".bmp":
                    encoderSettings = new TwainBmpEncoderSettings();
                    break;
                default:
                    encoderSettings = new TwainPngEncoderSettings();
                    break;
            }

            _acquiredImages[imageIndex].Save(filename, encoderSettings);
        }

        /// <summary>
        /// Returns scanned image as a stream.
        /// </summary>
        /// <param name="imageIndex">The index of image in image buffer.</param>
        /// <returns>The memory stream that contains scanned image.</returns>
        public Stream GetImageAsStream(int imageIndex)
        {
            if (imageIndex < 0 || imageIndex >= _acquiredImages.Count)
                throw new ArgumentOutOfRangeException();

            MemoryStream mem = new MemoryStream();
            _acquiredImages[imageIndex].Save(mem, new TwainPngEncoderSettings());
            return mem;
        }

        /// <summary>
        /// Clears the scanned images.
        /// </summary>
        public void ClearImages()
        {
            if (_acquiredImages != null)
            {
                for (int i = 0; i < _acquiredImages.Count; i++)
                {
                    _acquiredImages[i].Dispose();
                }
                _acquiredImages.Clear();
            }
        }

        /// <summary>
        /// Closes the image scanner.
        /// </summary>
        public void Close()
        {
            _twainDevice.Close();
        }

        /// <summary>
        /// Disposes used resources.
        /// </summary>
        public void Dispose()
        {
            ClearImages();
        }

    }
}
