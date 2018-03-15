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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Builder.Classic.Luis;

namespace Microsoft.Bot.Builder.Classic.Scorables
{
    public interface IDispatcher
    {
        Task<bool> TryPostAsync(CancellationToken token);
    }

    [Serializable]
    public class Dispatcher : IDispatcher
    {
        protected virtual IReadOnlyList<object> MakeServices()
        {
            return new[] { this };
        }

        protected virtual IResolver MakeResolver()
        {
            var services = this.MakeServices();
            var resolver = NoneResolver.Instance;
            resolver = new EnumResolver(resolver);
            resolver = new ArrayResolver(resolver, services);
            resolver = new ActivityResolver(resolver);
            resolver = new EventActivityValueResolver(resolver);
            resolver = new InvokeActivityValueResolver(resolver);

            return resolver;
        }

        protected virtual ILuisService MakeService(ILuisModel model)
        {
            return new LuisService(model);
        }

        protected virtual Regex MakeRegex(string pattern)
        {
            return new Regex(pattern);
        }

        private bool continueAfterPost;

        protected void ContinueWithNextGroup()
        {
            continueAfterPost = true;
        }

        protected virtual bool OnStage(FoldStage stage, IScorable<IResolver, object> scorable, IResolver item, object state, object score)
        {
            switch (stage)
            {
                case FoldStage.AfterFold: return true;
                case FoldStage.StartPost: continueAfterPost = false; return true;
                case FoldStage.AfterPost: return continueAfterPost;
                default: throw new NotImplementedException();
            }
        }

        protected virtual IComparer<object> MakeComparer()
        {
            return NullComparer<object>.Instance;
        }

        protected virtual IScorableFactory<IResolver, object> MakeFactory()
        {
            var comparer = MakeComparer();

            IScorableFactory<IResolver, object> factory = new OrderScorableFactory<IResolver, object>
                (
                    this.OnStage,
                    comparer,
                    new LuisIntentScorableFactory(MakeService),
                    new RegexMatchScorableFactory(MakeRegex),
                    new MethodScorableFactory()
                );

            return factory;
        }

        protected virtual BindingFlags MakeBindingFlags()
        {
            return BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        }

        protected virtual Type MakeType()
        {
            return this.GetType();
        }

        protected virtual IEnumerable<MethodInfo> MakeMethods()
        {
            var flags = this.MakeBindingFlags();
            var type = this.MakeType();
            var methods = type.GetMethods(flags);
            return methods;
        }

        protected virtual IScorable<IResolver, object> MakeScorable()
        {
            var factory = MakeFactory();
            var methods = MakeMethods();
            var scorable = factory.ScorableFor(methods);
            return scorable;
        }

        protected virtual async Task OnPostAsync()
        {
        }

        protected virtual async Task OnFailAsync()
        {
        }

        async Task<bool> IDispatcher.TryPostAsync(CancellationToken token)
        {
            var scorable = MakeScorable();
            var resolver = MakeResolver();

            if (await scorable.TryPostAsync(resolver, token))
            {
                await OnPostAsync();
                return true;
            }
            else
            {
                await OnFailAsync();
                return false;
            }
        }
    }
}
