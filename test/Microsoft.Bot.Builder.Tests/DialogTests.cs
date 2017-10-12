using Microsoft.Bot.Builder.Prague;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class DialogTests
    {
        [TestMethod]
        [TestCategory("Dialog")]
        public void DialogContextActiveTest()
        {
            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector);
            Activity a = new Activity();

            IDialogContext dc = new DialogContext(bot, a);
            Assert.IsTrue(dc is IBotContext, "Subclassing isn't correct on IDialogContext");
            Assert.IsTrue(dc is BotContext, "Subclassing isn't correct on DialogContext");
        }

        [TestMethod]
        [TestCategory("Dialog")]
        public void DialogRegistry_Find()
        {   
            //Make sure the registry is empty
            Assert.IsNull(Dialog.FindDialog("dialog"), "Incorrectly found a dialog");

            // Create a dialog, and make sure it's found in the registry
            Dialog d = new Dialog("dialog", new SimpleHandler(async () => { }));
            Dialog found = Dialog.FindDialog("dialog");
            Assert.IsTrue(found.Name == "dialog");

            // Make sure the registry clears properly
            Dialog.ResetDialogs();
            Assert.IsNull(Dialog.FindDialog("dialog"), "Incorrectly found a dialog");
        }

        [TestMethod]
        [TestCategory("Dialog")]
        public async Task Dialog_RouterTest()
        {
            bool wasCalled = false;
            IHandler h = new SimpleHandler(async () => wasCalled = true );
            Dialog d = new Dialog("name", h);

            // Trigger the action
            var route = await Dialog.FindDialog("name").RouterOrHandler.AsRouter().GetRoute(null);

            await route.Action();

            Assert.IsTrue(wasCalled == true, "Dialog didn't find the action");            
        }

        [TestMethod]
        [TestCategory("Dialog")]
        public async Task Dialog_IfActiveDialogTrue()
        {
            bool ifHandled = false;
            bool elseHandled = false;

            SimpleHandler ifHandler = new SimpleHandler(() => ifHandled = true);
            SimpleHandler elseHandler = new SimpleHandler(() => elseHandled = true);

            IRouterOrHandler rh = Dialog.IfActiveDialog(ifHandler, elseHandler);
            IDialogContext dc = TestUtilities.CreateEmptyContext<IDialogContext>();
            dc.IsActiveDialog = true;

            var route = await rh.AsRouter().GetRoute(dc);
            await route.Action();

            // Make sure the route we just ran did actually run
            Assert.IsTrue(ifHandled == true);

            // Make sure the 'else' did not run. 
            Assert.IsTrue(elseHandled == false);
        }

        [TestMethod]
        [TestCategory("Dialog")]
        public async Task Dialog_IfActiveDialogFalse()
        {
            bool ifHandled = false;
            bool elseHandled = false;

            SimpleHandler ifHandler = new SimpleHandler(() => ifHandled = true);
            SimpleHandler elseHandler = new SimpleHandler(() => elseHandled = true);

            IRouterOrHandler rh = Dialog.IfActiveDialog(ifHandler, elseHandler);
            IDialogContext dc = TestUtilities.CreateEmptyContext<IDialogContext>();
            dc.IsActiveDialog = false;

            var route = await rh.AsRouter().GetRoute(dc);
            await route.Action();

            // Make sure the "if" branch did not run.
            Assert.IsTrue(ifHandled == false);

            // Make sure the 'else' did run. 
            Assert.IsTrue(elseHandled == true);
        }
    }
}
