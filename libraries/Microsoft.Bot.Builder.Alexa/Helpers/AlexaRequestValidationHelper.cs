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
    public class AlexaRequestValidationHelper
    {
        public static Dictionary<string, X509Certificate2> ValidatedCertificateChains { get; } =
            new Dictionary<string, X509Certificate2>();

        private const string CertHeader = "-----BEGIN CERTIFICATE-----";
        private const string CertFooter = "-----END CERTIFICATE-----";

        private static readonly HttpClient Client = new HttpClient();

        public async Task ValidateRequestSecurity(HttpRequestMessage httpRequest, byte[] requestBytes, AlexaRequestBody requestBody)
        {
            if (requestBody?.Request?.Timestamp == null)
            {
                throw new InvalidOperationException("Alexa Request Invalid: Request Timestamp Missing");
            }

            var requestTimestamp = DateTime.Parse(requestBody.Request.Timestamp);

            if (requestTimestamp.AddSeconds(150) <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Alexa Request Invalid: Request Timestamp outside valid range");
            }

            httpRequest.Headers.TryGetValues("SignatureCertChainUrl", out var certUrls);
            httpRequest.Headers.TryGetValues("Signature", out var signatures);

            var certChainUrl = certUrls.FirstOrDefault();
            var signature = signatures.FirstOrDefault();

            if (string.IsNullOrEmpty(certChainUrl))
            {
                throw new InvalidOperationException("Alexa Request Invalid: missing SignatureCertChainUrl header");
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new InvalidOperationException("Alexa Request Invalid: missing Signature header");
            }

            var uri = new Uri(certChainUrl);

            if (uri.Scheme.ToLower() != "https")
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl bad scheme");
            }

            if (uri.Port != 443)
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl bad port");
            }

            if (uri.Host.ToLower() != "s3.amazonaws.com")
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl bad host");
            }

            if (!uri.AbsolutePath.StartsWith("/echo.api/"))
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl bad path");
            }

            X509Certificate2 signingCertificate;

            if (!ValidatedCertificateChains.ContainsKey(uri.ToString()))
            {
                var certList = await DownloadPemCertificatesAsync(uri.ToString());

                if (certList == null || certList.Length < 2)
                {
                    throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl download failed or too few certificates");
                }

                var primaryCert = certList[0];
                var subjectAlternativeNameList = ParseSujectAlternativeNames(primaryCert);

                if (!subjectAlternativeNameList.Contains("echo-api.amazon.com"))
                {
                    throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl certificate missing echo-api.amazon.com from Subject Alternative Names");
                }

                var chainCerts = new List<X509Certificate2>();

                for (var i = 1; i < certList.Length; i++)
                {
                    chainCerts.Add(certList[i]);
                }

                if (!ValidateCertificateChain(primaryCert, chainCerts))
                {
                    throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl certificate chain validation failed");
                }
                
                signingCertificate = primaryCert;

                lock (ValidatedCertificateChains)
                {
                    if (!ValidatedCertificateChains.ContainsKey(uri.ToString()))
                    {
                        ValidatedCertificateChains[uri.ToString()] = primaryCert;
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("Race condition hit while adding validated cert url: " + uri.ToString());
                    }
                }
            }
            else
            {
                signingCertificate = ValidatedCertificateChains[uri.ToString()];
            }

            if (signingCertificate == null)
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl certificate generic failure");
            }
            
            var signatureBytes = Convert.FromBase64String(signature);

            var publicKey = signingCertificate.GetRSAPublicKey();
            if (!publicKey.VerifyData(requestBytes, signatureBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1))
            {
                throw new InvalidOperationException("Alexa Request Invalid: Signature verification failed");
            }
        }

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
