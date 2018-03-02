using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    public class JwtTokenValidationTests
    {
        private readonly HttpClient client;
        private readonly HttpClient emptyClient;

        public JwtTokenValidationTests()
        {
            // Disable TokenLifetime validation
            EmulatorValidation.ToBotFromEmulatorTokenValidationParameters.ValidateLifetime = false;
            ChannelValidation.ToBotFromChannelTokenValidationParameters.ValidateLifetime = false;
            client = new HttpClient
            {
                BaseAddress = new Uri("https://webchat.botframework.com/")
            };
            emptyClient = new HttpClient();
        }

        [Fact]
        public async void Connector_AuthHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        {
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSIsIng1dCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSJ9.eyJzZXJ2aWNldXJsIjoiaHR0cHM6Ly93ZWJjaGF0LmJvdGZyYW1ld29yay5jb20vIiwiaXNzIjoiaHR0cHM6Ly9hcGkuYm90ZnJhbWV3b3JrLmNvbSIsImF1ZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImV4cCI6MTUxNjczNzUyMCwibmJmIjoxNTE2NzM2OTIwfQ.TBgpxbDS-gx1wm7ldvl7To-igfskccNhp-rU1mxUMtGaDjnsU--usH4OXZfzRsZqMlnXWXug_Hgd_qOr5RH8wVlnXnMWewoZTSGZrfp8GOd7jHF13Gz3F1GCl8akc3jeK0Ppc8R_uInpuUKa0SopY0lwpDclCmvDlz4PN6yahHkt_666k-9UGmRt0DDkxuYjbuYG8EDZxyyAhr7J6sFh3yE2UGRpJjRDB4wXWqv08Cp0Gn9PAW2NxOyN8irFzZH5_YZqE3DXDAYZ_IOLpygXQR0O-bFIhLDVxSz6uCeTBRjh8GU7XJ_yNiRDoaby7Rd2IfRrSnvMkBRsB8MsWN8oXg";
            var credentials = new SimpleCredentialProvider("39619a59-5a0c-4f9b-87c5-816c648ff357", "");
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, "https://webchat.botframework.com/", client);

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Connector_AuthHeader_BotAppIdDiffers_ShouldNotValidate()
        {
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSIsIng1dCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSJ9.eyJzZXJ2aWNldXJsIjoiaHR0cHM6Ly93ZWJjaGF0LmJvdGZyYW1ld29yay5jb20vIiwiaXNzIjoiaHR0cHM6Ly9hcGkuYm90ZnJhbWV3b3JrLmNvbSIsImF1ZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImV4cCI6MTUxNjczNzUyMCwibmJmIjoxNTE2NzM2OTIwfQ.TBgpxbDS-gx1wm7ldvl7To-igfskccNhp-rU1mxUMtGaDjnsU--usH4OXZfzRsZqMlnXWXug_Hgd_qOr5RH8wVlnXnMWewoZTSGZrfp8GOd7jHF13Gz3F1GCl8akc3jeK0Ppc8R_uInpuUKa0SopY0lwpDclCmvDlz4PN6yahHkt_666k-9UGmRt0DDkxuYjbuYG8EDZxyyAhr7J6sFh3yE2UGRpJjRDB4wXWqv08Cp0Gn9PAW2NxOyN8irFzZH5_YZqE3DXDAYZ_IOLpygXQR0O-bFIhLDVxSz6uCeTBRjh8GU7XJ_yNiRDoaby7Rd2IfRrSnvMkBRsB8MsWN8oXg";
            var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", "");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, null, client));
        }

        [Fact]
        public async void Connector_AuthHeader_BotWithNoCredentials_ShouldNotValidate()
        {
            // token received and auth disabled
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSIsIng1dCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSJ9.eyJzZXJ2aWNldXJsIjoiaHR0cHM6Ly93ZWJjaGF0LmJvdGZyYW1ld29yay5jb20vIiwiaXNzIjoiaHR0cHM6Ly9hcGkuYm90ZnJhbWV3b3JrLmNvbSIsImF1ZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImV4cCI6MTUxNjczNzUyMCwibmJmIjoxNTE2NzM2OTIwfQ.TBgpxbDS-gx1wm7ldvl7To-igfskccNhp-rU1mxUMtGaDjnsU--usH4OXZfzRsZqMlnXWXug_Hgd_qOr5RH8wVlnXnMWewoZTSGZrfp8GOd7jHF13Gz3F1GCl8akc3jeK0Ppc8R_uInpuUKa0SopY0lwpDclCmvDlz4PN6yahHkt_666k-9UGmRt0DDkxuYjbuYG8EDZxyyAhr7J6sFh3yE2UGRpJjRDB4wXWqv08Cp0Gn9PAW2NxOyN8irFzZH5_YZqE3DXDAYZ_IOLpygXQR0O-bFIhLDVxSz6uCeTBRjh8GU7XJ_yNiRDoaby7Rd2IfRrSnvMkBRsB8MsWN8oXg";
            var credentials = new SimpleCredentialProvider("", "");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, null, client));
        }

        [Fact]
        public async void EmptyHeader_BotWithNoCredentials_ShouldThrow()
        {
            var header = "";
            var credentials = new SimpleCredentialProvider("", "");


            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, null, emptyClient));
        }

        [Fact]
        public async void Emulator_MsaHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        {
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlNTUWRoSTFjS3ZoUUVEU0p4RTJnR1lzNDBRMCIsImtpZCI6IlNTUWRoSTFjS3ZoUUVEU0p4RTJnR1lzNDBRMCJ9.eyJhdWQiOiIzOTYxOWE1OS01YTBjLTRmOWItODdjNS04MTZjNjQ4ZmYzNTciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9kNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIvIiwiaWF0IjoxNTE4MTIzMTQxLCJuYmYiOjE1MTgxMjMxNDEsImV4cCI6MTUxODEyNzA0MSwiYWlvIjoiWTJOZ1lQZ1djOSsrenJvaW9QM28rZmw2OWR1c0FBPT0iLCJhcHBpZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2Q2ZDQ5NDIwLWYzOWItNGRmNy1hMWRjLWQ1OWE5MzU4NzFkYi8iLCJ0aWQiOiJkNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIiLCJ1dGkiOiJPVXE3M1lSbGtFcVoxQ3p2U3FZQkFBIiwidmVyIjoiMS4wIn0.B0t4sSsqIQ3IT2rfpZXqAdAGJSr3aihwk-jJd8as2pAoeQVcQNir_Anvvnjbo5MsB0DCyWFa9xnEmBRiTW_Ww97Z9bZhnCXq4D4vN8dmgEMV_Aci1tI4agy3coCX4fBRc76SHjqJ_ucl850aqR3d_0sfl0TPoDclE4jWssX2YTNzUAMEgisbYe9xv8FfK7AUR8ABS1teTfnWGVYyVFgC7vptSjw-de8sgz7pv8vVtLEKBrrb1FBSzHbbnZ-cQaLLHeIM4agamXf4w45o7_1uHorrp1Tg5oPrsbiayC-dt4lpC9smU5agpyUWCorKZI0Fp3aryG4519cYuLyXuUVh0A";
            var credentials = new SimpleCredentialProvider("39619a59-5a0c-4f9b-87c5-816c648ff357", "");
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, "https://webchat.botframework.com/", emptyClient);

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Emulator_MsaHeader_BotAppIdDiffers_ShouldNotValidate()
        {
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlNTUWRoSTFjS3ZoUUVEU0p4RTJnR1lzNDBRMCIsImtpZCI6IlNTUWRoSTFjS3ZoUUVEU0p4RTJnR1lzNDBRMCJ9.eyJhdWQiOiIzOTYxOWE1OS01YTBjLTRmOWItODdjNS04MTZjNjQ4ZmYzNTciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9kNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIvIiwiaWF0IjoxNTE4MTIzMTQxLCJuYmYiOjE1MTgxMjMxNDEsImV4cCI6MTUxODEyNzA0MSwiYWlvIjoiWTJOZ1lQZ1djOSsrenJvaW9QM28rZmw2OWR1c0FBPT0iLCJhcHBpZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2Q2ZDQ5NDIwLWYzOWItNGRmNy1hMWRjLWQ1OWE5MzU4NzFkYi8iLCJ0aWQiOiJkNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIiLCJ1dGkiOiJPVXE3M1lSbGtFcVoxQ3p2U3FZQkFBIiwidmVyIjoiMS4wIn0.B0t4sSsqIQ3IT2rfpZXqAdAGJSr3aihwk-jJd8as2pAoeQVcQNir_Anvvnjbo5MsB0DCyWFa9xnEmBRiTW_Ww97Z9bZhnCXq4D4vN8dmgEMV_Aci1tI4agy3coCX4fBRc76SHjqJ_ucl850aqR3d_0sfl0TPoDclE4jWssX2YTNzUAMEgisbYe9xv8FfK7AUR8ABS1teTfnWGVYyVFgC7vptSjw-de8sgz7pv8vVtLEKBrrb1FBSzHbbnZ-cQaLLHeIM4agamXf4w45o7_1uHorrp1Tg5oPrsbiayC-dt4lpC9smU5agpyUWCorKZI0Fp3aryG4519cYuLyXuUVh0A";
            var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", "");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, null, emptyClient));
            
        }

        /// <summary>
        /// Tests with a valid Token and service url; and ensures that Service url is added to Trusted service url list.
        /// </summary>
        [Fact]
        public async void Channel_MsaHeader_Valid_ServiceUrlShouldBeTrusted()
        {
            var header = "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6ImI0eXNPV0l0RDEzaVFmTExlQkZYOWxSUER0ayIsInR5cCI6IkpXVCIsIng1dCI6ImI0eXNPV0l0RDEzaVFmTExlQkZYOWxSUER0ayJ9.eyJzZXJ2aWNldXJsIjoiaHR0cHM6Ly9zbWJhLnRyYWZmaWNtYW5hZ2VyLm5ldC9hbWVyLWNsaWVudC1zcy5tc2cvIiwibmJmIjoxNTE5Njk3OTQ0LCJleHAiOjE1MTk3MDE1NDQsImlzcyI6Imh0dHBzOi8vYXBpLmJvdGZyYW1ld29yay5jb20iLCJhdWQiOiI3Zjc0NTEzZS02Zjk2LTRkYmMtYmU5ZC05YTgxZmVhMjJiODgifQ.wjApM-MBhEIHSRHJGmivfpyFg0-SrTFh6Xta2RrKlZT4urACPX7kdZAb6oGOeDIm0NU16BPcpEqtCm9nBPmwoKKRbLCQ4Q3DGcB_LY15VCYfiiAnaevNNcvq7j_Hu-oyTmKOqpjfzu8qMIsjySClf1qZFucUrqzccePtlb63DAVfv-nF3bp-sm-zFG7RBX32cCygBMvpVENBroAq3ANfUQCmixkExcGr5npV3dFihSE0H9ntLMGseBdW7dRe5xOXDIgCtcCJPid-A6Vz-DxWGabyy2mVXLwYYuDxP4L5aruGwJIl_Z2-_MjhrWVszoeCRoOlx9-LNtbdSYGWmXWSbg";
            var credentials = new SimpleCredentialProvider("7f74513e-6f96-4dbc-be9d-9a81fea22b88", "");

            await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://smba.trafficmanager.net/amer-client-ss.msg/" },
                header,
                credentials,
                emptyClient);

            Assert.True(MicrosoftAppCredentials.IsTrustedServiceUrl("https://smba.trafficmanager.net/amer-client-ss.msg/"));
        }

        /// <summary>
        /// Tests with a valid Token and invalid service url; and ensures that Service url is NOT added to Trusted service url list.
        /// </summary>
        [Fact]
        public async void Channel_MsaHeader_Invalid_ServiceUrlShouldNotBeTrusted()
        {
            var header = "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6ImI0eXNPV0l0RDEzaVFmTExlQkZYOWxSUER0ayIsInR5cCI6IkpXVCIsIng1dCI6ImI0eXNPV0l0RDEzaVFmTExlQkZYOWxSUER0ayJ9.eyJzZXJ2aWNldXJsIjoiaHR0cHM6Ly9zbWJhLnRyYWZmaWNtYW5hZ2VyLm5ldC9hbWVyLWNsaWVudC1zcy5tc2cvIiwibmJmIjoxNTE5Njk3OTQ0LCJleHAiOjE1MTk3MDE1NDQsImlzcyI6Imh0dHBzOi8vYXBpLmJvdGZyYW1ld29yay5jb20iLCJhdWQiOiI3Zjc0NTEzZS02Zjk2LTRkYmMtYmU5ZC05YTgxZmVhMjJiODgifQ.wjApM-MBhEIHSRHJGmivfpyFg0-SrTFh6Xta2RrKlZT4urACPX7kdZAb6oGOeDIm0NU16BPcpEqtCm9nBPmwoKKRbLCQ4Q3DGcB_LY15VCYfiiAnaevNNcvq7j_Hu-oyTmKOqpjfzu8qMIsjySClf1qZFucUrqzccePtlb63DAVfv-nF3bp-sm-zFG7RBX32cCygBMvpVENBroAq3ANfUQCmixkExcGr5npV3dFihSE0H9ntLMGseBdW7dRe5xOXDIgCtcCJPid-A6Vz-DxWGabyy2mVXLwYYuDxP4L5aruGwJIl_Z2-_MjhrWVszoeCRoOlx9-LNtbdSYGWmXWSbg";
            var credentials = new SimpleCredentialProvider("7f74513e-6f96-4dbc-be9d-9a81fea22b88", "");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://webchat.botframework.com/" },
                header,
                credentials,
                emptyClient));

            Assert.False(MicrosoftAppCredentials.IsTrustedServiceUrl("https://webchat.botframework.com/"));
        }

        /// <summary>
        /// Tests with no authentication header and makes sure the service URL is not added to the trusted list.
        /// </summary>
        [Fact]
        public async void Channel_AuthenticationDisabled_ShouldBeAnonymous()
        {
            var header = "";
            var credentials = new SimpleCredentialProvider();

            var claimsPrincipal = await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://webchat.botframework.com/" },
                header,
                credentials,
                emptyClient);

            Assert.Equal("anonymous", claimsPrincipal.AuthenticationType);
        }

        /// <summary>
        /// Tests with no authentication header and makes sure the service URL is not added to the trusted list.
        /// </summary>
        [Fact]
        public async void Channel_AuthenticationDisabled_ServiceUrlShouldNotBeTrusted()
        {
            var header = "";
            var credentials = new SimpleCredentialProvider();

            var claimsPrincipal = await JwtTokenValidation.AuthenticateRequest(
                new Activity { ServiceUrl = "https://webchat.botframework.com/" },
                header,
                credentials,
                emptyClient);

            Assert.False(MicrosoftAppCredentials.IsTrustedServiceUrl("https://webchat.botframework.com/"));
        }
    }
}
