// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class FileConsentCardResponseTests
    {
        [Fact]
        public void FileConsentCardResponseInits()
        {
            var action = "accept";
            var context = "some context around the action";
            var uploadInfo = new FileUploadInfo("pandas.txt", "https://example.com", "https://url-to-the-file.com", "unique-panda-file-id", ".txt");
            var fileConsentCardResponse = new FileConsentCardResponse(action, context, uploadInfo);

            Assert.NotNull(fileConsentCardResponse);
            Assert.IsType<FileConsentCardResponse>(fileConsentCardResponse);
            Assert.Equal(action, fileConsentCardResponse.Action);
            Assert.Equal(context, fileConsentCardResponse.Context);
            Assert.Equal(uploadInfo, fileConsentCardResponse.UploadInfo);
        }

        [Fact]
        public void FileConsentCardResponseInitsWithNoArgs()
        {
            var fileConsentCardResponse = new FileConsentCardResponse();

            Assert.NotNull(fileConsentCardResponse);
            Assert.IsType<FileConsentCardResponse>(fileConsentCardResponse);
        }
    }
}
