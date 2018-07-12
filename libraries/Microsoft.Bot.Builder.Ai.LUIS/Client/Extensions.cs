// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS.Models;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// LUIS extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Try to find an entity within the result.
        /// </summary>
        /// <param name="result">The LUIS result.</param>
        /// <param name="type">The entity type.</param>
        /// <param name="entity">The found entity.</param>
        /// <returns>True if the entity was found, false otherwise.</returns>
        public static bool TryFindEntity(this LuisResult result, string type, out EntityRecommendation entity)
        {
            entity = result.Entities?.FirstOrDefault(e => e.Type == type);
            return entity != null;
        }

        /// <summary>
        /// Parse all resolutions from a LUIS result.
        /// </summary>
        /// <param name="parser">The resolution parser.</param>
        /// <param name="entities">The LUIS entities.</param>
        /// <returns>The parsed resolutions.</returns>
        public static IEnumerable<Resolution> ParseResolutions(this IResolutionParser parser, IEnumerable<EntityRecommendation> entities)
        {
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (parser.TryParse(entity.Resolution, out var resolution))
                    {
                        yield return resolution;
                    }
                }
            }
        }

        /// <summary>
        /// Return the next <see cref="BuiltIn.DateTime.DayPart"/>.
        /// </summary>
        /// <param name="part">The <see cref="BuiltIn.DateTime.DayPart"/> query.</param>
        /// <returns>The next <see cref="BuiltIn.DateTime.DayPart"/> after the query.</returns>
        public static BuiltIn.DateTime.DayPart Next(this BuiltIn.DateTime.DayPart part)
        {
            switch (part)
            {
                case BuiltIn.DateTime.DayPart.MO: return BuiltIn.DateTime.DayPart.MI;
                case BuiltIn.DateTime.DayPart.MI: return BuiltIn.DateTime.DayPart.AF;
                case BuiltIn.DateTime.DayPart.AF: return BuiltIn.DateTime.DayPart.EV;
                case BuiltIn.DateTime.DayPart.EV: return BuiltIn.DateTime.DayPart.NI;
                case BuiltIn.DateTime.DayPart.NI: return BuiltIn.DateTime.DayPart.MO;
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Query the LUIS service using this text.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="text">The query text.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, string text, CancellationToken token)
        {
            var luisRequest = service.ModifyRequest(new LuisRequest(query: text));
            return await service.QueryAsync(luisRequest, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Query the LUIS service using this request.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="request">Query request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, LuisRequest request, CancellationToken token)
        {
            service.ModifyRequest(request);
            var uri = service.BuildUri(request);
            return await service.QueryAsync(uri, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds luis uri with text query.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="text">The query text.</param>
        /// <returns>The LUIS request Uri.</returns>
        public static Uri BuildUri(this ILuisService service, string text)
        {
            return service.BuildUri(service.ModifyRequest(new LuisRequest(query: text)));
        }
    }
}
