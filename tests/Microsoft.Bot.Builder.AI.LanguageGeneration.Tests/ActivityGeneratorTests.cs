using System;
using System.Collections.Generic;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class ActivityGeneratorTests
    {
        private string GetExampleFilePath(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "Examples\\" + fileName;
        }


        [TestMethod]
        public void TestBasicActivity()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("BasicActivity.lg"));
            var activityGenerator = new ActivityGenerator(engine);
            var options = new ActivityGenerationConfig();
            options.TextSpeakTemplateId = "RecentTasks";
            var activity = activityGenerator.Generate(options, new { recentTasks = new[] { "Task1" } });
            Assert.AreEqual("Your most recent task is Task1. You can let me know if you want to add or complete a task.", activity.Text);
            Assert.AreEqual("Your most recent task is Task1. You can let me know.", activity.Speak);

            // Test whitespace.
            // Only the last whitespace before separtor will be removed.
            // That means you need to type two whitespace if you want to keep one whitespace at the end of your Text string.
            // All whitespace after separtor(before speak string) will be removed.
            activity = activityGenerator.Generate(options, new { recentTasks = new[] { "Task1", "Task2" } });
            Assert.AreEqual("Your most recent tasks are Task1 and Task2. You can let me know if you want to add or complete a task.  ", activity.Text);
            Assert.AreEqual("Your most recent tasks are Task1 and Task2. You can let me know. ", activity.Speak);

            // Use "&&" as separtor
            options.TextSpeakSeperator = "&&";
            activity = activityGenerator.Generate(options, new { recentTasks = new[] { "Task1", "Task2", "Task3" } });
            Assert.AreEqual("Your most recent tasks are Task1, Task2 and Task3. You can let me know if you want to add or complete a task.", activity.Text);
            Assert.AreEqual("Your most recent tasks are Task1, Task2 and Task3. You can let me know.", activity.Speak);
        }

        [TestMethod]
        public void TestAdaptiveCardActivity()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("AdaptiveCardActivity.lg"));
            var activityGenerator = new ActivityGenerator(engine);
            var options = new ActivityGenerationConfig();
            options.Attachments = new List<AttachmentGenerationConfig>();
            options.Attachments.Add(new AttachmentGenerationConfig() { AttachementTemplateId = "adaptiveCardTemplate", IsAdaptiveCard = true });
            var activity = activityGenerator.Generate(options, new { adaptiveCardTitle = "This is adaptive card title" });
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(AdaptiveCard.ContentType, activity.Attachments[0].ContentType);
            var card = activity.Attachments[0].Content;
            var adaptiveCard = JsonConvert.DeserializeObject<AdaptiveCard>(JsonConvert.SerializeObject(card));
            var textBlock = adaptiveCard.Body[0] as AdaptiveTextBlock;
            Assert.AreEqual("This is adaptive card title", textBlock.Text);
        }

        [TestMethod]
        public void TestNonAdaptiveCardActivity()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("NonAdaptiveCardActivity.lg"));
            var activityGenerator = new ActivityGenerator(engine);
            var options = new ActivityGenerationConfig();
            options.Attachments = new List<AttachmentGenerationConfig>();
            options.Attachments.Add(new AttachmentGenerationConfig() { AttachementTemplateId = "HeroCardTemplate" });
            options.Attachments.Add(new AttachmentGenerationConfig() { AttachementTemplateId = "AnimationCardTemplate" });
            var activity = activityGenerator.Generate(options, null);
            Assert.AreEqual(2, activity.Attachments.Count);
            Assert.AreEqual(AttachmentLayoutTypes.Carousel, activity.AttachmentLayout);

            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = activity.Attachments[0].Content;
            var heroCard = JsonConvert.DeserializeObject<HeroCard>(JsonConvert.SerializeObject(card));
            Assert.AreEqual("Cheese gromit!", heroCard.Title);
            Assert.AreEqual("Hero Card", heroCard.Subtitle);
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", heroCard.Text);
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", heroCard.Images[0].Url);
            Assert.AreEqual("Option 3", heroCard.Buttons[2].Title);

            Assert.AreEqual(AnimationCard.ContentType, activity.Attachments[1].ContentType);
            card = activity.Attachments[1].Content;
            var animationCard = JsonConvert.DeserializeObject<AnimationCard>(JsonConvert.SerializeObject(card));
            Assert.AreEqual("Animation Card", animationCard.Title);
            Assert.AreEqual(true, animationCard.Autoloop);
            Assert.AreEqual(true, animationCard.Autostart);
            Assert.AreEqual("https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png", animationCard.Image.Url);
            Assert.AreEqual("http://oi42.tinypic.com/1rchlx.jpg", animationCard.Media[0].Url);
        }
    }
}
