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
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class WeChatClient
    {
        private static readonly string APIHost = "https://api.weixin.qq.com";
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly string appId;
        private readonly string appSecret;
        private readonly WeChatLogger logger;
        private readonly bool uploadTemporaryMedia;
        private readonly WeChatAttachmentStorage attachmentStorage;
        private readonly AccessTokenStorage tokenStorage;
        private readonly IAttachmentHash attachmentHash;

        public WeChatClient(
            IConfiguration configuration,
            WeChatLogger logger = null,
            WeChatAttachmentStorage attachmentStorage = null,
            AccessTokenStorage tokenStorage = null,
            IAttachmentHash attachmentHash = null)
        {
            this.appId = configuration.GetSection("WeChatSetting").GetSection("AppId")?.Value;
            this.appSecret = configuration.GetSection("WeChatSetting").GetSection("AppSecret")?.Value;
            this.uploadTemporaryMedia = configuration.GetSection("WeChatSetting")?.GetValue<bool>("UploadTemporaryMedia") ?? true;
            this.logger = logger ?? WeChatLogger.Instance;
            this.attachmentStorage = attachmentStorage ?? WeChatAttachmentStorage.Instance;
            this.tokenStorage = tokenStorage ?? AccessTokenStorage.Instance;
            this.attachmentHash = attachmentHash ?? MD5Hash.Instance;
        }

        /// <summary>
        /// Get media url from mediaId.
        /// </summary>
        /// <param name="mediaId">The media Id.</param>
        /// <returns>Url of the specific media.</returns>
        public async Task<string> GetMediaUrlAsync(string mediaId)
        {
            var accessToken = await this.GetAccessTokenAsync().ConfigureAwait(false);
            var mediaUrl = $"http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={accessToken}&media_id={mediaId}";

            // TODO: move the links out of the mapper to config file.
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
            this.logger.TrackTrace("Send new message to user.");
            var accessToken = await this.GetAccessTokenAsync().ConfigureAwait(false);
            var url = this.GetMessageApiEndPoint(accessToken);
            var bytes = await this.SendHttpRequestAsync(HttpMethod.Post, url, data).ConfigureAwait(false);
            var sendResult = this.ConvertBytesToType<WeChatJsonResult>(bytes);
            if (sendResult.ErrorCode != 0)
            {
                this.logger.TrackException("Send Message To User Failed", new Exception($"{sendResult.ToString()}"));
            }

            return sendResult;
        }

        public virtual async Task<byte[]> SendHttpRequestAsync(HttpMethod method, string url, object data = null, string token = null)
        {
            this.logger.TrackTrace($"Send {method.Method} request to {url}", Severity.Information);
            var result = await HttpRequestAsync(token, method, url, data).ConfigureAwait(false);
            return result;
        }

        public T ConvertBytesToType<T>(byte[] byteArray)
        {
            var result = Encoding.UTF8.GetString(byteArray);
            return JsonConvert.DeserializeObject<T>(result);
        }

        public virtual async Task<string> GetAccessTokenAsync()
        {
            var token = await this.tokenStorage.GetAsync(this.appId).ConfigureAwait(false);
            if (token == null)
            {
                token = new WeChatAccessToken(this.appId, this.appSecret);
            }

            if (token.ExpireTime.ToUnixTimeSeconds() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                var url = this.GetAccessTokenEndPoint(this.appId, this.appSecret);
                var bytes = await this.SendHttpRequestAsync(HttpMethod.Get, url).ConfigureAwait(false);
                var tokenResult = this.ConvertBytesToType<AccessTokenResult>(bytes);
                token.ExpireTime = DateTimeOffset.UtcNow.AddSeconds(tokenResult.ExpireIn);
                token.Token = tokenResult.Token;
                await this.tokenStorage.SaveAsync(this.appId, token);
            }

            return token.Token;
        }

        /// <summary>
        /// Upload temporary media (originally uploaded media files api).
        /// </summary>
        /// <param name="type">Media type.</param>
        /// <param name="attachmentData">attachment data to be uploaded.</param>
        /// <param name="timeOut">Request timeout (milliseconds).</param>
        /// <returns>Result of upload Temporary media.</returns>
        public virtual async Task<UploadTemporaryMediaResult> UploadTemporaryMediaAsync(string type, AttachmentData attachmentData, int timeOut = 10000)
        {
            var mediaHash = this.attachmentHash.Hash(attachmentData.OriginalBase64);
            var uploadResult = (await this.attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false)) as UploadTemporaryMediaResult;
            if (uploadResult == null)
            {
                var accessToken = await this.GetAccessTokenAsync().ConfigureAwait(false);
                var url = this.GetUploadMediaEndPoint(accessToken, type, true);
                uploadResult = await this.UploadMediaAsync<UploadTemporaryMediaResult>(attachmentData, url, type, mediaHash, this.uploadTemporaryMedia).ConfigureAwait(false);
                if (uploadResult.ErrorCode == 0)
                {
                    await this.attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
                }
                else
                {
                    this.logger.TrackException($"Upload Temporary Media Failed, Type: {type.ToString()}", new Exception($"{uploadResult.ToString()}"));
                }
            }

            return uploadResult;
        }

        /// <summary>
        /// Added other types of permanent material.
        /// </summary>
        /// <param name="type">Upload media file type.</param>
        /// <param name="attachmentData">Attachment data to be uploaded.</param>
        /// <param name="timeOut">Request timeout (milliseconds).</param>
        /// <returns>Result of upload persistent media.</returns>
        public virtual async Task<UploadPersistentMediaResult> UploadPersistentMediaAsync(string type, AttachmentData attachmentData, int timeOut = 10000)
        {
            var mediaHash = this.attachmentHash.Hash(attachmentData.OriginalBase64);
            var uploadResult = (await this.attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false)) as UploadPersistentMediaResult;
            if (uploadResult == null)
            {
                var accessToken = await this.GetAccessTokenAsync().ConfigureAwait(false);
                var url = this.GetUploadMediaEndPoint(accessToken, type, false);
                uploadResult = await this.UploadMediaAsync<UploadPersistentMediaResult>(attachmentData, url, type, mediaHash, this.uploadTemporaryMedia).ConfigureAwait(false);
                if (uploadResult.ErrorCode == 0)
                {
                    await this.attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
                }
                else
                {
                    this.logger.TrackException($"Upload Persistent Media Failed, Type: {type.ToString()}", new Exception($"{uploadResult.ToString()}"));
                }
            }

            return uploadResult;
        }

        public virtual async Task<T> UploadMediaAsync<T>(AttachmentData attachmentData, string url, string type, string md5Hash, bool isTemporaryMedia)
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

                contentByte.Headers.TryAddWithoutValidation("Content-Disposition", $"form-data; name=\"media\";filename=\"{md5Hash + ext}\"" + string.Empty);
                contentByte.Headers.Remove("Content-Type");
                contentByte.Headers.TryAddWithoutValidation("Content-Type", attachmentData.Type);
                content.Add(contentByte);

                // Additional form is required when upload a forever video.
                if (isTemporaryMedia == false && type == UploadMediaType.Video)
                {
                    var additionalForm = string.Format("{{\"title\":\"{0}\", \"introduction\":\"{1}\"}}", "title", "introduction");

                    // Important! name must be "description"
                    content.Add(new StringContent(additionalForm), "\"" + "description" + "\"");
                }

                logger.TrackTrace($"Upload {type.ToString()} to WeChat", Severity.Information);
                var response = await HttpClient.PostAsync(url, content).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }

                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            catch (Exception ex)
            {
                this.logger.TrackException($"Failed To Upload Media, Type: {type.ToString()}", ex);
                throw new Exception(ex.Message + ex.InnerException.Message);
            }
        }

        /// <summary>
        /// Upload temporary graphic media.
        /// </summary>
        /// <param name="timeOut">Request timeout (milliseconds).</param>
        /// <param name="newsList">Graphic material list.</param>
        /// <returns>Result of upload a persistent news.</returns>
        public virtual async Task<UploadPersistentMediaResult> UploadPersistentNewsAsync(int timeOut = 10000, params News[] newsList)
        {
            var mediaHash = this.attachmentHash.Hash(JsonConvert.SerializeObject(newsList));
            var uploadResult = (await this.attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false)) as UploadPersistentMediaResult;
            if (uploadResult == null)
            {
                var accessToken = await this.GetAccessTokenAsync().ConfigureAwait(false);
                var url = this.GetUploadNewsEndPoint(accessToken, false);
                var data = new
                {
                    articles = newsList,
                };
                var bytes = await this.SendHttpRequestAsync(HttpMethod.Post, url, data).ConfigureAwait(false);
                uploadResult = this.ConvertBytesToType<UploadPersistentMediaResult>(bytes);
                if (uploadResult.ErrorCode == 0)
                {
                    await this.attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
                }
                else
                {
                    this.logger.TrackException("Upload Persistent News Failed", new Exception($"{uploadResult.ToString()}"));
                }
            }

            return uploadResult;
        }

        /// <summary>
        /// Upload temporary graphic media.
        /// </summary>
        /// <param name="timeOut">Request timeout (milliseconds).</param>
        /// <param name="newsList">Graphic material list.</param>
        /// <returns>Result of upload a temporary news.</returns>
        public virtual async Task<UploadTemporaryMediaResult> UploadTemporaryNewsAsync(int timeOut = 10000, params News[] newsList)
        {
            var mediaHash = this.attachmentHash.Hash(JsonConvert.SerializeObject(newsList));
            var uploadResult = await this.attachmentStorage.GetAsync(mediaHash).ConfigureAwait(false) as UploadTemporaryMediaResult;
            if (uploadResult == null || uploadResult.ExpiredTime <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                var accessToken = await this.GetAccessTokenAsync().ConfigureAwait(false);
                var url = this.GetUploadNewsEndPoint(accessToken, true);
                var data = new
                {
                    articles = newsList,
                };
                var bytes = await this.SendHttpRequestAsync(HttpMethod.Post, url, data).ConfigureAwait(false);
                uploadResult = this.ConvertBytesToType<UploadTemporaryMediaResult>(bytes);
                if (uploadResult.ErrorCode == 0)
                {
                    await this.attachmentStorage.SaveAsync(mediaHash, uploadResult).ConfigureAwait(false);
                }
                else
                {
                    this.logger.TrackException("Upload Temporary News Failed", new Exception($"{uploadResult.ToString()}"));
                }
            }

            return uploadResult;
        }

        /// <summary>
        /// Get customer service agent input status.
        /// </summary>
        /// <param name="openId">WeChat user's open id.</param>
        /// <param name="typingStatus">Agent typing status, can be Tying/CancelTyping.</param>
        /// <param name="timeOut">Request timeout (milliseconds).</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendTypingStatusAsync(string openId, string typingStatus, int timeOut = 10000)
        {
            object data = new
            {
                touser = openId,
                command = typingStatus,
            };
            return await this.SendMessageToUser(data);
        }

        /// <summary>
        /// Send Image message.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="mediaId">Image media id.</param>
        /// <param name="timeOut">Send message operation time out.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendImageAsync(string openId, string mediaId, int timeOut = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = "image",
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
                    msgtype = "image",
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

            return await this.SendMessageToUser(data);
        }

        /// <summary>
        /// Send a graphic message (click to jump to the graphic message page) The number of the messages is limited to 8
        /// note that if the number of graphics more then 8, there will be no response.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="mediaId">Image media id.</param>
        /// <param name="timeOut">Send message operation time out.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendMpNewsAsync(string openId, string mediaId, int timeOut = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = "mpnews",
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
                    msgtype = "mpnews",
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

            return await this.SendMessageToUser(data);
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
        /// <param name="timeOut">Send message operation time out.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendMusicAsync(string openId, string title, string description, string musicUrl, string highQualityMusicUrl, string thumbMediaId, int timeOut = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = "music",
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
                    msgtype = "music",
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

            return await this.SendMessageToUser(data);
        }

        /// <summary>
        /// Send graphic message to user.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="articles">Article list will be sent to user.</param>
        /// <param name="timeOut">Send message operation time out.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendNewsAsync(string openId, List<Article> articles, int timeOut = 10000, string customerServiceAccount = "")
        {
            object data = null;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = "news",
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
                    msgtype = "news",
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

            return await this.SendMessageToUser(data);
        }

        /// <summary>
        /// Send text message to user.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="content">Text message content.</param>
        /// <param name="timeOut">Send message operation time out.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendTextAsync(string openId, string content, int timeOut = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = "text",
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
                    msgtype = "text",
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

            return await this.SendMessageToUser(data);
        }

        /// <summary>
        /// Send a video message.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="mediaId">Media id of the video.</param>
        /// <param name="title">The title of the video.</param>
        /// <param name="description">Video description.</param>
        /// <param name="timeOut">Send message operation time out.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <param name="thumbMeidaId">Thumbnail image media id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendVideoAsync(string openId, string mediaId, string title, string description, int timeOut = 10000, string customerServiceAccount = "", string thumbMeidaId = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = "video",
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
                    msgtype = "video",
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

            return await this.SendMessageToUser(data);
        }

        /// <summary>
        /// Send a voice message.
        /// </summary>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="mediaId">Media id of the voice.</param>
        /// <param name="timeOut">Send message operation time out.</param>
        /// <param name="customerServiceAccount">Customer service account open id.</param>
        /// <returns>Standard result of calling WeChat message API.</returns>
        public async Task<WeChatJsonResult> SendVoiceAsync(string openId, string mediaId, int timeOut = 10000, string customerServiceAccount = "")
        {
            object data;
            if (string.IsNullOrWhiteSpace(customerServiceAccount))
            {
                data = new
                {
                    touser = openId,
                    msgtype = "voice",
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
                    msgtype = "voice",
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

            return await this.SendMessageToUser(data);
        }

        /// <summary>
        /// All http request send to wechat will be handled by this method.
        /// </summary>
        /// <param name="token">Authentication token.</param>
        /// <param name="method">Http method.</param>
        /// <param name="url">Request URL.</param>
        /// <param name="data">Request data.</param>
        /// <returns>Http response content as byte array.</returns>
        private static async Task<byte[]> HttpRequestAsync(string token, HttpMethod method, string url, object data = null)
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

                    try
                    {
                        using (var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false))
                        {
                            try
                            {
                                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                            }
                            catch (Exception e) when (!response.IsSuccessStatusCode)
                            {
                                // swallow if we can't read content of failed request
                                Console.WriteLine($"Call {url} failed", e);
                            }

                            if (!response.IsSuccessStatusCode)
                            {
                                throw new HttpRequestException(response.ToString());
                            }

                            return default(byte[]);
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine(HttpStatusCode.GatewayTimeout + $"{method} - {url} timed out");
                throw new HttpRequestException($"{method} - {url} timed out");
            }
        }

        private string GetMessageApiEndPoint(string accessToken)
        {
            return $"{APIHost}/cgi-bin/message/custom/send?access_token={accessToken}";
        }

        private string GetAccessTokenEndPoint(string appId, string appSecret)
        {
            return $"{APIHost}/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={appSecret}";
        }

        private string GetUploadMediaEndPoint(string accessToken, string type, bool isTemporaryMedia)
        {
            if (isTemporaryMedia)
            {
                return $"{APIHost}/cgi-bin/media/upload?access_token={accessToken}&type={type.ToString()}";
            }

            return $"{APIHost}/cgi-bin/material/add_material?access_token={accessToken}&type={type.ToString()}";
        }

        private string GetUploadNewsEndPoint(string accessToken, bool isTemporaryNews)
        {
            if (isTemporaryNews)
            {
                return $"{APIHost}/cgi-bin/media/uploadnews?access_token={accessToken}";
            }

            return $"{APIHost}/cgi-bin/material/add_news?access_token={accessToken}";
        }
    }
}
