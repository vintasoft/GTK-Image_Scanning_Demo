using System;
using System.Collections.Generic;
using Vintasoft.EsclImageScanning;
using Vintasoft.Sane;
using Vintasoft.Twain;

namespace Vintasoft.UniversalImageScanner
{
    /// <summary>
    /// The manager for universal image scanners.
    /// </summary>
    public class LinuxUniversalImageScannerManager : IDisposable
    {

        private DeviceManager _twainDeviceManager;
        EsclDeviceManager _esclDeviceManager;
        SaneLocalDeviceManager _saneDeviceManager;
        Dictionary<string, IUniversalImageScanner> _nameToImageScanner = new Dictionary<string, IUniversalImageScanner>();



        /// <summary>
        /// Creates instance of LinuxUniversalImageScannerManager class.
        /// </summary>
        public LinuxUniversalImageScannerManager()
        {
            try
            {
                _twainDeviceManager = new DeviceManager();
                _twainDeviceManager.IsTwain2Compatible = true;
            }
            catch
            {
            }

            try
            {
                _esclDeviceManager = new EsclDeviceManager();
            }
            catch
            {
            }

            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    _saneDeviceManager = new SaneLocalDeviceManager();
                }
            }
            catch
            {
            }
        }



        /// <summary>
        /// Disposes resources, which are used by this manager.
        /// </summary>
        public void Dispose()
        {
            if (_twainDeviceManager != null)
            {
                _twainDeviceManager.Dispose();
                _twainDeviceManager = null;
            }

            if (_esclDeviceManager != null)
            {
                _esclDeviceManager.Dispose();
                _esclDeviceManager = null;
            }

            if (_saneDeviceManager != null)
            {
                _saneDeviceManager.Dispose();
                _saneDeviceManager = null;
            }
        }

        /// <summary>
        /// Opens this manager.
        /// </summary>
        public void Open()
        {
            try
            {
                if (_twainDeviceManager != null)
                    _twainDeviceManager.Open();
            }
            catch
            {
            }

            try
            {
                if (_esclDeviceManager != null)
                    _esclDeviceManager.Open();
            }
            catch
            {
            }

            try
            {
                if (_saneDeviceManager != null)
                    _saneDeviceManager.Open();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Returns the names of available image scanner.
        /// </summary>
        /// <returns>The names of available image scanner.</returns>
        public string[] GetUniversalImageScannerNames()
        {
            List<string> deviceNames = new List<string>();

            if (_twainDeviceManager != null)
            {
                if (_twainDeviceManager.State == DeviceManagerState.Opened)
                {
                    for (int i = 0; i < _twainDeviceManager.Devices.Count; i++)
                    {
                        deviceNames.Add(string.Format("TWAIN: {0}", _twainDeviceManager.Devices[i].Info.ProductName));
                    }
                }
            }

            if (_esclDeviceManager != null)
            {
                for (int i = 0; i < _esclDeviceManager.Devices.Count; i++)
                {
                    deviceNames.Add(string.Format("eSCL: {0}", _esclDeviceManager.Devices[i].Name));
                }
            }

            if (_saneDeviceManager != null)
            {
                for (int i = 0; i < _saneDeviceManager.Devices.Count; i++)
                {
                    deviceNames.Add(string.Format("SANE: {0}", _saneDeviceManager.Devices[i].Name));
                }
            }

            return deviceNames.ToArray();
        }

        /// <summary>
        /// Finds the image scanner by the name.
        /// </summary>
        /// <param name="name">The name of image scanner.</param>
        /// <returns>Image scanner if image scanner is found; otherwise, null.</returns>
        public IUniversalImageScanner Find(string name)
        {
            if (name.StartsWith("TWAIN: "))
            {
                if (_nameToImageScanner.ContainsKey(name))
                {
                    return _nameToImageScanner[name];
                }
                else
                {
                    Device twainDevice = _twainDeviceManager.Devices.Find(name.Substring("TWAIN: ".Length));
                    if (twainDevice != null)
                    {
                        _nameToImageScanner[name] = new UniversalTwainImageScanner(twainDevice);
                        return _nameToImageScanner[name];
                    }
                }
            }
            else if (name.StartsWith("eSCL: "))
            {
                if (_nameToImageScanner.ContainsKey(name))
                {
                    return _nameToImageScanner[name];
                }
                else
                {
                    EsclDevice esclDevice = _esclDeviceManager.Devices.FindByName(name.Substring("eSCL: ".Length));
                    if (esclDevice != null)
                    {
                        _nameToImageScanner[name] = new UniversalEsclImageScanner(esclDevice);
                        return _nameToImageScanner[name];
                    }
                }
            }
            else if (name.StartsWith("SANE: "))
            {
                if (_nameToImageScanner.ContainsKey(name))
                {
                    return _nameToImageScanner[name];
                }
                else
                {
                    SaneLocalDevice saneDevice = _saneDeviceManager.Devices.FindByName(name.Substring("SANE: ".Length));
                    if (saneDevice != null)
                    {
                        _nameToImageScanner[name] = new UniversalSaneImageScanner(saneDevice);
                        return _nameToImageScanner[name];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Closes this manager.
        /// </summary>
        public void Close()
        {
            try
            {
                if (_twainDeviceManager != null && _twainDeviceManager.State != DeviceManagerState.Closed)
                    _twainDeviceManager.Close();
            }
            catch
            {
            }

            try
            {
                if (_esclDeviceManager != null && _esclDeviceManager.IsOpened)
                    _esclDeviceManager.Close();
            }
            catch
            {
            }

            try
            {
                if (_saneDeviceManager != null && _saneDeviceManager.IsOpened)
                    _saneDeviceManager.Close();
            }
            catch
            {
            }
        }

    }
}
