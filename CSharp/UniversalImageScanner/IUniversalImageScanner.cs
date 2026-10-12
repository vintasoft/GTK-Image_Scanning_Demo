using System;
using System.IO;
using Vintasoft.Primitives;

namespace Vintasoft.UniversalImageScanner
{
    /// <summary>
    /// Defines an interface for universal image scanner.
    /// </summary>
    public interface IUniversalImageScanner : IDisposable
    {

        /// <summary>
        /// Gets the name of image scanner.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether image scanner should show the UI.
        /// </summary>
        bool ShowUI
        {
            get; set;
        }

        /// <summary>
        /// Gets a value indicating whether image scanner has the flated.
        /// </summary>
        bool HasFlatbed
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether flatbed is enabled.
        /// </summary>
        bool IsFlatbedEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether image scanner has the automatic document feeder.
        /// </summary>
        bool HasFeeder
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the automatic document feeder is enabled.
        /// </summary>
        bool IsFeederEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the automatic document feeder supports duplex scanning.
        /// </summary>
        bool HasDuplex
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the duplex scanning is enabled.
        /// </summary>
        bool IsDuplexEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pixel type for scanned images.
        /// </summary>
        ImagePixelType ImagePixelType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the horizontal resolution for scanned images.
        /// </summary>
        int ScanXResolution
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the vertical resolution for scanned images.
        /// </summary>
        int ScanYResolution
        {
            get; set;
        }

        /// <summary>
        /// Gets the count of scanned images.
        /// </summary>
        int ScannedImageCount
        {
            get;
        }



        /// <summary>
        /// Opens the image scanner.
        /// </summary>
        void Open();

        /// <summary>
        /// Returns the image layout, in inches, of image scanner.
        /// </summary>
        /// <returns>The image layout, in inches</returns>
        VintasoftRect GetImageLayout();

        /// <summary>
        /// Sets the image layout for image scanner.
        /// </summary>
        /// <param name="imageLayout">The image layout in inches.</param>
        void SetImageLayout(VintasoftRect imageLayout);

        /// <summary>
        /// Acquires one image from image scanner.
        /// </summary>
        /// <returns>True - image is acquired; False - image is not acquired.</returns>
        bool AcquireImageSync();

        /// <summary>
        /// Saves scanned image to a file.
        /// </summary>
        /// <param name="imageIndex">The index of image in image buffer.</param>
        /// <param name="filename">The name of file, where image must be saved.</param>
        void SaveImage(int imageIndex, string filename);

        /// <summary>
        /// Returns scanned image as a stream.
        /// </summary>
        /// <param name="imageIndex">The index of image in image buffer.</param>
        /// <returns>The memory stream that contains scanned image.</returns>
        Stream GetImageAsStream(int imageIndex);

        /// <summary>
        /// Clears the scanned images.
        /// </summary>
        void ClearImages();

        /// <summary>
        /// Closes the image scanner.
        /// </summary>
        void Close();

    }
}
