using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ModBus.Net
{
    /// <summary>
    /// 基本协议
    /// </summary>
    public abstract class BaseProtocal
    {
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="content">需要发送的数据</param>
        /// <returns>数据是否正确接收</returns>
        public ProtocalLinker ProtocalLinker { get; protected set; }

        protected BaseProtocal()
        {
            Protocals = new Dictionary<string, ProtocalUnit>();
        }

        /// <summary>
        /// 协议索引器，这是一个懒加载协议，当字典中不存在协议时自动加载协议，否则调用已经加载的协议
        /// </summary>
        /// <param name="protocalName">协议的类的名称</param>
        /// <returns></returns>
        public ProtocalUnit this[Type type]
        {
            get
            {
                string protocalName = type.FullName;
                if (Protocals.ContainsKey(protocalName))
                {
                    return Protocals[protocalName];
                }
                //自动寻找存在的协议并将其加载
                var protocalUnit =
                    Assembly.Load("ModBus.Net").CreateInstance(protocalName) as ProtocalUnit;
                if (protocalUnit == null) throw new InvalidCastException("没有相应的协议内容");
                Register(protocalUnit);
                return Protocals[protocalName];
            }
        }

        protected Dictionary<string, ProtocalUnit> Protocals { get; private set; }

        protected void Register(ProtocalUnit linkProtocal)
        {
            if (linkProtocal == null) return;
            Protocals.Add(linkProtocal.GetType().FullName, linkProtocal);
        }

        /// <summary>
        /// 发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual byte[] SendReceive(params object[] content)
        {
            return ProtocalLinker.SendReceive(ProtocalUnit.TranslateContent(content));
        }

        /// <summary>
        /// 发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual OutputStruct SendReceive(ProtocalUnit unit, InputStruct content)
        {
            int t = 0;
            //如果为特别处理协议的话，跳过协议扩展收缩
            if (unit is SpecialProtocalUnit)
            {
                return unit.Unformat(ProtocalLinker.SendReceiveWithoutExtAndDec(unit.Format(content)), ref t);
            }
            else
            {
                return unit.Unformat(ProtocalLinker.SendReceive(unit.Format(content)), ref t);
            }
        }

        /// <summary>
        /// 协议连接开始
        /// </summary>
        /// <returns></returns>
        public abstract bool Connect();

        /// <summary>
        /// 协议连接断开
        /// </summary>
        /// <returns></returns>
        public virtual bool Disconnect()
        {
            if (ProtocalLinker != null)
            {
                return ProtocalLinker.Disconnect();
            }
            return false;
        }
    }
}