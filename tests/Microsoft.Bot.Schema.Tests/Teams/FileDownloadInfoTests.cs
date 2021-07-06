// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class FileDownloadInfoTests
    {
        [Fact]
        public void FileDownloadInfoInits()
        {
            var downloadUrl = "https://example-download-url.com";
            var uniqueId = "file-unique-id-123";
            var fileType = ".txt";
            var etag = "etag123";

            var fileDownloadInfo = new FileDownloadInfo(downloadUrl, uniqueId, fileType, etag);

            Assert.NotNull(fileDownloadInfo);
            Assert.IsType<FileDownloadInfo>(fileDownloadInfo);
            Assert.Equal(downloadUrl, fileDownloadInfo.DownloadUrl);
            Assert.Equal(uniqueId, fileDownloadInfo.UniqueId);
            Assert.Equal(fileType, fileDownloadInfo.FileType);
            Assert.Equal(etag, fileDownloadInfo.Etag);
        }
        
        [Fact]
        public void FileDownloadInfoInitsWithNoArgs()
        {
            var fileDownloadInfo = new FileDownloadInfo();

            Assert.NotNull(fileDownloadInfo);
            Assert.IsType<FileDownloadInfo>(fileDownloadInfo);
        }
    }
}
