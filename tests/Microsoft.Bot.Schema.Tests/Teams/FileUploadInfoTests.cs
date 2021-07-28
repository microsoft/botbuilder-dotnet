// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class FileUploadInfoTests
    {
        [Fact]
        public void FileUploadInfoInits()
        {
            var name = "pandas";
            var uploadUrl = "https://example-happy-panda.com";
            var contentUrl = "https://example-url-to-pandas-content";
            var uniqueId = "uniqueId-pandas123";
            var fileType = ".png";

            var fileUploadInfo = new FileUploadInfo(name, uploadUrl, contentUrl, uniqueId, fileType);

            Assert.NotNull(fileUploadInfo);
            Assert.IsType<FileUploadInfo>(fileUploadInfo);
            Assert.Equal(name, fileUploadInfo.Name);
            Assert.Equal(uploadUrl, fileUploadInfo.UploadUrl);
            Assert.Equal(contentUrl, fileUploadInfo.ContentUrl);
            Assert.Equal(uniqueId, fileUploadInfo.UniqueId);
            Assert.Equal(fileType, fileUploadInfo.FileType);
        }
        
        [Fact]
        public void FileUploadInfoInitsWithNoArgs()
        {
            var fileUploadInfo = new FileUploadInfo();

            Assert.NotNull(fileUploadInfo);
            Assert.IsType<FileUploadInfo>(fileUploadInfo);
        }
    }
}
