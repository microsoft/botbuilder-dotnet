// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    public class EntityInfo
    {
        public string Name { get; set; }

        public object Value { get; set; }

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
            => $"{Name}:{Value} P{Priority} {Score} {Coverage}";
    }
}
