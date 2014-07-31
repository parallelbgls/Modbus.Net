using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModBus.Net
{
    public abstract class BaseProtocal
    {
        /// <summary>
        ///     发送数据
        /// </summary>
        /// <param name="content">需要发送的数据</param>
        /// <returns>数据是否正确接收</returns>
        protected ProtocalLinker _protocalLinker;

        protected BaseProtocal()
        {
            Protocals = new Dictionary<string, ProtocalUnit>();
        }

        public ProtocalUnit this[string protocalName]
        {
            get
            {
                if (Protocals.ContainsKey(protocalName))
                {
                    return Protocals[protocalName];
                }
                var protocalUnit =
                    Assembly.Load("ModBus.Net").CreateInstance("ModBus.Net." + protocalName) as ProtocalUnit;
                if (protocalUnit == null) throw new InvalidCastException("没有相应的协议内容");
                Register(protocalUnit);
                return Protocals[protocalName];
            }
        }

        protected Dictionary<string, ProtocalUnit> Protocals { get; private set; }

        public void Register(ProtocalUnit linkProtocal)
        {
            if (linkProtocal == null) return;
            Protocals.Add(linkProtocal.GetType().Name, linkProtocal);
        }

        public virtual OutputStruct SendReceive(ProtocalUnit unit, params object[] content)
        {
            int t = 0;
            return unit.Unformat(_protocalLinker.SendReceive(unit.Format(content)), ref t);
        }

        public virtual OutputStruct SendReceive(ProtocalUnit unit, InputStruct content)
        {
            int t = 0;
            return unit.Unformat(_protocalLinker.SendReceive(unit.Format(content)), ref t);
        }

        /// <summary>
        ///     接收数据
        /// </summary>
        /// <returns>接收到的数据</returns>
        public virtual bool SendOnly(ProtocalUnit unit, params object[] content)
        {
            return _protocalLinker.SendOnly(unit.Format(content));
        }

        public virtual bool SendOnly(ProtocalUnit unit, InputStruct content)
        {
            return _protocalLinker.SendOnly(unit.Format(content));
        }
    }
}