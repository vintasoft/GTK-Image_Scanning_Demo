using Gtk;

namespace GtkImageScanningDemo
{
    class Program
    {

        static void Main(string[] args)
        {
            // register the evaluation license for VintaSoft Twain .NET SDK
            Vintasoft.Twain.TwainGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");
            Vintasoft.Sane.SaneGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");
            Vintasoft.EsclImageScanning.EsclGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");

            MainWindow mainWindow = new MainWindow();
            Application.Run();
        }

    }
}
