// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// BotConfiguration represents configuration information for a bot.
    /// </summary>
    /// <remarks>It is typically loaded from a .bot file on disk
    /// This class implements methods for encrypting and manipulating the in memory representation of the configuration</remarks>
    public class BotConfiguration
    {
        private string location;

        public BotConfiguration()
        {
            this.Version = "2.0";
        }

        /// <summary>
        /// Gets or sets name of the bot.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets description of the bot.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets secretKey - Used to validate that the secret is consistent for all encrypted fields.
        /// </summary>
        [JsonProperty("secretKey")]
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets the version .
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets connected services.
        /// </summary>
        [JsonProperty("services")]
        public List<ConnectedService> Services { get; set; } = new List<ConnectedService>();

        /// <summary>
        /// Generate a new key suitable for encrypting
        /// </summary>
        /// <returns>key to use with .Encrypt() method</returns>
        public static string GenerateKey()
        {
            return EncryptUtilities.GenerateKey();
        }

        /// <summary>
        /// Load the bot configuration by looking in a folder and finding the first .bot file in the folder.
        /// </summary>
        /// <param name="folder">folder to look for bot files</param>
        /// <param name="secret">secret to use to encrypt keys</param>
        /// <returns>task for BotConfiguration</returns>
        public static async Task<BotConfiguration> LoadFromFolderAsync(string folder, string secret = null)
        {
            var file = Directory.GetFiles(folder, "*.bot", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (file != null)
            {
                return await BotConfiguration.LoadAsync(file, secret);
            }

            throw new FileNotFoundException($"Error: no bot file found in {folder}. Choose a different location or use msbot init to create a.bot file.");
        }

        /// <summary>
        /// load the configuration from a .bot file.
        /// </summary>
        /// <param name="file">path to bot file</param>
        /// <param name="secret">secret to use to decrypt the file on disk</param>
        /// <returns>async task for BotConfiguration</returns>
        public static async Task<BotConfiguration> LoadAsync(string file, string secret = null)
        {
            string json = string.Empty;
            using (var stream = File.OpenText(file))
            {
                json = await stream.ReadToEndAsync().ConfigureAwait(false);
            }

            var bot = JsonConvert.DeserializeObject<BotConfiguration>(json, new BotConfigConverter());
            bot.location = file;

            var hasSecret = bot.SecretKey?.Length > 0;
            if (hasSecret)
            {
                bot.Decrypt(secret);
            }

            return bot;
        }

        /// <summary>
        /// Save the file with secret
        /// </summary>
        /// <param name="secret">secret for encryption</param>
        /// <returns>task</returns>
        public Task SaveAsync(string secret = null)
        {
            return this.SaveAsAsync(this.location, secret);
        }

        /// <summary>
        /// Save the configuration to a .bot file.
        /// </summary>
        /// <param name="path">path to bot file</param>
        /// <param name="secret">secret for encrypting the file keys.</param>
        public async Task SaveAsAsync(string path, string secret = null)
        {
            if (!string.IsNullOrEmpty(secret))
            {
                this.ValidateSecretKey(secret);
            }

            var hasSecret = this.SecretKey?.Length > 0;

            // make sure that all dispatch serviceIds still match services that are in the bot
            foreach (var dispatchService in this.Services.Where(s => s.Type == ServiceTypes.Dispatch).Cast<DispatchService>())
            {
                dispatchService.ServiceIds = dispatchService.ServiceIds
                        .Where(serviceId => this.Services.Any(s => s.Id == serviceId))
                        .ToList();
            }

            if (hasSecret)
            {
                // make sure fields are encrypted before serialization
                this.Encrypt(secret);
            }

            // save it to disk
            using (var file = File.Open(path ?? this.location, FileMode.Create))
            {
                using (TextWriter writer = new StreamWriter(file))
                {
                    await writer.WriteLineAsync(JsonConvert.SerializeObject(this, Formatting.Indented)).ConfigureAwait(false);
                }
            }

            if (hasSecret)
            {
                // make sure all in memory fields are decrypted again for continued operations
                this.Decrypt(secret);
            }
        }

        public void ClearSecret()
        {
            this.SecretKey = string.Empty;
        }

        /// <summary>
        /// connect a service to the bot file.
        /// </summary>
        /// <param name="newService">sevice to add</param>
        public void ConnectService(ConnectedService newService)
        {
            if (this.Services.Where(s => s.Type == newService.Type && s.Id == newService.Id).Any())
            {
                throw new Exception($"service with {newService.Id} is already connected");
            }
            else
            {
                // assign a unique random id between 0-255 (255 services seems like a LOT of services
                Random rnd = new Random();
                do
                {
                    newService.Id = rnd.Next(byte.MaxValue).ToString();
                }
                while (this.Services.Where(s => s.Id == newService.Id).Any());

                this.Services.Add(newService);
            }
        }

        /// <summary>
        /// encrypt all values in the in memory config
        /// </summary>
        /// <param name="secret">secret to encrypt</param>
        public void Encrypt(string secret)
        {
            this.ValidateSecretKey(secret);

            foreach (var service in this.Services)
            {
                service.Encrypt(secret);
            }
        }

        /// <summary>
        /// decrypt all values in the in memory config.
        /// </summary>
        /// <param name="secret">secret to encrypt</param>
        public void Decrypt(string secret)
        {
            this.ValidateSecretKey(secret);

            foreach (var service in this.Services)
            {
                service.Decrypt(secret);
            }
        }

        /// <summary>
        /// find service by name or id.
        /// </summary>
        /// <param name="nameOrId">name or service id</param>
        /// <returns>found service</returns>
        public ConnectedService FindServiceByNameOrId(string nameOrId)
        {
            var svs = new List<ConnectedService>(this.Services);

            for (var i = 0; i < svs.Count(); i++)
            {
                var service = svs.ElementAt(i);
                if (service.Id == nameOrId || service.Name == nameOrId)
                {
                    return service;
                }
            }

            return null;
        }

        /// <summary>
        /// find a service by id.
        /// </summary>
        /// <param name="id">id of the service</param>
        /// <returns>service</returns>
        public ConnectedService FindService(string id)
        {
            var svs = new List<ConnectedService>(this.Services);

            for (var i = 0; i < svs.Count(); i++)
            {
                var service = svs.ElementAt(i);
                if (service.Id == id)
                {
                    return service;
                }
            }

            return null;
        }

        /// <summary>
        /// remove service by name or id.
        /// </summary>
        /// <param name="nameOrId">name or service id</param>
        /// <returns>found service</returns>
        public ConnectedService DisconnectServiceByNameOrId(string nameOrId)
        {
            var svs = new List<ConnectedService>(this.Services);

            for (var i = 0; i < svs.Count(); i++)
            {
                var service = svs.ElementAt(i);
                if (service.Id == nameOrId || service.Name == nameOrId)
                {
                    svs.RemoveAt(i);
                    this.Services = svs.ToList();
                    return service;
                }
            }

            throw new Exception($"a service with id or name of[{nameOrId}] was not found");
        }

        /// <summary>
        /// remove a service by id.
        /// </summary>
        /// <param name="id">id of the service</param>
        public void DisconnectService(string id)
        {
            var svs = new List<ConnectedService>(this.Services);

            for (var i = 0; i < svs.Count(); i++)
            {
                var service = svs.ElementAt(i);
                if (service.Id == id)
                {
                    svs.RemoveAt(i);
                    this.Services = svs.ToList();
                    return;
                }
            }
        }

        /// <summary>
        ///  make sure secret is correct by decrypting the secretKey with it
        /// </summary>
        /// <param name="secret">secret to use</param>
        protected void ValidateSecretKey(string secret)
        {
            if (secret?.Length == null)
            {
                throw new Exception("You are attempting to perform an operation which needs access to the secret and --secret is missing");
            }

            try
            {
                if (this.SecretKey?.Length == 0)
                {
                    // if no key, create a guid and enrypt that to use as secret validator
                    this.SecretKey = Guid.NewGuid().ToString("n").Encrypt(secret);
                }
                else
                {
                    // this will throw exception if invalid secret
                    this.SecretKey.Decrypt(secret);
                }
            }
            catch
            {
                throw new Exception("You are attempting to perform an operation which needs access to the secret and --secret is incorrect.");
            }
        }

        /// <summary>
        /// return strong typed connected service objects.
        /// </summary>
        internal class BotConfigConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(ConnectedService).IsAssignableFrom(objectType);
            }

            /// <inheritdoc/>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // Load JObject from stream
                JObject jObject = JObject.Load(reader);

                switch ((string)jObject["type"])
                {
                    case ServiceTypes.Bot:
                        return jObject.ToObject<BotService>();
                    case ServiceTypes.AppInsights:
                        return jObject.ToObject<AppInsightsService>();
                    case ServiceTypes.BlobStorage:
                        return jObject.ToObject<BlobStorageService>();
                    case ServiceTypes.CosmosDB:
                        return jObject.ToObject<CosmosDbService>();
                    case ServiceTypes.Dispatch:
                        return jObject.ToObject<DispatchService>();
                    case ServiceTypes.Endpoint:
                        return jObject.ToObject<EndpointService>();
                    case ServiceTypes.File:
                        return jObject.ToObject<FileService>();
                    case ServiceTypes.Luis:
                        return jObject.ToObject<LuisService>();
                    case ServiceTypes.QnA:
                        return jObject.ToObject<QnAMakerService>();
                    case ServiceTypes.Generic:
                        return jObject.ToObject<GenericService>();
                    default:
                        throw new Exception($"Unknown service type {(string)jObject["type"]}");
                }
            }

            /// <inheritdoc/>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
