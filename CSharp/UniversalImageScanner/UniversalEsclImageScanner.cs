using System;
using System.Collections.Generic;
using System.IO;
using Vintasoft.EsclImageScanning;
using Vintasoft.EsclImageScanning.ImageEncoders;
using Vintasoft.Primitives;

namespace Vintasoft.UniversalImageScanner
{
    /// <summary>
    /// The universal eSCL image scanner.
    /// </summary>
    public class UniversalEsclImageScanner : IUniversalImageScanner
    {

        EsclDevice _esclDevice;
        List<EsclAcquiredImage> _acquiredImages;



        public UniversalEsclImageScanner(EsclDevice esclDevice)
        {
            _esclDevice = esclDevice;
            _acquiredImages = new List<EsclAcquiredImage>();
        }



        /// <summary>
        /// Gets the name of image scanner.
        /// </summary>
        public string Name
        {
            get { return _esclDevice.Name; }
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
            get { return _esclDevice.HasFlatbed; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether flatbed is enabled.
        /// </summary>
        public bool IsFlatbedEnabled
        {
            get { return _esclDevice.IsFlatbedEnabled; }
            set { _esclDevice.IsFlatbedEnabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether image scanner has the automatic document feeder.
        /// </summary>
        public bool HasFeeder
        {
            get { return _esclDevice.HasFeeder; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the automatic document feeder is enabled.
        /// </summary>
        public bool IsFeederEnabled
        {
            get { return _esclDevice.IsFeederEnabled; }
            set { _esclDevice.IsFeederEnabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the automatic document feeder supports duplex scanning.
        /// </summary>
        public bool HasDuplex
        {
            get { return _esclDevice.HasDuplex; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the duplex scanning is enabled.
        /// </summary>
        public bool IsDuplexEnabled
        {
            get { return _esclDevice.IsDuplexEnabled; }
            set { _esclDevice.IsDuplexEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the pixel type for scanned images.
        /// </summary>
        public ImagePixelType ImagePixelType
        {
            get
            {
                EsclScanColorMode? colorMode = _esclDevice.ScanColorMode;
                if (colorMode.HasValue)
                {
                    switch (colorMode.Value)
                    {
                        case EsclScanColorMode.BlackAndWhite1:
                            return ImagePixelType.BlackWhite;
                        case EsclScanColorMode.Grayscale8:
                            return ImagePixelType.Grayscale;
                    }
                }
                return ImagePixelType.RGB;
            }
            set
            {
                switch (value)
                {
                    case ImagePixelType.BlackWhite:
                        _esclDevice.ScanColorMode = EsclScanColorMode.BlackAndWhite1;
                        break;
                    case ImagePixelType.Grayscale:
                        _esclDevice.ScanColorMode = EsclScanColorMode.Grayscale8;
                        break;
                    case ImagePixelType.RGB:
                        _esclDevice.ScanColorMode = EsclScanColorMode.RGB24;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal resolution for scanned images.
        /// </summary>
        public int ScanXResolution
        {
            get
            {
                if (_esclDevice.ScanXResolution.HasValue)
                {
                    return _esclDevice.ScanXResolution.Value;
                }
                return 0;
            }
            set { _esclDevice.ScanXResolution = value; }
        }

        /// <summary>
        /// Gets or sets the vertical resolution for scanned images.
        /// </summary>
        public int ScanYResolution
        {
            get
            {
                if (_esclDevice.ScanYResolution.HasValue)
                {
                    return _esclDevice.ScanYResolution.Value;
                }
                return 0;
            }
            set { _esclDevice.ScanYResolution = value; }
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
            _esclDevice.Open();
        }

        /// <summary>
        /// Returns the image layout, in inches, of image scanner.
        /// </summary>
        /// <returns>The image layout, in inches</returns>
        public VintasoftRect GetImageLayout()
        {
            VintasoftRect[] scanRegions = _esclDevice.GetScanRegions(EsclScanUnitOfMeasure.Inches);
            if (scanRegions.Length > 0)
                return scanRegions[0];
            return new VintasoftRect();
        }

        /// <summary>
        /// Sets the image layout for image scanner.
        /// </summary>
        /// <param name="imageLayout">The image layout in inches.</param>
        public void SetImageLayout(VintasoftRect imageLayout)
        {
            _esclDevice.ClearScanRegions();
            _esclDevice.AddScanRegion(imageLayout, EsclScanUnitOfMeasure.Inches);
        }

        /// <summary>
        /// Acquires one image from image scanner.
        /// </summary>
        /// <returns>True - image is acquired; False - image is not acquired.</returns>
        public bool AcquireImageSync()
        {
            EsclAcquiredImage acquiredImage = null;
            if (this.ScanXResolution > 300)
                acquiredImage = _esclDevice.AcquireImageSync(EsclScanDocumentFormat.JPEG);
            else
                acquiredImage = _esclDevice.AcquireImageSync();
            if (acquiredImage == null)
                return false;

            _acquiredImages.Add(acquiredImage);
            return true;
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

            EsclImageEncoderSettings encoderSettings = null;
            switch (Path.GetExtension(filename).ToLowerInvariant())
            {
                case ".jpg":
                    encoderSettings = new EsclJpegEncoderSettings();
                    break;
                case ".tif":
                    encoderSettings = new EsclTiffEncoderSettings();
                    ((EsclTiffEncoderSettings)encoderSettings).TiffMultiPage = true;
                    break;
                case ".pdf":
                    encoderSettings = new EsclPdfEncoderSettings();
                    ((EsclPdfEncoderSettings)encoderSettings).PdfMultiPage = true;
                    break;
                case ".bmp":
                    encoderSettings = new EsclBmpEncoderSettings();
                    break;
                default:
                    encoderSettings = new EsclPngEncoderSettings();
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
            _acquiredImages[imageIndex].Save(mem, new EsclPngEncoderSettings());
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
            _esclDevice.Close();
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
