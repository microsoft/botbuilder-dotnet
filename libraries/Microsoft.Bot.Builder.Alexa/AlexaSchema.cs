using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Alexa
{
    public class AlexaRequestBody
    {
        public string Version { get; set; }

        public AlexaSession Session { get; set; }

        public AlexaContext Context { get; set; }

        [JsonConverter(typeof(AlexaRequestConvertor))]
        public IAlexaRequest Request { get; set; }

        public Type GetAlexaRequestType()
        {
            if (Request is AlexaIntentRequest)
                return typeof(AlexaIntentRequest);

            if (Request is AlexaLaunchRequest)
                return typeof(AlexaLaunchRequest);

            if (Request is AlexaSessionEndRequest)
                return typeof(AlexaSessionEndRequest);

            return null;
        }
    }

    public class AlexaSession
    {

        /** A boolean value indicating whether this is a new session. */
        public bool New { get; set; }      
        
        /** A string that represents a unique identifier per a user's active session. */
        public string SessionId { get; set; }

        /** A map of key-value pairs. */
        public IDictionary<string, string> Attributes { get; set; }
        
        /** An object containing an application ID. This is used to verify that the request was intended for your service. */
        public AlexaApplication Application { get; set; }
        
        /** An object that describes the user making the request. */
        public AlexaUser User { get; set; }
    }

    public class AlexaContext
    {
        public AlexaSystem System { get; set; }

        public AlexaAudioPlayer AudioPlayer { get; set; }
    }

    public class AlexaSystem
    {
        public string ApiAccessToken { get; set; }

        public string ApiEndpoint { get; set; }

        public AlexaApplication Application { get; set; }

        public AlexaDevice Device { get; set; }

        public AlexaUser User { get; set; }
    }

    public class AlexaApplication
    {
        public string ApplicationId { get; set; }
    }

    public class AlexaDevice
    {
        public string DeviceId { get; set; }

        public AlexaSupportedInterfaces SupportedInterfaces { get; set; }
    }

    public class AlexaSupportedInterfaces
    {
        public List<string> Interfaces { get; set; }
    }

    public class AlexaUser
    {
        public string UserId { get; set; }

        public string AccessToken { get; set; }
    }

    public class AlexaAudioPlayer
    {
        public string Token{ get; set; }

        public int OffsetInMilliseconds { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AlexaPlayerActivityTypes PlayerActivity { get; set; }
    }

    public enum AlexaPlayerActivityTypes
    {
        IDLE,
        PAUSED,
        PLAYING,
        BUFFER_UNDERRUN,
        FINISHED,
        STOPPED
    }

    public class AlexaResponseBody
    {
        public string Version { get; set; }

        public IDictionary<string, string> SessionAttributes { get; set; }

        public AlexaResponse Response { get; set; }
    }

    public class AlexaResponse
    {
        public AlexaOutputSpeech OutputSpeech { get; set; }

        public AlexaCard Card { get; set; }

        public ResponseReprompt Reprompt { get; set; }

        public bool ShouldEndSession { get; set; }

        public object[] Directives { get; set; }
    }

    public class ResponseReprompt
    {
        public AlexaOutputSpeech OutputSpeech { get; set; }
    }

    public class AlexaOutputSpeech
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AlexaOutputSpeechType Type { get; set; }

        public string Text { get; set; }

        public string Ssml { get; set; }
    }

    public enum AlexaOutputSpeechType
    {
        SSML,
        PlainText
    }

    public class AlexaCard
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AlexaCardType Type { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Text { get; set; }

        public AlexaCardImage Image { get; set; }
    }

    public class AlexaCardImage
    {
        public string SmallImageUrl { get; set; }

        public string LargeImageUrl { get; set; }
    }

    public enum AlexaCardType
    {
        Simple,
        Standard,
        LinkAccount
    }

    public interface IAlexaRequest
    {
        string Type { get; set; }

        string Timestamp { get; set; }

        string RequestId { get; set; }

        string Locale { get; set; }
    }

    public class AlexaLaunchRequest : IAlexaRequest
    {
        public string Type { get; set; }

        public string Timestamp { get; set; }

        public string RequestId { get; set; }

        public string Locale { get; set; }
    }

    public class AlexaIntentRequest : IAlexaRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AlexaDialogState DialogState { get; set; }

        public AlexaIntent Intent { get; set; }

        public string Type { get; set; }

        public string Timestamp { get; set; }

        public string RequestId { get; set; }

        public string Locale { get; set; }
    }

    public class AlexaSessionEndRequest : IAlexaRequest
    {
        public string Reason { get; set; }

        public AlexaError Error { get; set; }

        public string Type { get; set; }

        public string Timestamp { get; set; }

        public string RequestId { get; set; }

        public string Locale { get; set; }
    }

    public class AlexaError
    {
        public string Type { get; set; }

        public string Message { get; set; }
    }

    public enum AlexaDialogState
    {
        STARTED,
        IN_PROGRESS,
        COMPLETED
    }

    public class AlexaIntent
    {
        public string Name { get; set; }

        public string ConfirmationStatus { get; set; }

        public Dictionary<string,AlexaSlot> Slots { get; set; }
    }

    public class AlexaSlot
    {
        public string Name { get; set; }

        public string Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AlexaConfirmationState ConfirmationStatus { get; set; }

        public AlexaResolution Resolution { get; set; }
    }

    public class AlexaResolution
    {
        public List<AlexaResolutionAuthority> ResolutionsPerAuthority { get; set; }
    }

    public class AlexaResolutionAuthority
    {
        public string Authority { get; set; }

        public AlexaResolutionStatus Status { get; set; }

        public List<AlexaResolvedValue> Values { get; set; }
    }

    public class AlexaResolutionStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AlexaResolutionStatusCode Code { get; set; }
    }

    public class AlexaResolvedValue
    {
        public AlexaResolvedValueValue Value { get; set; }
    }

    public class AlexaResolvedValueValue
    {
        public string Name { get; set; }

        public string Id { get; set; }
    }

    public enum AlexaConfirmationState
    {
        NONE,
        CONFIRMED,
        DENIED
    }

    public enum AlexaResolutionStatusCode
    {
        ER_SUCCESS_MATCH,
        ER_SUCCESS_NO_MATCH,
        ER_ERROR_TIMEOUT,
        ER_ERROR_EXCEPTION
    }

    public class AlexaDirectiveRequest
    {
        public DirectiveHeader Header { get; set; }

        public DirectiveContent Directive { get; set; }

        public class DirectiveHeader
        {
            public string RequestId { get; set; }
        }

        public class DirectiveContent
        {
            public string Type { get; set; }
            public string Speech { get; set; }
        }
    }

    public class AlexaRequestConvertor : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IAlexaRequest);
        }
        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var profession = default(IAlexaRequest);
            switch (jsonObject["type"].Value<string>())
            {
                case AlexaRequestTypes.LaunchRequest:
                    profession = new AlexaLaunchRequest();
                    break;
                case AlexaRequestTypes.SessionEndedRequest:
                    profession = new AlexaSessionEndRequest();
                    break;
                case AlexaRequestTypes.IntentRequest:
                    profession = new AlexaIntentRequest();
                    break;
            }
            serializer.Populate(jsonObject.CreateReader(), profession);
            return profession;
        }
    }
}
