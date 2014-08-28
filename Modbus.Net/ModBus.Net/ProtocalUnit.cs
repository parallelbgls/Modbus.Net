using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    public abstract class ProtocalUnit : IProtocalFormatting
    {
        /// <summary>
        /// 格式化，将输入结构转换为字节数组
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract byte[] Format(InputStruct message);

        /// <summary>
        /// 格式化，将对象数组转换为字节数组
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual byte[] Format(params object[] message)
        {
            return TranslateContent(message);
        }

        /// <summary>
        /// 结构化，将字节数组转换为输出结构。
        /// </summary>
        /// <param name="messageBytes"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public abstract OutputStruct Unformat(byte[] messageBytes, ref int pos);

        /// <summary>
        /// 转换静态方法，把对象数组转换为字节数组。
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static byte[] TranslateContent(params object[] contents)
        {
            bool b = false;
            //先查找传入的结构中有没有数组，有的话将其打开
            var newContentsList = new List<object>();
            foreach (object content in contents)
            {
                string t = content.GetType().ToString();
                if (t.Substring(t.Length - 2, 2) == "[]")
                {
                    b = true;
                    IEnumerable<object> contentArray =
                        ArrayList.Adapter((Array)content).ToArray(typeof(object)).OfType<object>();
                    newContentsList.AddRange(contentArray);
                }
                else
                {
                    newContentsList.Add(content);
                }
            }
            //重新调用一边这个函数，这个传入的参数中一定没有数组。
            if (b) return TranslateContent(newContentsList.ToArray());
            //把参数一个一个翻译为相对应的字节，然后拼成一个队列
            var translateTarget = new List<byte>();
            foreach (object content in contents)
            {
                string t = content.GetType().ToString();
                switch (t)
                {
                    case "System.Int16":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((short)content));
                            break;
                        }
                    case "System.Int32":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((int)content));
                            break;
                        }
                    case "System.Int64":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((long)content));
                            break;
                        }
                    case "System.UInt16":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((ushort)content));
                            break;
                        }
                    case "System.UInt32":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((uint)content));
                            break;
                        }
                    case "System.UInt64":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((ulong)content));
                            break;
                        }
                    case "System.Single":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((float)content));
                            break;
                        }
                    case "System.Double":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((double)content));
                            break;
                        }
                    case "System.Byte":
                        {
                            translateTarget.AddRange(ValueHelper.Instance.GetBytes((byte)content));
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException("没有实现除整数以外的其它转换方式");
                        }
                }
            }
            //最后把队列转换为数组
            return translateTarget.ToArray();
        }
    }

    /// <summary>
    /// 输入结构
    /// </summary>
    public class InputStruct
    {
    }

    /// <summary>
    /// 输出结构
    /// </summary>
    public class OutputStruct
    {
    }
}