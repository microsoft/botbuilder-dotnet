// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Bridge
{
    public abstract class Options : DialogOptions
    {
        public static Options<T> From<T>(IDialog<T> dialog)
        {
            return new Options<T>(dialog);
        }

        public abstract Task StartAsync(IDialogContext context);
        public abstract object Dialog { get; }
    }

    public sealed class Options<T> : Options
    {
        public IDialog<T> dialog;
        public Options(IDialog<T> dialog)
        {
            this.dialog = dialog;
        }
        public override object Dialog => this.dialog;

        public override async Task StartAsync(IDialogContext context)
        {
            await this.dialog.StartAsync(context);
        }
    }
}
