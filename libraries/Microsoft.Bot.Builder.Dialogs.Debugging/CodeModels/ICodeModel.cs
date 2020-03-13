// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface ICodeModel
    {
        string NameFor(object item);

        IReadOnlyList<ICodePoint> PointsFor(DialogContext dialogContext, object item, string more);
    }

    public interface ICodePoint
    {
        object Item { get; }

        string More { get; }

        string Name { get; }

        object Data { get; }

        object Evaluate(string expression);
    }

    public sealed class CodeModel : ICodeModel
    {
        string ICodeModel.NameFor(object item)
        {
            if (item is Dialog dialog)
            {
                return dialog.Id;
            }

            if (item is IItemIdentity identity)
            {
                return identity.GetIdentity();
            }

            var type = item.GetType().Name;
            return type;
        }

        IReadOnlyList<ICodePoint> ICodeModel.PointsFor(DialogContext dialogContext, object item, string more)
        {
            var frames = new List<CodePoint>();

            if (item != null)
            {
                var frame = new CodePoint(this, dialogContext, item, more);
                frames.Add(frame);
            }

            while (dialogContext != null)
            {
                foreach (var instance in dialogContext.Stack)
                {
                    var dialog = dialogContext.FindDialog(instance.Id);
                    var frame = new CodePoint(this, dialogContext, dialog, null);
                    frames.Add(frame);
                }

                dialogContext = dialogContext.Parent;
            }

            return frames;
        }
    }
}
