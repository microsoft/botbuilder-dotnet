using System;
using Microsoft.Bot.Configuration.Encryption;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Bot.Configuration.Tests
{
    public class EncryptionTests
    {
        [Fact]
        public void EncryptDecrypt()
        {
            var value = "1234567890";
            var key = "lgCbJPXnfOlatjbBDKMbh0ie6bc8PD/cjqA/2tPgMS0=";

            var encrypted = value.Encrypt(key);
            Assert.NotEqual(value, encrypted);

            var decrypted = encrypted.Decrypt(key);
            Assert.Equal(value, decrypted);
        }

        [Fact]
        public void EncryptDecryptEmptyWorks()
        {
            var key = "lgCbJPXnfOlatjbBDKMbh0ie6bc8PD/cjqA/2tPgMS0=";

            var encrypted = string.Empty.Encrypt(key);
            Assert.Equal(string.Empty, encrypted);

            var decrypted = encrypted.Decrypt(key);
            Assert.Equal(string.Empty, decrypted);
        }

        [Fact]
        public void EncryptDecryptNullWorks()
        {
            var key = "lgCbJPXnfOlatjbBDKMbh0ie6bc8PD/cjqA/2tPgMS0=";

            var encrypted = EncryptUtilities.Encrypt(null, key);
            Assert.Null(encrypted);

            var decrypted = EncryptUtilities.Decrypt(encrypted, key);
            Assert.Null(decrypted);
        }

        [Fact]
        public void GenerateKeyWorks()
        {
            var value = "1234567890";
            var key = EncryptUtilities.GenerateKey();

            var encrypted = value.Encrypt(key);
            Assert.NotEqual(value, encrypted);

            var decrypted = encrypted.Decrypt(key);
            Assert.Equal(value, decrypted);
        }

        [Fact]
        public void EncryptWithNullKeyThrows()
        {
            var value = "1234567890";

            try
            {
                var encrypted = value.Encrypt(null);
                throw new XunitException("Encrypt with null key should throw");
            }
            catch (Exception)
            {
            }
        }

        [Fact]
        public void DecryptWithNullKeyThrows()
        {
            var value = "1234567890";

            try
            {
                var key = EncryptUtilities.GenerateKey();
                var encrypted = value.Encrypt(key);
                var nonresult = encrypted.Decrypt(null);
                throw new XunitException("Decrypt with null key should throw");
            }
            catch (Exception)
            {
            }
        }

        [Fact]
        public void DecryptWithBadKeyThrows()
        {
            var value = "1234567890";

            try
            {
                var key = EncryptUtilities.GenerateKey();
                var encrypted = value.Encrypt(key);
                var nonresult = encrypted.Decrypt("bad");
                throw new XunitException("Decrypt with bad key should throw");
            }
            catch (Exception)
            {
            }

            try
            {
                var key = EncryptUtilities.GenerateKey();
                var encrypted = value.Encrypt(key);

                var key2 = EncryptUtilities.GenerateKey();
                var nonresult = encrypted.Decrypt(key2);
                throw new XunitException("Decrypt with different key should throw");
            }
            catch (Exception)
            {
            }
        }
    }
}
