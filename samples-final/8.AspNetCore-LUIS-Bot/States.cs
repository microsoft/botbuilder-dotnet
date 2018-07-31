// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspNetCore_LUIS_Bot
{
    internal static class ConversationStateProperty
    {
        internal static string DialogState = $"{nameof(ConversationStateProperty)}.DialogState";
    }

    //public class BaseState : Dictionary<string, object>
    //{
    //    public BaseState(IDictionary<string, object> source)
    //    {
    //        if (source != null)
    //        {
    //            source.ToList().ForEach(x => this.Add(x.Key, x.Value));
    //        }
    //    }

    //    protected T GetProperty<T>([CallerMemberName]string propName = null)
    //    {
    //        if (this.TryGetValue(propName, out object value))
    //        {
    //            return (T)value;
    //        }
    //        return default(T);
    //    }

    //    protected void SetProperty(object value, [CallerMemberName]string propName = null)
    //    {
    //        this[propName] = value;
    //    }
    //}

    public class Reminder
    {               
        public string Title { get; set; }
    }
    
    //public class UserState : BaseState
    //{
    //    public UserState() : base(null) { }

    //    public UserState(IDictionary<string, object> source) : base(source) { }

    //    public IList<Reminder> Reminders
    //    {
    //        get { return GetProperty<IList<Reminder>>(); }
    //        set { SetProperty(value); }
    //    }
    //}
}
