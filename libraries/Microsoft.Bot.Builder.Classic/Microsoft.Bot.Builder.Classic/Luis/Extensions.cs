// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Classic.Luis.Models;
using static Microsoft.Bot.Builder.Classic.Luis.BuiltIn.DateTime;

namespace Microsoft.Bot.Builder.Classic.Luis
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
        public static bool TryFindEntity(this LuisResult result, string type, out EntityModel entity)
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
        public static IEnumerable<Resolution> ParseResolutions(this IResolutionParser parser, IEnumerable<EntityModel> entities)
        {
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    Resolution resolution;
                    if (parser.TryParse(entity.Resolution, out resolution))
                    {
                        yield return resolution;
                    }
                }
            }
        }

        /// <summary>
        /// Return the next <see cref="DayPart"/>. 
        /// </summary>
        /// <param name="part">The <see cref="DayPart"/> query.</param>
        /// <returns>The next <see cref="DayPart"/> after the query.</returns>
        public static DayPart Next(this DayPart part)
        {
            switch (part)
            {
                case DayPart.MO: return DayPart.MI;
                case DayPart.MI: return DayPart.AF;
                case DayPart.AF: return DayPart.EV;
                case DayPart.EV: return DayPart.NI;
                case DayPart.NI: return DayPart.MO;
                default: throw new NotImplementedException();
            }
        }

    }
}
