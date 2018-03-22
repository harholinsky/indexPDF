using System;
using System.IO;
using System.Security.Cryptography;

namespace LestLucene.PdfHelper
{
    public static class MD5Helper
    {
        public static string CalculateMD5ForFile(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}
