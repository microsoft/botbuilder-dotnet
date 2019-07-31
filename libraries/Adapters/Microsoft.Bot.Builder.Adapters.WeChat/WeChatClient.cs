using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// A WeChat client is used to commnicate with WeChat API.
    /// </summary>
    public class WeChatClient
    {
        private static readonly string ApiHost = "https://api.weixin.qq.com";
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
            ILogger logger = null,
            WeChatAttachmentStorage attachmentStorage = null,
            AccessTokenStorage tokenStorage = null,
            IAttachmentHash attachmentHash = null)
        {
            _appId = appId;
            _appSecret = appSecret;
            _logger = logger ?? NullLogger.Instance;
            _attachmentStorage = attachmentStorage ?? WeChatAttachmentStorage.Instance;
            _tokenStorage = tokenStorage ?? AccessTokenStorage.Instance;
            _attachmentHash = attachmentHash ?? MD5Hash.Instance;
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
        /// <param name="type">Message type.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendMessageToUser(object data, string type = null)
        {
            _logger.LogInformation("Send new message to user.");
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            var url = GetMessageApiEndPoint(accessToken);
            var bytes = await SendHttpRequestAsync(HttpMethod.Post, url, data).ConfigureAwait(false);
            var sendResult = ConvertBytesToType<WeChatJsonResult>(bytes);
            if (sendResult.ErrorCode != 0)
            {
                var exception = new Exception($"{sendResult.ToString()}");
                _logger.LogError(exception, "Send Message To User Failed");
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
        /// <returns>Response content as byte array.</returns>
        public virtual async Task<byte[]> SendHttpRequestAsync(HttpMethod method, string url, object data = null, string token = null)
        {
            _logger.LogInformation($"Send {method.Method} request to {url}", Severity.Information);
            var result = await MakeHttpRequestAsync(token, method, url, data).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get access token used to call WeChat API.
        /// </summary>
        /// <returns>Access token string.</returns>
        public virtual async Task<string> GetAccessTokenAsync()
        {
            var token = await _tokenStorage.GetAsync(_appId).ConfigureAwait(false);
            if (token == null)
            {
                token = new WeChatAccessToken(_appId, _appSecret);
            }

            if (token.ExpireTime.ToUnixTimeSeconds() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                var url = GetAccessTokenEndPoint(_appId, _appSecret);
                var bytes = await SendHttpRequestAsync(HttpMethod.Get, url).ConfigureAwait(false);
                var tokenResult = ConvertBytesToType<AccessTokenResult>(bytes);
                token.ExpireTime = DateTimeOffset.UtcNow.AddSeconds(tokenResult.ExpireIn);
                token.Token = tokenResult.Token;
                await _tokenStorage.SaveAsync(_appId, token);
            }

            return token.Token;
        }

        /// <summary>
        /// Upload temporary media (originally uploaded media files api).
        /// </summary>
        /// <param name="type">Media type.</param>
        /// <param name="attachmentData">attachment data to be uploaded.</param>
        /// <param name="timeout">Request timeout (milliseconds).</param>
        /// <returns>Result of upload Temporary media.</returns>
        public virtual async Task<UploadTemporaryMediaResult> UploadTemporaryMediaAsync(string type, AttachmentData attachmentData, int timeout = 10000)
        {
            var mediaHash = _attachmentHash.Hash(attachmentData.OriginalBase64);
            var uploadResult = (await _attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false)) as UploadTemporaryMediaResult;
            if (uploadResult == null)
            {
                var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
                var url = GetUploadMediaEndPoint(accessToken, type, true);
                uploadResult = await UploadMediaAsync<UploadTemporaryMediaResult>(attachmentData, url, type, mediaHash, true).ConfigureAwait(false);
                if (uploadResult.ErrorCode == 0)
                {
                    await _attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
                }
                else
                {
                    var exception = new Exception($"{uploadResult.ToString()}");
                    _logger.LogError(exception, $"Upload Temporary Media Failed, Type: {type.ToString()}");
                    throw exception;
                }
            }

            return uploadResult;
        }

        /// <summary>
        /// Added other types of permanent material.
        /// </summary>
        /// <param name="type">Upload media file type.</param>
        /// <param name="attachmentData">Attachment data to be uploaded.</param>
        /// <param name="timeout">Request timeout (milliseconds).</param>
        /// <returns>Result of upload persistent media.</returns>
        public virtual async Task<UploadPersistentMediaResult> UploadPersistentMediaAsync(string type, AttachmentData attachmentData, int timeout = 10000)
        {
            var mediaHash = _attachmentHash.Hash(attachmentData.OriginalBase64);
            var uploadResult = (await _attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false)) as UploadPersistentMediaResult;
            if (uploadResult == null)
            {
                var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
                var url = GetUploadMediaEndPoint(accessToken, type, false);
                uploadResult = await UploadMediaAsync<UploadPersistentMediaResult>(attachmentData, url, type, mediaHash, false).ConfigureAwait(false);
                if (uploadResult.ErrorCode == 0)
                {
                    await _attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogError(new Exception($"{uploadResult.ToString()}"), $"Upload Persistent Media Failed, Type: {type.ToString()}");
                }
            }

            return uploadResult;
        }

        /// <summary>
        /// Upload temporary graphic media.
        /// </summary>
        /// <param name="timeout">Request timeout (milliseconds).</param>
        /// <param name="newsList">Graphic material list.</param>
        /// <returns>Result of upload a persistent news.</returns>
        public virtual async Task<UploadPersistentMediaResult> UploadPersistentNewsAsync(int timeout = 10000, params News[] newsList)
        {
            var mediaHash = _attachmentHash.Hash(JsonConvert.SerializeObject(newsList));
            var uploadResult = (await _attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false)) as UploadPersistentMediaResult;
            if (uploadResult == null)
            {
                var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
                var url = GetUploadNewsEndPoint(accessToken, false);
                var data = new
                {
                    articles = newsList,
                };
                var bytes = await SendHttpRequestAsync(HttpMethod.Post, url, data).ConfigureAwait(false);
                uploadResult = ConvertBytesToType<UploadPersistentMediaResult>(bytes);
                if (uploadResult.ErrorCode == 0)
                {
                    await _attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
                }
                else
                {
                    var exception = new Exception($"{uploadResult.ToString()}");
                    _logger.LogError(exception, "Upload Persistent News Failed");
                    throw exception;
                }
            }

            return uploadResult;
        }

        /// <summary>
        /// Upload temporary graphic media.
        /// </summary>
        /// <param name="timeout">Request timeout (milliseconds).</param>
        /// <param name="newsList">Graphic material list.</param>
        /// <returns>Result of upload a temporary news.</returns>
        public virtual async Task<UploadTemporaryMediaResult> UploadTemporaryNewsAsync(int timeout = 10000, params News[] newsList)
        {
            var mediaHash = _attachmentHash.Hash(JsonConvert.SerializeObject(newsList));
            var uploadResult = await _attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false) as UploadTemporaryMediaResult;
            if (uploadResult == null || uploadResult.ExpiredTime <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
                var url = GetUploadNewsEndPoint(accessToken, true);
                var data = new
                {
                    articles = newsList,
                };
                var bytes = await SendHttpRequestAsync(HttpMethod.Post, url, data).ConfigureAwait(false);
                uploadResult = ConvertBytesToType<UploadTemporaryMediaResult>(bytes);
                if (uploadResult.ErrorCode == 0)
                {
                    await _attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
                }
                else
                {
                    var exception = new Exception($"{uploadResult.ToString()}");
                    _logger.LogError(exception, "Upload Temporary News Failed");
                    throw exception;
                }
            }

            return uploadResult;
        }

        /// <summary>
        /// Send customer service agent input status.
        /// </summary>
        /// <param name="openId">WeChat user's open id.</param>
        /// <param name="typingStatus">Agent typing status, can be Tying/CancelTyping.</param>
        /// <param name="timeout">Request timeout (milliseconds).</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendTypingStatusAsync(string openId, string typingStatus, int timeout = 10000)
        {
            object data = new
            {
                touser = openId,
                command = typingStatus,
            };
            return await SendMessageToUser(data);
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
                    msgtype = ResponseMessageType.Image,
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
                    msgtype = ResponseMessageType.Image,
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

            return await SendMessageToUser(data);
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
                    msgtype = ResponseMessageType.MPNews,
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
                    msgtype = ResponseMessageType.MPNews,
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

            return await SendMessageToUser(data);
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
                    msgtype = ResponseMessageType.Music,
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
                    msgtype = ResponseMessageType.Music,
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

            return await SendMessageToUser(data);
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
                    msgtype = ResponseMessageType.News,
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
                    msgtype = ResponseMessageType.News,
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

            return await SendMessageToUser(data);
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
                    msgtype = ResponseMessageType.Text,
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
                    msgtype = ResponseMessageType.Text,
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

            return await SendMessageToUser(data);
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
                    msgtype = ResponseMessageType.Video,
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
                    msgtype = ResponseMessageType.Video,
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

            return await SendMessageToUser(data);
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
                    msgtype = ResponseMessageType.Voice,
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
                    msgtype = ResponseMessageType.Voice,
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

            return await SendMessageToUser(data);
        }

        /// <summary>
        /// All http request send to wechat will be handled by this method.
        /// </summary>
        /// <param name="token">Authentication token.</param>
        /// <param name="method">Http method.</param>
        /// <param name="url">Request URL.</param>
        /// <param name="data">Request data.</param>
        /// <returns>Http response content as byte array.</returns>
        private static async Task<byte[]> MakeHttpRequestAsync(string token, HttpMethod method, string url, object data = null)
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

                    using (var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false))
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
                Console.WriteLine(HttpStatusCode.GatewayTimeout + $"{method} - {url} timed out");
                throw new HttpRequestException($"{method} - {url} timed out");
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
        /// Upload media data to WeChat.
        /// </summary>
        /// <typeparam name="T">The upload result type.</typeparam>
        /// <param name="attachmentData">The attachment data need to be uploaded.</param>
        /// <param name="url">The endpoint when upload the data.</param>
        /// <param name="type">The upload media type.</param>
        /// <param name="mediaHash">The media content hash result.</param>
        /// <param name="isTemporaryMedia">If upload media as a temporary media.</param>
        /// <returns>Uploaded result from WeChat.</returns>
        private async Task<T> UploadMediaAsync<T>(AttachmentData attachmentData, string url, string type, string mediaHash, bool isTemporaryMedia)
        {
            try
            {
                var ext = MapperUtils.GetMediaExtension(attachmentData.Name, attachmentData.Type, type);

                // Border break
                var boundary = "---------------" + DateTime.UtcNow.Ticks.ToString("x");
                var content = new MultipartFormDataContent(boundary);
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
                var contentByte = new ByteArrayContent(attachmentData.OriginalBase64);
                contentByte.Headers.Remove("Content-Disposition");

                contentByte.Headers.TryAddWithoutValidation("Content-Disposition", $"form-data; name=\"media\";filename=\"{mediaHash + ext}\"" + string.Empty);
                contentByte.Headers.Remove("Content-Type");
                contentByte.Headers.TryAddWithoutValidation("Content-Type", attachmentData.Type);
                content.Add(contentByte);

                // Additional form is required when upload a forever video.
                if (isTemporaryMedia == false && type == UploadMediaType.Video)
                {
                    var additionalForm = string.Format("{{\"title\":\"{0}\", \"introduction\":\"introduction\"}}", attachmentData.Name);

                    // Important! name must be "description"
                    content.Add(new StringContent(additionalForm), "\"" + "description" + "\"");
                }

                _logger.LogInformation($"Upload {type.ToString()} to WeChat", Severity.Information);
                var response = await SendHttpRequestAsync(HttpMethod.Post, url, content).ConfigureAwait(false);
                return ConvertBytesToType<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed To Upload Media, Type: {type.ToString()}");
                throw ex;
            }
        }
    }
}
