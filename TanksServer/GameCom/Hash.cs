using System.Linq;

namespace GameCom
{
    public static class Hash
    {
        //https://stackoverflow.com/questions/12416249/hashing-a-string-with-sha256
        public static byte[] HashData(byte[] dataToHash)
        {
            if(dataToHash == null || dataToHash.Length == 0)
            {
                return new byte[32];
            }
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            return crypt.ComputeHash(dataToHash);
        }

        public static bool HashAndCompare(byte[] dataToHash, byte[] toCompareHash)
        {
            var newHash = HashData(dataToHash);
            return newHash.SequenceEqual(toCompareHash);
        }

    }
}
