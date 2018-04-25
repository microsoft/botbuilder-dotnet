using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Alexa.Helpers
{
    public static class PemHelper
    {
        private const string CertHeader = "-----BEGIN CERTIFICATE-----";
        private const string CertFooter = "-----END CERTIFICATE-----";

        private static readonly HttpClient Client = new HttpClient();

        public static IEnumerable<string> ParseSujectAlternativeNames(X509Certificate2 cert)
        {
            var sanRex = new Regex(@"^DNS Name=(.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            var sanList = from X509Extension ext in cert.Extensions
                          where ext.Oid.FriendlyName.Equals("Subject Alternative Name", StringComparison.Ordinal)
                          let data = new AsnEncodedData(ext.Oid, ext.RawData)
                          let text = data.Format(true)
                          from line in text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                          let match = sanRex.Match(line)
                          where match.Success && match.Groups.Count > 0 && !string.IsNullOrEmpty(match.Groups[1].Value)
                          select match.Groups[1].Value;

            return sanList;
        }

        public static bool ValidateCertificateChain(X509Certificate2 certificate, IEnumerable<X509Certificate2> chain)
        {
            using (var verifier = new X509Chain())
            {
                verifier.ChainPolicy.ExtraStore.AddRange(chain.ToArray());
                var result = verifier.Build(certificate);
                return result;
            }
        }

        public static X509Certificate2 ParseCertificate(string base64CertificateText)
        {
            var bytes = Convert.FromBase64String(base64CertificateText);
            var cert = new X509Certificate2(bytes);
            return cert;
        }

        public static async Task<X509Certificate2[]> DownloadPemCertificatesAsync(string pemUri)
        {
            var pemText = await Client.GetStringAsync(pemUri);
            return string.IsNullOrEmpty(pemText) ? null : ReadPemCertificates(pemText);
        }

        public static X509Certificate2[] ReadPemCertificates(string pemString)
        {
            var lines = pemString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var certList = new List<string>();
            StringBuilder grouper = null;
            for (int i = 0; i < lines.Length; i++)
            {
                var curLine = lines[i];
                if (curLine.Equals(CertHeader, StringComparison.Ordinal))
                {
                    grouper = new StringBuilder();
                }
                else if (curLine.Equals(CertFooter, StringComparison.Ordinal))
                {
                    certList.Add(grouper.ToString());
                    grouper = null;
                }
                else
                {
                    grouper?.Append(curLine);
                }
            }

            var collection = new List<X509Certificate2>();

            foreach (var certText in certList)
            {
                var cert = ParseCertificate(certText);
                collection.Add(cert);
            }

            return collection.ToArray();
        }
    }
}
