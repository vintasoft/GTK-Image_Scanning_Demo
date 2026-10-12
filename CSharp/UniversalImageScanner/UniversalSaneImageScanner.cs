#if !DOTNET_FRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using Vintasoft.Primitives;
using Vintasoft.Sane;
using Vintasoft.Sane.ImageEncoders;

namespace Vintasoft.UniversalImageScanner
{
    /// <summary>
    /// The universal SANE image scanner.
    /// </summary>
    public class UniversalSaneImageScanner : IUniversalImageScanner
    {

        SaneLocalDevice _saneDevice;
        List<SaneAcquiredImage> _acquiredImages;



        public UniversalSaneImageScanner(SaneLocalDevice saneDevice)
        {
            _saneDevice = saneDevice;
            _acquiredImages = new List<SaneAcquiredImage>();
        }



        /// <summary>
        /// Gets the name of image scanner.
        /// </summary>
        public string Name
        {
            get { return _saneDevice.Name; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether image scanner should show the UI.
        /// </summary>
        public bool ShowUI
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Gets a value indicating whether image scanner has the flated.
        /// </summary>
        public bool HasFlatbed
        {
            get { return _saneDevice.HasFlatbed; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether flatbed is enabled.
        /// </summary>
        public bool IsFlatbedEnabled
        {
            get { return _saneDevice.IsFlatbedEnabled; }
            set
            {

                try
                {
                    _saneDevice.IsFlatbedEnabled = !value;
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
            get { return _saneDevice.HasFeeder; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the automatic document feeder is enabled.
        /// </summary>
        public bool IsFeederEnabled
        {
            get { return _saneDevice.IsFeederEnabled; }
            set { _saneDevice.IsFeederEnabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the automatic document feeder supports duplex scanning.
        /// </summary>
        public bool HasDuplex
        {
            get { return _saneDevice.HasDuplex; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the duplex scanning is enabled.
        /// </summary>
        public bool IsDuplexEnabled
        {
            get { return _saneDevice.IsDuplexEnabled; }
            set { _saneDevice.IsDuplexEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the pixel type for scanned images.
        /// </summary>
        public ImagePixelType ImagePixelType
        {
            get
            {
                string scanMode = _saneDevice.ScanMode;
                switch (scanMode)
                {
                    case "Lineart":
                    case "Black & White":
                    case "Mono":
                        return ImagePixelType.BlackWhite;
                    case "Gray":
                    case "Gray[Error Diffusion]":
                    case "True Gray":
                        return ImagePixelType.Grayscale;
                }
                return ImagePixelType.RGB;
            }
            set
            {
                string[] supportedScanModes = _saneDevice.GetSupportedScanModes();
                switch (value)
                {
                    case ImagePixelType.BlackWhite:
                        foreach (string scanMode in supportedScanModes)
                        {
                            if (scanMode.ToLowerInvariant().Contains("lineart") || scanMode.ToLowerInvariant().Contains("black") ||
                                scanMode.ToLowerInvariant().Contains("mono"))
                            {
                                _saneDevice.ScanMode = scanMode;
                                break;
                            }
                        }
                        break;

                    case ImagePixelType.Grayscale:
                        foreach (string scanMode in supportedScanModes)
                        {
                            if (scanMode.ToLowerInvariant().Contains("gray"))
                            {
                                _saneDevice.ScanMode = scanMode;
                                break;
                            }
                        }
                        break;

                    case ImagePixelType.RGB:
                        foreach (string scanMode in supportedScanModes)
                        {
                            if (scanMode.ToLowerInvariant().Contains("color"))
                            {
                                _saneDevice.ScanMode = scanMode;
                                break;
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal resolution for scanned images.
        /// </summary>
        public int ScanXResolution
        {
            get { return (int)_saneDevice.ScanResolution; }
            set { _saneDevice.ScanResolution = value; }
        }

        /// <summary>
        /// Gets or sets the vertical resolution for scanned images.
        /// </summary>
        public int ScanYResolution
        {
            get { return (int)_saneDevice.ScanResolution; }
            set { _saneDevice.ScanResolution = value; }
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
            _saneDevice.Open();
        }

        /// <summary>
        /// Returns the image layout, in inches, of image scanner.
        /// </summary>
        /// <returns>The image layout, in inches</returns>
        public VintasoftRect GetImageLayout()
        {
            const double MILLIMETERS_TO_INCHES = 25.4;
            VintasoftRect scanArea = _saneDevice.ScanArea;
            return new VintasoftRect(
                scanArea.X / MILLIMETERS_TO_INCHES, scanArea.Y / MILLIMETERS_TO_INCHES,
                scanArea.Width / MILLIMETERS_TO_INCHES, scanArea.Height / MILLIMETERS_TO_INCHES);
        }

        /// <summary>
        /// Sets the image layout for image scanner.
        /// </summary>
        /// <param name="imageLayout">The image layout in inches.</param>
        public void SetImageLayout(VintasoftRect imageLayout)
        {
            const double MILLIMETERS_TO_INCHES = 25.4;
            _saneDevice.ScanArea = new VintasoftRectF(
                (float)(imageLayout.X * MILLIMETERS_TO_INCHES), (float)(imageLayout.Y * MILLIMETERS_TO_INCHES),
                (float)(imageLayout.Width * MILLIMETERS_TO_INCHES), (float)(imageLayout.Height * MILLIMETERS_TO_INCHES));
        }

        /// <summary>
        /// Acquires one image from image scanner.
        /// </summary>
        /// <returns>True - image is acquired; False - image is not acquired.</returns>
        public bool AcquireImageSync()
        {
            while (true)
            {
                SaneAcquiredImage acquiredImage = _saneDevice.AcquireImageSync();
                if (acquiredImage == null)
                    return false;

                _acquiredImages.Add(acquiredImage);
                return true;
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

            SaneImageEncoderSettings encoderSettings = null;
            switch (Path.GetExtension(filename).ToLowerInvariant())
            {
                case ".jpg":
                    encoderSettings = new SaneJpegEncoderSettings();
                    break;
                case ".tif":
                    encoderSettings = new SaneTiffEncoderSettings();
                    ((SaneTiffEncoderSettings)encoderSettings).TiffMultiPage = true;
                    break;
                case ".pdf":
                    encoderSettings = new SanePdfEncoderSettings();
                    ((SanePdfEncoderSettings)encoderSettings).PdfMultiPage = true;
                    break;
                case ".bmp":
                    encoderSettings = new SaneBmpEncoderSettings();
                    break;
                default:
                    encoderSettings = new SanePngEncoderSettings();
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
            _acquiredImages[imageIndex].Save(mem, new SanePngEncoderSettings());
            return mem;
        }

        /// <summary>
        /// Clears the scanned images.
        /// </summary>
        public void ClearImages()
        {
            if (_acquiredImages != null)
            {
                _acquiredImages.Clear();
            }
        }

        /// <summary>
        /// Closes the image scanner.
        /// </summary>
        public void Close()
        {
            _saneDevice.Close();
        }

        /// <summary>
        /// Disposes used resources.
        /// </summary>
        public void Dispose()
        {
            if (_acquiredImages != null)
            {
                _acquiredImages.Clear();
                _acquiredImages = null;
            }
        }

    }
}
#endif