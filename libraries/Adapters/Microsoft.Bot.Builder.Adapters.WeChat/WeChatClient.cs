// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses;
using Microsoft.Bot.Builder.Adapters.WeChat.Storage;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// A WeChat client is used to communicate with WeChat API.
    /// </summary>
    public class WeChatClient
    {
        private const string ApiHost = "https://api.weixin.qq.com";
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly string _appId;
        private readonly string _appSecret;
        private readonly ILogger _logger;
        private readonly WeChatAttachmentStorage _attachmentStorage;
        private readonly AccessTokenStorage _tokenStorage;
        private readonly IAttachmentHash _attachmentHash;

        public WeChatClient(
            string appId,
            string appSecret,
            IStorage storage,
            ILogger logger = null,
            IAttachmentHash attachmentHash = null)
        {
            _appId = appId;
            _appSecret = appSecret;
            _attachmentStorage = new WeChatAttachmentStorage(storage);
            _tokenStorage = new AccessTokenStorage(storage);
            _logger = logger ?? NullLogger.Instance;
            _attachmentHash = attachmentHash ?? new AttachmentHash();
        }

        /// <summary>
        /// Get media url from mediaId.
        /// </summary>
        /// <param name="mediaId">The media Id.</param>
        /// <returns>Url of the specific media.</returns>
        public async Task<string> GetMediaUrlAsync(string mediaId)
        {
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            var mediaUrl = $"{ApiHost}/cgi-bin/media/get?access_token={accessToken}&media_id={mediaId}";

            return mediaUrl;
        }

        /// <summary>
        /// Send message to user through customer service message api.
        /// </summary>
        /// <param name="data">Message data.</param>
        /// <param name="timeout">Send message to user timeout.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendMessageToUser(object data, int timeout = 10000)
        {
            _logger.LogInformation("Send message to user.");
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            var url = GetMessageApiEndPoint(accessToken);

            var bytes = await SendHttpRequestAsync(HttpMethod.Post, url, data, null, timeout).ConfigureAwait(false);
            var sendResult = ConvertBytesToType<WeChatJsonResult>(bytes);
            if (sendResult.ErrorCode != 0)
            {
                var exception = new Exception($"{sendResult}");
                _logger.LogError(exception, "Send message to user failed.");
                throw exception;
            }

            return sendResult;
        }

        /// <summary>
        /// Send http request wrapper for WeChat adapter.
        /// Can be override by developer.
        /// </summary>
        /// <param name="method">Http request method.</param>
        /// <param name="url">The url need to be requested.</param>
        /// <param name="data">The request payload.</param>
        /// <param name="token">Request auth token.</param>
        /// <param name="timeout">Send http request timeout.</param>
        /// <returns>Response content as byte array.</returns>
        public virtual async Task<byte[]> SendHttpRequestAsync(HttpMethod method, string url, object data = null, string token = null, int timeout = 10000)
        {
            _logger.LogInformation($"Send {method.Method} request to {url}");
            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout)))
            {
                var result = await MakeHttpRequestAsync(token, method, url, data, cancellationTokenSource.Token).ConfigureAwait(false);
                return result;
            }
        }

        /// <summary>
        /// Get access token used to call WeChat API.
        /// </summary>
        /// <returns>Access token string.</returns>
        public virtual async Task<string> GetAccessTokenAsync()
        {
            var token = await _tokenStorage.GetAsync(_appId).ConfigureAwait(false);
            if (token == null || token.ExpireTime.ToUnixTimeSeconds() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                var url = GetAccessTokenEndPoint(_appId, _appSecret);
                var bytes = await SendHttpRequestAsync(HttpMethod.Get, url).ConfigureAwait(false);
                var tokenResult = ConvertBytesToType<AccessTokenResult>(bytes);
                if (tokenResult.ErrorCode != 0)
                {
                    var exception = new Exception(tokenResult.ToString());
                    _logger.LogError(exception, "Get access token failed.");
                    throw exception;
                }

                token = new WeChatAccessToken()
                {
                    AppId = _appId,
                    Secret = _appSecret,
                    ExpireTime = DateTimeOffset.UtcNow.AddSeconds(tokenResult.ExpireIn),
                    Token = tokenResult.Token,
                };
                await _tokenStorage.SaveAsync(_appId, token).ConfigureAwait(false);
            }

            return token.Token;
        }

        /// <summary>
        /// Upload temporary media (originally uploaded media files api).
        /// </summary>
        /// <param name="attachmentData">attachment data to be uploaded.</param>
        /// <param name="isTemporary">If upload media as a temporary media.</param>
        /// <param name="timeout">Upload temporary media timeout.</param>
        /// <returns>Result of upload Temporary media.</returns>
        public virtual async Task<UploadMediaResult> UploadMediaAsync(AttachmentData attachmentData, bool isTemporary, int timeout = 30000)
        {
            var mediaHash = _attachmentHash.ComputeHash(attachmentData.OriginalBase64) + isTemporary;
            var cachedResult = await _attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false);
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            var url = GetUploadMediaEndPoint(accessToken, attachmentData.Type, isTemporary);
            if (cachedResult == null || cachedResult.Expired())
            {
                var uploadResult = await UploadMediaAsync(attachmentData, url, attachmentData.Type, mediaHash, isTemporary, timeout).ConfigureAwait(false);
                await CheckAndUpdateAttachmentStorage(mediaHash, uploadResult).ConfigureAwait(false);
                return uploadResult;
            }

            return cachedResult;
        }

        /// <summary>
        /// Added other types of permanent material.
        /// </summary>
        /// <param name="attachmentData">Attachment data to be uploaded.</param>
        /// <param name="timeout">Upload persistent media timeout.</param>
        /// <returns>Result of upload persistent media.</returns>
        public virtual async Task<UploadMediaResult> UploadNewsImageAsync(AttachmentData attachmentData, int timeout = 30000)
        {
            var mediaHash = _attachmentHash.ComputeHash(attachmentData.OriginalBase64);
            var cachedResult = await _attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false);
            if (cachedResult == null || cachedResult.Expired())
            {
                var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
                var url = GetAcquireMediaUrlEndPoint(accessToken);
                var uploadResult = await UploadMediaAsync(attachmentData, url, attachmentData.Type, mediaHash, false, timeout).ConfigureAwait(false);
                await CheckAndUpdateAttachmentStorage(mediaHash, uploadResult).ConfigureAwait(false);
                return uploadResult;
            }

            return cachedResult;
        }

        /// <summary>
        /// Upload temporary graphic media.
        /// </summary>
        /// <param name="newsList">Graphic material list.</param>
        /// <param name="isTemporary">If upload media as a temporary media.</param>
        /// <param name="timeout">Upload temporary news timeout.</param>
        /// <returns>Result of upload a temporary news.</returns>
        public virtual async Task<UploadMediaResult> UploadNewsAsync(News[] newsList, bool isTemporary, int timeout = 30000)
        {
            var mediaHash = _attachmentHash.ComputeHash(JsonConvert.SerializeObject(newsList)) + isTemporary;
            var cachedResult = await _attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false);
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            var url = GetUploadNewsEndPoint(accessToken, isTemporary);
            if (cachedResult == null || cachedResult.Expired())
            {
                var data = new
                {
                    articles = newsList,
                };
                var bytes = await SendHttpRequestAsync(HttpMethod.Post, url, data, null, timeout).ConfigureAwait(false);
                UploadMediaResult uploadResult;
                if (isTemporary)
                {
                    uploadResult = ConvertBytesToType<UploadTemporaryMediaResult>(bytes);
                }
                else
                {
                    uploadResult = ConvertBytesToType<UploadPersistentMediaResult>(bytes);
                }

                await CheckAndUpdateAttachmentStorage(mediaHash, uploadResult).ConfigureAwait(false);
                return uploadResult;
            }

            return cachedResult;
        }

        /// <summary>
        /// Send Image message.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="mediaId">Image media id.</param>
        /// <param name="timeout">Send message operation timeout.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendImageAsync(string openId, string mediaId, int timeout = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Image,
                    image = new
                    {
                        media_id = mediaId,
                    },
                };
            }
            else
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Image,
                    image = new
                    {
                        media_id = mediaId,
                    },
                    customservice = new
                    {
                        kf_account = customerServiceAccount,
                    },
                };
            }

            return await SendMessageToUser(data, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a graphic message (click to jump to the graphic message page) The number of the messages is limited to 8
        /// note that if the number of graphics more then 8, there will be no response.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="mediaId">Image media id.</param>
        /// <param name="timeout">Send message operation timeout.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendMPNewsAsync(string openId, string mediaId, int timeout = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.MPNews,
                    mpnews = new
                    {
                        media_id = mediaId,
                    },
                };
            }
            else
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.MPNews,
                    mpnews = new
                    {
                        media_id = mediaId,
                    },
                    customservice = new
                    {
                        kf_account = customerServiceAccount,
                    },
                };
            }

            return await SendMessageToUser(data, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Send music message to user.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="title">Music Title, Not required.</param>
        /// <param name="description">Not required.</param>
        /// <param name="musicUrl">Music url send to user.</param>
        /// <param name="highQualityMusicUrl">High-quality music link, wifi environment priority use this link to play music.</param>
        /// <param name="thumbMediaId">Media id for thumbnail image.</param>
        /// <param name="timeout">Send message operation timeout.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendMusicAsync(string openId, string title, string description, string musicUrl, string highQualityMusicUrl, string thumbMediaId, int timeout = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Music,
                    music = new
                    {
                        title = title,
                        description = description,
                        musicurl = musicUrl,
                        hqmusicurl = highQualityMusicUrl,
                        thumb_media_id = thumbMediaId,
                    },
                };
            }
            else
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Music,
                    music = new
                    {
                        title = title,
                        description = description,
                        musicurl = musicUrl,
                        hqmusicurl = highQualityMusicUrl,
                        thumb_media_id = thumbMediaId,
                    },
                    customservice = new
                    {
                        kf_account = customerServiceAccount,
                    },
                };
            }

            return await SendMessageToUser(data, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Send graphic message to user.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="articles">Article list will be sent to user.</param>
        /// <param name="timeout">Send message operation timeout.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendNewsAsync(string openId, List<Article> articles, int timeout = 10000, string customerServiceAccount = "")
        {
            object data = null;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.News,
                    news = new
                    {
                        articles = articles.Select(article => new
                        {
                            title = article.Title,
                            description = article.Description,
                            url = article.Url,
                            picurl = article.PicUrl,
                        }).ToList(),
                    },
                };
            }
            else
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.News,
                    news = new
                    {
                        articles = articles.Select(article => new
                        {
                            title = article.Title,
                            description = article.Description,
                            url = article.Url,
                            picurl = article.PicUrl,
                        }).ToList(),
                    },
                    customservice = new
                    {
                        kf_account = customerServiceAccount,
                    },
                };
            }

            return await SendMessageToUser(data, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Send text message to user.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="content">Text message content.</param>
        /// <param name="timeout">Send message operation timeout.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendTextAsync(string openId, string content, int timeout = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Text,
                    text = new
                    {
                        content = content,
                    },
                };
            }
            else
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Text,
                    text = new
                    {
                        content = content,
                    },
                    customservice = new
                    {
                        kf_account = customerServiceAccount,
                    },
                };
            }

            return await SendMessageToUser(data, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a video message.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="mediaId">Media id of the video.</param>
        /// <param name="title">The title of the video.</param>
        /// <param name="description">Video description.</param>
        /// <param name="timeout">Send message operation timeout.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <param name="thumbMeidaId">Thumbnail image media id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendVideoAsync(string openId, string mediaId, string title, string description, int timeout = 10000, string customerServiceAccount = "", string thumbMeidaId = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Video,
                    video = new
                    {
                        media_id = mediaId,
                        thumb_media_id = thumbMeidaId,
                        title = title,
                        description = description,
                    },
                };
            }
            else
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Video,
                    video = new
                    {
                        media_id = mediaId,
                        thumb_media_id = thumbMeidaId,
                        title = title,
                        description = description,
                    },
                    customservice = new
                    {
                        kf_account = customerServiceAccount,
                    },
                };
            }

            return await SendMessageToUser(data, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a voice message.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="mediaId">Media id of the voice.</param>
        /// <param name="timeout">Send message operation timeout.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendVoiceAsync(string openId, string mediaId, int timeout = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Voice,
                    voice = new
                    {
                        media_id = mediaId,
                    },
                };
            }
            else
            {
                data = new
                {
                    touser = openId,
                    msgtype = ResponseMessageTypes.Voice,
                    voice = new
                    {
                        media_id = mediaId,
                    },
                    customservice = new
                    {
                        kf_account = customerServiceAccount,
                    },
                };
            }

            return await SendMessageToUser(data, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// All http request send to WeChat will be handled by this method.
        /// </summary>
        /// <param name="token">Authentication token.</param>
        /// <param name="method">Http method.</param>
        /// <param name="url">Request URL.</param>
        /// <param name="data">Request data.</param>
        /// <param name="cancellationToken">Cancellation token to cancell the request.</param>
        /// <returns>Http response content as byte array.</returns>
        private static async Task<byte[]> MakeHttpRequestAsync(string token, HttpMethod method, string url, object data = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var uri = new Uri(url);
                using (var requestMessage = new HttpRequestMessage(method, uri))
                {
                    if (data != null)
                    {
                        requestMessage.Content = data as HttpContent ?? new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    using (var response = await HttpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException(response.ToString());
                        }

                        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                throw new HttpRequestException($"{method} - {url} timed out.");
            }
        }

        private static string GetMessageApiEndPoint(string accessToken)
        {
            return $"{ApiHost}/cgi-bin/message/custom/send?access_token={accessToken}";
        }

        private static string GetAccessTokenEndPoint(string appId, string appSecret)
        {
            return $"{ApiHost}/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={appSecret}";
        }

        private static string GetUploadMediaEndPoint(string accessToken, string type, bool isTemporaryMedia)
        {
            if (isTemporaryMedia)
            {
                return $"{ApiHost}/cgi-bin/media/upload?access_token={accessToken}&type={type}";
            }

            return $"{ApiHost}/cgi-bin/material/add_material?access_token={accessToken}&type={type}";
        }

        private static string GetUploadNewsEndPoint(string accessToken, bool isTemporaryNews)
        {
            if (isTemporaryNews)
            {
                return $"{ApiHost}/cgi-bin/media/uploadnews?access_token={accessToken}";
            }

            return $"{ApiHost}/cgi-bin/material/add_news?access_token={accessToken}";
        }

        private static string GetAcquireMediaUrlEndPoint(string accessToken)
        {
            return $"{ApiHost}/cgi-bin/media/uploadimg?access_token={accessToken}";
        }

        /// <summary>
        /// Get media's extension.
        /// </summary>
        /// <param name="link">The original link of the media.</param>
        /// <param name="mimeType">The media's mimeType.</param>
        /// <param name="type">The media's fallback type.</param>
        /// <returns>Media file extension.</returns>
        private static string GetMediaExtension(string link, string mimeType, string type)
        {
            var ext = MimeTypesMap.GetExtension(mimeType);
            if (string.IsNullOrEmpty(ext))
            {
                mimeType = MimeTypesMap.GetMimeType(link);
                ext = MimeTypesMap.GetExtension(mimeType);
            }

            if (string.IsNullOrEmpty(ext))
            {
                switch (type)
                {
                    case MediaTypes.Image:
                    case MediaTypes.Thumb:
                        ext = "jpg";
                        break;
                    case MediaTypes.Video:
                        ext = "mp4";
                        break;
                    case MediaTypes.Voice:
                        ext = "mp3";
                        break;
                }
            }

            return $".{ext}";
        }

        /// <summary>
        /// Convert result byte array from http call to a specific type.
        /// </summary>
        /// <typeparam name="T">The type need to be converted to.</typeparam>
        /// <param name="byteArray">The byte array need to be converted.</param>
        /// <returns>The result instance.</returns>
        private T ConvertBytesToType<T>(byte[] byteArray)
        {
            var result = Encoding.UTF8.GetString(byteArray);
            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// Check if upload media successful then update attachment storage.
        /// </summary>
        /// <param name="mediaHash">Hash value of the media.</param>
        /// <param name="uploadResult">Upload media result.</param>
        /// <returns>Task of updating media.</returns>
        private async Task CheckAndUpdateAttachmentStorage(string mediaHash, UploadMediaResult uploadResult)
        {
            if (uploadResult.ErrorCode == 0)
            {
                await _attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
            }
            else
            {
                var exception = new Exception($"{uploadResult}");
                _logger.LogError(exception, $"Upload media to WeChat failed.");
                throw exception;
            }
        }

        /// <summary>
        /// Upload media data to WeChat.
        /// </summary>
        /// <typeparam name="T">The upload result type.</typeparam>
        /// <param name="attachmentData">The attachment data need to be uploaded.</param>
        /// <param name="url">The endpoint when upload the data.</param>
        /// <param name="type">The upload media type.</param>
        /// <param name="mediaHash">The media content hash result.</param>
        /// <param name="isTemporaryMedia">If upload media as a temporary media.</param>
        /// <param name="timeout">Upload media timeout.</param>
        /// <returns>Uploaded result from WeChat.</returns>
        private async Task<UploadMediaResult> UploadMediaAsync(AttachmentData attachmentData, string url, string type, string mediaHash, bool isTemporaryMedia, int timeout = 30000)
        {
            try
            {
                // Border break
                var boundary = "---------------" + DateTime.UtcNow.Ticks.ToString("x", CultureInfo.InvariantCulture);
                using (var mutipartDataContent = new MultipartFormDataContent(boundary))
                {
                    mutipartDataContent.Headers.Remove("Content-Type");
                    mutipartDataContent.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);

                    // Add attachment content.
                    var contentByte = new ByteArrayContent(attachmentData.OriginalBase64);
                    contentByte.Headers.Remove("Content-Disposition");
                    var ext = GetMediaExtension(attachmentData.Name, attachmentData.Type, type);
                    contentByte.Headers.TryAddWithoutValidation("Content-Disposition", $"form-data; name=\"media\";filename=\"{mediaHash + ext}\"" + string.Empty);
                    contentByte.Headers.Remove("Content-Type");
                    contentByte.Headers.TryAddWithoutValidation("Content-Type", attachmentData.Type);
                    mutipartDataContent.Add(contentByte);

                    // Additional form is required when upload a forever video.
                    StringContent stringContent = null;
                    if (isTemporaryMedia == false && type == MediaTypes.Video)
                    {
                        var additionalForm = string.Format(CultureInfo.InvariantCulture, "{{\"title\":\"{0}\", \"introduction\":\"introduction\"}}", attachmentData.Name);

                        // Important! name must be "description"
                        stringContent = new StringContent(additionalForm);
                        mutipartDataContent.Add(stringContent, "\"" + "description" + "\"");
                    }

                    _logger.LogInformation($"Upload {type} to WeChat", Severity.Information);
                    var response = await SendHttpRequestAsync(HttpMethod.Post, url, mutipartDataContent, null, timeout).ConfigureAwait(false);

                    // Disponse all http content in mutipart form data content before return.
                    contentByte.Dispose();
                    if (stringContent != null)
                    {
                        stringContent.Dispose();
                    }

                    if (isTemporaryMedia)
                    {
                        return ConvertBytesToType<UploadTemporaryMediaResult>(response);
                    }

                    return ConvertBytesToType<UploadPersistentMediaResult>(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed To Upload Media, Type: {type}");
                throw;
            }
        }
    }
}
