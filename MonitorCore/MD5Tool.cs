using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    public static class MD5Tool
    {
        public static string ToMD5 (string str) 
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes (str);

            string md5Hash;

            using (MD5 md5 = MD5.Create ())
            {
                byte[] hashBytes = md5.ComputeHash (inputBytes);

                StringBuilder sb = new StringBuilder ();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    //16進制
                    sb.Append (hashBytes[i].ToString ("x2"));
                }

                md5Hash = sb.ToString ();
            }

            return md5Hash;
        }
    }
}
