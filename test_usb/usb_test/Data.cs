using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usb_test
{
    class Data
    {
    }
    // bootroom defined TL header
    public class Image_header
    {
        public byte[] header_magic = new byte[4];          /* IM*H */
        public uint header_len;                //当前版本长度为64
        public int header_version;            //当前版本为1
        public byte[] image_id = new byte[4];
        public int image_attr;
        public int image_counter;
        public UInt64 dest_addr;                     //image加到memory中的地址
        public int image_offset=4096;              //image_data的起始地址 当前版本为4096
        public int image_len;                 //image密文数据的长度
        public int image_plain_len;           //image明文数据的长度
        public int []reserved = new int[5];
        private const int IMAGE_HEADER_LENGTH = 64;
        private const int IMAGE_HEADER_VERSION = 1;
        private const int IMAGE_DATA_PLAIN = 0;
        private const string IMAGE_MAGIC = "IM*H";
        private const string IMAGE_ID = "TLDR";

        public Image_header()
        {
            int i = 0;
            for (i = 0; i < 4; i++)
            {
                header_magic[i] = (byte)IMAGE_MAGIC[i];
            }
            header_len = IMAGE_HEADER_LENGTH;
            header_version = IMAGE_HEADER_VERSION;
            for (i = 0; i < 4; i++)
            {
                image_id[i] = (byte)IMAGE_ID[i];
            }
            image_attr = IMAGE_DATA_PLAIN;
            image_counter = IMAGE_HEADER_VERSION;
            image_len = 0;
            image_plain_len = 0;
            image_offset = 4096;
        }
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[Size];
            Array.Clear(bytes, 0, bytes.Length);
            Array.Copy(header_magic,0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(header_len), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(header_version), 0, bytes, 8, 4);
            Array.Copy(image_id, 0, bytes, 12, 4);
            Array.Copy(BitConverter.GetBytes(image_attr), 0, bytes, 16, 4);
            Array.Copy(BitConverter.GetBytes(image_counter), 0, bytes, 20, 4);
            Array.Copy(BitConverter.GetBytes(dest_addr), 0, bytes, 24, 8);
            Array.Copy(BitConverter.GetBytes(image_offset), 0, bytes, 32, 4);
            Array.Copy(BitConverter.GetBytes(image_len), 0, bytes, 36, 4);
            Array.Copy(BitConverter.GetBytes(image_plain_len), 0, bytes, 40, 4);
            //Array.Copy(reserved, 0, bytes, 44, 20);
            return bytes;
        }
             
        public int Size
        {
            get {
                return 64;
            }
        }
    }
    public enum BinType : byte
    {
        BOOTLOADER = 0,
        BIN = 1,
    }
}
