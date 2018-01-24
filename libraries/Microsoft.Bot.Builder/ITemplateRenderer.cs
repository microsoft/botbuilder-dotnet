﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Defines interface for data binding to template and rendering a string
    /// </summary>
    public interface ITemplateRenderer
    {
        /// <summary>
        /// render a template to an activity or string
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="language">language to render</param>
        /// <param name="templateId">tenmplate to render</param>
        /// <param name="data">data object to use to render</param>
        /// <returns></returns>
        Task<object> RenderTemplate(IBotContext context, string language, string templateId, object data);
    }
}
