// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
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

        public const string PassiveXmlText = "<xml>\r\n  <ToUserName><![CDATA[toUser]]></ToUserName>\r\n  <FromUserName><![CDATA[fromUser]]></FromUserName>\r\n  <CreateTime>12345678</CreateTime>\r\n  <MsgType><![CDATA[text]]></MsgType>\r\n  <Content><![CDATA[Hi]]></Content>\r\n</xml>";

        public const string PassiveXmlImage = "<xml>\r\n  <ToUserName><![CDATA[toUser]]></ToUserName>\r\n  <FromUserName><![CDATA[fromUser]]></FromUserName>\r\n  <CreateTime>12345678</CreateTime>\r\n  <MsgType><![CDATA[image]]></MsgType>\r\n  <Image>\r\n    <MediaId><![CDATA[media_id]]></MediaId>\r\n  </Image>\r\n</xml>";

        public const string PassiveXmlVoice = "<xml>\r\n  <ToUserName><![CDATA[toUser]]></ToUserName>\r\n  <FromUserName><![CDATA[fromUser]]></FromUserName>\r\n  <CreateTime>12345678</CreateTime>\r\n  <MsgType><![CDATA[voice]]></MsgType>\r\n  <Voice>\r\n    <MediaId><![CDATA[media_id]]></MediaId>\r\n  </Voice>\r\n</xml>";

        public const string PassiveXmlVideo = "<xml>\r\n  <ToUserName><![CDATA[toUser]]></ToUserName>\r\n  <FromUserName><![CDATA[fromUser]]></FromUserName>\r\n  <CreateTime>12345678</CreateTime>\r\n  <MsgType><![CDATA[video]]></MsgType>\r\n  <Video>\r\n    <MediaId><![CDATA[media_id]]></MediaId>\r\n    <Title><![CDATA[title]]></Title>\r\n    <Description><![CDATA[description]]></Description>\r\n  </Video>\r\n</xml>";

        public const string PassiveXmlMusic = "<xml>\r\n  <ToUserName><![CDATA[toUser]]></ToUserName>\r\n  <FromUserName><![CDATA[fromUser]]></FromUserName>\r\n  <CreateTime>12345678</CreateTime>\r\n  <MsgType><![CDATA[music]]></MsgType>\r\n  <Music>\r\n    <Title><![CDATA[TITLE]]></Title>\r\n    <Description><![CDATA[DESCRIPTION]]></Description>\r\n    <MusicUrl><![CDATA[MUSIC_Url]]></MusicUrl>\r\n    <HQMusicUrl><![CDATA[HQ_MUSIC_Url]]></HQMusicUrl>\r\n    <ThumbMediaId><![CDATA[media_id]]></ThumbMediaId>\r\n  </Music>\r\n</xml>";

        public const string PassiveXmlNews = "<xml>\r\n  <ToUserName><![CDATA[toUser]]></ToUserName>\r\n  <FromUserName><![CDATA[fromUser]]></FromUserName>\r\n  <CreateTime>12345678</CreateTime>\r\n  <MsgType><![CDATA[news]]></MsgType>\r\n  <ArticleCount>2</ArticleCount>\r\n  <item>\r\n    <Title><![CDATA[title1]]></Title>\r\n    <Description><![CDATA[description1]]></Description>\r\n    <Url><![CDATA[url1]]></Url>\r\n    <PicUrl><![CDATA[picurl1]]></PicUrl>\r\n  </item>\r\n  <item>\r\n    <Title><![CDATA[title2]]></Title>\r\n    <Description><![CDATA[description2]]></Description>\r\n    <Url><![CDATA[url2]]></Url>\r\n    <PicUrl><![CDATA[picurl2]]></PicUrl>\r\n  </item>\r\n</xml>";

        public const string ImageDataUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKwAAACeCAYAAACvg+F+AAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAAJcEhZcwAAFiUAABYlAUlSJPAAAAAhdEVYdENyZWF0aW9uIFRpbWUAMjAxOTowMzoxMyAxOTo0Mjo0OBCBEeIAAAG8SURBVHhe7dJBDQAgEMCwA/+egQcmlrSfGdg6z0DE/oUEw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phSTEsKYYlxbCkGJYUw5JiWFIMS4phCZm52U4FOCAVGHQAAAAASUVORK5CYII=";

        public static readonly SecretInfo SecretInfo = new SecretInfo()
        {
            WebhookSignature = "4e17212123b3ce5a6b11643dc658af83fdb54c7d",
            Timestamp = "1562066088",
            Nonce = "236161902",
            MessageSignature = "f3187a0efd9709c8f6550190147f43c279e9bc43",
        };

        public static readonly WeChatSettings WeChatSettings = new WeChatSettings()
        {
            Token = "bmwipabotwx",
            EncodingAesKey = "P7PIjIGpA7axbjbffRoWYq7G0BsIaEpqdawIir4KqCt",
            AppId = "wx77f941c869071d99",
        };

        public static readonly SecretInfo SecretInfoAesKeyError = new SecretInfo()
        {
            WebhookSignature = "4e17212123b3ce5a6b11643dc658af83fdb54c7",
            Timestamp = "1562066088",
            Nonce = "236161902",
            MessageSignature = "4e17212123b3ce5a6b11643dc658af83fdb54c7",
        };

        public static readonly WeChatSettings WeChatSettingsAesKeyError = new WeChatSettings()
        {
            Token = "bmwipabotwx",
            EncodingAesKey = "bmwipabotwx",
            AppId = "wx77f941c869071d99",
        };

        public static readonly SecretInfo SecretInfoMsgSignatureError = new SecretInfo()
        {
            WebhookSignature = "4e17212123b3ce5a6b11643dc658af83fdb54c7d",
            Timestamp = "1562066088",
            Nonce = "236161902",
            MessageSignature = "4e17212123b3ce5a6b11643dc658af83fdb54c7d",
        };

        public static readonly WeChatJsonResult WeChatJsonResult = new WeChatJsonResult()
        {
            ErrorCode = 0,
            ErrorMessage = "ok",
        };

        public static readonly MessageCryptography TestDecryptMsg = new MessageCryptography(SecretInfo, WeChatSettings);
        public static readonly MessageCryptography TestSignature = new MessageCryptography(SecretInfoMsgSignatureError, WeChatSettings);

        public static readonly TextResponse TextResponse = new TextResponse()
        {
            ToUserName = "toUser",
            FromUserName = "fromUser",
            CreateTime = 12345678,
            Content = "Hi",
        };

        public static readonly Image Image = new Image()
        {
            MediaId = "media_id",
        };

        public static readonly ImageResponse ImageResponse = new ImageResponse()
        {
            ToUserName = "toUser",
            FromUserName = "fromUser",
            CreateTime = 12345678,
            Image = Image,
        };

        public static readonly Voice Voice = new Voice()
        {
            MediaId = "media_id",
        };

        public static readonly VoiceResponse VoiceResponse = new VoiceResponse()
        {
            ToUserName = "toUser",
            FromUserName = "fromUser",
            CreateTime = 12345678,
            Voice = Voice,
        };

        public static readonly Video Video = new Video()
        {
            MediaId = "media_id",
            Title = "title",
            Description = "description",
        };

        public static readonly VideoResponse VideoResponse = new VideoResponse()
        {
            Video = Video,
            ToUserName = "toUser",
            FromUserName = "fromUser",
            CreateTime = 12345678,
        };

        public static readonly Music Music = new Music()
        {
            Title = "TITLE",
            Description = "DESCRIPTION",
            MusicUrl = "MUSIC_Url",
            HQMusicUrl = "HQ_MUSIC_Url",
            ThumbMediaId = "media_id",
        };

        public static readonly MusicResponse MusicResponse = new MusicResponse()
        {
            ToUserName = "toUser",
            FromUserName = "fromUser",
            CreateTime = 12345678,
            Music = Music,
        };

        public static readonly Article Article1 = new Article()
        {
            Title = "title1",
            Description = "description1",
            PicUrl = "picurl1",
            Url = "url1",
        };

        public static readonly Article Article2 = new Article()
        {
            Title = "title2",
            Description = "description2",
            PicUrl = "picurl2",
            Url = "url2",
        };

        public static readonly NewsResponse NewsResponse = new NewsResponse()
        {
            ToUserName = "toUser",
            FromUserName = "fromUser",
            CreateTime = 12345678,
            Articles = new List<Article>() { Article1, Article2 },
        };

        public static SecretInfo GetMockSecretInfo() => SecretInfo;

        public static ILogger GetMockLogger() => NullLogger.Instance;

        public static AttachmentData GetMockAttachmentData() => new AttachmentData()
        {
            Name = "tempImage",
            Type = "image/png",
            OriginalBase64 = Encoding.UTF8.GetBytes(ImageDataUrl),
            ThumbnailBase64 = Encoding.UTF8.GetBytes(ImageDataUrl),
        };

        public static List<IRequestMessageBase> GetMockRequestMessageList()
        {
            var logger = GetMockLogger();
            var requestList = new List<IRequestMessageBase>
            {
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlText), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlImage), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlVoice), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlVideo), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlShortVideo), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlLocation), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlLink), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventClick), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventLocation), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventView), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventSubscribe), logger),
                WeChatMessageFactory.GetRequestEntity(XDocument.Parse(XmlEventScan), logger),
            };
            return requestList;
        }

        public static IMessageActivity GetMockMessageActivity()
        {
            var mockActivity = Activity.CreateMessageActivity();
            mockActivity.Id = "id";
            mockActivity.ChannelId = "wechat";
            mockActivity.From = new ChannelAccount("FromId", "Bot", "bot");
            mockActivity.Recipient = new ChannelAccount("RecipientId", "User", "user");
            mockActivity.Conversation = new ConversationAccount(false, id: "FromId");
            mockActivity.Timestamp = DateTimeOffset.UtcNow;
            mockActivity.Text = "text";

            mockActivity.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = ActionTypes.MessageBack, Type = ActionTypes.MessageBack, Value = "messageBack" },
                        new CardAction() { Title = ActionTypes.ImBack, Type = ActionTypes.ImBack, Value = "imBack" },
                        new CardAction() { Title = ActionTypes.OpenUrl, Type = ActionTypes.OpenUrl, Value = "http://test.com" },
                    },
            };
            return mockActivity;
        }

        public static IEventActivity GetMockEventActivity()
        {
            var mockActivity = Activity.CreateEventActivity();
            mockActivity.ChannelId = "wechat";
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
            mockActivity.Attachments.Add(Cards.GetOAuthCardAttachment());
            mockActivity.Attachments.Add(Cards.GetVideoCard().ToAttachment());
            mockActivity.Attachments.Add(Cards.CreateAdaptiveCardAttachment());

            activityList.Add(mockActivity);

            var mockActivity_Markdown = GetMockMessageActivity();
            mockActivity_Markdown.TextFormat = TextFormatTypes.Markdown;
            mockActivity_Markdown.Text = "*text*";
            activityList.Add(mockActivity_Markdown);

            return activityList;
        }

        public static async Task<List<Attachment>> GetGeneralAttachmentList(bool isValid)
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
            };

            var contentUrls = new string[]
            {
                    url,
                    dataUri,
            };

            var thumbnailUrls = new string[]
            {
                    url,
                    dataUri,
                    null,
                    string.Empty,
            };

            var content = new object[]
            {
                    url,
                    dataUri,
                    rawBytes,
                    null,
                    string.Empty,
                    "data:,",  // valid uri but no data
            };
            if (!isValid)
            {
                names = new string[]
                {
                    null,
                    string.Empty,
                    "fi€€€le.jpg", // Unicode chars
                };
                contentTypes = new string[]
                {
                    null,
                    string.Empty,
                    "invalid",
                };
                contentUrls = new string[]
                {
                    null,
                    string.Empty,
                    invalidUrl,
                };
                thumbnailUrls = new string[]
                {
                    null,
                    string.Empty,
                    invalidUrl,
                };
                content = new object[]
                {
                    null,
                    string.Empty,
                    "data:,",  // valid uri but no data
                    "12345",
                    invalidUrl,
                    new byte[10],
                    Array.Empty<byte>(),
                };
            }

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

        public static WeChatSettings MockWeChatSettings(bool isTemp = true, bool passiveResponseMode = false)
        {
            return new WeChatSettings
            {
                Token = "bmwipabotwx",
                AppId = "wx77f941c869071d99",
                EncodingAesKey = "P7PIjIGpA7axbjbffRoWYq7G0BsIaEpqdawIir4KqCt",
                AppSecret = "secret",
                UploadTemporaryMedia = isTemp,
                PassiveResponseMode = passiveResponseMode,
            };
        }

        public static UploadMediaResult MockTempMediaResult(string type, string mediaId = null)
        {
            var uploadResult = new UploadTemporaryMediaResult()
            {
                MediaId = mediaId ?? "mediaId",
                ErrorCode = 0,
                ErrorMessage = "ok",
                ThumbMediaId = "thumbMediaId",
                Type = type,
            };
            return uploadResult;
        }

        internal static UploadMediaResult MockForeverMediaResult(string mediaId = null)
        {
            var uploadResult = new UploadPersistentMediaResult()
            {
                MediaId = mediaId ?? "mediaId",
                Url = "https://mediaUrl",
                ErrorCode = 0,
                ErrorMessage = "ok",
            };
            return uploadResult;
        }

        internal static WeChatClient GetMockWeChatClient()
        {
            var storage = new MemoryStorage();
            var wechatClient = new Mock<WeChatClient>(WeChatSettings, storage, null);
            var result = JsonConvert.SerializeObject(WeChatJsonResult);
            var byteResult = Encoding.UTF8.GetBytes(result);
            wechatClient.Setup(c => c.GetAccessTokenAsync()).Returns(Task.FromResult("mockToken"));
            wechatClient.Setup(c => c.SendHttpRequestAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(byteResult));
            wechatClient.Setup(c => c.UploadMediaAsync(It.IsAny<AttachmentData>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(Task.FromResult(MockTempMediaResult(MediaTypes.Image)));
            wechatClient.Setup(c => c.UploadNewsAsync(It.IsAny<News[]>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(Task.FromResult(MockTempMediaResult(MediaTypes.News)));
            wechatClient.Setup(c => c.UploadNewsImageAsync(It.IsAny<AttachmentData>(), It.IsAny<int>())).Returns(Task.FromResult(MockForeverMediaResult("foreverMedia")));

            return wechatClient.Object;
        }
    }
}
