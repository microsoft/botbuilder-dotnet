// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ChannelServiceExceptionFilterAttribute : Attribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception != null)
            {
                if (context.Exception is NotImplementedException)
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status501NotImplemented);
                }
                else if (context.Exception is UnauthorizedAccessException)
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else if (context.Exception is KeyNotFoundException)
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                else
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                context.ExceptionHandled = true;
            }
        }
    }
}
