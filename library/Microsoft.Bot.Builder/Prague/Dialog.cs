using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Adapters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using static Microsoft.Bot.Builder.Prague.RoutingRules;


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
        private static IDictionary<string, Dialog> _dialogRegistry = new Dictionary<string, Dialog>();        
        private RouterOrHandler _routerOrHandler;
        private string _dialogName; 

        public Dialog(string name, RouterOrHandler routerOrHandler)
        {
            _dialogName = name ?? throw new ArgumentNullException(nameof(name));
            _routerOrHandler = routerOrHandler ?? throw new ArgumentOutOfRangeException(nameof(routerOrHandler)); 
            AddDialogToRegistry(this);            
        }
             
        public Dialog(RouterOrHandler routerOrHandler)
        {
            _dialogName = this.GetType().FullName;
            _routerOrHandler = routerOrHandler;
            AddDialogToRegistry(this);
        }        

        public string Name { get { return _dialogName; } }
        public RouterOrHandler RouterOrHandler {  get { return _routerOrHandler; } }
 
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
                throw new ArgumentNullException(nameof(name));

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
       
        public static RouterOrHandler IfActiveDialog(RouterOrHandler ifRouterOrHandler, RouterOrHandler elseRouterOrHandler)            
        {
            Router router = IfTrue(            
                async (context) => {
                    if (context is IDialogContext)
                        return ((IDialogContext)context).IsActiveDialog;
                    else
                        return false;
                    },
                ifRouterOrHandler,
                elseRouterOrHandler);

            return router;
        }
    }
}
