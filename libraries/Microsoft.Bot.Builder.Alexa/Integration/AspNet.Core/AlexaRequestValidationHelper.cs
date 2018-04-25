using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Alexa.Helpers;

namespace Microsoft.Bot.Builder.Alexa.Integration.AspNet.Core
{
    public class AlexaRequestValidationHelper
    {
        public static Dictionary<string, X509Certificate2> ValidatedCertificateChains { get; } =
            new Dictionary<string, X509Certificate2>();

        public async Task ValidateRequestSecurity(HttpRequest httpRequest,
            AlexaRequestBody requestBody)
        {
            var memoryStream = new MemoryStream();
            await httpRequest.Body.CopyToAsync(memoryStream);
            byte[] requestBytes = memoryStream.ToArray();

            if (requestBody?.Request?.Timestamp == null)
            {
                throw new InvalidOperationException("Alexa Request Invalid: Request Timestamp Missing");
            }

            var requestTimestamp = DateTime.Parse(requestBody.Request.Timestamp);

            if (requestTimestamp.AddSeconds(150) <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Alexa Request Invalid: Request Timestamp outside valid range");
            }

            httpRequest.Headers.TryGetValue("SignatureCertChainUrl", out var certUrls);
            httpRequest.Headers.TryGetValue("Signature", out var signatures);

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
                var certList = await PemHelper.DownloadPemCertificatesAsync(uri.ToString());

                if (certList == null || certList.Length < 2)
                {
                    throw new InvalidOperationException(
                        "Alexa Request Invalid: SignatureCertChainUrl download failed or too few certificates");
                }

                var primaryCert = certList[0];
                var subjectAlternativeNameList = PemHelper.ParseSujectAlternativeNames(primaryCert);

                if (!subjectAlternativeNameList.Contains("echo-api.amazon.com"))
                {
                    throw new InvalidOperationException(
                        "Alexa Request Invalid: SignatureCertChainUrl certificate missing echo-api.amazon.com from Subject Alternative Names");
                }

                var chainCerts = new List<X509Certificate2>();

                for (var i = 1; i < certList.Length; i++)
                {
                    chainCerts.Add(certList[i]);
                }

                if (!PemHelper.ValidateCertificateChain(primaryCert, chainCerts))
                {
                    throw new InvalidOperationException(
                        "Alexa Request Invalid: SignatureCertChainUrl certificate chain validation failed");
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
                        System.Diagnostics.Trace.WriteLine("Race condition hit while adding validated cert url: " +
                                                           uri.ToString());
                    }
                }
            }
            else
            {
                signingCertificate = ValidatedCertificateChains[uri.ToString()];
            }

            if (signingCertificate == null)
            {
                throw new InvalidOperationException(
                    "Alexa Request Invalid: SignatureCertChainUrl certificate generic failure");
            }

            var signatureBytes = Convert.FromBase64String(signature);

            var publicKey = signingCertificate.GetRSAPublicKey();
            if (!publicKey.VerifyData(requestBytes, signatureBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1))
            {
                throw new InvalidOperationException("Alexa Request Invalid: Signature verification failed");
            }
        }
    }
}
