// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the XML version of a string that contains a JSON object.
    /// </summary>
#pragma warning disable CA1724 // Type names should not match namespaces (by design and we can't change this without breaking binary compat)
    internal class Xml : ExpressionEvaluator
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Xml"/> class.
        /// </summary>
        public Xml()
            : base(ExpressionType.Xml, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => ToXml(args[0]));
        }

        private static (object, string) ToXml(object contentToConvert)
        {
            string error = null;
            string result = null;
            try
            {
                XDocument xml;
                if (contentToConvert is string str)
                {
                    using (var xmlDictionaryReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.ASCII.GetBytes(str), new XmlDictionaryReaderQuotas()))
                    {
                        xml = XDocument.Load(xmlDictionaryReader);
                    }
                }
                else
                {
                    using (var xmlDictionaryReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.ASCII.GetBytes(contentToConvert.ToString()), new XmlDictionaryReaderQuotas()))
                    {
                        xml = XDocument.Load(xmlDictionaryReader);
                    }
                }

                result = xml.ToString().TrimStart('{').TrimEnd('}');
            }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return generic error)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = "Invalid json";
            }

            return (result, error);
        }
    }
}
