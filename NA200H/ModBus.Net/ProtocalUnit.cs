using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    public abstract class ProtocalUnit : IProtocalFormatting
    {
        public abstract byte[] Format(InputStruct message);

        public virtual byte[] Format(params object[] message)
        {
            return TranslateContent(message);
        }

        public abstract OutputStruct Unformat(byte[] messageBytes, ref int pos);

        /// <summary>
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public byte[] TranslateContent(params object[] contents)
        {
            bool b = false;
            var newContentsList = new List<object>();
            foreach (object content in contents)
            {
                string t = content.GetType().ToString();
                if (t.Substring(t.Length - 2, 2) == "[]")
                {
                    b = true;
                    IEnumerable<object> contentArray =
                        ArrayList.Adapter((Array) content).ToArray(typeof (object)).OfType<object>();
                    newContentsList.AddRange(contentArray);
                }
                else
                {
                    newContentsList.Add(content);
                }
            }
            if (b) return TranslateContent(newContentsList.ToArray());
            var translateTarget = new List<byte>();
            foreach (object content in contents)
            {
                string t = content.GetType().ToString();
                switch (t)
                {
                    case "System.Int16":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((short) content));
                        break;
                    }
                    case "System.Int32":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((int) content));
                        break;
                    }
                    case "System.Int64":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((long) content));
                        break;
                    }
                    case "System.UInt16":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((ushort) content));
                        break;
                    }
                    case "System.UInt32":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((uint) content));
                        break;
                    }
                    case "System.UInt64":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((ulong) content));
                        break;
                    }
                    case "System.Single":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((float) content));
                        break;
                    }
                    case "System.Double":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((double) content));
                        break;
                    }
                    case "System.Byte":
                    {
                        translateTarget.AddRange(ValueHelper.Instance.GetBytes((byte) content));
                        break;
                    }
                    default:
                    {
                        throw new NotImplementedException("没有实现除整数以外的其它转换方式");
                    }
                }
            }
            return translateTarget.ToArray();
        }
    }

    public class InputStruct
    {
    }

    public class OutputStruct
    {
    }
}