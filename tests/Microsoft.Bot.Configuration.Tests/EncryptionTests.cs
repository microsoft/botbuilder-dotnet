using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Configuration.Encryption;

namespace Microsoft.Bot.Configuration.Tests
{
    [TestClass]
    public class EncryptionTests
    {
        [TestMethod]
        public void EncryptWithShortSecret()
        {
            string secret = "test";
            string value = "1234567890";

            string encrypted = value.Encrypt(secret);
            Assert.AreNotEqual(value, encrypted, "encryption failed");
            string decrypted = encrypted.Decrypt(secret);
            Assert.AreEqual(value, decrypted, "decryption failed");
        }

        [TestMethod]
        public void EncryptWithLongSecret()
        {
            string secret = "this is a test of the emergency broadcasting system";
            string value = "1234567890";

            string encrypted = value.Encrypt(secret);
            Assert.AreNotEqual(value, encrypted, "encryption failed with long secret");
            string decrypted = encrypted.Decrypt(secret);
            Assert.AreEqual(value, decrypted, "decryption failed with long secret");
        }

        [TestMethod]
        public void EncryptWithNullSecretThrows()
        {
            string value = "1234567890";

            try
            {

                string encrypted = value.Encrypt(null);
                Assert.Fail("Encrypt with null secret should throw");
            }
            catch (Exception)
            {

            }
        }


        [TestMethod]
        public void DecryptWithNullSecretThrows()
        {
            string value = "1234567890";

            try
            {

                string encrypted = value.Encrypt("test");
                string nonresult = encrypted.Decrypt(null);
                Assert.Fail("Decrypt with null secret should throw");
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void DecryptWithBadSecretThrows()
        {
            string value = "1234567890";

            try
            {

                string encrypted = value.Encrypt("good");
                string nonresult = encrypted.Decrypt("bad");
                Assert.Fail("Decrypt with bad secret should throw");
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void EncryptShouldMatchJavascriptEnryption()
        {
            string value = "1234567890";

            string encrypted = value.Encrypt("good");
            Assert.AreEqual("5SUSNKAk/20DW/9cAEcL9A==", encrypted, "the encryption settings should give same result that javascript does");
        }

        [TestMethod]
        public void EncryptWithIv()
        {
            string secret = "test";
            string value = "1234567890";
            string iv = "Yo";
            string encrypted = value.Encrypt(secret, iv);
            Assert.AreNotEqual(value, encrypted, "encryption with IV failed");

            string decrypted = encrypted.Decrypt(secret, iv);
            Assert.AreEqual(value, decrypted, "decryption with IV failed");

            iv = "this is a really long iv and it should work";
            encrypted = value.Encrypt(secret, iv);
            Assert.AreNotEqual(value, encrypted, "encryption with long IV failed");

            decrypted = encrypted.Decrypt(secret, iv);
            Assert.AreEqual(value, decrypted, "decryption with long IV failed");

            var encrypted1 = value.Encrypt(secret, "one");
            var encrypted2 = value.Encrypt(secret, "two");
            Assert.AreNotEqual(encrypted1, encrypted2, "using same value with 2 different salts should give different results");
        }

    }
}
