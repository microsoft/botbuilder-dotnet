#if !NET46
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;

// CXuesong: Serialization extensions for .NET Standard 2.0
// MIT License
// https://github.com/CXuesong/BotBuilder.Standard/wiki
// https://github.com/CXuesong/BotBuilder.Standard/blob/d77d50e1b78711bbb4a9b3daebd827e67594d799/CSharp/Library/Microsoft.Bot.Builder/Fibers/NetStandardSerialization.cs

namespace Microsoft.Bot.Builder.Dialogs.Bridge
{
    public static class NetStandardSerialization
    {
        // See SurrogateSelector.(ISurrogateSelector.GetSurrogate) for the reasoning.
        private const int LowestPriority = 10 - int.MaxValue;

        public sealed class TypeSerializationSurrogate : Serialization.ISurrogateProvider
        {

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var type = (Type)obj;
                // BinaryFormatter in .NET Core 2.0 cannot persist types in System.Private.CoreLib.dll
                // that are not forwareded to mscorlib, including System.RuntimeType
                info.SetType(typeof(TypeReference));
                info.AddValue("AssemblyName", type.Assembly.FullName);
                info.AddValue("FullName", type.FullName);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
                ISurrogateSelector selector)
            {
                throw new NotSupportedException();
                //var AssemblyQualifiedName = info.GetString("AssemblyQualifiedName");
                //return Type.GetType(AssemblyQualifiedName, true);
            }

            public bool Handles(Type type, StreamingContext context, out int priority)
            {
                if (typeof(Type).IsAssignableFrom(type))
                {
                    priority = LowestPriority;
                    return true;
                }
                priority = 0;
                return false;
            }

            [Serializable]
            internal sealed class TypeReference : IObjectReference
            {

                private readonly string AssemblyName;

                private readonly string FullName;

                public TypeReference(Type type)
                {
                    if (type == null) throw new ArgumentNullException(nameof(type));
                    AssemblyName = type.Assembly.FullName;
                    FullName = type.FullName;
                }

                public object GetRealObject(StreamingContext context)
                {
                    var assembly = Assembly.Load(AssemblyName);
                    return assembly.GetType(FullName, true);
                }
            }

        }

        public sealed class MemberInfoSerializationSurrogate : Serialization.ISurrogateProvider
        {

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                MemberInfoReference.GetObjectData((MemberInfo)obj, info, context);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
                ISurrogateSelector selector)
            {
                throw new NotSupportedException();
            }

            public bool Handles(Type type, StreamingContext context, out int priority)
            {
                if (typeof(MemberInfo).IsAssignableFrom(type))
                {
                    priority = LowestPriority;
                    return true;
                }
                priority = 0;
                return false;
            }

            [Serializable]
            private sealed class MemberInfoReference : IObjectReference
            {

                private readonly Type DeclaringType = null;
                private readonly string Name = null;
                private readonly MemberTypes MemberType = default(MemberTypes);
                private readonly BindingFlags BindingAttr = default(BindingFlags);
                private readonly Type[] GenericParameters = null;
                private readonly Type[] Parameters = null;

                private static BindingFlags GetBindingAttr(MemberInfo member)
                {
                    if (member == null) throw new ArgumentNullException(nameof(member));
                    var bindingFlags = default(BindingFlags);
                    switch (member)
                    {
                        case MethodBase method:
                            bindingFlags |= method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                            bindingFlags |= method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                            break;
                        case FieldInfo field:
                            bindingFlags |= field.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                            bindingFlags |= field.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                            break;
                        default:
                            return BindingFlags.Public | BindingFlags.NonPublic
                                   | BindingFlags.Static | BindingFlags.Instance;
                    }
                    return bindingFlags;
                }

                public static void GetObjectData(MemberInfo member, SerializationInfo info, StreamingContext context)
                {
                    info.SetType(typeof(MemberInfoReference));
                    info.AddValue(nameof(DeclaringType), new TypeSerializationSurrogate.TypeReference(member.DeclaringType));
                    info.AddValue(nameof(Name), member.Name);
                    info.AddValue(nameof(MemberType), member.MemberType);
                    info.AddValue(nameof(BindingAttr), GetBindingAttr(member));
                    if (member is MethodBase method)
                    {
                        info.AddValue(nameof(GenericParameters),
                            method.GetGenericArguments().ToArray());
                        info.AddValue(nameof(Parameters),
                            method.GetParameters().Select(p => p.ParameterType).ToArray());
                    }
                }

                private bool MatchMethodSignature(MethodBase method)
                {
                    Debug.Assert(method.Name == Name);
                    var gpa = method.GetGenericArguments();
                    if (gpa.Length != GenericParameters.Length) return false;
                    var pa = method.GetParameters();
                    if (pa.Length != Parameters.Length) return false;
                    if (gpa.Length > 0)
                    {
                        var genericMethod = ((MethodInfo)method).MakeGenericMethod(GenericParameters);
                        pa = genericMethod.GetParameters();
                    }
                    for (int i = 0; i < pa.Length; i++)
                    {
                        if (pa[i].ParameterType != Parameters[i]) return false;
                    }
                    return true;
                }

                public object GetRealObject(StreamingContext context)
                {
                    if (MemberType == MemberTypes.Method || MemberType == MemberTypes.Constructor)
                    {
                        var methods = DeclaringType.GetMember(Name, MemberType, BindingAttr | BindingFlags.DeclaredOnly);
                        if (Parameters.Any(t => t == null))
                            throw new ArgumentException("Detected null argument type in the method signature.",
                                nameof(Parameters));
                        if (Parameters.Any(t => t.IsGenericParameter))
                            throw new ArgumentException("Detected generic parameter in the method signature.",
                                nameof(Parameters));
                        try
                        {
                            return methods.Cast<MethodBase>().First(MatchMethodSignature);
                        }
                        catch (InvalidOperationException)
                        {
                            throw new MissingMethodException(DeclaringType.FullName, Name);
                        }
                    }
                    var members = DeclaringType.GetMember(Name, MemberType, BindingAttr | BindingFlags.DeclaredOnly);
                    if (members.Length == 0) throw new MissingMemberException(DeclaringType.FullName, Name);
                    if (members.Length > 1)
                        throw new AmbiguousMatchException($"Found multiple \"{Name}\" in \"{DeclaringType}\".");
                    return members[0];
                }
            }
        }

        public sealed class DelegateSerializationSurrogate : Serialization.ISurrogateProvider
        {

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var del = (Delegate)obj;
                info.SetType(typeof(DelegateReference));
                info.AddValue("Delegates", DelegateReference.FlattenDelegate(del));
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
                ISurrogateSelector selector)
            {
                throw new NotSupportedException();
            }

            public bool Handles(Type type, StreamingContext context, out int priority)
            {
                if (typeof(Delegate).IsAssignableFrom(type))
                {
                    priority = LowestPriority;
                    return true;
                }
                priority = 0;
                return false;
            }

            [Serializable]
            private sealed class DelegateReference : IObjectReference
            {

                private readonly DelegateEntry[] Delegates = null;

                public static DelegateEntry[] FlattenDelegate(Delegate d)
                {
                    if (d == null) throw new ArgumentNullException(nameof(d));
                    return d.GetInvocationList().Select(d1 =>
                        new DelegateEntry(d1.GetType(), d1.GetMethodInfo(), d1.Target)).ToArray();
                }

                public object GetRealObject(StreamingContext context)
                {
                    if (Delegates == null || Delegates.Length == 0)
                        throw new ArgumentException(nameof(Delegates));
                    // The further reference has not been deserialized.
                    // Caller, i.e. ObjectManager.ResolveObjectReference will retry later.
                    if (Delegates[0] == null || !Delegates[0].IsValid) return null;
                    var d = Delegates[0].ToDelegate();
                    for (int i = 1; i < Delegates.Length; i++)
                        d = Delegate.Combine(d, Delegates[i].ToDelegate());
                    return d;
                }
            }

            /// <summary>
            /// Represents an item in a chained delegate.
            /// </summary>
            [Serializable]
            private sealed class DelegateEntry
            {
                public DelegateEntry(Type delegateType, MethodInfo method, object target)
                {
                    if (delegateType == null) throw new ArgumentNullException(nameof(delegateType));
                    if (method == null) throw new ArgumentNullException(nameof(method));
                    Debug.Assert(method.IsStatic || target != null);
                    DelegateType = delegateType;
                    Method = method;
                    Target = target;
                }

                private readonly object Target;

                private readonly MethodInfo Method;

                private readonly Type DelegateType;

                public bool IsValid => Method != null;

                public Delegate ToDelegate()
                {
                    if (Method == null) throw new ArgumentNullException(nameof(Method));
                    // Might raise NullReferenceException when invoking the returned delegate.
                    if (Target == null && !Method.IsStatic)
                        Debug.WriteLine("Warning: Instance method delegate deserialized with null target.");
                    return Method.CreateDelegate(DelegateType, Target);
                }

                public override string ToString()
                {
                    return Method == null ? "<Invalid>" : Method.Name;
                }

            }

        }

        public sealed class RegexSerializationSurrogate : Serialization.ISurrogateProvider
        {

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var inst = (Regex)obj;
                info.AddValue("Pattern", inst.ToString());
                info.AddValue("Options", inst.Options);
                info.AddValue("MatchTimeout", inst.MatchTimeout);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
                ISurrogateSelector selector)
            {
                return new Regex(info.GetString("Pattern"),
                    info.GetValue<RegexOptions>("Options"),
                    info.GetValue<TimeSpan>("MatchTimeout"));
            }

            public bool Handles(Type type, StreamingContext context, out int priority)
            {
                if (typeof(Regex).IsAssignableFrom(type))
                {
                    priority = LowestPriority;
                    return true;
                }
                priority = 0;
                return false;
            }
        }

    }
}

#endif
