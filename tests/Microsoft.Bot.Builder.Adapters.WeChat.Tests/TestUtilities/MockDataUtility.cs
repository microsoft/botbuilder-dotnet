using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test.TestUtilities
{
    public class MockDataUtility
    {
        public const string XmlText = @"<xml>
                                        <ToUserName><![CDATA[toUser]]></ToUserName>
                                        <FromUserName><![CDATA[fromUser]]></FromUserName> 
                                        <CreateTime>1348831860</CreateTime>
                                        <MsgType><![CDATA[text]]></MsgType>
                                        <Content><![CDATA[this is a test]]></Content>
                                        <MsgId>1234567890123456</MsgId>
                                    </xml>";

        public const string XmlImage = @"<xml>
                                        <ToUserName><![CDATA[toUser]]></ToUserName>
                                        <FromUserName><![CDATA[fromUser]]></FromUserName>
                                        <CreateTime>1348831860</CreateTime>
                                        <MsgType><![CDATA[image]]></MsgType>
                                        <PicUrl><![CDATA[this is a url]]></PicUrl>
                                        <MediaId><![CDATA[media_id]]></MediaId>
                                        <MsgId>1234567890123456</MsgId>
                                    </xml>";

        public const string XmlVoice = @"<xml>
                                       <ToUserName><![CDATA[toUser]]></ToUserName>
                                       <FromUserName><![CDATA[fromUser]]></FromUserName>
                                       <CreateTime>1348831860</CreateTime>
                                       <MsgType><![CDATA[voice]]></MsgType>
                                       <MediaId><![CDATA[media_id]]></MediaId>
                                       <Format><![CDATA[Format]]></Format>
                                       <MsgId>1234567890123456</MsgId>
                                    </xml>";

        public const string XmlVideo = @"<xml>
                                       <ToUserName><![CDATA[toUser]]></ToUserName>
                                       <FromUserName><![CDATA[fromUser]]></FromUserName>
                                       <CreateTime>1348831860</CreateTime>
                                       <MsgType><![CDATA[video]]></MsgType>
                                       <MediaId><![CDATA[media_id]]></MediaId>
                                       <ThumbMediaId><![CDATA[thumb_media_id]]></ThumbMediaId>
                                       <MsgId>1234567890123456</MsgId>
                                    </xml>";

        public const string XmlLocation = @"<xml>
                                       <ToUserName><![CDATA[toUser]]></ToUserName>
                                       <FromUserName><![CDATA[fromUser]]></FromUserName>
                                       <CreateTime>1348831860</CreateTime>
                                       <MsgType><![CDATA[location]]></MsgType>
                                       <Location_X>23.134521</Location_X>
                                       <Location_Y>113.358803</Location_Y>
                                       <Scale>20</Scale>
                                       <Label><![CDATA[LocationInfo]]></Label>
                                       <MsgId>1234567890123456</MsgId>
                                    </xml>";

        public const string XmlShortVideo = @"<xml>
                                             <ToUserName><![CDATA[toUser]]></ToUserName>
                                             <FromUserName><![CDATA[fromUser]]></FromUserName>
                                             <CreateTime>1348831860</CreateTime>
                                             <MsgType><![CDATA[shortvideo]]></MsgType>
                                             <MediaId><![CDATA[media_id]]></MediaId>
                                             <ThumbMediaId><![CDATA[thumb_media_id]]></ThumbMediaId>
                                             <MsgId>1234567890123456</MsgId>
                                          </xml>";

        public const string XmlLink = @"<xml>
                                      <ToUserName><![CDATA[toUser]]></ToUserName>
                                      <FromUserName><![CDATA[fromUser]]></FromUserName>
                                      <CreateTime>1348831860</CreateTime>
                                      <MsgType><![CDATA[link]]></MsgType>
                                      <Title><![CDATA[This is a link]]></Title>
                                      <Description><![CDATA[This is a link]]></Description>
                                      <Url><![CDATA[url]]></Url>
                                      <MsgId>1234567890123456</MsgId>
                                    </xml>";

        public const string XmlEventLocation = @"<xml>
                                               <ToUserName><![CDATA[toUser]]></ToUserName>
                                               <FromUserName><![CDATA[FromUser]]></FromUserName>
                                               <CreateTime>123456789</CreateTime>
                                               <MsgType><![CDATA[event]]></MsgType>
                                               <Event><![CDATA[LOCATION]]></Event>
                                               <Latitude>23.104105</Latitude>
                                               <Longitude>113.320107</Longitude>
                                               <Precision>65.000000</Precision>
                                            </xml>";

        public const string XmlEventClick = @"<xml>
                                            <ToUserName><![CDATA[toUser]]></ToUserName>
                                            <FromUserName><![CDATA[FromUser]]></FromUserName>
                                            <CreateTime>123456789</CreateTime>
                                            <MsgType><![CDATA[event]]></MsgType>
                                            <Event><![CDATA[CLICK]]></Event>
                                            <EventKey><![CDATA[EVENTKEY]]></EventKey>
                                        </xml>";

        public const string XmlEventView = @"<xml>
                                            <ToUserName><![CDATA[toUser]]></ToUserName>
                                            <FromUserName><![CDATA[FromUser]]></FromUserName>
                                            <CreateTime>123456789</CreateTime>
                                            <MsgType><![CDATA[event]]></MsgType>
                                            <Event><![CDATA[VIEW]]></Event>
                                            <EventKey><![CDATA[www.qq.com]]></EventKey>
                                         </xml>";

        public const string XmlEventSubscribe = @"<xml>
                                              <ToUserName><![CDATA[toUser]]></ToUserName>
                                              <FromUserName><![CDATA[FromUser]]></FromUserName>
                                              <CreateTime>123456789</CreateTime>
                                              <MsgType><![CDATA[event]]></MsgType>
                                              <Event><![CDATA[subscribe]]></Event>
                                              <EventKey><![CDATA[qrscene_123123]]></EventKey>
                                              <Ticket><![CDATA[TICKET]]></Ticket>
                                            </xml>";

        public const string XmlEventScan = @"<xml>
                                          <ToUserName><![CDATA[toUser]]></ToUserName>
                                          <FromUserName><![CDATA[FromUser]]></FromUserName>
                                          <CreateTime>123456789</CreateTime>
                                          <MsgType><![CDATA[event]]></MsgType>
                                          <Event><![CDATA[SCAN]]></Event>
                                          <EventKey><![CDATA[SCENE_VALUE]]></EventKey>
                                          <Ticket><![CDATA[TICKET]]></Ticket>
                                        </xml>";

        public const string XmlEventScanPush = @"<xml>
                                                    <ToUserName><![CDATA[toUser]]></ToUserName>
                                                    <FromUserName><![CDATA[FromUser]]></FromUserName>
                                                    <CreateTime>123456789</CreateTime>
                                                    <MsgType><![CDATA[event]]></MsgType>
                                                    <Event><![CDATA[scancode_push]]></Event>
                                                    <EventKey><![CDATA[6]]></EventKey>
                                                    <ScanCodeInfo><ScanType><![CDATA[qrcode]]></ScanType>
                                                    <ScanResult><![CDATA[1]]></ScanResult>
                                                    </ScanCodeInfo>
                                                    </xml>";

        public const string XmlEventWaitScanPush = @"<xml>
                                                        <ToUserName><![CDATA[toUser]]></ToUserName>
                                                        <FromUserName><![CDATA[FromUser]]></FromUserName>
                                                        <CreateTime>123456789</CreateTime>
                                                        <MsgType><![CDATA[event]]></MsgType>
                                                        <Event><![CDATA[scancode_waitmsg]]></Event>
                                                        <EventKey><![CDATA[6]]></EventKey>
                                                        <ScanCodeInfo><ScanType><![CDATA[qrcode]]></ScanType>
                                                        <ScanResult><![CDATA[2]]></ScanResult>
                                                        </ScanCodeInfo>
                                                        </xml>";

        public const string XmlEventCamera = @"<xml>
                                                    <ToUserName><![CDATA[toUser]]></ToUserName>
                                                    <FromUserName><![CDATA[FromUser]]></FromUserName>
                                                    <CreateTime>123456789</CreateTime>
                                                    <MsgType><![CDATA[event]]></MsgType>
                                                    <Event><![CDATA[pic_sysphoto]]></Event>
                                                    <EventKey><![CDATA[6]]></EventKey>
                                                    <SendPicsInfo><Count>1</Count>
                                                    <PicList><item><PicMd5Sum><![CDATA[1b5f7c23b5bf75682a53e7b6d163e185]]></PicMd5Sum>
                                                    </item>
                                                    </PicList>
                                                    </SendPicsInfo>
                                                    </xml>";

        public const string XmlEventCameraOrAlbum = @"<xml>
                                                        <ToUserName><![CDATA[toUser]]></ToUserName>
                                                        <FromUserName><![CDATA[FromUser]]></FromUserName>
                                                        <CreateTime>123456789</CreateTime>
                                                        <MsgType><![CDATA[event]]></MsgType>
                                                        <Event><![CDATA[pic_photo_or_album]]></Event>
                                                        <EventKey><![CDATA[6]]></EventKey>
                                                        <SendPicsInfo><Count>1</Count>
                                                        <PicList><item><PicMd5Sum><![CDATA[5a75aaca956d97be686719218f275c6b]]></PicMd5Sum>
                                                        </item>
                                                        </PicList>
                                                        </SendPicsInfo>
                                                        </xml>";

        public const string XmlEventWeChatAlbum = @"<xml>
                                                        <ToUserName><![CDATA[toUser]]></ToUserName>
                                                        <FromUserName><![CDATA[FromUser]]></FromUserName>
                                                        <CreateTime>123456789</CreateTime>
                                                        <MsgType><![CDATA[event]]></MsgType>
                                                        <Event><![CDATA[pic_weixin]]></Event>
                                                        <EventKey><![CDATA[6]]></EventKey>
                                                        <SendPicsInfo><Count>1</Count>
                                                        <PicList><item><PicMd5Sum><![CDATA[5a75aaca956d97be686719218f275c6b]]></PicMd5Sum>
                                                        </item>
                                                        </PicList>
                                                        </SendPicsInfo>
                                                        </xml>";

        public const string XmlEventSelectLocation = @"<xml>
                                                            <ToUserName><![CDATA[toUser]]></ToUserName>
                                                            <FromUserName><![CDATA[FromUser]]></FromUserName>
                                                            <CreateTime>123456789</CreateTime>
                                                            <MsgType><![CDATA[event]]></MsgType>
                                                            <Event><![CDATA[location_select]]></Event>
                                                            <EventKey><![CDATA[6]]></EventKey>
                                                            <SendLocationInfo><Location_X><![CDATA[23]]></Location_X>
                                                            <Location_Y><![CDATA[113]]></Location_Y>
                                                            <Scale><![CDATA[15]]></Scale>
                                                            <Label><![CDATA[No.328 Xinghu Street]]></Label>
                                                            <Poiname><![CDATA[test]]></Poiname>
                                                            </SendLocationInfo>
                                                            </xml>";

        public const string XmlEventViewMiniProgram = @"<xml>
                                                            <ToUserName><![CDATA[toUser]]></ToUserName>
                                                            <FromUserName><![CDATA[FromUser]]></FromUserName>
                                                            <CreateTime>123456789</CreateTime>
                                                            <MsgType><![CDATA[event]]></MsgType>
                                                            <Event><![CDATA[view_miniprogram]]></Event>
                                                            <EventKey><![CDATA[pages/index/index]]></EventKey>
                                                            <MenuId>MENUID</MenuId>
                                                            </xml>";

        public const string XmlEventMassSendJobFinished = @"<xml> 
                                                              <ToUserName><![CDATA[toUser]]></ToUserName>  
                                                              <FromUserName><![CDATA[FromUser]]></FromUserName>  
                                                              <CreateTime>123456789</CreateTime>  
                                                              <MsgType><![CDATA[event]]></MsgType>  
                                                              <Event><![CDATA[MASSSENDJOBFINISH]]></Event>  
                                                              <MsgID>1000001625</MsgID>  
                                                              <Status><![CDATA[err(30003)]]></Status>  
                                                              <TotalCount>0</TotalCount>  
                                                              <FilterCount>0</FilterCount>  
                                                              <SentCount>0</SentCount>  
                                                              <ErrorCount>0</ErrorCount>  
                                                              <CopyrightCheckResult> 
                                                                <Count>2</Count>  
                                                                <ResultList> 
                                                                  <item> 
                                                                    <ArticleIdx>1</ArticleIdx>  
                                                                    <UserDeclareState>0</UserDeclareState>  
                                                                    <AuditState>2</AuditState>  
                                                                    <OriginalArticleUrl><![CDATA[Url_1]]></OriginalArticleUrl>  
                                                                    <OriginalArticleType>1</OriginalArticleType>  
                                                                    <CanReprint>1</CanReprint>  
                                                                    <NeedReplaceContent>1</NeedReplaceContent>  
                                                                    <NeedShowReprintSource>1</NeedShowReprintSource> 
                                                                  </item>  
                                                                  <item> 
                                                                    <ArticleIdx>2</ArticleIdx>  
                                                                    <UserDeclareState>0</UserDeclareState>  
                                                                    <AuditState>2</AuditState>  
                                                                    <OriginalArticleUrl><![CDATA[Url_2]]></OriginalArticleUrl>  
                                                                    <OriginalArticleType>1</OriginalArticleType>  
                                                                    <CanReprint>1</CanReprint>  
                                                                    <NeedReplaceContent>1</NeedReplaceContent>  
                                                                    <NeedShowReprintSource>1</NeedShowReprintSource> 
                                                                  </item> 
                                                                </ResultList>  
                                                                <CheckState>2</CheckState> 
                                                              </CopyrightCheckResult> 
                                                            </xml>";

        public const string XmlEventTemplateSendFinished = @"<xml> 
                                                                  <ToUserName><![CDATA[toUser]]></ToUserName>  
                                                                  <FromUserName><![CDATA[FromUser]]></FromUserName>  
                                                                  <CreateTime>123456789</CreateTime>  
                                                                  <MsgType><![CDATA[event]]></MsgType>  
                                                                  <Event><![CDATA[TEMPLATESENDJOBFINISH]]></Event>  
                                                                  <MsgID>200163840</MsgID>  
                                                                  <Status><![CDATA[failed:user block]]></Status> 
                                                                </xml>";

        public const string XmlEncrypt = @"<xml>
                                            <ToUserName><![CDATA[gh_d13df7f4ef38]]></ToUserName>
                                            <Encrypt><![CDATA[8VfmSJqZFzMlnaDohVD7I0T+9LIG1fT8kl221jOyL9TwkTJ38AZ9A6kMxvADvvxfg+azCEOEXtdVElhLs/roYyf25YfGH4kZp0O2t6XngOzwClG9HAhUV29OomouAqVpZ1ySqV60THKQ8E25N+fYF8RnXboae0r/ZTGnUJPuPwPVtbBj1dIGuFjpls+mnaSyg6Ag04FF5GcqO7exfEugQtNS44yQbmel/EKmxtvzz9CClJ3QnsHUODCMj5e6lYNSM7b84s+OBtKKsD0ObRnrAN5IfFLbDqK6twKlwTqHM0O1icSmfFo2MHT2+iizTcJfpbFnQeIj1zlSQdexvQ8fH9JwoSaHjQad/CyQ4D/PSxYi2Thu2ZFt5C2/NJ0ixL++GlOZpdaL/SQvxsVPrqsNhp7tteT69EVbpZux7c+eib4=]]></Encrypt>
                                           </xml>";

        public const string XmlDecrypt = @"<xml><ToUserName><![CDATA[gh_d13df7f4ef38]]></ToUserName>
<FromUserName><![CDATA[of3ss6NTm25BKyE9KfDPD-ALSeWg]]></FromUserName>
<CreateTime>1562066088</CreateTime>
<MsgType><![CDATA[text]]></MsgType>
<Content><![CDATA[hi]]></Content>
<MsgId>22363405356629000</MsgId>
</xml>";

        public static readonly SecretInfo SecretInfo = new SecretInfo()
        {
            Token = "bmwipabotwx",
            EncodingAESKey = "P7PIjIGpA7axbjbffRoWYq7G0BsIaEpqdawIir4KqCt",
            AppId = "wx77f941c869071d99",
            Signature = "4e17212123b3ce5a6b11643dc658af83fdb54c7d",
            Timestamp = "1562066088",
            Nonce = "236161902",
            Msg_Signature = "f3187a0efd9709c8f6550190147f43c279e9bc43",
        };

        public static readonly SecretInfo SecretInfoAESKeyError = new SecretInfo()
        {
            Token = "bmwipabotwx",
            EncodingAESKey = "bmwipabotwx",
            AppId = "wx77f941c869071d99",
            Signature = "4e17212123b3ce5a6b11643dc658af83fdb54c7",
            Timestamp = "1562066088",
            Nonce = "236161902",
            Msg_Signature = "4e17212123b3ce5a6b11643dc658af83fdb54c7",
        };

        public static readonly SecretInfo SecretInfoMsgSignatureError = new SecretInfo()
        {
            Token = "bmwipabotwx",
            EncodingAESKey = "P7PIjIGpA7axbjbffRoWYq7G0BsIaEpqdawIir4KqCt",
            AppId = "wx77f941c869071d99",
            Signature = "4e17212123b3ce5a6b11643dc658af83fdb54c7d",
            Timestamp = "1562066088",
            Nonce = "236161902",
            Msg_Signature = "4e17212123b3ce5a6b11643dc658af83fdb54c7d",
        };

        public static readonly WeChatJsonResult WeChatJsonResult = new WeChatJsonResult()
        {
            ErrorCode = 0,
            ErrorMessage = "ok",
        };

        public static readonly MessageCryptography TestDecryptMsg = new MessageCryptography(SecretInfo);
        public static readonly MessageCryptography TestAESKey = new MessageCryptography(SecretInfoAESKeyError);
        public static readonly MessageCryptography TestSignature = new MessageCryptography(SecretInfoMsgSignatureError);

        // public const string xmlEvent_Enter_Agent = @"<xml>
        //                                            <ToUserName><![CDATA[toUser]]></ToUserName>
        //                                            <FromUserName><![CDATA[FromUser]]></FromUserName>
        //                                            <CreateTime>1408091189</CreateTime>
        //                                            <MsgType><![CDATA[event]]></MsgType>
        //                                            <Event><![CDATA[enter_agent]]></Event>
        //                                            <EventKey><![CDATA[]]></EventKey>
        //                                            <AgentID>1</AgentID>
        //                                       </xml>";

        // public const string xml_Suite_Ticket = @"<xml>
        //                              <SuiteId><![CDATA[wxfc918a2d200c9a4c]]></SuiteId>
        //                              <InfoType> <![CDATA[suite_ticket]]></InfoType>
        //                              <TimeStamp>1403610513</TimeStamp>
        //                              <SuiteTicket><![CDATA[asdfasfdasdfasdf]]></SuiteTicket>
        //                             </xml>";

        // public const string xml_Change_Auth = @"<xml>
        //                              <SuiteId><![CDATA[wxfc918a2d200c9a4c]]></SuiteId>
        //                              <InfoType><![CDATA[change_auth]]></InfoType>
        //                              <TimeStamp>1403610513</TimeStamp>
        //                              <AuthCorpId><![CDATA[wxf8b4f85f3a794e77]]></AuthCorpId>
        //                             </xml>   ";

        // public const string xml_Cancel_Auth = @"<xml>
        //                              <SuiteId><![CDATA[wxfc918a2d200c9a4c]]></SuiteId>
        //                              <InfoType><![CDATA[cancel_auth]]></InfoType>
        //                              <TimeStamp>1403610513</TimeStamp>
        //                              <AuthCorpId><![CDATA[wxf8b4f85f3a794e77]]></AuthCorpId>
        //                             </xml>   ";

        // public const string xml_Batch_Job_Result = @"<xml>
        //                            <ToUserName><![CDATA[wx28dbb14e37208abe]]></ToUserName>
        //                            <FromUserName><![CDATA[FromUser]]></FromUserName>
        //                            <CreateTime>1425284517</CreateTime>
        //                            <MsgType><![CDATA[event]]></MsgType>
        //                            <Event><![CDATA[batch_job_result]]></Event>
        //                            <BatchJob><JobId><![CDATA[S0MrnndvRG5fadSlLwiBqiDDbM143UqTmKP3152FZk4]]></JobId>
        //                            <JobType><![CDATA[sync_user]]></JobType>
        //                            <ErrCode>0</ErrCode>
        //                            <ErrMsg><![CDATA[ok]]></ErrMsg>
        //                            </BatchJob>
        //                            </xml>";
        public static SecretInfo GetMockSecretInfo()
        {
            return SecretInfo;
        }

        public static List<IRequestMessageBase> GetMockRequestMessageList()
        {
            var requestList = new List<IRequestMessageBase>
            {
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlText)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlImage)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlVoice)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlVideo)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlShortVideo)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlLocation)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlLink)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventClick)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventLocation)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventView)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventSubscribe)),
                RequestMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventScan)),
            };
            return requestList;
        }

        public static IMessageActivity GetMockMessageActivity()
        {
            var mockActivity = Activity.CreateMessageActivity();
            mockActivity.Id = "id";
            mockActivity.ChannelId = Constants.ChannelId;
            mockActivity.From = new ChannelAccount("FromId", "Bot", "bot");
            mockActivity.Recipient = new ChannelAccount("RecipientId", "User", "user");
            mockActivity.Conversation = new ConversationAccount(false, id: "FromId");
            mockActivity.Timestamp = DateTimeOffset.UtcNow;
            mockActivity.Text = "text";

            // TODO: enable channelData test
            // mockActivity.ChannelData = request;
            return mockActivity;
        }

        public static IEventActivity GetMockEventActivity()
        {
            var mockActivity = Activity.CreateEventActivity();
            mockActivity.ChannelId = Constants.ChannelId;
            mockActivity.From = new ChannelAccount("FromId", "Bot", "bot");
            mockActivity.Recipient = new ChannelAccount("RecipientId", "User", "user");

            mockActivity.Conversation = new ConversationAccount(false, id: "FromId");
            mockActivity.Timestamp = DateTimeOffset.UtcNow;
            return mockActivity;
        }

        public static List<IEventActivity> GetMockEventActivityList()
        {
            var activityList = new List<IEventActivity>();
            var mockActivity = GetMockEventActivity();

            activityList.Add(mockActivity);
            return activityList;
        }

        public static List<IMessageActivity> GetMockMessageActivityList()
        {
            var activityList = new List<IMessageActivity>();
            var mockActivity = GetMockMessageActivity();

            mockActivity.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
            mockActivity.Attachments.Add(Cards.GetAudioCard().ToAttachment());
            mockActivity.Attachments.Add(Cards.GetHeroCard().ToAttachment());
            mockActivity.Attachments.Add(Cards.GetReceiptCard().ToAttachment());
            mockActivity.Attachments.Add(Cards.GetSigninCard().ToAttachment());
            mockActivity.Attachments.Add(Cards.GetThumbnailCard().ToAttachment());
            mockActivity.Attachments.Add(Cards.GetVideoCard().ToAttachment());
            mockActivity.Attachments.Add(Cards.CreateAdaptiveCardAttachment());

            activityList.Add(mockActivity);
            return activityList;
        }

        public static async Task<List<Attachment>> GetGeneralAttachmentList()
        {
            var attachmentList = new List<Attachment>();

            // text attachment
            var textAttachment = new Attachment { Content = "some content", ContentType = "text/plain" };
            attachmentList.Add(textAttachment);

            // media attachment
            var invalidUrl = "https://invalidUrl/foo.jpg";
            var url = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSA7hUzrXRZWxQcvgh78OYHHiG2us7dk3F4bKXzNRYTs9EfbQmJ"; // Url("/media/einstein1.jpg");
            byte[] rawBytes;

            using (var httpClient = new HttpClient())
            {
                rawBytes = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
            }

            var dataUri = "data:image/jpeg;base64," + Convert.ToBase64String(rawBytes);

            // default values for each type
            // try multiple possibilites for each param specified in message text
            var names = new string[]
            {
                    "Einstein.jpg",
                    null,
                    string.Empty,
                    "fi€€€le.jpg", // Unicode chars
            };

            var contentTypes = new string[]
            {
                    "image/jpeg",
                    null,
                    string.Empty,
                    "invalid",
            };

            var contentUrls = new string[]
            {
                    url,
                    dataUri,
                    null,
                    string.Empty,
                    invalidUrl,
            };

            var thumbnailUrls = new string[]
            {
                    url,
                    dataUri,
                    null,
                    string.Empty,
                    invalidUrl,
            };

            var content = new object[]
            {
                    url,
                    dataUri,
                    rawBytes,
                    null,
                    string.Empty,
                    "data:,",  // valid uri but no data
                    "12345",
                    invalidUrl,
                    new byte[10],
                    Array.Empty<byte>(),
            };

            var num = 0;
            foreach (var n in names)
            {
                foreach (var ct in contentTypes)
                {
                    foreach (var c in content ?? new object[] { null })
                    {
                        foreach (var cu in contentUrls ?? new string[] { null })
                        {
                            foreach (var tu in thumbnailUrls ?? new string[] { null })
                            {
                                // specifying all commands would generate 4000 messages
                                // which would go on for a long time
                                if (num++ < 50)
                                {
                                    var attachment = new Attachment
                                    {
                                        Name = n,
                                        ContentType = ct,
                                        Content = c,
                                        ContentUrl = cu,
                                        ThumbnailUrl = tu,
                                    };

                                    attachmentList.Add(attachment);
                                }
                            }
                        }
                    }
                }
            }

            return attachmentList;
        }

        public static IConfiguration MockConfiguration(bool isTemp = true)
        {
            var mockConfSection = new Mock<IConfigurationSection>();
            var tokenSection = new Mock<IConfigurationSection>();
            var appIdSection = new Mock<IConfigurationSection>();
            var encondingSection = new Mock<IConfigurationSection>();
            var secretSection = new Mock<IConfigurationSection>();
            var isTempSection = new Mock<IConfigurationSection>();

            mockConfSection.Setup(a => a.GetSection(It.Is<string>(s => s == "Token"))).Returns(tokenSection.Object);
            mockConfSection.Setup(a => a.GetSection(It.Is<string>(s => s == "AppId"))).Returns(appIdSection.Object);
            mockConfSection.Setup(a => a.GetSection(It.Is<string>(s => s == "EncodingAESKey"))).Returns(encondingSection.Object);
            mockConfSection.Setup(a => a.GetSection(It.Is<string>(s => s == "AppSecret"))).Returns(secretSection.Object);
            mockConfSection.Setup(a => a.GetSection(It.Is<string>(s => s == "UploadTemporaryMedia"))).Returns(isTempSection.Object);

            mockConfSection.Setup(a => a.Value).Returns("default");
            tokenSection.Setup(a => a.Value).Returns("bmwipabotwx");
            appIdSection.Setup(a => a.Value).Returns("wx77f941c869071d99");
            encondingSection.Setup(a => a.Value).Returns("P7PIjIGpA7axbjbffRoWYq7G0BsIaEpqdawIir4KqCt");
            secretSection.Setup(a => a.Value).Returns("secret");
            isTempSection.Setup(a => a.Value).Returns(isTemp.ToString());

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(a => a.GetSection(It.Is<string>(s => s == "WeChatSetting"))).Returns(mockConfSection.Object);

            // var section1 = new Mock<IConfigurationSection>();
            // var section2 = new Mock<IConfigurationSection>();
            // section2.SetupGet(x => x[It.Is<string>(s => s == "WeChatSetting:Token")]).Returns("bmwipabotwx");
            // section2.SetupGet(x => x[It.Is<string>(s => s == "WeChatSetting:AppId")]).Returns("bmwipabotwx");
            // section2.SetupGet(x => x[It.Is<string>(s => s == "WeChatSetting:EncodingAESKey")]).Returns("bmwipabotwx");
            // section2.SetupGet(x => x[It.Is<string>(s => s == "Token")]).Returns("bmwipabotwx");
            // var value = section2.Object;
            // value.Value = "";
            // section1.Setup(c => c.GetSection(It.IsAny<string>())).Returns(value);
            // configuration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(section1.Object);
            return mockConfiguration.Object;
        }

        public static WeChatClient MockWeChatClient()
        {
            var wechatClient = new Mock<WeChatClient>(MockConfiguration(), null, null, null, null);
            var result = JsonConvert.SerializeObject(WeChatJsonResult);
            var byteResult = Encoding.UTF8.GetBytes(result);
            wechatClient.Setup(c => c.GetAccessTokenAsync()).Returns(Task.FromResult("mockToken"));
            wechatClient.Setup(c => c.SendHttpRequestAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(Task.FromResult(byteResult));

            wechatClient.Setup(c => c.UploadTemporaryMediaAsync(It.IsAny<string>(), It.IsAny<AttachmentData>(), It.IsAny<int>())).Returns(MockTempMediaResult(UploadMediaType.Image));
            wechatClient.Setup(c => c.UploadTemporaryNewsAsync(It.IsAny<int>(), It.IsAny<News[]>())).Returns(MockTempMediaResult(UploadMediaType.News));
            wechatClient.Setup(c => c.UploadPersistentMediaAsync(It.IsAny<string>(), It.IsAny<AttachmentData>(), It.IsAny<int>())).Returns(MockForeverMediaResult());
            wechatClient.Setup(c => c.UploadPersistentNewsAsync(It.IsAny<int>(), It.IsAny<News[]>())).Returns(MockForeverMediaResult());

            return wechatClient.Object;
        }

        public static Task<UploadTemporaryMediaResult> MockTempMediaResult(string type)
        {
            var uploadResult = new UploadTemporaryMediaResult()
            {
                MediaId = "mediaId",
                ErrorCode = 0,
                ErrorMessage = "ok",
                ThumbMediaId = "thumbMediaId",
                Type = type,
            };
            return Task.FromResult(uploadResult);
        }

        public static Task<UploadPersistentMediaResult> MockForeverMediaResult()
        {
            var uploadResult = new UploadPersistentMediaResult()
            {
                MediaId = "mediaId",
                Url = "mediaUrl",
                ErrorCode = 0,
                ErrorMessage = "ok",
            };
            return Task.FromResult(uploadResult);
        }
    }
}
