using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using System.Collections.ObjectModel;
using LibUsbDotNet.Main;
using LibUsbDotNet;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace usb_test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #region SET YOUR USB Vendor and Product ID!
        private const int vendor = 0x31EF;
        private static MainWindow instance = null;
        public static MainWindow GetInstance()
        {
            return instance;
        }

        private const int product_id = 0x3001;
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(vendor, product_id);
        #endregion
        public static UsbDevice MyUsbDevice;
        private Tool tool = Tool.GetInstance();
        private string debuginfo="";
        public bool IsConnected
        {
            get
            {
                if (ConnectStat == "未连接")
                    return false;
                return true;
            }
            set
            {
                if (!value)
                    ConnectStat = "未连接";
                else
                    ConnectStat = "已连接";
            }
        }
        private string connect_stat = "未连接";
        public string ConnectStat
        {
            get
            {
                return connect_stat;
            }
            set
            {
                connect_stat = value;
                Notify("ConnectStat");
            }
        }
        public string DebugInfo
        {
            get
            {
                return debuginfo;
            }
            set
            {
                debuginfo = value; Notify("DebugInfo");
            }
        }
        private int percent = 0;
        public int Percent
        {
            set
            {
                if (percent != value)
                {
                    percent = value;
                    Notify("Percent");
                }
            }
            get { return percent; }
        }
        string bootload_file = "E:\\jlq\\hci\\DDR_INIT_JA_310_1333.bin";
        string tmp_blf = "bl.bin.tmp";
        string bin_file = "E:\\jlq\\sv2020\\bin\\bin.bin";
        string tmp_bin = "case.tmp";
        public MainWindow()
        {
            instance = this;
            InitializeComponent();
            this.DataContext = this;
            //pb.Visibility = Visibility.Hidden;
            DLStateService.run();
        }

        private void send_file(string file_name)
        {
            ErrorCode ec = ErrorCode.None;
            try
            {
                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the 
                // variable will be null indicating this is an interface of a 
                // device.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                // open read endpoint 1.
                //UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                // open write endpoint 1.
                UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep08);

                byte[] buf = new byte[4096];
                long left = 0;
                int size = 0;
                int usb_send_size = 0;
                int curpb = MainWindow.GetInstance().Percent;
                FileStream fs = new FileStream(file_name, FileMode.Open);
                left = fs.Length;
                // percent 25%
                while (left > 0)
                {
                    size = left > 4096 ? 4096 : (int)left;
                    fs.Read(buf, 0, size);
                    ec = writer.Write(buf, 0, size, 2000, out usb_send_size);
                    if (ec != ErrorCode.None)
                    {
                        fs.Close();
                        throw new Exception("usb write failed\n" + UsbDevice.LastErrorString);
                    }
                    if (usb_send_size != size)
                    {
                        fs.Close();
                        throw new Exception("usb_send_size:" + usb_send_size.ToString() +  " != size:" + size.ToString() + " " + UsbDevice.LastErrorString);
                    }
                    left -= 4096;
                    MainWindow.GetInstance().Percent = curpb + (int)(25 *(1.0 - left*1.0/fs.Length));
                }
                fs.Close();
            }
            catch (Exception ex)
            {
                DebugInfo = "";
                //Console.WriteLine();
                DebugInfo = ((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
                //Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();

                }

                // Wait for user input..
                Console.Read();
            }
        }
        private void view_usb(object sender, RoutedEventArgs e)
        {
            // Dump all devices and descriptor information to console output.
            DebugInfo = "";
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                if (usbRegistry.Open(out MyUsbDevice))
                {
                    DebugInfo += "usbdevice info\n";
                    DebugInfo += MyUsbDevice.Info.ToString();
                    DebugInfo += "\n\n\n";
                    //Console.WriteLine(MyUsbDevice.Info.ToString());
                    for (int iConfig = 0; iConfig < MyUsbDevice.Configs.Count; iConfig++)
                    {
                        UsbConfigInfo configInfo = MyUsbDevice.Configs[iConfig];
                        DebugInfo += "configInfo info\n";
                        DebugInfo += configInfo.ToString();
                        DebugInfo += "\n\n\n";
                        //Console.WriteLine(configInfo.ToString());

                        ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.InterfaceInfoList;
                        for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
                        {
                            UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
                            DebugInfo += "interfaceInfo info\n";
                            DebugInfo += interfaceInfo.ToString();
                            //Console.WriteLine(interfaceInfo.ToString());
                            DebugInfo += "\n\n\n";

                            ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.EndpointInfoList;
                            for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
                            {
                                DebugInfo += "endpointList iEndpoint[" + iEndpoint.ToString() + "]:\n";
                                DebugInfo += endpointList[iEndpoint].ToString();
                                DebugInfo += "\n\n\n";
                                //Console.WriteLine(endpointList[iEndpoint].ToString());
                            }
                        }
                    }
                }
            }
            // Free usb resources.
            // This is necessary for libusb-1.0 and Linux compatibility.
            UsbDevice.Exit();

            // Wait for user input..
            Console.Read();
        }
        private void gen_tmp(BinType type)
        {
            Image_header header = new Image_header();
            switch (type)
            {
                case BinType.BOOTLOADER:
                    header.dest_addr = 0xf8000000;
                    break;
                case BinType.BIN:
                    header.dest_addr = 0x0;
                    break;
                default:
                    throw new Exception("BinType err\n");
            }
            if (type == BinType.BOOTLOADER)
            {
                tool.BuildPackage(header, bootload_file, tmp_blf);
            }
            if (type == BinType.BIN)
            {
                tool.BuildPackage(header, bin_file, tmp_bin);
            }
        }
        public void UpdateProgressBar(int value)
        {
            if (value == 0)
            {
                pb.Visibility = Visibility.Visible;
                Percent = 0;
                return;
            }
            if (value == 100)
            {
                Percent = 100;
                Thread.Sleep(2000);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    pb.Visibility = Visibility.Hidden;
                    download.IsEnabled = true;
                }));
                return;
            }
            Percent = value;
        }
        private void DownLoad_Click(object sender, RoutedEventArgs e)
        {
            Image_header header = new Image_header();
            DebugInfo = "";
            //download.IsEnabled = false;
            UpdateProgressBar(0);
            Task.Factory.StartNew(() =>
            {
                gen_tmp(BinType.BOOTLOADER);
                UpdateProgressBar(25);
                gen_tmp(BinType.BIN);
                UpdateProgressBar(50);
                send_file(tmp_blf);
                UpdateProgressBar(75);
                Thread.Sleep(4000);
                send_file(tmp_bin);
                UpdateProgressBar(100);
            });
        }
    }
}
