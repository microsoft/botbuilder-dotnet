using System;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    public class JwtTokenValidationTests
    {
        public JwtTokenValidationTests()
        {
            // Disable TokenLifetime validation
            JwtConfig.ToBotFromChannelTokenValidationParameters.ValidateLifetime = false;
            JwtConfig.ToBotFromEmulatorTokenValidationParameters.ValidateLifetime = false;
        }

        [Fact]
        public async void Connector_AuthHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        {
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSIsIng1dCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSJ9.eyJzZXJ2aWNldXJsIjoiaHR0cHM6Ly93ZWJjaGF0LmJvdGZyYW1ld29yay5jb20vIiwiaXNzIjoiaHR0cHM6Ly9hcGkuYm90ZnJhbWV3b3JrLmNvbSIsImF1ZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImV4cCI6MTUxNjczNzUyMCwibmJmIjoxNTE2NzM2OTIwfQ.TBgpxbDS-gx1wm7ldvl7To-igfskccNhp-rU1mxUMtGaDjnsU--usH4OXZfzRsZqMlnXWXug_Hgd_qOr5RH8wVlnXnMWewoZTSGZrfp8GOd7jHF13Gz3F1GCl8akc3jeK0Ppc8R_uInpuUKa0SopY0lwpDclCmvDlz4PN6yahHkt_666k-9UGmRt0DDkxuYjbuYG8EDZxyyAhr7J6sFh3yE2UGRpJjRDB4wXWqv08Cp0Gn9PAW2NxOyN8irFzZH5_YZqE3DXDAYZ_IOLpygXQR0O-bFIhLDVxSz6uCeTBRjh8GU7XJ_yNiRDoaby7Rd2IfRrSnvMkBRsB8MsWN8oXg";
            var credentials = new SimpleCredentialProvider("39619a59-5a0c-4f9b-87c5-816c648ff357", "");
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, "https://webchat.botframework.com/");

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Connector_AuthHeader_BotAppIdDiffers_ShouldNotValidate()
        {
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSIsIng1dCI6IkdDeEFyWG9OOFNxbzdQd2VBNy16NjVkZW5KUSJ9.eyJzZXJ2aWNldXJsIjoiaHR0cHM6Ly93ZWJjaGF0LmJvdGZyYW1ld29yay5jb20vIiwiaXNzIjoiaHR0cHM6Ly9hcGkuYm90ZnJhbWV3b3JrLmNvbSIsImF1ZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImV4cCI6MTUxNjczNzUyMCwibmJmIjoxNTE2NzM2OTIwfQ.TBgpxbDS-gx1wm7ldvl7To-igfskccNhp-rU1mxUMtGaDjnsU--usH4OXZfzRsZqMlnXWXug_Hgd_qOr5RH8wVlnXnMWewoZTSGZrfp8GOd7jHF13Gz3F1GCl8akc3jeK0Ppc8R_uInpuUKa0SopY0lwpDclCmvDlz4PN6yahHkt_666k-9UGmRt0DDkxuYjbuYG8EDZxyyAhr7J6sFh3yE2UGRpJjRDB4wXWqv08Cp0Gn9PAW2NxOyN8irFzZH5_YZqE3DXDAYZ_IOLpygXQR0O-bFIhLDVxSz6uCeTBRjh8GU7XJ_yNiRDoaby7Rd2IfRrSnvMkBRsB8MsWN8oXg";
            var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", "");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, "https://webchat.botframework.com/"));
        }

        [Fact]
        public async void EmptyHeader_BotWithNoCredentials_ShouldValidate()
        {
            var header = "";
            var credentials = new SimpleCredentialProvider("", "");
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, "");

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Emulator_MsaHeader_CorrectAppIdAndServiceUrl_ShouldValidate()
        {
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ino0NHdNZEh1OHdLc3VtcmJmYUs5OHF4czVZSSIsImtpZCI6Ino0NHdNZEh1OHdLc3VtcmJmYUs5OHF4czVZSSJ9.eyJhdWQiOiIzOTYxOWE1OS01YTBjLTRmOWItODdjNS04MTZjNjQ4ZmYzNTciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9kNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIvIiwiaWF0IjoxNTE2OTI2NTUxLCJuYmYiOjE1MTY5MjY1NTEsImV4cCI6MTUxNjkzMDQ1MSwiYWlvIjoiWTJOZ1lJaVhOSisxeWxtbjJmenUxT3ZIZHk5ZEJRQT0iLCJhcHBpZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2Q2ZDQ5NDIwLWYzOWItNGRmNy1hMWRjLWQ1OWE5MzU4NzFkYi8iLCJ0aWQiOiJkNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIiLCJ1dGkiOiI5ellETVhfelgwcWJmT2VhTGtvZUFBIiwidmVyIjoiMS4wIn0.kNobwPhk40nCqjHQJWUanymXrmt9C1K7w6Jk-F3OrErKJILcdr9GhBxfnvBtfTZ9n769r7_UV5w2_JzN5oDfK8ipfgemtzcuJ8zVvPiVIBpYwPFCztkUhSCSmjY0DA9iazwGF1AmH7B7MYKyDkaqoVCZpGBLbEYJmZUCvaB4jOiR80paHROyEILa5_X0HM4gL0J5QnorJgihNti5Y7k03AoHHK0nUAPLQPKaWglNnj8DRKb4-RcmxmrW-5p091NFDxDRUv0hES9wigQ79SvJtu1LH0gNJlLL7lRW-EYqlkGoiMiPdnCnNE39v8mOXKrlJIaK1XuUWApDjxyA5FGSEw";
            var credentials = new SimpleCredentialProvider("39619a59-5a0c-4f9b-87c5-816c648ff357", "");
            var result = await JwtTokenValidation.ValidateAuthHeader(header, credentials, "");

            Assert.True(result.IsAuthenticated);
        }

        [Fact]
        public async void Emulator_MsaHeader_BotAppIdDiffers_ShouldNotValidate()
        {
            var header = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ino0NHdNZEh1OHdLc3VtcmJmYUs5OHF4czVZSSIsImtpZCI6Ino0NHdNZEh1OHdLc3VtcmJmYUs5OHF4czVZSSJ9.eyJhdWQiOiIzOTYxOWE1OS01YTBjLTRmOWItODdjNS04MTZjNjQ4ZmYzNTciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9kNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIvIiwiaWF0IjoxNTE2OTI2NTUxLCJuYmYiOjE1MTY5MjY1NTEsImV4cCI6MTUxNjkzMDQ1MSwiYWlvIjoiWTJOZ1lJaVhOSisxeWxtbjJmenUxT3ZIZHk5ZEJRQT0iLCJhcHBpZCI6IjM5NjE5YTU5LTVhMGMtNGY5Yi04N2M1LTgxNmM2NDhmZjM1NyIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2Q2ZDQ5NDIwLWYzOWItNGRmNy1hMWRjLWQ1OWE5MzU4NzFkYi8iLCJ0aWQiOiJkNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIiLCJ1dGkiOiI5ellETVhfelgwcWJmT2VhTGtvZUFBIiwidmVyIjoiMS4wIn0.kNobwPhk40nCqjHQJWUanymXrmt9C1K7w6Jk-F3OrErKJILcdr9GhBxfnvBtfTZ9n769r7_UV5w2_JzN5oDfK8ipfgemtzcuJ8zVvPiVIBpYwPFCztkUhSCSmjY0DA9iazwGF1AmH7B7MYKyDkaqoVCZpGBLbEYJmZUCvaB4jOiR80paHROyEILa5_X0HM4gL0J5QnorJgihNti5Y7k03AoHHK0nUAPLQPKaWglNnj8DRKb4-RcmxmrW-5p091NFDxDRUv0hES9wigQ79SvJtu1LH0gNJlLL7lRW-EYqlkGoiMiPdnCnNE39v8mOXKrlJIaK1XuUWApDjxyA5FGSEw";
            var credentials = new SimpleCredentialProvider("00000000-0000-0000-0000-000000000000", "");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await JwtTokenValidation.ValidateAuthHeader(header, credentials, ""));
            
        }
    }
}
