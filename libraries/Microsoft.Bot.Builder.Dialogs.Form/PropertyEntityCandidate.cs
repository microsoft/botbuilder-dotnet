// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.Bot.Builder.Dialogs.Form.Events
{
    // Proposed mapping
    public class PropertyEntityCandidate
    {
        public PropertySchema Property { get; set; }

        public EntityInfo Entity { get; set; }

        public bool Expected { get; set; }

        public override string ToString()
        {
            var expected = Expected ? "expected" : string.Empty;
            return $"{expected} {Property} = {Entity.Name}";
        }
    }
}
