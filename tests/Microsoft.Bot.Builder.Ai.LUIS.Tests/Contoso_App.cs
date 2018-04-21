// File generated via LUISGen ..\..\..\..\LUISGenTest\Contoso App.json -c Luis.Contoso_App
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Core.Extensions;
namespace Luis 
{
    public class Contoso_App: IRecognizerConvert
    {
        public string Text;
        public string AlteredText;
        public enum Intent {
            Cancel, 
            Delivery, 
            EntityTests, 
            Greeting, 
            Help, 
            None, 
            SpecifyName, 
            Travel
        };
        public Dictionary<Intent, double> Intents;

        public class _Entities
        {
            // Simple entities
            public string[] Name;
            public string[] State;
            public string[] City;
            public string[] To;
            public string[] From;

            // Lists
            public string[][] Airline;

            // Composites
            public class _InstanceAddress
            {
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_number;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] State;
            }
            public class AddressClass
            {
                public double[] builtin_number;
                public string[] State;
                [JsonProperty("$instance")]
                public _InstanceAddress _instance;
            }
            public AddressClass[] Address;

            public class _InstanceComposite1
            {
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_age;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_datetime;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_dimension;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_email;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_money;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_number;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_ordinal;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_percentage;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_phonenumber;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_temperature;
            }
            public class Composite1Class
            {
                public Microsoft.Bot.Builder.Ai.LUIS.Age[] builtin_age;
                public Microsoft.Bot.Builder.Ai.LUIS.DateTimeSpec[] builtin_datetime;
                public Microsoft.Bot.Builder.Ai.LUIS.Dimension[] builtin_dimension;
                public string[] builtin_email;
                public Microsoft.Bot.Builder.Ai.LUIS.Money[] builtin_money;
                public double[] builtin_number;
                public double[] builtin_ordinal;
                public double[] builtin_percentage;
                public string[] builtin_phonenumber;
                public Microsoft.Bot.Builder.Ai.LUIS.Temperature[] builtin_temperature;
                [JsonProperty("$instance")]
                public _InstanceComposite1 _instance;
            }
            public Composite1Class[] Composite1;

            public class _InstanceComposite2
            {
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] Airline;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] City;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] builtin_url;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] From;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] To;
            }
            public class Composite2Class
            {
                public string[][] Airline;
                public string[] City;
                public string[] builtin_url;
                public string[] From;
                public string[] To;
                [JsonProperty("$instance")]
                public _InstanceComposite2 _instance;
            }
            public Composite2Class[] Composite2;


            // Instance
            public class _Instance
            {
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] Name;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] State;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] City;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] To;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] From;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] Airline;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] Address;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] Composite1;
                public Microsoft.Bot.Builder.Ai.LUIS.InstanceData[] Composite2;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;
        }
        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties {get; set; }

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<Contoso_App>(JsonConvert.SerializeObject(result));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }
    }
}
