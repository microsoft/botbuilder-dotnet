// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Scorables;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    /// <summary>
    /// A fluent, chainable interface for IDialogs.
    /// </summary>
    public static partial class Chain
    {
        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, execute this continuation method to construct the next <see cref="IDialog{R}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="R">The type of the next dialog.</typeparam>
        /// <param name="context">The bot context.</param>
        /// <param name="item">The result of the previous <see cref="IDialog{T}"/>.</param>
        /// <returns>A task that represents the next <see cref="IDialog{R}"/>.</returns>
        public delegate Task<IDialog<R>> Continuation<in T, R>(IBotContext context, IAwaitable<T> item);

        /// <summary>
        /// Construct a <see cref="IDialog{T}"/> that will make a new copy of another <see cref="IDialog{T}"/> when started.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <param name="MakeDialog">The dialog factory method.</param>
        /// <returns>The new dialog.</returns>
        public static IDialog<T> From<T>(Func<IDialog<T>> MakeDialog)
        {
            return new FromDialog<T>(MakeDialog);
        }

        /// <summary>
        /// Execute a side-effect after a <see cref="IDialog{T}"/> completes.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <param name="callback">The callback method.</param>
        /// <returns>The antecedent dialog.</returns>
        public static IDialog<T> Do<T>(this IDialog<T> antecedent, Func<IBotContext, IAwaitable<T>, Task> callback)
        {
            return new DoDialog<T>(antecedent, callback);
        }

        /// <summary>
        /// Execute an action after the <see cref="IDialog{T}"/> completes. 
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <typeparam name="R">They type returned by action.</typeparam>
        /// <param name="Antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <param name="Action">The action that will be called after the antecedent dialog completes.</param>
        /// <returns>The antecedent dialog.</returns>
        public static IDialog<R> Then<T, R>(this IDialog<T> Antecedent, Func<IBotContext, IAwaitable<T>, Task<R>> Action)
        {
            return new ThenDialog<T, R>(Antecedent, Action);
        }

        /// <summary>
        /// Post to the user the result of a <see cref="IDialog{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <returns>The antecedent dialog.</returns>
        public static IDialog<T> PostToUser<T>(this IDialog<T> antecedent)
        {
            return new PostToUserDialog<T>(antecedent);
        }

        /// <summary>
        /// Post to the chain the message to the bot after the antecedent completes.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <returns>The dialog representing the message sent to the bot.</returns>
        public static IDialog<IMessageActivity> WaitToBot<T>(this IDialog<T> antecedent)
        {
            return new WaitToBotDialog<T>(antecedent);
        }

        /// <summary>
        /// Post the message from the user to Chain.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="IDialog{T}"/> can be used as the root dialog for a chain.
        /// </remarks>
        /// <returns> The dialog that dispatches the incoming message from the user to chain.</returns>
        public static IDialog<IMessageActivity> PostToChain()
        {
            return Chain.Return(string.Empty).WaitToBot();
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, post the item to the event queue.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="E">The type of the event.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <param name="event">The event.</param>
        /// <returns>The result from the antecedent <see cref="IDialog{T}"/>.</returns>
        public static IDialog<T> PostEvent<T, E>(this IDialog<T> antecedent, E @event)
        {
            return new PostEventDialog<T, E>(antecedent, @event);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, execute the continuation to produce the next <see cref="IDialog{R}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="R">The type of the next dialog.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <param name="continuation">The continuation to produce the next <see cref="IDialog{R}"/>.</param>
        /// <returns>The next <see cref="IDialog{R}"/>.</returns>
        public static IDialog<R> ContinueWith<T, R>(this IDialog<T> antecedent, Continuation<T, R> continuation)
        {
            return new ContinueWithDialog<T, R>(antecedent, continuation);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, project the result into a new <see cref="IDialog{R}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="R">The type of the projected dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="selector">The projection function from <typeparamref name="T"/> to <typeparamref name="R"/>.</param>
        /// <returns>The result <see cref="IDialog{R}"/>.</returns>
        public static IDialog<R> Select<T, R>(this IDialog<T> antecedent, Func<T, R> selector)
        {
            return new SelectDialog<T, R>(antecedent, selector);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, evaluate the predicate and decide whether to continue.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="predicate">The predicate to decide whether to continue the chain.</param>
        /// <returns>The result from the antecedent <see cref="IDialog{T}"/> or its cancellation, wrapped in a <see cref="IDialog{T}"/>.</returns>
        public static IDialog<T> Where<T>(this IDialog<T> antecedent, Func<T, bool> predicate)
        {
            return new WhereDialog<T>(antecedent, predicate);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> where T is <see cref="IDialog{T}"/> completes, unwrap the result into a new <see cref="IDialog{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/> where T is <see cref="IDialog{T}"/>.</param>
        /// <returns>An <see cref="IDialog{T}"/>.</returns>
        public static IDialog<T> Unwrap<T>(this IDialog<IDialog<T>> antecedent)
        {
            return new UnwrapDialog<T>(antecedent);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, execute the next <see cref="IDialog{C}"/>, and use the projection to combine the results.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="C">The type of the intermediate dialog.</typeparam>
        /// <typeparam name="R">The type of the projected dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="function">The factory method to create the next dialog <see cref="IDialog{C}"/>.</param>
        /// <param name="projection">The projection function for the combination of the two dialogs.</param>
        /// <returns>The result <see cref="IDialog{R}"/>.</returns>
        public static IDialog<R> SelectMany<T, C, R>(this IDialog<T> antecedent, Func<T, IDialog<C>> function, Func<T, C, R> projection)
        {
            return new SelectManyDialog<T, C, R>(antecedent, function, projection);
        }

        /// <summary>
        /// Loop the <see cref="IDialog{T}"/> forever.
        /// </summary>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <returns>The looping dialog.</returns>
        public static IDialog<T> Loop<T>(this IDialog<T> antecedent)
        {
            return new LoopDialog<T>(antecedent);
        }

        /// <summary>
        /// Call the voided <see cref="IDialog{T}"/>, ignore the result, then restart the original dialog wait.
        /// </summary>
        /// <remarks>
        /// The purpose of this method is to wrap an antecedent dialog A with a new dialog D to push on the stack
        /// on top of the existing stack top dialog L.
        /// 1. D will call A.
        /// 2. D will receive the value of A when A is done.
        /// 3. D will re-initiate the typed wait (often for a message) for which a method of L was waiting
        /// 4. D will receive that value of the re-initiated typed wait.
        /// 5. D will return that value of the typed wait to L.
        /// This depends on the symmetry of IDialogStack.Done and IDialogStack.Wait in how they satisfy typed waits.
        /// </remarks>
        /// <typeparam name="T">The type of the voided dialog.</typeparam>
        /// <typeparam name="R">The type of the original dialog wait.</typeparam>
        /// <param name="antecedent">The voided dialog.</param>
        /// <returns>The dialog that produces the item to satisfy the original wait.</returns>
        public static IDialog<R> Void<T, R>(this IDialog<T> antecedent)
        {
            return new VoidDialog<T, R>(antecedent);
        }

        /// <summary>
        /// Call the voided <see cref="IDialog{T}"/>, ignore the result, then restart the original dialog wait.
        /// </summary>
        /// <remarks>
        /// (value types don't support generic parameter variance - so this reflection-based method may not work)
        /// It's okay to loose type information (i.e. IDialog{object}) because voided dialogs are called with a null 
        /// <see cref="ResumeAfter{T}"/> because they are hacking the stack to satisfy the wait of the interrupted dialog. 
        /// </remarks>
        /// <typeparam name="T">The type of the voided dialog.</typeparam>
        /// <param name="antecedent">The voided dialog.</param>
        /// <param name="stack">The dialog stack.</param>
        /// <returns>The dialog that produces the item to satisfy the original wait.</returns>
        public static IDialog<object> Void<T>(this IDialog<T> antecedent, IDialogStack stack)
        {
            var frames = stack.Frames;
            if (frames.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty");
            }

            var frame = stack.Frames[0];
            var restType = frame.GetType();
            bool valid = restType.IsGenericType && restType.GetGenericTypeDefinition() == typeof(ResumeAfter<>);
            if (valid)
            {
                var waitType = restType.GetGenericArguments()[0];
                var voidType = typeof(VoidDialog<,>).MakeGenericType(typeof(T), waitType);
                var instance = Activator.CreateInstance(voidType, antecedent);
                var dialog = (IDialog<object>)instance;
                return dialog;
            }
            else
            {
                throw new ArgumentOutOfRangeException(restType.Name);
            }
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, catch and handle any exceptions of type <typeparamref name="E"/>.
        /// </summary>
        /// <typeparam name="T">The type returned by the antecedent dialog.</typeparam>
        /// <typeparam name="E">The type of exception to catch and handle.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="block">The lambda expression representing the catch block handler.</param>
        /// <returns>The result of the catch block handler if there is an exception of type <typeparamref name="E"/>.</returns>
        public static IDialog<T> Catch<T, E>(this IDialog<T> antecedent, Func<IDialog<T>, E, IDialog<T>> block) where E: Exception
        {
            return new CatchDialog<T, E>(antecedent, block);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, catch and handle any exceptions.
        /// </summary>
        /// <typeparam name="T">The type returned by the antecedent dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="block">The lambda expression representing the catch block handler.</param>
        /// <returns>The result of the catch block handler if there is an exception.</returns>
        public static IDialog<T> Catch<T>(this IDialog<T> antecedent, Func<IDialog<T>, Exception, IDialog<T>> block)
        {
            return new CatchDialog<T, Exception>(antecedent, block);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, stop the propagation of an exception of <typeparamref name="E"/>.
        /// </summary>
        /// <typeparam name="T">The type returned by the antecedent dialog.</typeparam>
        /// <typeparam name="E">The type of exception to swallow.</typeparam>
        /// <param name="antecedent"> The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <returns>The default value of <typeparamref name="T"/> if there is an exception of type <typeparamref name="E"/>.</returns>
        public static IDialog<T> DefaultIfException<T, E>(this IDialog<T> antecedent) where E : Exception
        {
            return new DefaultIfExceptionDialog<T, E>(antecedent);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, stop the propagation of Exception.
        /// </summary>
        /// <typeparam name="T">The type returned by the antecedent dialog.</typeparam>
        /// <param name="antecedent"> The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <returns>The default value of <typeparamref name="T"/> if there is an Exception.</returns>
        public static IDialog<T> DefaultIfException<T>(this IDialog<T> antecedent)
        {
            return new DefaultIfExceptionDialog<T, Exception>(antecedent);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, go through each <see cref="ICase{T, R}"/> 
        /// and run the <see cref="ContextualSelector{T, R}"/>" of the first <see cref="ICase{T, R}"/> that 
        /// the returned value by the antecedent dialog satisfies.
        /// </summary>
        /// <typeparam name="T"> The type of the antecedent dialog.</typeparam>
        /// <typeparam name="R"> The type of the Dialog returned by <see cref="ContextualSelector{T, R}"/></typeparam>
        /// <param name="antecedent"> The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="cases"> Cases for the switch</param>
        /// <returns>The result <see cref="IDialog{R}"/>.</returns>
        public static IDialog<R> Switch<T, R>(this IDialog<T> antecedent, params ICase<T, R>[] cases)
        {
            return new SwitchDialog<T, R>(antecedent, cases);
        }

        /// <summary>
        /// Creates a <see cref="IDialog{T}"/> that returns a value.
        /// </summary>
        /// <remarks>
        /// The type of the value should be serializable.
        /// </remarks>
        /// <typeparam name="T"> Type of the value.</typeparam>
        /// <param name="item"> The value to be wrapped.</param>
        /// <returns> The <see cref="IDialog{T}"/> that wraps the value.</returns>
        public static IDialog<T> Return<T>(T item)
        {
            return new ReturnDialog<T>(item);
        }

        /// <summary>
        /// Create a <see cref="IDialog{T}"/> that represents a while loop.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="initial">The value if <paramref name="test"/> is never true.</param>
        /// <param name="test">The test to enter the while loop <paramref name="body"/>.</param>
        /// <param name="body">The body of the while loop.</param>
        /// <returns>Zero or the last value returned by the <paramref name="body"/> of the while loop.</returns>
        public static IDialog<T> While<T>(this IDialog<T> initial, Func<T, IDialog<bool>> test, Func<T, IDialog<T>> body)
        {
            return new WhileDialog<T>(initial, test, body);
        }

        /// <summary>
        /// Fold items from an enumeration of dialogs.
        /// </summary>
        /// <typeparam name="T"> The type of the dialogs in the enumeration produced by the antecedent dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog that produces an enumeration of <see cref="IDialog{T}"/>.</param>
        /// <param name="folder">The accumulator for the dialog enumeration.</param>
        /// <returns>The accumulated result.</returns>
        public static IDialog<T> Fold<T>(this IDialog<IEnumerable<IDialog<T>>> antecedent, Func<T, T, T> folder)
        {
            return new FoldDialog<T>(antecedent, folder);
        }

        /// <summary>
        /// Decorate a dialog with a scorable, so that a scorable can participate on the dialog stack.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <typeparam name="Item">The type of the item scored by the scorable.</typeparam>
        /// <typeparam name="Score">The type of the scope produced by the scorable.</typeparam>
        /// <param name="antecedent">The antecedent dialog.</param>
        /// <param name="scorable">The scorable.</param>
        /// <returns>The dialog augmented with the scorables.</returns>
        public static IDialog<T> WithScorable<T, Item, Score>(this IDialog<T> antecedent, IScorable<Item, Score> scorable)
        {
            return new WithScorableDialog<T, Item, Score>(antecedent, scorable);
        }

        /// <summary>
        /// Constructs a case. 
        /// </summary>
        /// <typeparam name="T"> The type of incoming value to case.</typeparam>
        /// <typeparam name="R"> The type of the object returned by selector.</typeparam>
        /// <param name="condition"> The condition of the case.</param>
        /// <param name="selector"> The contextual selector of the case.</param>
        /// <returns></returns>
        public static ICase<T, R> Case<T, R>(Func<T, bool> condition, ContextualSelector<T, R> selector)
        {
            return new Case<T, R>(condition, selector);
        }

        /// <summary>
        /// Constructs a case based on a regular expression.
        /// </summary>
        /// <typeparam name="R"> The type of the object returned by selector.</typeparam>
        /// <param name="regex"> The regex for condition.</param>
        /// <param name="selector"> The contextual selector for the case.</param>
        /// <returns>The case.</returns>
        public static ICase<string, R> Case<R>(Regex regex, ContextualSelector<string, R> selector)
        {
            return new RegexCase<R>(regex, selector);
        }

        /// <summary>
        /// Constructs a case to use as the default.
        /// </summary>
        /// <typeparam name="T"> The type of incoming value to case.</typeparam>
        /// <typeparam name="R"> The type of the object returned by selector.</typeparam>
        /// <param name="selector"> The contextual selector of the case.</param>
        /// <returns>The case.</returns>
        public static ICase<T, R> Default<T, R>(ContextualSelector<T, R> selector)
        {
            return new DefaultCase<T, R>(selector);
        }

        [Serializable]
        private sealed class FromDialog<T> : IDialog<T>
        {
            public readonly Func<IDialog<T>> MakeDialog;
            public FromDialog(Func<IDialog<T>> MakeDialog)
            {
                SetField.NotNull(out this.MakeDialog, nameof(MakeDialog), MakeDialog);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                var dialog = this.MakeDialog();
                context.Call<T>(dialog, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                context.Done<T>(await result);
            }
        }

        [Serializable]
        private sealed class DoDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<IBotContext, IAwaitable<T>, Task> Action;
            public DoDialog(IDialog<T> antecedent, Func<IBotContext, IAwaitable<T>, Task> Action)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Action, nameof(Action), Action);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                await this.Action(context, result);
                context.Done<T>(await result);
            }
        }

        [Serializable]
        private sealed class ThenDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<IBotContext, IAwaitable<T>, Task<R>> Action;
            public ThenDialog(IDialog<T> antecedent, Func<IBotContext, IAwaitable<T>, Task<R>> Action)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Action, nameof(Action), Action);
            }

            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                context.Done<R>(await this.Action(context, result));
            }
        }

        [Serializable]
        private sealed class PostToUserDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public PostToUserDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                var item = await result;
                await context.PostAsync(item.ToString());
                context.Done<T>(item);
            }
        }

        [Serializable]
        private sealed class WaitToBotDialog<T> : IDialog<IMessageActivity>
        {
            public readonly IDialog<T> Antecedent;
            public WaitToBotDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            public async Task StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                var item = await result;
                context.Wait(MessageReceivedAsync);
            }
            public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
            {
                context.Done(await argument);
            }
        }

        [Serializable]
        private sealed class PostEventDialog<T, E> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public readonly E Event;

            public PostEventDialog(IDialog<T> antecedent, E @event)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                this.Event = @event;
            }

            public override string ToString()
            {
                return $"{this.GetType().Name}({this.Event})";
            }

            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, AfterAntecedent);
            }

            private T item;
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                this.item = await result;
                context.Post(this.Event, AfterEvent);
            }

            private async Task AfterEvent(IDialogContext context, IAwaitable<E> result)
            {
                var @event = await result;
                if (! object.ReferenceEquals(@event, this.Event))
                {
                    throw new InvalidOperationException();
                }

                context.Done(this.item);
            }
        }

        [Serializable]
        private sealed class ContinueWithDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Continuation<T, R> Continuation;

            public ContinueWithDialog(IDialog<T> antecedent, Continuation<T, R> continuation)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Continuation, nameof(continuation), continuation);
            }

            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }

            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                var next = await this.Continuation(context, result);
                context.Call<R>(next, DoneAsync);
            }

            private async Task DoneAsync(IDialogContext context, IAwaitable<R> result)
            {
                context.Done(await result);
            }
        }

        [Serializable]
        private sealed class SelectDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<T, R> Selector;
            public SelectDialog(IDialog<T> antecedent, Func<T, R> selector)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Selector, nameof(selector), selector);
            }
            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, AfterAntecedent);
            }
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                var itemR = this.Selector(itemT);
                context.Done(itemR);
            }
        }

        /// <summary>
        /// The exception that is thrown when the where is canceled.
        /// </summary>
        [Serializable]
        public sealed class WhereCanceledException : OperationCanceledException
        {
            /// <summary>
            /// Construct the exception.
            /// </summary>
            public WhereCanceledException()
            {
            }

            /// <summary>
            /// This is the serialization constructor.
            /// </summary>
            /// <param name="info">The serialization info.</param>
            /// <param name="context">The streaming context.</param>
            private WhereCanceledException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        [Serializable]
        private sealed class WhereDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<T, bool> Predicate;
            public WhereDialog(IDialog<T> antecedent, Func<T, bool> predicate)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Predicate, nameof(predicate), predicate);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, AfterAntecedent);
            }
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                var itemR = this.Predicate(itemT);
                if (itemR)
                {
                    context.Done(itemT);
                }
                else
                {
                    throw new WhereCanceledException();
                }
            }
        }

        [Serializable]
        private sealed class UnwrapDialog<T> : IDialog<T>
        {
            public readonly IDialog<IDialog<T>> Antecedent;
            public UnwrapDialog(IDialog<IDialog<T>> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<IDialog<T>>(this.Antecedent, AfterAntecedent);
            }
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<IDialog<T>> result)
            {
                var dialogT = await result;
                context.Call<T>(dialogT, AfterDialog);
            }
            private async Task AfterDialog(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                context.Done(itemT);
            }
        }

        // http://blogs.msdn.com/b/pfxteam/archive/2013/04/03/tasks-monads-and-linq.aspx
        [Serializable]
        private sealed class SelectManyDialog<T, C, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<T, IDialog<C>> Function;
            public readonly Func<T, C, R> Projection;
            public SelectManyDialog(IDialog<T> antecedent, Func<T, IDialog<C>> function, Func<T, C, R> projection)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Function, nameof(function), function);
                SetField.NotNull(out this.Projection, nameof(projection), projection);
            }
            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, AfterAntecedent);
            }
            private T itemT;
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                this.itemT = await result;
                var dialog = this.Function(this.itemT);
                context.Call<C>(dialog, AfterFunction);
            }
            private async Task AfterFunction(IDialogContext context, IAwaitable<C> result)
            {
                var itemC = await result;
                var itemR = this.Projection(itemT, itemC);
                context.Done(itemR);
            }
        }

        [Serializable]
        private sealed class LoopDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public LoopDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                await result;
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
        }

        [Serializable]
        private sealed class VoidDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public VoidDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                var ignore = await result;
                context.Wait<R>(ItemReceived);
            }
            private async Task ItemReceived(IDialogContext context, IAwaitable<R> result)
            {
                var item = await result;
                context.Done(item);
            }
        }

        [Serializable]
        private sealed class CatchDialog<T, E> : IDialog<T> where E : Exception
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<IDialog<T>, E, IDialog<T>> Block;
            public CatchDialog(IDialog<T> antecedent, Func<IDialog<T>, E, IDialog<T>> block)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Block, nameof(block), block);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                try
                {
                    context.Done(await result);
                }
                catch (E error)
                {
                    context.Call(this.Block(this.Antecedent, error), ResumeAsync);
                }
            }
        }

        [Serializable]
        private sealed class DefaultIfExceptionDialog<T, E> : IDialog<T> where E : Exception
        {
            public readonly IDialog<T> Antecedent;
            public DefaultIfExceptionDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                try
                {
                    context.Done(await result);
                }
                catch (E)
                {
                    context.Done(default(T));
                }
            }
        }

        [Serializable]
        private sealed class SwitchDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly IReadOnlyList<ICase<T, R>> Cases;
            public SwitchDialog(IDialog<T> antecedent, IReadOnlyList<ICase<T, R>> cases)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Cases, nameof(cases), cases);
            }

            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(Antecedent, AfterAntecedent);
            }

            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                R itemR = default(R);
                foreach (var condition in this.Cases)
                {
                    if (condition.Condition(itemT))
                    {
                        itemR = condition.Selector(context, itemT);
                        break;
                    }
                }
                context.Done(itemR);
            }
        }

        /// <summary>
        /// A Dialog that wraps a value of type T.
        /// </summary>
        /// <remarks>
        /// The type of the value should be serializable.
        /// </remarks>
        /// <typeparam name="T">The result type of the Dialog. </typeparam>
        [Serializable]
        private sealed class ReturnDialog<T> : IDialog<T>
        {
            public readonly T Value;

            public ReturnDialog(T value)
            {
                this.Value = value;
            }

            public async Task StartAsync(IDialogContext context)
            {
                context.Done(Value);
            }
        }

        [Serializable]
        private sealed class WhileDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Zero;
            public readonly Func<T, IDialog<bool>> Test;
            public readonly Func<T, IDialog<T>> Body;
            public WhileDialog(IDialog<T> zero, Func<T, IDialog<bool>> test, Func<T, IDialog<T>> body)
            {
                SetField.NotNull(out this.Zero, nameof(Zero), zero);
                SetField.NotNull(out this.Test, nameof(Test), test);
                SetField.NotNull(out this.Body, nameof(Body), body);
            }

            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call(this.Zero, this.AfterZeroOrBody);
            }

            private T item = default(T);
            private async Task AfterZeroOrBody(IDialogContext context, IAwaitable<T> result)
            {
                this.item = await result;
                var test = this.Test(this.item);
                context.Call(test, this.AfterTest);
            }

            private async Task AfterTest(IDialogContext context, IAwaitable<bool> result)
            {
                if (await result)
                {
                    var body = this.Body(this.item);
                    context.Call(body, this.AfterZeroOrBody);
                }
                else
                {
                    context.Done(this.item);
                }
            }
        }

        [Serializable]
        private sealed class FoldDialog<T> : IDialog<T>
        {
            public readonly IDialog<IEnumerable<IDialog<T>>> Antecedent;
            public readonly Func<T, T, T> Folder;
            public FoldDialog(IDialog<IEnumerable<IDialog<T>>> antecedent, Func<T, T, T> folder)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Folder, nameof(folder), folder);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call(this.Antecedent, this.AfterAntecedent);
            }
            private IReadOnlyList<IDialog<T>> items;
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<IEnumerable<IDialog<T>>> result)
            {
                this.items = (await result).ToArray();
                await Iterate(context);
            }
            private int index = 0;
            private T folded = default(T);
            private async Task Iterate(IDialogContext context)
            {
                if (this.index < this.items.Count)
                {
                    var child = this.items[this.index];
                    context.Call(child, AfterItem);
                }
                else
                {
                    context.Done(this.folded);
                }
            }
            private async Task AfterItem(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                if (this.index == 0)
                {
                    this.folded = itemT;
                }
                else
                {
                    this.folded = this.Folder(this.folded, itemT);
                }

                ++this.index;

                await this.Iterate(context);
            }
        }

        [Serializable]
        private sealed class WithScorableDialog<T, Item, Score> : DelegatingScorable<Item, Score>, IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public WithScorableDialog(IDialog<T> antecedent, IScorable<Item, Score> scorable)
                : base(scorable)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                context.Done(await result);
            }
        }
    }

    /// <summary>
    /// The contextual selector function.
    /// </summary>
    /// <typeparam name="T"> The type of value passed to selector.</typeparam>
    /// <typeparam name="R"> The returned type of the selector.</typeparam>
    /// <param name="context"> <see cref="IBotContext"/> passed to selector.</param>
    /// <param name="item"> The value passed to selector.</param>
    /// <returns> The value returned by selector.</returns>
    public delegate R ContextualSelector<in T, R>(IBotContext context, T item);

    /// <summary>
    /// The interface for cases evaluated by switch.
    /// </summary>
    /// <typeparam name="T"> The type of incoming value to case.</typeparam>
    /// <typeparam name="R"> The type of the object returned by selector.</typeparam>
    public interface ICase<in T, R>
    {
        /// <summary>
        /// The condition field of the case.
        /// </summary>
        Func<T, bool> Condition { get; }
        /// <summary>
        /// The selector that will be invoked if condition is met.
        /// </summary>
        ContextualSelector<T, R> Selector { get; }
    }

    /// <summary>
    /// The default implementation of <see cref="ICase{T, R}"/>.
    /// </summary>
    [Serializable]
    public class Case<T, R> : ICase<T, R>
    {
        public Func<T, bool> Condition { get; protected set; }
        public ContextualSelector<T, R> Selector { get; protected set; }

        protected Case()
        {
        }

        /// <summary>
        /// Constructs a case. 
        /// </summary>
        /// <param name="condition"> The condition of the case.</param>
        /// <param name="selector"> The contextual selector of the case.</param>
        public Case(Func<T, bool> condition, ContextualSelector<T, R> selector)
        {
            SetField.CheckNull(nameof(condition), condition);
            this.Condition = condition;
            SetField.CheckNull(nameof(selector), selector);
            this.Selector = selector;
        }
    }

    /// <summary>
    /// The regex case for switch.
    /// </summary>
    /// <remarks>
    /// The condition will be true if the regex matches the text.
    /// </remarks>
    [Serializable]
    public sealed class RegexCase<R> : Case<string, R>
    {
        private readonly Regex Regex;

        /// <summary>
        /// Constructs a case based on a regular expression.
        /// </summary>
        /// <param name="regex"> The regex for condition.</param>
        /// <param name="selector"> The contextual selector for the case.</param>
        public RegexCase(Regex regex, ContextualSelector<string, R> selector)
        {
            SetField.CheckNull(nameof(selector), selector);
            this.Selector = selector;
            SetField.NotNull(out this.Regex, nameof(regex), regex);
            this.Condition = this.IsMatch;
        }

        private bool IsMatch(string text)
        {
            return this.Regex.Match(text).Success;
        }
    }

    /// <summary>
    /// The default case for switch. <see cref="ICase{T, R}"/>
    /// </summary>
    [Serializable]
    public sealed class DefaultCase<T, R> : Case<T, R>
    {
        /// <summary>
        /// Constructs the default case for switch.
        /// </summary>
        /// <param name="selector"> The contextual selector that will be called in default case.</param>
        public DefaultCase(ContextualSelector<T, R> selector)
            : base(obj => true, selector)
        {
        }
    }
}
