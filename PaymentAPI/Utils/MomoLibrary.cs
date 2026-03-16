using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PaymentAPI.Utils
{
    public class MomoLibrary
    {
        public string CreateSignature(string secretKey, string rawHash)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(rawHash);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                string hex = "";
                foreach (byte x in hashmessage)
                {
                    hex += String.Format("{0:x2}", x);
                }
                return hex;
            }
        }
    }
}
