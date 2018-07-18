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

using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Dialogs;

using Moq;
using Autofac;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public abstract class FiberTestBase
    {
        public struct C
        {
        }

        public static readonly C Context = default(C);
        public static readonly CancellationToken Token = new CancellationTokenSource().Token;

        public interface IMethod
        {
            Task<IWait<C>> CodeAsync<T>(IFiber<C> fiber, C context, IAwaitable<T> item, CancellationToken token);
        }

        public static Moq.Mock<IMethod> MockMethod()
        {
            var method = new Moq.Mock<IMethod>(Moq.MockBehavior.Loose);
            return method;
        }

        public static Expression<Func<IAwaitable<T>, bool>> Item<T>(T value)
        {
            return item => value.Equals(item.GetAwaiter().GetResult());
        }

        protected sealed class CodeException : Exception
        {
        }

        public static bool ExceptionOfType<T, E>(IAwaitable<T> item) where E : Exception
        {
            try
            {
                item.GetAwaiter().GetResult();
                return false;
            }
            catch (E)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Expression<Func<IAwaitable<T>, bool>> ExceptionOfType<T, E>() where E : Exception
        {
            return item => ExceptionOfType<T, E>(item);
        }

        public static async Task PollAsync(IFiberLoop<C> fiber)
        {
            IWait wait;
            do
            {
                wait = await fiber.PollAsync(Context, Token);
            }
            while (wait.Need != Need.None && wait.Need != Need.Done);
        }

        public static IContainer Build()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FiberModule<C>());
            return builder.Build();
        }

        public sealed class ResolveMoqAssembly : IDisposable
        {
            private readonly object[] instances;
            public ResolveMoqAssembly(params object[] instances)
            {
                SetField.NotNull(out this.instances, nameof(instances), instances);

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
            void IDisposable.Dispose()
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
            private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs arguments)
            {
                foreach (var instance in instances)
                {
                    var type = instance.GetType();
                    if (arguments.Name == type.Assembly.FullName)
                    {
                        return type.Assembly;
                    }
                }

                return null;
            }
        }

        public static void AssertSerializable<T>(ILifetimeScope scope, ref T item) where T : class
        {
            var formatter = scope.Resolve<IFormatter>();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, item);
                stream.Position = 0;
                item = (T)formatter.Deserialize(stream);
            }
        }
    }

    [TestClass]
    public sealed class FiberTests : FiberTestBase
    {
        public static async Task Wait_is_Awaitable<ItemType, PostType>(PostType expected)
        {
            var completion = new Wait<object, ItemType>();

            IWait wait = completion;
            Assert.AreEqual(Need.None, wait.Need, "at initial state");

            IFiber<object> fiber = new Mock<IFiber<object>>(MockBehavior.Strict).Object;
            object context = new object();

            IWait<object, ItemType> typed = completion;
            typed.Wait(async (f, c, item, token) =>
            {
                Assert.AreEqual(Need.Call, wait.Need, "inside callback state");

                Assert.AreEqual(fiber, f);
                Assert.AreEqual(context, c);

                Assert.AreEqual(expected, item.GetAwaiter().GetResult());
                Assert.AreEqual(expected, await item);
                return null;
            });
            Assert.AreEqual(Need.Wait, wait.Need, "waiting state");

            wait.Post(expected);
            Assert.AreEqual(Need.Poll, wait.Need, "need to poll state");

            await typed.PollAsync(fiber, context, CancellationToken.None);
            Assert.AreEqual(Need.Done, wait.Need, "done state");

            IAwaitable<ItemType> awaitable = completion;
            var actual = await awaitable;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async Task Wait_is_Awaitable_Guid_Guid()
        {
            await Wait_is_Awaitable<Guid, Guid>(Guid.NewGuid());
        }

        public class TypeA { }
        public class TypeB : TypeA { }
        public class TypeC : TypeB { }

        [TestMethod]
        public async Task Wait_is_Awaitable_A_B_C()
        {
            // ItemType = TypeA
            await Wait_is_Awaitable<TypeA, TypeA>(new TypeA());
            await Wait_is_Awaitable<TypeA, TypeA>(new TypeB());
            await Wait_is_Awaitable<TypeA, TypeA>(new TypeC());

            await Wait_is_Awaitable<TypeA, TypeB>(new TypeB());
            await Wait_is_Awaitable<TypeA, TypeB>(new TypeC());

            await Wait_is_Awaitable<TypeA, TypeC>(new TypeC());

            // ItemType = TypeB
            await Wait_is_Awaitable<TypeB, TypeA>(new TypeB());
            await Wait_is_Awaitable<TypeB, TypeA>(new TypeC());

            await Wait_is_Awaitable<TypeB, TypeB>(new TypeB());
            await Wait_is_Awaitable<TypeB, TypeB>(new TypeC());

            await Wait_is_Awaitable<TypeB, TypeC>(new TypeC());

            // ItemType = TypeC
            await Wait_is_Awaitable<TypeC, TypeA>(new TypeC());

            await Wait_is_Awaitable<TypeC, TypeB>(new TypeC());

            await Wait_is_Awaitable<TypeC, TypeC>(new TypeC());
        }

        [TestMethod]
        public async Task Awaitable_From_Item()
        {
            var expected = Guid.NewGuid();
            var awaitable = Awaitable.FromItem(expected);
            Assert.AreEqual(expected, await awaitable);
        }

        [TestMethod]
        public async Task Fiber_Is_Serializable()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = (Fiber<C>)container.Resolve<IFiberLoop<C>>();
                // assert
                var previous = fiber;
                AssertSerializable(container, ref fiber);
                Assert.IsFalse(object.ReferenceEquals(previous, fiber));
                Assert.IsTrue(object.ReferenceEquals(((IFiber<C>)previous).Waits, ((IFiber<C>)fiber).Waits));
            }
        }

        [Serializable]
        private sealed class SerializableMethod : IMethod
        {
            async Task<IWait<C>> IMethod.CodeAsync<T>(IFiber<C> fiber, C context, IAwaitable<T> item, CancellationToken token)
            {
                return NullWait<C>.Instance;
            }
        }

        [TestMethod]
        public async Task Fiber_With_Wait_Is_Serializable()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                IMethod method = new SerializableMethod();

                // act
                var value = 42;
                fiber.Call(method.CodeAsync, value);

                // assert
                AssertSerializable(container, ref fiber);
                var next = await fiber.PollAsync(Context, Token);
                Assert.AreEqual(Need.Done, next.Need);
            }
        }


        [TestMethod]
        public async Task Fiber_NoCall_NeedNone()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();

                // assert
                var next = await fiber.PollAsync(Context, Token);
                Assert.AreEqual(Need.None, next.Need);
            }
        }

        [TestMethod]
        public async Task Fiber_OneCall_NeedDone()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                var value = 42;
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value)), Token))
                    .ReturnsAsync(NullWait<C>.Instance);

                // act
                fiber.Call(method.Object.CodeAsync, value);

                // assert
                var next = await fiber.PollAsync(Context, Token);
                Assert.AreEqual(Need.Done, next.Need);
                method.VerifyAll();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Fiber_OneCall_ThenDone_Throws()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.IsAny<IAwaitable<int>>(), Token))
                    .Returns(async () => { return fiber.Done(42); });

                // act
                fiber.Call(method.Object.CodeAsync, 42);
                await PollAsync(fiber);

                // assert
                method.VerifyAll();
            }
        }

        [TestMethod]
        public async Task Code_Is_Called()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                var value = 42;
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value)), Token))
                    .ReturnsAsync(NullWait<C>.Instance);

                // act
                fiber.Call(method.Object.CodeAsync, value);
                await PollAsync(fiber);

                // assert
                method.VerifyAll();
            }
        }

        [TestMethod]
        public async Task Code_Call_Code()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                var valueOne = 42;
                var valueTwo = "hello world";
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(valueOne)), Token))
                    .Returns(async () => { return fiber.Call(method.Object.CodeAsync, valueTwo); });
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(valueTwo)), Token))
                    .ReturnsAsync(NullWait<C>.Instance);

                // act
                fiber.Call(method.Object.CodeAsync, valueOne);
                await PollAsync(fiber);

                // assert
                method.VerifyAll();
            }
        }

        [TestMethod]
        public async Task Code_Call_Method_With_Return()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var methodOne = MockMethod();
                var methodTwo = MockMethod();
                var value1 = 42;
                var value2 = "hello world";
                var value3 = Guid.NewGuid();
                methodOne
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value1)), Token))
                    .Returns(async () => { return fiber.Call<C, string, Guid>(methodTwo.Object.CodeAsync, value2, methodOne.Object.CodeAsync); });
                methodTwo
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value2)), Token))
                    .Returns(async () => { return fiber.Done(value3); });
                methodOne
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value3)), Token))
                    .ReturnsAsync(NullWait<C>.Instance);

                // act
                fiber.Call(methodOne.Object.CodeAsync, value1);
                await PollAsync(fiber);

                // assert
                methodOne.VerifyAll();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNeedException))]
        public async Task Code_Call_Method_No_Return_Throws()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var methodOne = MockMethod();
                var methodTwo = MockMethod();
                var value1 = 42;
                var value2 = "hello world";
                var value3 = Guid.NewGuid();
                methodOne
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value1)), Token))
                    .Returns(async () => { return fiber.Call<C, string>(methodTwo.Object.CodeAsync, value2); });
                methodTwo
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value2)), Token))
                    .Returns(async () => { return fiber.Done(value3); });

                // act
                fiber.Call(methodOne.Object.CodeAsync, value1);
                await PollAsync(fiber);

                // assert
                methodOne.VerifyAll();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CodeException))]
        public async Task Code_Throws_To_User()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                var value = 42;
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value)), Token))
                    .Returns(async () => { throw new CodeException(); });

                // act
                fiber.Call(method.Object.CodeAsync, value);
                await PollAsync(fiber);

                // assert
                method.VerifyAll();
            }
        }

        // TODO: maybe test for unobserved exceptions sent to callers?

        [TestMethod]
        public async Task Code_Call_Method_That_Throws_To_Code()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var methodOne = MockMethod();
                var methodTwo = MockMethod();
                var value1 = 42;
                var value2 = "hello world";
                var value3 = Guid.NewGuid();
                methodOne
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value1)), Token))
                    .Returns(async () => { return fiber.Call<C, string, Guid>(methodTwo.Object.CodeAsync, value2, methodOne.Object.CodeAsync); });
                methodTwo
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value2)), Token))
                    .Returns(async () => { throw new CodeException(); });
                methodOne
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(ExceptionOfType<Guid, CodeException>()), Token))
                    .ReturnsAsync(NullWait<C>.Instance);

                // act
                fiber.Call(methodOne.Object.CodeAsync, value1);
                await PollAsync(fiber);

                // assert
                methodOne.VerifyAll();
            }
        }

        [TestMethod]
        public async Task Code_Call_Method_That_Posts_Invalid_Type_To_Code()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var methodOne = MockMethod();
                var methodTwo = MockMethod();
                var value1 = 42;
                var value2 = "hello world";
                var value3 = Guid.NewGuid();
                methodOne
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value1)), Token))
                    .Returns(async () => { return fiber.Call<C, string, Guid>(methodTwo.Object.CodeAsync, value2, methodOne.Object.CodeAsync); });
                methodTwo
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value2)), Token))
                    .Returns(async () => { return fiber.Done("not a guid"); });
                methodOne
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(ExceptionOfType<Guid, InvalidTypeException>()), Token))
                    .ReturnsAsync(NullWait<C>.Instance);

                // act
                fiber.Call(methodOne.Object.CodeAsync, value1);
                await PollAsync(fiber);

                // assert
                methodOne.VerifyAll();
            }
        }

        [TestMethod]
        public async Task Code_Item_Variance()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                string valueAsstring = "hello world";
                object valueAsObject = valueAsstring;
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(valueAsObject)), Token))
                    .ReturnsAsync(NullWait<C>.Instance);

                // act
                fiber.Call(method.Object.CodeAsync, valueAsstring);
                await PollAsync(fiber);

                // assert
                method.VerifyAll();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNeedException))]
        public async Task Poll_Is_Not_Reentrant()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                var value = 42;
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value)), Token))
                    .Returns(async () => { await fiber.PollAsync(Context, Token); return null; });

                // act
                fiber.Call(method.Object.CodeAsync, value);
                await PollAsync(fiber);

                // assert
                method.VerifyAll();
            }
        }

        [TestMethod]
        public async Task Method_Void()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                var value = "hello world";
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value)), Token))
                    .Returns(async () => fiber.Done(42));

                // act
                var loop = Methods.Void<C, string>(method.Object.CodeAsync);
                fiber.Call(loop, value);
                await PollAsync(fiber);

                // assert
                method.Verify(m => m.CodeAsync(fiber, Context, It.Is(Item(value)), Token), Times.Exactly(1));
            }
        }

        [TestMethod]
        public async Task Method_Loop()
        {
            // arrange
            using (var container = Build())
            {
                var fiber = container.Resolve<IFiberLoop<C>>();
                var method = MockMethod();
                var value = "hello world";
                method
                    .Setup(m => m.CodeAsync(fiber, Context, It.Is(Item(value)), Token))
                    .Returns(async () => fiber.Done(42));

                // act
                const int CallCount = 5;
                var loop = Methods.Void(Methods.Loop<C, string>(method.Object.CodeAsync, CallCount));
                fiber.Call(loop, value);
                await PollAsync(fiber);

                // assert
                method.Verify(m => m.CodeAsync(fiber, Context, It.Is(Item(value)), Token), Times.Exactly(CallCount));
            }
        }
    }
}
