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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace RView
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("RView [<resource file path> [-c <locale>]] [-g <assembly> <static form method>]");
            Console.WriteLine("If -c is not specified will print the resource file on the console.");
            Console.WriteLine("-c <locale> : Will copy resources to a <path-locale> resource file in same format as input.");
            Console.WriteLine("-g <assembly> <static form building method>: ");
            Console.WriteLine("   Will load assembly and invoke static method for building a FormFlow");
            Console.WriteLine("   form and then call SaveResources on it to generate a resource file for the form.");
            Console.WriteLine("   Example: RView -g Formtest.exe Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder.BuildForm");
            Console.WriteLine("            would generate a Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder.resx resource file.");
            System.Environment.Exit(-1);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
            }
            string path = null;
            string locale = null;
            string assemblyPath = null;
            string methodPath = null;
            for (var i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "-c":
                            if (++i < args.Length)
                            {
                                locale = args[i];
                            }
                            break;
                        case "-g":
                            if (++i < args.Length)
                            {
                                assemblyPath = Path.GetFullPath(args[i]);
                            }
                            if (++i < args.Length)
                            {
                                methodPath = args[i];
                            }
                            break;
                        default:
                            Usage();
                            break;
                    }
                }
                else
                {
                    path = arg;
                }
            }

            if (assemblyPath != null)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var methodParts = methodPath.Split('.');
                var methodName = methodParts.Last();
                var className = string.Join(".", methodParts.Take(methodParts.Count() - 1));
                var classType = assembly.GetType(className);
                var method = classType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                var methodArgs = new object[method.GetParameters().Length];
                var form = method.Invoke(null, methodArgs);
                var formPath = className + ".resx";
                using (var stream = new FileStream(formPath, FileMode.Create))
                using (var writer = new ResXResourceWriter(stream))
                {
                    form.GetType().InvokeMember("SaveResources",
                        BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                        null, form, new object[] { writer });
                }
                Console.WriteLine($"Generated {formPath} by calling {methodPath} in {assemblyPath}.");
            }
            else
            {
                var isResX = Path.GetExtension(path) == ".resx";
                IResourceWriter writer = null;
                FileStream outStream = null;
                if (locale != null)
                {
                    var outPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "-" + locale + Path.GetExtension(path));
                    Console.Write($"Copying to {outPath}");
                    Console.WriteLine();
                    outStream = new FileStream(outPath, FileMode.Create);
                    writer = isResX ? (IResourceWriter)new ResXResourceWriter(outStream) : (IResourceWriter)new ResourceWriter(outStream);
                }
                using (var stream = new FileStream(path, FileMode.Open))
                using (var reader = isResX ? (IResourceReader)new ResXResourceReader(stream) : (IResourceReader)new ResourceReader(stream))
                using (outStream)
                using (writer)
                {
                    int values = 0;
                    int lists = 0;
                    int templates = 0;
                    foreach (DictionaryEntry entry in reader)
                    {
                        var fullKey = (string)entry.Key;
                        var value = (string)entry.Value;
                        var typeAndKey = fullKey.Split(SEPERATOR);
                        var type = typeAndKey.Last();
                        if (writer == null)
                        {
                            Console.WriteLine($"{fullKey}: {value}");
                        }
                        else
                        {
                            if (type == "LIST" || type == "TEMPLATE")
                            {
                                writer.AddResource(fullKey, MakeList(from elt in SplitList(value) select "<" + elt + ">"));
                            }
                            else
                            {
                                writer.AddResource(fullKey, "<" + value + ">");
                            }
                        }
                        switch (type)
                        {
                            case "VALUE": ++values; break;
                            case "LIST": ++lists; break;
                            case "TEMPLATE": ++templates; break;
                        }
                    }
                    Console.WriteLine($"Found {values} values, {lists} lists and {templates} templates");
                }
            }
        }

        const char SEPERATOR = ';';
        const string SSEPERATOR = ";";
        const string ESCAPED_SEPERATOR = "__semi";

        static string MakeList(IEnumerable<string> elements)
        {
            return string.Join(SSEPERATOR, from elt in elements select elt.Replace(SSEPERATOR, ESCAPED_SEPERATOR));
        }

        static IEnumerable<string> SplitList(string str)
        {
            var elements = str.Split(SEPERATOR);
            return from elt in elements select elt.Replace(ESCAPED_SEPERATOR, SSEPERATOR);
        }
    }
}

