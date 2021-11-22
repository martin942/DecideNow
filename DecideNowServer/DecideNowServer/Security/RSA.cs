using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DecideNowServer.Security
{
    public class RSA
    {
        

        private static Dictionary<string, (RSAParameters publicKey, RSAParameters privateKey)> keys = new Dictionary<string, (RSAParameters publicKey, RSAParameters privateKey)>(); // <publicKey, privateKey>


        /// <summary>
        /// generate new keypair and add's it to list of all keys
        /// </summary>
        /// <returns>
        /// return only public key
        /// </returns>
        public static string GetPublicKey()
        {
            (RSAParameters publicKey, RSAParameters privateKey) keyPair = GenerateKeyPair();
            keys.Add(Convert.ToBase64String(keyPair.publicKey.Modulus), keyPair);
            return Convert.ToBase64String(keyPair.publicKey.Modulus);
        }


        /// <summary>
        /// generates public and private key
        /// </summary>
        /// <returns>
        /// returns a Tuple of keys with 2 items [publicKey] = public key, [privateKey] = private key
        /// </returns>
        private static (RSAParameters publicKey, RSAParameters privateKey) GenerateKeyPair()
        {
            RSAParameters publicKey;
            RSAParameters privateKey;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            publicKey = rsa.ExportParameters(false);
            privateKey = rsa.ExportParameters(true);
            return (publicKey, privateKey);
        }

        /// <summary>
        /// decrypts password and checks if the checksum is the same
        /// </summary>
        /// <returns>
        /// if the checksum is ok then the password is returned else "" is retured
        /// </returns>
        public static string DecryptPassword(string passhash, string pk, string checksum)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] bytesPasswordCypher = Convert.FromBase64String(passhash);
            rsa.ImportParameters(keys[pk].privateKey);
            byte[] bytesPassowrdPlain = rsa.Decrypt(bytesPasswordCypher, false);
            string pass = Encoding.Unicode.GetString(bytesPassowrdPlain);
            string sum;
            using (MD5 md5 = MD5.Create())
            {
                sum = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(pass))
                ).Replace("-", String.Empty);
            }
            if (sum.Equals(checksum))
            {
                return pass;
            }
            return "";
        }

    }
}
