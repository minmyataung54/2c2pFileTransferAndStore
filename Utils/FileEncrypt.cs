using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2c2pFileTransferAndStore.Utils
{
    public class FileEncrypt
    {
        public static string EncryptFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }
            fileName = "Encrypted_" + fileName;
            return fileName;

        }
    }
}
