using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Kms.V1;
using System.IO;
using Google.Protobuf;
using System.Text;

namespace WebApplication1.DataAccess
{
    public class KeyRepository
    {
        public static string Encrypt(string plaintext)
        {
            KeyManagementServiceClient client = KeyManagementServiceClient.Create();

            CryptoKeyName kn = CryptoKeyName.FromUnparsed(
                new Google.Api.Gax.UnparsedResourceName("projects/jurgen-cloud-project/locations/global/keyRings/pftckeyring/cryptoKeys/pftckeys"));
            string cipher = client.Encrypt(kn, ByteString.CopyFromUtf8(plaintext)).Ciphertext.ToBase64();

            return cipher;
        }

        public static string Decrypt(string cipher)
        {
            KeyManagementServiceClient client = KeyManagementServiceClient.Create();

            CryptoKeyName kn = CryptoKeyName.FromUnparsed(
                new Google.Api.Gax.UnparsedResourceName("projects/jurgen-cloud-project/locations/global/keyRings/pftckeyring/cryptoKeys/pftckeys"));

            byte[] cipherText = Convert.FromBase64String(cipher);

            DecryptResponse result = client.Decrypt(kn, ByteString.CopyFrom(cipherText));

            byte[] bytes = result.Plaintext.ToByteArray();
            string finalResult = Encoding.Default.GetString(bytes);
            
            return finalResult;
        }
    }
}