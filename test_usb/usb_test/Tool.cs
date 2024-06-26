﻿using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace usb_test
{
    public enum SocType
    {
        JA310,
        JR510,
    }
    public class Tool
    {
        public static int[] vendors = { 0x31EF, 0x31ef};
        public static int[] product_ids = { 0x3001, 0x5100};

        private static SocType type = SocType.JA310;
        public SocType SocT
        {
            get
            {
                return type;
            }
        }
        private static Tool instance = null;
        public static Tool GetInstance()
        {
            if (instance == null)
            {
                instance = new Tool();
            }
            return instance;
        }

        public bool BuildPackage(Image_header header, string orginfile, string tmppath)
        {
            int total_len = 0;
            bool ret = true;
            FileInfo fi = new FileInfo(orginfile);
            int param_len = 0;
            int file_align = 1;
            int para_align = 1;

            total_len = (int)fi.Length;
            int case_align_len = 0;
            int para_align_len = 0;
            int para_len = param_len;
            int mod = total_len % file_align;
            if (mod != 0)
            {
                //补齐8字节
                case_align_len = file_align - mod;
            }

            mod = para_len % para_align;
            if (mod != 0)
            {
                //补齐4字节
                para_align_len = para_align - mod;
            }
            byte[] image_head_bytes = null;
            header.image_len = total_len + case_align_len + para_len + para_align_len;//镜像密文数据大小
            header.image_plain_len = total_len + case_align_len + para_len + para_align_len;//镜像明文数据大小
            image_head_bytes = header.ToBytes();

            int head_hash_len = header.Size;
            int hash_len = head_hash_len + header.image_plain_len;

            FileStream fs = new FileStream(orginfile, FileMode.Open);
            int left = (int)fs.Length;
            int file_size = (int)fs.Length;
            int size = 0;
            /* header(64B) + imagedata */
            byte[] sha256 = new byte[header.Size + left];

            /*
             *    JA310
             * 0  --header    --   64B
             *    --ROTPK     --   524B 公钥
             *    --Signature --   256B(header + imagedata)
             * 4K --Padding   -- 
             * --Image Data-- 
             * 
             *    JR510
             * 0  --header    --   64B
             *    --ROTPK     --   524B 公钥
             *    --Signature --   256B(header + imagedata)
             *    --Hash      --   32B(header + imagedata)
             * 4K --Padding   -- 
             * --Image Data-- 
             */

            Array.Copy(image_head_bytes, 0, sha256, 0, header.Size);
            while (left > 0)
            {
                size = left > 4096 ? 4096 : (int)left;
                fs.Read(sha256, header.Size + (file_size - left), size);
                left -= 4096;
            }
            fs.Close();
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] sha256hash = Sha256.ComputeHash(sha256);
            byte[] head = new byte[HEAD_SIZE];
            Array.Clear(head, 0, HEAD_SIZE);
            Array.Copy(image_head_bytes, 0, head, 0, header.Size);
            switch (type)
            {
                case SocType.JA310:
                    Array.Copy(sha256hash, 0, head, header.Size + ROTPK_LENGTH, 32);
                    break;
                case SocType.JR510:
                    Array.Copy(sha256hash, 0, head, header.Size + ROTPK_LENGTH + SIGNATURE_LENGTH, 32);
                    break;
            }

            /*
             * 头写入文件
             */
            FileStream outfs = new FileStream(tmppath, FileMode.Create);
            outfs.Write(head, 0, HEAD_SIZE);
            /* 
             * 文件内容写入 tmppath
             */
            outfs.Write(sha256/**/, header.Size, file_size);
            outfs.Close();

            return ret;
        }
        const int ROTPK_LENGTH = 524;
        const int SIGNATURE_LENGTH = 256;
        const int HEAD_SIZE = 4096;
        public static UsbDevice MyUsbDevice;
        public bool GetUSBConnectState()
        {
            bool found = false;
            try
            {
                if (MainWindow.GetInstance().ChoicePlatForm == "JA310")
                {
                    type = SocType.JA310;
                }
                if (MainWindow.GetInstance().ChoicePlatForm == "JR510")
                {
                    type = SocType.JR510;
                }
                UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(vendors[(int)(type)], product_ids[(int)(type)]);

                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");
                found = true;
            }
            catch (Exception ex)
            {
                found = false;

            }
            return found;
        }
     
    }
    class DLStateService
    {
        public static Tool tool = Tool.GetInstance();
        public static IDeviceNotifier UsbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
        public static void TimerThread(object param)
        {
            bool connected = false;
            connected = tool.GetUSBConnectState();
            MainWindow.GetInstance().IsConnected = connected;
            while (true)
            {
                Thread.Sleep(5000);
#if xx
                connected = tool.GetUSBConnectState();
                if (connected != MainWindow.GetInstance().IsConnected)
                {
                    MainWindow.GetInstance().IsConnected = connected;
                }
#endif
             }
        }
        private static void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            bool connected = false;
            // A Device system-level event has occured
            connected = tool.GetUSBConnectState();
            if (connected != MainWindow.GetInstance().IsConnected)
            {
                MainWindow.GetInstance().IsConnected = connected;
            }
        }
        public static void run()
        {
            UsbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
            Thread playing = new Thread(new ParameterizedThreadStart(DLStateService.TimerThread));
            playing.IsBackground = true;
            playing.Start();
        }
    }
 }
