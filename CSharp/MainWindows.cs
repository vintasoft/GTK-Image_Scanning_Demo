using Gtk;
using System;
using System.IO;
using System.Threading;
using Vintasoft.Primitives;
using Vintasoft.UniversalImageScanner;

namespace GtkImageScanningDemo
{
    public class MainWindow
    {

        #region Fields

        Window _window;

        Fixed _fixedContainer;

        ComboBoxText _devicesComboBox;

        CheckButton _showDeviceUiCheckBox;
        CheckButton _useDocumentFeederCheckBox;
        CheckButton _useDuplexScanningCheckBox;

        ComboBoxText _imagePixelTypesComboBox;

        ComboBoxText _imageResolutionsComboBox;

        ComboBoxText _pageSizesComboBox;

        ComboBoxText _imageEncodersComboBox;

        Button _scanImagesButton;

        Image _scannedPictureBox;

        Label _scannedImagesInfoLabel;

        Button _previousImageButton;
        Button _nextImageButton;
        Button _saveImageButton;
        Button _saveAllImagesButton;
        Button _deleteAllImagesButton;

        /// <summary>
        /// The image scanner manager.
        /// </summary>
        LinuxUniversalImageScannerManager _imageScannerManager;
        /// <summary>
        /// The selected image scanner.
        /// </summary>
        IUniversalImageScanner _imageScanner;
        /// <summary>
        /// The index of scanned image that displays in picture box.
        /// </summary>
        int _scannedImageIndex = -1;

        #endregion



        #region Constructors

        public MainWindow()
        {
            // initialize the window UI
            InitUI();

            _imagePixelTypesComboBox.Active = 0;
            _imageResolutionsComboBox.Active = 0;
            _pageSizesComboBox.Active = 0;
            _imageEncodersComboBox.Active = 0;

            _scanImagesButton.Sensitive = false;

            DisableImageManipulationButtons();

            _devicesComboBox.AppendText("Device list is loading...");
            _devicesComboBox.Active = 0;
            _window.Window.Cursor = new Gdk.Cursor(_window.Display, Gdk.CursorType.Watch);

            _window.FocusInEvent += Window_FocusInEvent;
        }

        #endregion



        #region Methods

        /// <summary>
        /// Initialize the window UI.
        /// </summary>
        private void InitUI()
        {
            // init the application
            Application.Init();

            CreateWindow();

            _fixedContainer = new Fixed();

            CreateLabel("Devices:", 12, 17, 50, 15);
            _devicesComboBox = CreateComboBoxText(
                new string[] { }, 115, 12, 493, 23);
            _devicesComboBox.Changed += devicesComboBox_Changed;

            _showDeviceUiCheckBox = CreateCheckButton("Show device UI", 8, 46, 106, 19);
            _useDocumentFeederCheckBox = CreateCheckButton("Use the document feeder", 150, 46, 159, 19);
            _useDuplexScanningCheckBox = CreateCheckButton("Use duplex scanning", 360, 46, 135, 19);

            CreateLabel("Pixel type:", 0, 73, 97, 15);
            _imagePixelTypesComboBox = CreateComboBoxText(
                new string[] { "Black-white", "Grayscale", "RGB" }, 100, 70, 180, 23);

            CreateLabel("Resolution:", 307, 73, 99, 15);
            _imageResolutionsComboBox = CreateComboBoxText(
                new string[] { "200 dpi", "300 dpi", "600 dpi" }, 429, 70, 180, 23);

            CreateLabel("Page size:", 12, 106, 58, 15);
            _pageSizesComboBox = CreateComboBoxText(
                new string[] { "US Letter (8.5 x 11 inches)", "A5 (5.8 x 8.2 inches)", "A4 (8.2 x 11.6 inches)", "B5 dpi (6.9 x 9.8 inches)" },
                100, 99, 180, 23);

            CreateLabel("Image encoder:", 320, 106, 89, 15);
            _imageEncodersComboBox = CreateComboBoxText(
                new string[] { "PNG", "JPG", "TIF", "PDF", "BMP" }, 428, 99, 180, 23);

            // create "Scan images" button
            _scanImagesButton = CreateButton("Scan images", 12, 134, 596, 37);
            _scanImagesButton.Sensitive = false;
            _scanImagesButton.Clicked += scanImagesButton_Click;

            _scannedPictureBox = CreatePictureBox(12, 184, 596, 516);

            _scannedImagesInfoLabel = CreateLabel("", 258, 703, 110, 21);

            _previousImageButton = CreateButton("Previous image", 12, 727, 110, 23);
            _previousImageButton.Clicked += previousImageButton_Click;

            _nextImageButton = CreateButton("Next image", 150, 727, 110, 23);
            _nextImageButton.Clicked += nextImageButton_Click;

            _saveImageButton = CreateButton("Save image", 263, 727, 110, 23);
            _saveImageButton.Clicked += saveImageButton_Click;

            _saveAllImagesButton = CreateButton("Save images", 378, 727, 110, 23);
            _saveAllImagesButton.Clicked += saveAllImagesButton_Click;

            _deleteAllImagesButton = CreateButton("Delete images", 499, 727, 110, 23);
            _deleteAllImagesButton.Clicked += deleteAllImagesButton_Click;

            DisableImageManipulationButtons();

            // add the fixed container to the main window
            _window.Add(_fixedContainer);

            _window.ShowAll();
        }

        /// <summary>
        /// Window is shown.
        /// </summary>
        private void Window_FocusInEvent(object sender, EventArgs e)
        {
            _window.FocusInEvent -= Window_FocusInEvent;

            try
            {
                // create the universal image scanner manager
                _imageScannerManager = new LinuxUniversalImageScannerManager();

                // open the image scanner manager
                _imageScannerManager.Open();

                // get the names of available image scanners
                string[] deviceNames = _imageScannerManager.GetUniversalImageScannerNames();
                _devicesComboBox.RemoveAll();
                // if image scanners are not found in the system
                if (deviceNames.Length == 0)
                {
                    return;
                }

                // for each image scanner
                for (int i = 0; i < deviceNames.Length; i++)
                {
                    // add the name of image scanner to the device list
                    _devicesComboBox.AppendText(string.Format("{0}", deviceNames[i]));
                }
                // select the first device in the device list
                _devicesComboBox.Active = 0;

                _scanImagesButton.Sensitive = true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(GetFullExceptionMessage(ex));
            }
            finally
            {
                _window.Window.Cursor = new Gdk.Cursor(_window.Display, Gdk.CursorType.Arrow);
            }
        }

        /// <summary>
        /// Window is closing.
        /// </summary>
        private void Window_DeleteEvent(object sender, EventArgs e)
        {
            ClearImageInPictureBox();

            if (_imageScannerManager != null)
            {
                _imageScannerManager.Dispose();
                _imageScannerManager = null;
            }

            Application.Quit();
        }

        /// <summary>
        /// Selected device is changed in the device list.
        /// </summary>
        private void devicesComboBox_Changed(object sender, EventArgs e)
        {
            // if image scanner was previously selected
            if (_imageScanner != null)
            {
                // clear scanned images
                _imageScanner.ClearImages();
                _scannedImageIndex = -1;

                // clear reference to the selected image scanner
                _imageScanner = null;

                DisableImageManipulationButtons();
            }
        }

        /// <summary>
        /// "Scan images" button is clicked.
        /// </summary>
        private void scanImagesButton_Click(object sender, EventArgs e)
        {
            try
            {
                string imageScannerName = _devicesComboBox.ActiveText;
                // select the image scanner
                _imageScanner = _imageScannerManager.Find(imageScannerName);
                // if image scanner is not selected
                if (_imageScanner == null)
                    return;

                // open the image scanner
                _imageScanner.Open();

                // specify that device UI should be used
                _imageScanner.ShowUI = _showDeviceUiCheckBox.Active;

                // if image scanner has the automatic document feeder
                if (_imageScanner.HasFeeder)
                    // set the state of the automatic document feeder
                    _imageScanner.IsFeederEnabled = _useDocumentFeederCheckBox.Active;
                // image scanner supports the duplex scanning
                if (_imageScanner.HasDuplex)
                    // set the state of duplex scanning
                    _imageScanner.IsDuplexEnabled = _useDuplexScanningCheckBox.Active;

                // set the image pixel type for scanned images
                switch (_imagePixelTypesComboBox.ActiveText)
                {
                    case "Black-white":
                        _imageScanner.ImagePixelType = ImagePixelType.BlackWhite;
                        break;
                    case "Grayscale":
                        _imageScanner.ImagePixelType = ImagePixelType.Grayscale;
                        break;
                    default:
                        _imageScanner.ImagePixelType = ImagePixelType.RGB;
                        break;
                }

                // set the image resolution for scanned images
                switch (_imageResolutionsComboBox.ActiveText)
                {
                    case "200 dpi":
                        _imageScanner.ScanXResolution = 200;
                        _imageScanner.ScanYResolution = 200;
                        break;
                    case "300 dpi":
                        _imageScanner.ScanXResolution = 300;
                        _imageScanner.ScanYResolution = 300;
                        break;
                    case "600 dpi":
                        _imageScanner.ScanXResolution = 600;
                        _imageScanner.ScanYResolution = 600;
                        break;
                }

                // set the page size for scanned images
                switch (_pageSizesComboBox.ActiveText)
                {
                    case "US Letter (8.5 x 11 inches)":
                        _imageScanner.SetImageLayout(new VintasoftRect(0, 0, 8.5, 11));
                        break;
                    case "A5 (5.8 x 8.2 inches)":
                        _imageScanner.SetImageLayout(new VintasoftRect(0, 0, 5.8, 8.2));
                        break;
                    case "A4 (8.2 x 11.6 inches)":
                        _imageScanner.SetImageLayout(new VintasoftRect(0, 0, 8.2, 11.6));
                        break;
                    case "B5 dpi (6.9 x 9.8 inches)":
                        _imageScanner.SetImageLayout(new VintasoftRect(0, 0, 6.9, 9.8));
                        break;
                }

                do
                {
                    try
                    {
                        // if image is acquired from image scanner
                        if (_imageScanner.AcquireImageSync())
                        {
                            int scannedImageIndex = _imageScanner.ScannedImageCount - 1;

                            // get scanned image as a stream
                            Stream imageAsStream = _imageScanner.GetImageAsStream(scannedImageIndex);

                            ClearImageInPictureBox();

                            // show scanned image in picture box
                            SetImageInPictureBox(imageAsStream);
                            _scannedImageIndex = scannedImageIndex;

                            EnableImageManipulationButtons();

                            UpdateScannedImagesInfoLabel();
                        }
                        // if image is not acquired
                        else
                        {
                            Console.WriteLine("Scan is finished.");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Scan is failed: {0}", ex.Message));
                        break;
                    }
                }
                while (true);

                // close the image scanner
                _imageScanner.Close();

            }
            catch (Exception ex)
            {
                ShowErrorMessage(GetFullExceptionMessage(ex));
            }
        }

        /// <summary>
        /// "Previous image" button is clicked.
        /// </summary>
        private void previousImageButton_Click(object sender, EventArgs e)
        {
            if (_scannedImageIndex <= 0)
                return;

            // get image as a stream
            Stream imageAsStream = _imageScanner.GetImageAsStream(_scannedImageIndex - 1);

            ClearImageInPictureBox();

            // set image to the picture box
            SetImageInPictureBox(imageAsStream);
            _scannedImageIndex--;

            UpdatePreviousAndNextImageButton();
            UpdateScannedImagesInfoLabel();
        }

        /// <summary>
        /// "Next image" button is clicked.
        /// </summary>
        private void nextImageButton_Click(object sender, EventArgs e)
        {
            if (_scannedImageIndex >= (_imageScanner.ScannedImageCount - 1))
                return;

            // get image as a stream
            Stream imageAsStream = _imageScanner.GetImageAsStream(_scannedImageIndex + 1);

            ClearImageInPictureBox();

            // set image to the picture box
            SetImageInPictureBox(imageAsStream);
            _scannedImageIndex++;

            UpdatePreviousAndNextImageButton();
            UpdateScannedImagesInfoLabel();
        }

        /// <summary>
        /// "Save image" button is clicked.
        /// </summary>
        private void saveImageButton_Click(object sender, EventArgs e)
        {
            try
            {
                // get the name of image encoder
                string imageEncoderName = _imageEncodersComboBox.ActiveText.ToLowerInvariant();
                // get the name of file, where image should be saved
                string filename = string.Format("scannedImage.{0}", imageEncoderName);
                // save the scanned image to a file
                _imageScanner.SaveImage(_scannedImageIndex, filename);
                ShowInfoMessage("Image is saved successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Image is not saved: " + ex.Message);
            }
        }

        /// <summary>
        /// "Save images" button is clicked.
        /// </summary>
        private void saveAllImagesButton_Click(object sender, EventArgs e)
        {
            try
            {
                // get the name of image encoder
                string imageEncoderName = _imageEncodersComboBox.ActiveText.ToLowerInvariant();
                if (imageEncoderName != "tif" && imageEncoderName != "pdf")
                {
                    ShowInfoMessage("All images can be saved to a multipage TIFF or PDF file only.");
                    return;
                }

                // get the name of file, where image should be saved
                string filename = string.Format("scannedImage.{0}", imageEncoderName);
                // for each scanned image
                for (int i = 0; i < _imageScanner.ScannedImageCount; i++)
                {
                    // add scanned image to a file
                    _imageScanner.SaveImage(i, filename);
                }
                ShowInfoMessage("All images are saved successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Images are not saved: " + ex.Message);
            }
        }

        /// <summary>
        /// "Delete images" button is clicked.
        /// </summary>
        private void deleteAllImagesButton_Click(object sender, EventArgs e)
        {
            // clear scanned images
            _imageScanner.ClearImages();
            _scannedImageIndex = -1;

            ClearImageInPictureBox();

            DisableImageManipulationButtons();
            UpdateScannedImagesInfoLabel();
        }

        /// <summary>
        /// Create the window.
        /// </summary>
        private void CreateWindow()
        {
            // create the main window
            _window = new Window("VintaSoft Image Scanning Demo");
            _window.SetDefaultSize(637, 797);
            _window.WindowPosition = WindowPosition.Center;
            _window.Resizable = false;

            // add handler for the DeleteEvent of main window
            _window.DeleteEvent += Window_DeleteEvent;
        }

        /// <summary>
        /// Creates the label.
        /// </summary>
        /// <param name="text">The label text.</param>
        /// <param name="x">X-coordinate.</param>
        /// <param name="y">Y-coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>The GTK Label-control.</returns>
        private Label CreateLabel(string text, int x, int y, int width, int height)
        {
            Label label = new Label(text);

            label.SetSizeRequest(width, height);
            label.Halign = Gtk.Align.Center;
            label.Valign = Gtk.Align.Center;

            _fixedContainer.Put(label, x, y);

            return label;
        }

        /// <summary>
        /// Creates the checkbox.
        /// </summary>
        /// <param name="text">The checkbox text.</param>
        /// <param name="x">X-coordinate.</param>
        /// <param name="y">Y-coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>The GTK CheckButton-control.</returns>
        private CheckButton CreateCheckButton(string text, int x, int y, int width, int height)
        {
            CheckButton checkButton = new CheckButton(text);

            checkButton.SetSizeRequest(width, height);
            checkButton.Halign = Gtk.Align.Center;
            checkButton.Valign = Gtk.Align.Center;

            _fixedContainer.Put(checkButton, x, y);

            return checkButton;
        }

        /// <summary>
        /// Creates the combobox.
        /// </summary>
        /// <param name="text">The combobox text.</param>
        /// <param name="x">X-coordinate.</param>
        /// <param name="y">Y-coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>The GTK ComboBoxText-control.</returns>
        private ComboBoxText CreateComboBoxText(string[] texts, int x, int y, int width, int height)
        {
            ComboBoxText comboBoxText = new ComboBoxText();
            for (int i = 0; i < texts.Length; i++)
            {
                comboBoxText.AppendText(texts[i]);
            }

            comboBoxText.SetSizeRequest(width, height);
            comboBoxText.Halign = Gtk.Align.Center;
            comboBoxText.Valign = Gtk.Align.Center;

            _fixedContainer.Put(comboBoxText, x, y);

            return comboBoxText;
        }

        /// <summary>
        /// Creates the button.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="x">X-coordinate.</param>
        /// <param name="y">Y-coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>The GTK Button-control.</returns>
        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            Button button = new Button(text);

            button.SetSizeRequest(width, height);
            button.Halign = Gtk.Align.Center;
            button.Valign = Gtk.Align.Center;

            _fixedContainer.Put(button, x, y);

            return button;
        }

        /// <summary>
        /// Creates the picture box.
        /// </summary>
        /// <param name="x">X-coordinate.</param>
        /// <param name="y">Y-coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>The GTK Image-control.</returns>
        private Image CreatePictureBox(int x, int y, int width, int height)
        {
            Image pixtureBox = new Image();

            pixtureBox.SetSizeRequest(width, height);
            pixtureBox.Halign = Gtk.Align.Center;
            pixtureBox.Valign = Gtk.Align.Center;

            _fixedContainer.Put(pixtureBox, x, y);

            return pixtureBox;
        }

        /// <summary>
        /// Sets the image in picture box.
        /// </summary>
        /// <param name="imageAsStream">A stream that contains image.</param>
        private void SetImageInPictureBox(Stream imageAsStream)
        {
            imageAsStream.Position = 0;

            // create Pixbuf object for image
            using (Gdk.Pixbuf originalPixbuf = new Gdk.Pixbuf(imageAsStream))
            {
                // set the scaled Pixbuf object in picture box
                _scannedPictureBox.Pixbuf = originalPixbuf.ScaleSimple(_scannedPictureBox.Allocation.Width, _scannedPictureBox.Allocation.Height, Gdk.InterpType.Bilinear);
            }
        }

        /// <summary>
        /// Clears the image in picture box.
        /// </summary>
        private void ClearImageInPictureBox()
        {
            // if picture box contains previously scanned image
            if (_scannedPictureBox.Pixbuf != null)
            {
                Gdk.Pixbuf oldPixbuf = _scannedPictureBox.Pixbuf;

                // clear previously scanned image in picture box
                _scannedPictureBox.Pixbuf = null;

                // wait a while to make sure that picture box frees the pixbuf
                Thread.Sleep(5);

                // dispose pixbuf of previously scanned image
                oldPixbuf.Dispose();
            }
        }

        /// <summary>
        /// Enables the image manipulation buttons.
        /// </summary>
        private void EnableImageManipulationButtons()
        {
            _previousImageButton.Sensitive = true;
            _nextImageButton.Sensitive = true;
            _saveImageButton.Sensitive = true;
            _saveAllImagesButton.Sensitive = true;
            _deleteAllImagesButton.Sensitive = true;
        }

        /// <summary>
        /// Disables the image manipulation buttons.
        /// </summary>
        private void DisableImageManipulationButtons()
        {
            _previousImageButton.Sensitive = false;
            _nextImageButton.Sensitive = false;
            _saveImageButton.Sensitive = false;
            _saveAllImagesButton.Sensitive = false;
            _deleteAllImagesButton.Sensitive = false;
        }

        /// <summary>
        /// Updates the "Previous image" and "Next image" buttons.
        /// </summary>
        private void UpdatePreviousAndNextImageButton()
        {
            _previousImageButton.Sensitive = _scannedImageIndex != 0;
            _nextImageButton.Sensitive = _scannedImageIndex != _imageScanner.ScannedImageCount - 1;
        }

        /// <summary>
        /// Updates the label with information about scanned images.
        /// </summary>
        private void UpdateScannedImagesInfoLabel()
        {
            _scannedImagesInfoLabel.Text = string.Format("{0} from {1}", _scannedImageIndex + 1, _imageScanner.ScannedImageCount);
        }

        /// <summary>
        /// Shows info message.
        /// </summary>
        /// <param name="message">Message text.</param>
        private void ShowInfoMessage(string message)
        {
            MessageDialog md = new MessageDialog(_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Info, Gtk.ButtonsType.Ok, message);
            md.Run();
            md.Destroy();
        }

        /// <summary>
        /// Shows error message.
        /// </summary>
        /// <param name="message">Message text.</param>
        private void ShowErrorMessage(string message)
        {
            MessageDialog md = new MessageDialog(_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Error, Gtk.ButtonsType.Ok, message);
            md.Run();
            md.Destroy();
        }

        /// <summary>
        /// Returns message that contains information about exception and inner exceptions.
        /// </summary>
        private string GetFullExceptionMessage(Exception ex)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(ex.Message);

            Exception innerException = ex.InnerException;
            while (innerException != null)
            {
                if (ex.Message != innerException.Message)
                    sb.AppendLine(string.Format("Inner exception: {0}", innerException.Message));
                innerException = innerException.InnerException;
            }

            return sb.ToString();
        }

        #endregion

    }
}
