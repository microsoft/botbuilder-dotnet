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
    public class Xml : ExpressionEvaluator
    {
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
            XDocument xml;
            string result = null;
            try
            {
                if (contentToConvert is string str)
                {
                    xml = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(Encoding.ASCII.GetBytes(str), new XmlDictionaryReaderQuotas()));
                }
                else
                {
                    xml = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(Encoding.ASCII.GetBytes(contentToConvert.ToString()), new XmlDictionaryReaderQuotas()));
                }

                result = xml.ToString().TrimStart('{').TrimEnd('}');
            }
            catch
            {
                error = "Invalid json";
            }

            return (result, error);
        }
    }
}
