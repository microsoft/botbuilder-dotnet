using Microsoft.Bot.Connector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Prague
{
    public class DialogState
    {
        public string Name { get; set; }
    }

    public interface IDialogContext : IBotContext
    {
        Boolean IsActiveDialog { get; set; }
    }

    public class DialogContext : BotContext, IDialogContext
    {
        public DialogContext(Bot bot, Activity request) : base (bot, request)
        {
        }
        public bool IsActiveDialog { get; set; }
    }
  
    public class Dialog
    {
        private static IDictionary<string, Dialog> _dialogRegistry = new ConcurrentDictionary<string, Dialog>();        
        private IRouter _router;
        private string _dialogName; 

        public Dialog(string name, IRouter router)
        {
            _dialogName = name ?? throw new ArgumentNullException("name");                
            _router = router ?? throw new ArgumentOutOfRangeException("router");
            AddDialogToRegistry(this);            
        }
             
        public Dialog(string name, IHandler handler) : this(name, new SimpleRouter(handler))
        {
        }

        public Dialog(IRouter router)
        {
            _dialogName = this.GetType().FullName;
            _router = router;
            AddDialogToRegistry(this);
        }

        public Dialog(IHandler handler) : this (new SimpleRouter(handler))
        {
        }

        public string Name { get { return _dialogName; } }
        public IRouter Router {  get { return _router; } }
 
        private static void AddDialogToRegistry(Dialog d)
        {
            lock (_dialogRegistry)
            {
                if (_dialogRegistry.ContainsKey(d.Name))
                    throw new ArgumentException($"Dialog.constructor(): a dialog named {d.Name} already exists.");

                _dialogRegistry[d.Name] = d;
            }
        }

        public static Dialog FindDialog(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            lock(_dialogRegistry)
            {
                if (_dialogRegistry.ContainsKey(name))
                    return _dialogRegistry[name];
                else
                    return null;
            }
        }

        public static void ResetDialogs()
        {
            lock (_dialogRegistry)
            {
                _dialogRegistry.Clear();
            }
        }
        public static IRouter IfActiveDialog(IHandler ifRouter, IHandler elseRouter)
        {
            return IfActiveDialog(
                new SimpleRouter(ifRouter),
                new SimpleRouter(elseRouter));
        }

        public static IRouter IfActiveDialog(IRouter ifRouter, IRouter elseRouter)            
        {
            IfMatch ifMatch = new IfMatch(
                (context) => {
                    if (context is IDialogContext)
                        return ((IDialogContext)context).IsActiveDialog;
                    else
                        return false;
                    },
                ifRouter,
                elseRouter);

            return ifMatch;
        }
    }
}
