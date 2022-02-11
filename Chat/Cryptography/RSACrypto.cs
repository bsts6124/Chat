using System.Security.Cryptography;

namespace Crypto
{
    class RSACrypto
    {
        public class PublicKey
        {
            byte[] m_modulus;
            byte[] m_exponent;

            public PublicKey(byte[] modulus, byte[] exponent)
            {
                m_modulus = modulus;
                m_exponent = exponent;
            }

            public PublicKey(string modulusBase64, string exponentBase64)
            {
                m_modulus = System.Convert.FromBase64String(modulusBase64);
                m_exponent = System.Convert.FromBase64String(exponentBase64);
            }


            public static implicit operator RSAParameters(PublicKey key)
            {
                RSAParameters rsaParas = new RSAParameters();
                rsaParas.Modulus = key.m_modulus;
                rsaParas.Exponent = key.m_exponent;

                return rsaParas;
            }

            public byte[] Modulus
            {
                get { return m_modulus; }
            }

            public byte[] Exponent
            {
                get { return m_exponent; }
            }
        }

        public class PrivateKey
        {
            RSAParameters m_parameters;
            public PrivateKey(RSAParameters parameters)
            {
                m_parameters = parameters;
            }

            public static implicit operator RSAParameters(PrivateKey key)
            {
                return key.m_parameters;
            }
        }

        public class KeyPair
        {
            public PublicKey publicKey;
            public PrivateKey privateKey;

            public KeyPair(int KeyBits)
            {
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(KeyBits))
                {
                    RSAParameters privateParam = RSA.ExportParameters(true);
                    publicKey = new PublicKey(privateParam.Modulus, privateParam.Exponent);
                    privateKey = new PrivateKey(privateParam);
                }
            }
        }

        public static KeyPair GenKey(int KeyBits)
        {
            return new KeyPair(KeyBits);
        }

        public static byte[] Encrypt(PublicKey key, byte[] data, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData = null;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(key);
                    encryptedData = rsa.Encrypt(data, DoOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                //Debug.WriteLine(e);
            }

            return null;
        }

        public static byte[] Decrypt(PrivateKey key, byte[] data, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData = null;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(key);
                    decryptedData = rsa.Decrypt(data, DoOAEPPadding);
                }

                return decryptedData;
            }
            catch (CryptographicException e)
            {
                //Debug.WriteLine(e);
            }

            return null;
        }
        public static void WritePublicKeyToFile(PublicKey key, string fileName)
        {
            using (System.IO.StreamWriter fw = new System.IO.StreamWriter(fileName))
            {
                fw.WriteLine(System.Convert.ToBase64String(key.Modulus));
                fw.WriteLine(System.Convert.ToBase64String(key.Exponent));
            }
        }

        public static PublicKey ReadPublicKeyFromFile(string fileName)
        {
            using (System.IO.StreamReader fr = new System.IO.StreamReader(fileName))
            {
                byte[] modulus = System.Convert.FromBase64String(fr.ReadLine());
                byte[] exponent = System.Convert.FromBase64String(fr.ReadLine());

                return new RSACrypto.PublicKey(modulus, exponent);
            }
        }
    }
}
