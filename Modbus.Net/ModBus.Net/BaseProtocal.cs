using System;
using System.Collections.Generic;
using System.Reflection;

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
        protected ProtocalLinker _protocalLinker;

        protected BaseProtocal()
        {
            Protocals = new Dictionary<string, ProtocalUnit>();
        }

        /// <summary>
        /// 协议索引器，这是一个懒加载协议，当字典中不存在协议时自动加载协议，否则调用已经加载的协议
        /// </summary>
        /// <param name="protocalName">协议的类的名称</param>
        /// <returns></returns>
        public ProtocalUnit this[string protocalName]
        {
            get
            {
                if (Protocals.ContainsKey(protocalName))
                {
                    return Protocals[protocalName];
                }
                //自动寻找存在的协议并将其加载
                var protocalUnit =
                    Assembly.Load("ModBus.Net").CreateInstance("ModBus.Net." + protocalName) as ProtocalUnit;
                if (protocalUnit == null) throw new InvalidCastException("没有相应的协议内容");
                Register(protocalUnit);
                return Protocals[protocalName];
            }
        }

        protected Dictionary<string, ProtocalUnit> Protocals { get; private set; }

        protected void Register(ProtocalUnit linkProtocal)
        {
            if (linkProtocal == null) return;
            Protocals.Add(linkProtocal.GetType().Name, linkProtocal);
        }

        /// <summary>
        /// 发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual byte[] SendReceive(params object[] content)
        {
            int t;
            return _protocalLinker.SendReceive(ProtocalUnit.TranslateContent(content));
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
            return unit.Unformat(_protocalLinker.SendReceive(unit.Format(content)), ref t);
        }

        /// <summary>
        /// 仅发送数据
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual bool SendOnly(ProtocalUnit unit, params object[] content)
        {
            return _protocalLinker.SendOnly(unit.Format(content));
        }

        /// <summary>
        /// 仅发送数据
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual bool SendOnly(ProtocalUnit unit, InputStruct content)
        {
            return _protocalLinker.SendOnly(unit.Format(content));
        }
    }
}