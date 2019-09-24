namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public partial class FormDialog
    {
        public class EntityInfo
        {
            public string Name { get; set; }

            public object Entity { get; set; }

            public int Start { get; set; }

            public int End { get; set; }

            public double Score { get; set; }

            public string Text { get; set; }

            public string Role { get; set; }

            public string Type { get; set; }

            public int Priority { get; set; }

            public double Coverage { get; set; }

            public int Turn { get; set; }

            public bool Overlaps(EntityInfo entity)
                => Start <= entity.End && End >= entity.Start;

            public override string ToString()
                => $"{Name}:{Entity} P{Priority} {Score} {Coverage}";
        }
    }
}
