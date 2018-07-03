// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspNetCore_LUIS_Bot
{
    public class BaseState : Dictionary<string, object>
    {
        public BaseState(IDictionary<string, object> source)
        {
            if (source != null)
            {
                source.ToList().ForEach(x => this.Add(x.Key, x.Value));
            }
        }

        protected T GetProperty<T>([CallerMemberName]string propName = null)
        {
            if (this.TryGetValue(propName, out object value))
            {
                return (T)value;
            }
            return default(T);
        }

        protected void SetProperty(object value, [CallerMemberName]string propName = null)
        {
            this[propName] = value;
        }
    }

    public class Reminder : BaseState
    {
        public Reminder() : base(null) { }

        public Reminder(IDictionary<string, object> source = null) : base(source) { }

        public string Title
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }

    public class UserState : BaseState
    {
        public UserState() : base(null) { }

        public UserState(IDictionary<string, object> source) : base(source) { }

        public IList<Reminder> Reminders
        {
            get { return GetProperty<IList<Reminder>>(); }
            set { SetProperty(value); }
        }
    }
}
