using System.Xml.Linq;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common;
using Microsoft.Bot.Builder.Adapters.WeChat.Test.TestUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test
{
    public class MessageFactoryTest
    {
        [Fact]
        public void GetRequestEntityTest()
        {
            var logger = NullLogger.Instance;
            {
                // Text
                var doc = XDocument.Parse(MockDataUtility.XmlText);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is TextRequest);
                MessageBaseTest(result as TextRequest);
                var textRequest = result as TextRequest;
                Assert.Equal(RequestMessageType.Text, result.MsgType);
                Assert.Equal("this is a test", textRequest.Content);
            }

            {
                // Image
                var doc = XDocument.Parse(MockDataUtility.XmlImage);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is ImageRequest);
                Assert.Equal(RequestMessageType.Image, result.MsgType);
                MessageBaseTest(result as RequestMessage);
                var imageRequest = result as ImageRequest;
                Assert.Equal("this is a url", imageRequest.PicUrl);
                Assert.Equal("media_id", imageRequest.MediaId);
            }

            {
                // Voice
                var doc = XDocument.Parse(MockDataUtility.XmlVoice);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is VoiceRequest);
                Assert.Equal(RequestMessageType.Voice, result.MsgType);
                MessageBaseTest(result as RequestMessage);
                var voiceRequest = result as VoiceRequest;
                Assert.Equal("media_id", voiceRequest.MediaId);
                Assert.Equal("Format", voiceRequest.Format);
            }

            {
                // Video
                var doc = XDocument.Parse(MockDataUtility.XmlVideo);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is VideoRequest);
                Assert.Equal(RequestMessageType.Video, result.MsgType);
                MessageBaseTest(result as RequestMessage);
                var videoRequest = result as VideoRequest;
                Assert.Equal("media_id", videoRequest.MediaId);
                Assert.Equal("thumb_media_id", videoRequest.ThumbMediaId);
            }

            {
                // Short Video
                var doc = XDocument.Parse(MockDataUtility.XmlShortVideo);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is ShortVideoRequest);
                Assert.Equal(RequestMessageType.ShortVideo, result.MsgType);
                MessageBaseTest(result as RequestMessage);
                var shortvideoRequest = result as ShortVideoRequest;
                Assert.Equal("media_id", shortvideoRequest.MediaId);
                Assert.Equal("thumb_media_id", shortvideoRequest.ThumbMediaId);
            }

            {
                // Location
                var doc = XDocument.Parse(MockDataUtility.XmlLocation);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is LocationRequest);
                Assert.Equal(RequestMessageType.Location, result.MsgType);
                MessageBaseTest(result as RequestMessage);
                var locationRequest = result as LocationRequest;
                Assert.Equal(23.134521, locationRequest.Location_X);
                Assert.Equal(113.358803, locationRequest.Location_Y);
                Assert.Equal(20, locationRequest.Scale);
                Assert.Equal("LocationInfo", locationRequest.Label);
            }

            {
                // Link
                var doc = XDocument.Parse(MockDataUtility.XmlLink);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is LinkRequest);
                Assert.Equal(RequestMessageType.Link, result.MsgType);
                MessageBaseTest(result as RequestMessage);
                var linkRequest = result as LinkRequest;
                Assert.Equal("This is a link", linkRequest.Title);
                Assert.Equal("This is a link", linkRequest.Description);
                Assert.Equal("url", linkRequest.Url);
            }

            {
                // Click Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventClick);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is ClickEvent);
                EventBaseTest(result as RequestEvent);
                var clickEvent = result as ClickEvent;
                Assert.Equal(EventType.Click, clickEvent.Event);
                Assert.Equal(clickEvent.Event, EventType.Click);
                Assert.Equal("EVENTKEY", clickEvent.EventKey);
            }

            {
                // Location Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventLocation);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is LocationEvent);
                EventBaseTest(result as RequestEvent);
                var locationEvent = result as LocationEvent;
                Assert.Equal(EventType.Location, locationEvent.Event);
                Assert.Equal(23.104105, locationEvent.Latitude);
                Assert.Equal(113.320107, locationEvent.Longitude);
                Assert.Equal(65.000000, locationEvent.Precision);
            }

            {
                // View Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventView);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is ViewEvent);
                EventBaseTest(result as RequestEvent);
                var viewEvent = result as ViewEvent;
                Assert.Equal(EventType.View, viewEvent.Event);
                Assert.Equal("www.qq.com", viewEvent.EventKey);
            }

            {
                // Subscribe Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventSubscribe);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is SubscribeEvent);
                EventBaseTest(result as RequestEvent);
                var subscribeEvent = result as SubscribeEvent;
                Assert.Equal(EventType.Subscribe, subscribeEvent.Event);
                Assert.Equal("qrscene_123123", subscribeEvent.EventKey);
                Assert.Equal("TICKET", subscribeEvent.Ticket);
            }

            {
                // Scan Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventScan);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is ScanEvent);
                EventBaseTest(result as RequestEvent);
                var scanEvent = result as ScanEvent;
                Assert.Equal(EventType.Scan, scanEvent.Event);
                Assert.Equal("SCENE_VALUE", scanEvent.EventKey);
                Assert.Equal("TICKET", scanEvent.Ticket);
            }

            {
                // ScanPush Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventScanPush);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is ScanPushEvent);
                EventBaseTest(result as RequestEvent);
                var scanPushEvent = result as ScanPushEvent;
                Assert.Equal(EventType.ScanPush, scanPushEvent.Event);
                Assert.Equal("6", scanPushEvent.EventKey);
                Assert.Equal("qrcode", scanPushEvent.ScanCodeInfo.ScanType);
                Assert.Equal("1", scanPushEvent.ScanCodeInfo.ScanResult);
            }

            {
                // WaitScanPush Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventWaitScanPush);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is WaitScanPushEvent);
                EventBaseTest(result as RequestEvent);
                var waitScanPushEvent = result as WaitScanPushEvent;
                Assert.Equal(EventType.WaitScanPush, waitScanPushEvent.Event);
                Assert.Equal("6", waitScanPushEvent.EventKey);
                Assert.Equal("qrcode", waitScanPushEvent.ScanCodeInfo.ScanType);
                Assert.Equal("2", waitScanPushEvent.ScanCodeInfo.ScanResult);
            }

            {
                // Camera Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventCamera);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is CameraEvent);
                EventBaseTest(result as RequestEvent);
                var cameraEvent = result as CameraEvent;
                Assert.Equal(EventType.Camera, cameraEvent.Event);
                Assert.Equal("6", cameraEvent.EventKey);
                Assert.Equal(1, cameraEvent.SendPicsInfo.Count);
                Assert.Equal("1b5f7c23b5bf75682a53e7b6d163e185", cameraEvent.SendPicsInfo.PicList[0].Item.PicMD5Sum);
            }

            {
                // CameraOrAlbum Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventCameraOrAlbum);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is CameraOrAlbumEvent);
                EventBaseTest(result as RequestEvent);
                var cameraOrAlbumEvent = result as CameraOrAlbumEvent;
                Assert.Equal(EventType.CameraOrAlbum, cameraOrAlbumEvent.Event);
                Assert.Equal("6", cameraOrAlbumEvent.EventKey);
                Assert.Equal(1, cameraOrAlbumEvent.SendPicsInfo.Count);
                Assert.Equal("5a75aaca956d97be686719218f275c6b", cameraOrAlbumEvent.SendPicsInfo.PicList[0].Item.PicMD5Sum);
            }

            {
                // WeChatAlbum Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventWeChatAlbum);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is WeChatAlbumEvent);
                EventBaseTest(result as RequestEvent);
                var wechatAlbumEvent = result as WeChatAlbumEvent;
                Assert.Equal(EventType.WeChatAlbum, wechatAlbumEvent.Event);
                Assert.Equal("6", wechatAlbumEvent.EventKey);
                Assert.Equal(1, wechatAlbumEvent.SendPicsInfo.Count);
                Assert.Equal("5a75aaca956d97be686719218f275c6b", wechatAlbumEvent.SendPicsInfo.PicList[0].Item.PicMD5Sum);
            }

            {
                // SelectLocation Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventSelectLocation);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is SelectLocationEvent);
                EventBaseTest(result as RequestEvent);
                var selectLocationEvent = result as SelectLocationEvent;
                Assert.Equal(EventType.SelectLocation, selectLocationEvent.Event);
                Assert.Equal("6", selectLocationEvent.EventKey);
                Assert.Equal("23", selectLocationEvent.SendLocationInfo.Location_X);
                Assert.Equal("113", selectLocationEvent.SendLocationInfo.Location_Y);
                Assert.Equal("15", selectLocationEvent.SendLocationInfo.Scale);
                Assert.Equal("No.328 Xinghu Street", selectLocationEvent.SendLocationInfo.Label);
                Assert.Equal("test", selectLocationEvent.SendLocationInfo.Poiname);
            }

            {
                // ViewMiniProgram Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventViewMiniProgram);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is ViewMiniProgramEvent);
                EventBaseTest(result as RequestEvent);
                var viewMiniProgramEvent = result as ViewMiniProgramEvent;
                Assert.Equal(EventType.ViewMiniProgram, viewMiniProgramEvent.Event);
                Assert.Equal("pages/index/index", viewMiniProgramEvent.EventKey);
                Assert.Equal("MENUID", viewMiniProgramEvent.MenuId);
            }

            {
                // MassSendJobFinished Event
                var doc = XDocument.Parse(MockDataUtility.XmlEventMassSendJobFinished);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is MassSendJobFinishedEvent);
                EventBaseTest(result as RequestEvent);
                var massSendJobFinishedEvent = result as MassSendJobFinishedEvent;
                Assert.Equal(EventType.MassSendJobFinished, massSendJobFinishedEvent.Event);
                Assert.Equal(1000001625, massSendJobFinishedEvent.MsgID);
                Assert.Equal("err(30003)", massSendJobFinishedEvent.Status);
                Assert.Equal(0, massSendJobFinishedEvent.TotalCount);
                Assert.Equal(0, massSendJobFinishedEvent.FilterCount);
                Assert.Equal(0, massSendJobFinishedEvent.SentCount);
                Assert.Equal(0, massSendJobFinishedEvent.ErrorCount);
                Assert.Equal(2, massSendJobFinishedEvent.CopyrightCheckResult.Count);
                Assert.Equal(1, massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[0].ArticleIdx);
                Assert.Equal(0, massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[0].UserDeclareState);
                Assert.Equal(2, massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[0].AuditState);
                Assert.Equal("Url_1", massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[0].OriginalArticleUrl);
                Assert.Equal(1, massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[0].OriginalArticleType);
                Assert.Equal(1, massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[0].CanReprint);
                Assert.Equal(1, massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[0].NeedReplaceContent);
                Assert.Equal(1, massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[0].NeedShowReprintSource);
                Assert.Equal("Url_2", massSendJobFinishedEvent.CopyrightCheckResult.ResultList.Items[1].OriginalArticleUrl);
                Assert.Equal(2, massSendJobFinishedEvent.CopyrightCheckResult.CheckState);
            }

            {
                // TemplateSendFinished
                var doc = XDocument.Parse(MockDataUtility.XmlEventTemplateSendFinished);
                var result = Schema.WeChatMessageFactory.GetRequestEntity(doc, logger);
                Assert.True(result is TemplateSendFinishedEvent);
                EventBaseTest(result as RequestEvent);
                var templateSendFinishedEvent = result as TemplateSendFinishedEvent;
                Assert.Equal(EventType.TemplateSendFinished, templateSendFinishedEvent.Event);
                Assert.Equal(200163840, templateSendFinishedEvent.MsgID);
                Assert.Equal("failed:user block", templateSendFinishedEvent.Status);
            }
        }

        [Fact]
        public void GetResponseXmlTest()
        {
            {
                // Text
                var result = Schema.WeChatMessageFactory.ConvertResponseToXml(MockDataUtility.TextResponse);
                Assert.Equal(MockDataUtility.PassiveXmlText, result);
            }

            {
                // Image
                var result = Schema.WeChatMessageFactory.ConvertResponseToXml(MockDataUtility.ImageResponse);
                Assert.Equal(MockDataUtility.PassiveXmlImage, result);
            }

            {
                // Voice
                var result = Schema.WeChatMessageFactory.ConvertResponseToXml(MockDataUtility.VoiceResponse);
                Assert.Equal(MockDataUtility.PassiveXmlVoice, result);
            }

            {
                // Video
                var result = Schema.WeChatMessageFactory.ConvertResponseToXml(MockDataUtility.VideoResponse);
                Assert.Equal(MockDataUtility.PassiveXmlVideo, result);
            }

            {
                // Music
                var result = Schema.WeChatMessageFactory.ConvertResponseToXml(MockDataUtility.MusicResponse);
                Assert.Equal(MockDataUtility.PassiveXmlMusic, result);
            }

            {
                // News
                var result = Schema.WeChatMessageFactory.ConvertResponseToXml(MockDataUtility.NewsResponse);
                Assert.Equal(MockDataUtility.PassiveXmlNews, result);
            }
        }

        internal void MessageBaseTest(RequestMessage result)
        {
            Assert.Equal("toUser", result.ToUserName);
            Assert.Equal("fromUser", result.FromUserName);
            Assert.Equal(1234567890123456, result.MsgId);
            Assert.Equal(1348831860, result.CreateTime);
        }

        internal void EventBaseTest(RequestEvent result)
        {
            Assert.Equal("toUser", result.ToUserName);
            Assert.Equal("FromUser", result.FromUserName);
            Assert.Equal(123456789, result.CreateTime);
        }
    }
}
