using System;
using System.Threading.Tasks;

namespace MHWSaveUtils
{
    public class Crypto
    {
        private readonly Blowfish blowfish;

        public Crypto()
        {
            blowfish = new Blowfish("xieZjoe#P2134-3zmaghgpqoe0z8$3azeq");
        }

        private static readonly (int offset, int size)[] encryptedRegions = new[]
        {
            (0x70, 0xDA50),
            (0x3010D8, 0x2098C0),
            (0x50AB98, 0x2098C0),
            (0x714658, 0x2098C0)
        };

        /// <summary>
        /// Decrypts a byte array in-place.
        /// </summary>
        /// <param name="buffer">Ciphertext to decrypt.</param>
        public void Decrypt(byte[] buffer)
        {
            blowfish.Decrypt(buffer);

            Parallel.ForEach(encryptedRegions, region => Cirilla.Core.Crypto.IceborneCrypto.DecryptRegion(buffer, region.offset, region.size));
        }

        /// <summary>
        /// Decrypts a byte array in-place.
        /// </summary>
        /// <param name="buffer">Ciphertext to decrypt.</param>
        /// <returns>Returns a task that completes when decryption is done.</returns>
        public Task DecryptAsync(byte[] buffer)
        {
            return Task.Run(() => Decrypt(buffer));
        }
    }
}
