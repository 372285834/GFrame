using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;

namespace TileEditor
{
    public class HELua
    {
        private const int BUILDER_CAPACITY = 1024;

        public static string Encode(object lua)
        {
            StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
            builder.Append("return ");
            bool success = SerializeValue(lua, builder);
            return (success ? builder.ToString() : null);
        }

        protected static bool SerializeValue(object value, StringBuilder builder)
        {
            bool success = true;

            if (value is string)
            {
                success = SerializeString((string)value, builder);
            }
            else if (value is IDictionary)
            {
                success = SerializeObject((IDictionary)value, builder);
            }
            else if (value is IList)
            {
                success = SerializeArray(value as IList, builder);
            }
            else if ((value is Boolean) && ((Boolean)value == true))
            {
                builder.Append("true");
            }
            else if ((value is Boolean) && ((Boolean)value == false))
            {
                builder.Append("false");
            }
            else if (value is ValueType)
            {
                success = SerializeNumber(Convert.ToDouble(value), builder);
            }
            else if (value == null)
            {
                builder.Append("nil");
            }
            else
            {
                success = false;
            }

            return success;
        }

        protected static bool SerializeString(string aString, StringBuilder builder)
        {
            builder.Append("\"");

            char[] charArray = aString.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"')
                {
                    builder.Append("\\\"");
                }
                else if (c == '\\')
                {
                    builder.Append("\\\\");
                }
                else if (c == '\b')
                {
                    builder.Append("\\b");
                }
                else if (c == '\f')
                {
                    builder.Append("\\f");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else
                {
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126))
                    {
                        builder.Append(c);
                    }
                    else
                    {
                        builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                    }
                }
            }

            builder.Append("\"");
            return true;
        }

        protected static bool SerializeObject(IDictionary anObject, StringBuilder builder)
        {
            builder.Append("{");
            IDictionaryEnumerator e = anObject.GetEnumerator();

            bool first = true;
            while (e.MoveNext())
            {
                string key = e.Key.ToString();
                object value = e.Value;

                if (!first)
                {
                    builder.Append(", ");
                }
                builder.Append("[");
                SerializeString(key, builder);
                builder.Append("]");
                builder.Append(" = ");
                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("}");
            return true;
        }

        protected static bool SerializeArray(IList anArray, StringBuilder builder)
        {
            builder.Append("{");
            bool first = true;
            for (int i = 0; i < anArray.Count; i++)
            {
                object value = anArray[i];
                if (!first)
                {
                    builder.Append(", ");
                }
                if (!SerializeValue(value, builder))
                {
                    return false;
                }
                first = false;
            }
            builder.Append("}");
            return true;
        }

        protected static bool SerializeNumber(Double number, StringBuilder builder)
        {
            builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
            return true;
        }
    }
}