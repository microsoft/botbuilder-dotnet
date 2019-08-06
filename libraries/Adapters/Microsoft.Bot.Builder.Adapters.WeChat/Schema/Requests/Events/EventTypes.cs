// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    /// <summary>
    /// Detail Event type from WeChat.
    /// </summary>
    public static class EventTypes
    {
        /// <summary>
        /// Enter a conversation, may removed by wechat.
        /// deprecated.
        /// </summary>
        public const string Enter = "ENTER";

        /// <summary>
        /// Location, may removed by wechat.
        /// deprecated.
        /// </summary>
        public const string Location = "LOCATION";

        /// <summary>
        /// Subscribtion event.
        /// </summary>
        public const string Subscribe = "subscribe";

        /// <summary>
        /// Unsubscribtion event.
        /// </summary>
        public const string Unsubscribe = "unsubscribe";

        /// <summary>
        /// Static menu click event.
        /// </summary>
        public const string Click = "CLICK";

        /// <summary>
        /// QR code scan event.
        /// </summary>
        public const string Scan = "SCAN";

        /// <summary>
        /// Redirect Url Event.
        /// </summary>
        public const string View = "VIEW";

        /// <summary>
        /// Group message send finish event.
        /// </summary>
        public const string MassSendJobFinished = "MASSSENDJOBFINISH";

        /// <summary>
        /// template message send finish event.
        /// </summary>
        public const string TemplateSendFinished = "TEMPLATESENDJOBFINISH";

        /// <summary>
        /// Scan code then push event.
        /// TODO: need a demo to clear what is this used for.
        /// </summary>
        public const string ScanPush = "scancode_push";

        /// <summary>
        /// Show 'please wait' to user when ScanPush.
        /// </summary>
        public const string WaitScanPush = "scancode_waitmsg";

        /// <summary>
        /// Open system camera.
        /// </summary>
        public const string Camera = "pic_sysphoto";

        /// <summary>
        /// Open system camera or album.
        /// </summary>
        public const string CameraOrAlbum = "pic_photo_or_album";

        /// <summary>
        /// Open wechat album.
        /// </summary>
        public const string WeChatAlbum = "pic_weixin";

        /// <summary>
        /// Open location selector.
        /// </summary>
        public const string SelectLocation = "location_select";

        // Membership card, coupon and giftcard etc.

        /// <summary>
        /// Card review passed.
        /// </summary>
        public const string CardReviewSuccessful = "card_pass_check";

        /// <summary>
        /// Card review failed.
        /// </summary>
        public const string CardReviewFailed = "card_not_pass_check";

        /// <summary>
        /// User collect a card.
        /// </summary>
        public const string CardCollected = "user_get_card";

        /// <summary>
        /// User delete a card.
        /// </summary>
        public const string CardDeleted = "user_del_card";

        /// <summary>
        /// Gifting card to others.
        /// </summary>
        public const string CardGifting = "user_gifting_card";

        /// <summary>
        /// Remove card after the card consumed.
        /// </summary>
        public const string RemoveAfterUse = "user_consume_card";

        /// <summary>
        /// User enter the card detail page.
        /// </summary>
        public const string ViewCard = "user_view_card";

        /// <summary>
        /// Membership Card Content Update event: Membership card points when the balance changes.
        /// </summary>
        public const string MemberCardUpdated = "update_member_card";

        /// <summary>
        /// Card low in stock event, when the initial inventory number of a card_id is greater than 200 and the current inventory is less than or equal to 100.
        /// </summary>
        public const string CardLowInStock = "card_sku_remind";

        /// <summary>
        /// Card point change event：When the merchant’s friend’s card point changes
        /// https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1451025274.
        /// </summary>
        public const string CardPointChange = "card_pay_order";

        /// <summary>
        /// Membership card activated. User submit the info used to create a membership card.
        /// </summary>
        public const string MemberShipActivated = "submit_membercard_user_info";

        /// <summary>
        /// User buy a giftcard.
        /// </summary>
        public const string GiftcardPayed = "giftcard_pay_done";

        /// <summary>
        /// Buy giftcard and send to other.
        /// </summary>
        public const string GiftcardPayedAndSend = "giftcard_send_to_friend";

        /// <summary>
        /// Accept a giftcard.
        /// </summary>
        public const string GiftcardAccepted = "giftcard_user_accept";

        /// <summary>
        /// Open a session with mutiple customer service staff.
        /// </summary>
        public const string MutipleCSSessionStart = "kf_create_session";

        /// <summary>
        /// Mutiple customer service session closed.
        /// </summary
        public const string MutipleCSSessionClosed = "kf_close_session";

        /// <summary>
        /// Switch customer service staff.
        /// </summary>
        public const string SwitchCS = "kf_switch_session";

        /// <summary>
        /// POI review result notification.
        /// </summary>
        public const string POIReviewed = "poi_check_notify";

        /// <summary>
        /// Wi-Fi connected event.
        /// </summary>
        public const string WifiConnected = "WifiConnected";

        /// <summary>
        /// Enter offical account from card.
        /// </summary>
        public const string EnterFromCard = "user_enter_session_from_card";

        /// <summary>
        /// An order has been created from wechat store.
        /// </summary>
        public const string MerchantOrderCreated = "merchant_order";

        /// <summary>
        /// Shakearound(摇一摇) notification event.
        /// </summary>
        public const string Shakearound = "ShakearoundUserShake";

        /// <summary>
        /// User paid using card(wechat card, not bank card).
        /// </summary>
        public const string PaidWithCard = "user_pay_from_pay_cell";

        /// <summary>
        /// Create store small program audit events.
        /// </summary>
        public const string AuditStore = "apply_merchant_audit_info";

        /// <summary>
        /// Create a store audit event from a Tencent map.
        /// </summary>
        public const string AuditStoreFromTencentMap = "create_map_poi_audit_info";

        /// <summary>
        /// Create store audit event from mini program.
        /// </summary>
        public const string AuditStoreFromMP = "add_store_audit_info";

        /// <summary>
        /// Modify store audit info.
        /// </summary>
        public const string AuditInfoChanged = "modify_store_audit_info";

        /// <summary>
        /// Qualification certification is successful (can access to interface immediately at this time).
        /// </summary>
        public const string QualificationVerifySuccess = "qualification_verify_success";

        /// <summary>
        /// Qualification certification is failed.
        /// </summary>
        public const string QualificationVerifyFailed = "qualification_verify_fail";

        /// <summary>
        /// Naming success.
        /// </summary>
        public const string NamingSuccess = "naming_verify_success";

        /// <summary>
        /// Naming failed.
        /// </summary>
        public const string NamingFailed = "naming_verify_fail";

        /// <summary>
        /// Account annual review notification.
        /// </summary>
        public const string AnnualReview = "annual_renew";

        /// <summary>
        /// Account verification expired.
        /// </summary>
        public const string VerifyExpired = "verify_expired";

        /// <summary>
        /// Mini program review successful.
        /// </summary>
        public const string MiniAppReviewSuccess = "weapp_audit_success";

        /// <summary>
        /// Mini program review failed.
        /// </summary>
        public const string MiniAppReviewFailed = "weapp_audit_fail";

        /// <summary>
        /// Jump to mini program.
        /// </summary>
        public const string ViewMiniProgram = "view_miniprogram";
    }
}
