using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    public class SimplePropertyAccessor<T> : IPropertyAccessor<T>
    {
        private IPropertyContainer propertyContainer;
        private T defaultValue;

        public SimplePropertyAccessor(IPropertyContainer propertyContainer, string name, T defaultValue = default(T))
        {
            this.propertyContainer = propertyContainer;
            this.Name = name;
            this.defaultValue = defaultValue;
        }

        public string Name { get; private set; }

        public Task DeleteAsync(ITurnContext turnContext)
        {
            return this.propertyContainer.DeletePropertyAsync(turnContext, this.Name);
        }

        public async Task<T> GetAsync(ITurnContext turnContext)
        {
            T result = await this.propertyContainer.GetPropertyAsync<T>(turnContext, this.Name);
            if (result == null)
            {
                // assign default value
                if (typeof(T).IsValueType)
                    result = defaultValue;
                else if (typeof(T) == typeof(string))
                    result = defaultValue;
                else if (defaultValue != null && defaultValue is ICloneable)
                    result = (T)((ICloneable)defaultValue).Clone();
                else
                {
                    try
                    {
                        result = Activator.CreateInstance<T>();
                    }
                    catch
                    {
                        result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(defaultValue));
                    }
                }

                // save default value for any further calls
                await this.SetAsync(turnContext, result);
            }
            return result;
        }

        public Task SetAsync(ITurnContext turnContext, T value)
        {
            return this.propertyContainer.SetPropertyAsync(turnContext, this.Name, value);
        }
    }
}
