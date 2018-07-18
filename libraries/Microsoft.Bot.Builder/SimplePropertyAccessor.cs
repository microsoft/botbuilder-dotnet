// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    public class SimplePropertyAccessor<T> : IPropertyAccessor<T>
    {
        private IPropertyContainer _propertyContainer;
        private Func<T> _defaultValueFactory;

        public SimplePropertyAccessor(IPropertyContainer propertyContainer, string name, Func<T> defaultValueFactory)
        {
            this._propertyContainer = propertyContainer;
            this.Name = name;
            if (defaultValueFactory == null)
            {
                this._defaultValueFactory = () => default(T);
            }
            else
            {
                this._defaultValueFactory = defaultValueFactory;
            }
        }

        public string Name { get; private set; }

        public Task DeleteAsync(ITurnContext turnContext)
        {
            return this._propertyContainer.DeletePropertyAsync(turnContext, this.Name);
        }

        public async Task<T> GetAsync(ITurnContext turnContext)
        {
            T result = await this._propertyContainer.GetPropertyAsync<T>(turnContext, this.Name).ConfigureAwait(false);
            if (result == null)
            {
                result = _defaultValueFactory();

                // save default value for any further calls
                await this.SetAsync(turnContext, result).ConfigureAwait(false);
            }

            return result;
        }

        public Task SetAsync(ITurnContext turnContext, T value)
        {
            return this._propertyContainer.SetPropertyAsync(turnContext, this.Name, value);
        }
    }
}
