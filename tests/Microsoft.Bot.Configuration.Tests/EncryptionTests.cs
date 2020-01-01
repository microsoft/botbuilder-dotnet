using System;
using Microsoft.Bot.Configuration.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Configuration.Tests
{
    [TestClass]
    public class EncryptionTests
    {
        [TestMethod]
        public void EncryptDecrypt()
        {
            string value = "1234567890";
            var key = "lgCbJPXnfOlatjbBDKMbh0ie6bc8PD/cjqA/2tPgMS0=";

            string encrypted = value.Encrypt(key);
            Assert.AreNotEqual(value, encrypted, "encryption failed");

            string decrypted = encrypted.Decrypt(key);
            Assert.AreEqual(value, decrypted, "decryption failed");
        }

        [TestMethod]
        public void EncryptDecryptEmptyWorks()
        {
            var key = "lgCbJPXnfOlatjbBDKMbh0ie6bc8PD/cjqA/2tPgMS0=";

            string encrypted = string.Empty.Encrypt(key);
            Assert.AreEqual(string.Empty, encrypted, "encryption failed");

            string decrypted = encrypted.Decrypt(key);
            Assert.AreEqual(string.Empty, decrypted, "decryption failed");
        }

        [TestMethod]
        public void EncryptDecryptNullWorks()
        {
            var key = "lgCbJPXnfOlatjbBDKMbh0ie6bc8PD/cjqA/2tPgMS0=";

            string encrypted = EncryptUtilities.Encrypt(null, key);
            Assert.AreEqual(null, encrypted, "encryption failed");

            string decrypted = EncryptUtilities.Decrypt(encrypted, key);
            Assert.AreEqual(null, decrypted, "decryption failed");
        }

        [TestMethod]
        public void GenerateKeyWorks()
        {
            string value = "1234567890";
            var key = EncryptUtilities.GenerateKey();

            string encrypted = value.Encrypt(key);
            Assert.AreNotEqual(value, encrypted, "encryption failed");

            string decrypted = encrypted.Decrypt(key);
            Assert.AreEqual(value, decrypted, "decryption failed");
        }

        [TestMethod]
        public void EncryptWithNullKeyThrows()
        {
            string value = "1234567890";

            try
            {
                string encrypted = value.Encrypt(null);
                Assert.Fail("Encrypt with null key should throw");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void DecryptWithNullKeyThrows()
        {
            string value = "1234567890";

            try
            {
                var key = EncryptUtilities.GenerateKey();
                string encrypted = value.Encrypt(key);
                string nonresult = encrypted.Decrypt(null);
                Assert.Fail("Decrypt with null secret should throw");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void DecryptWithBadKeyThrows()
        {
            string value = "1234567890";

            try
            {
                var key = EncryptUtilities.GenerateKey();
                string encrypted = value.Encrypt(key);
                string nonresult = encrypted.Decrypt("bad");
                Assert.Fail("Decrypt with bad key should throw");
            }
            catch (Exception)
            {
            }

            try
            {
                var key = EncryptUtilities.GenerateKey();
                string encrypted = value.Encrypt(key);

                var key2 = EncryptUtilities.GenerateKey();
                string nonresult = encrypted.Decrypt(key2);
                Assert.Fail("Decrypt with different key should throw");
            }
            catch (Exception)
            {
            }
        }
    }
}
