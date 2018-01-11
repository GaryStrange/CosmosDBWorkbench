using System;
using System.Collections.Generic;
using System.Text;

namespace WorkBench.DataAccess.SchemaAttributes
{
    public static class CryptographicHelper
    {
        public static string SHA256<T>(T inValue)
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                // Get the arrary of hexidecimal bytes from the hash algorithm.  
                byte[] hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(inValue.ToString()));
                
                // Convert the hash to a string and remove the hyphens BitConverter adds.  
                return BitConverter.ToString(hashedBytes).Replace("-", "");
            }

        }

    }
}
