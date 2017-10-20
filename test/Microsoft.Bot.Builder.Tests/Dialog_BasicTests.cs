using Microsoft.Bot.Builder.Prague;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using static Microsoft.Bot.Builder.Prague.Routers;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Dialog")]
    public class Dialog_BasicTests
    {
        [TestMethod]
        public void Dialog_ContextActiveTest()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);
            Activity a = new Activity();

            IDialogContext dc = new DialogContext(bot, a);
            Assert.IsTrue(dc is IBotContext, "Subclassing isn't correct on IDialogContext");
            Assert.IsTrue(dc is BotContext, "Subclassing isn't correct on DialogContext");
        }

        [TestMethod]
        public void Dialog_Registry_Find()
        {
            //Make sure the registry is empty
            Assert.IsNull(Dialog.FindDialog("dialog"), "Incorrectly found a dialog");

            // Create a dialog, and make sure it's found in the registry
            Dialog d = new Dialog("dialog", Simple ( () => { }));
            Dialog found = Dialog.FindDialog("dialog");
            Assert.IsTrue(found.Name == "dialog");

            // Make sure the registry clears properly
            Dialog.ResetDialogs();
            Assert.IsNull(Dialog.FindDialog("dialog"), "Incorrectly found a dialog");
        }

        [TestMethod]
        public async Task Dialog_RouterTest()
        {
            bool wasCalled = false;
            Handler h = Simple(() => wasCalled = true);
            Dialog d = new Dialog("name", h);

            // Trigger the action
            RouterOrHandler rh = Dialog.FindDialog("name").RouterOrHandler;
            var route = await Router.ToRouter(rh).GetRoute(null);

            await route.Action();

            Assert.IsTrue(wasCalled == true, "Dialog didn't find the action");
        }

        [TestMethod]
        public async Task Dialog_IfActiveDialogTrue()
        {
            bool ifHandled = false;
            bool elseHandled = false;

            Handler ifHandler = Simple ( () => ifHandled = true);
            Handler elseHandler = Simple ( () => elseHandled = true);

            RouterOrHandler rh = Dialog.IfActiveDialog(ifHandler, elseHandler);
            IDialogContext dc = TestUtilities.CreateEmptyContext<IDialogContext>();
            dc.IsActiveDialog = true;

            var route = await Router.ToRouter(rh).GetRoute(dc);
            await route.Action();

            // Make sure the route we just ran did actually run
            Assert.IsTrue(ifHandled == true);

            // Make sure the 'else' did not run. 
            Assert.IsTrue(elseHandled == false);
        }

        [TestMethod]
        public async Task Dialog_IfActiveDialogFalse()
        {
            bool ifHandled = false;
            bool elseHandled = false;

            Handler ifHandler = Simple (() => ifHandled = true);
            Handler elseHandler = Simple (() => elseHandled = true);

            RouterOrHandler rh = Dialog.IfActiveDialog(ifHandler, elseHandler);
            Assert.IsNotNull(rh);

            IDialogContext dc = TestUtilities.CreateEmptyContext<IDialogContext>();
            dc.IsActiveDialog = false;

            var route = await Router.ToRouter(rh).GetRoute(dc);
            await route.Action();

            // Make sure the "if" branch did not run.
            Assert.IsTrue(ifHandled == false);

            // Make sure the 'else' did run. 
            Assert.IsTrue(elseHandled == true);
        }
    }
}
