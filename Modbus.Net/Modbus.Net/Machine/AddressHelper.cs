using System;

namespace Modbus.Net
{
    /// <summary>
    ///     地址辅助类
    /// </summary>
    public static class AddressHelper
    {
        /// <summary>
        ///     将字节坐标变为协议坐标
        /// </summary>
        /// <param name="abstractAddress">字节坐标地址</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double MapAbstractCoordinateToProtocolCoordinate(double abstractAddress, int startAddress,
            double byteLength)
        {
            return abstractAddress / byteLength + startAddress;
        }

        /// <summary>
        ///     将协议坐标变为字节坐标
        /// </summary>
        /// <param name="protocolAddress">协议坐标地址</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double MapProtocolCoordinateToAbstractCoordinate(double protocolAddress, int startAddress,
            double byteLength)
        {
            return (protocolAddress - startAddress) * byteLength;
        }

        /// <summary>
        ///     将协议获取数变为字节获取数
        /// </summary>
        /// <param name="protocolGetCount">协议坐标获取个数</param>
        /// <param name="areaLength">协议坐标区域与字节之间的防缩倍数</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double MapProtocolGetCountToAbstractByteCount(double protocolGetCount, double areaLength,
            double byteLength)
        {
            return protocolGetCount * areaLength + byteLength;
        }

        /// <summary>
        ///     获取协议坐标
        /// </summary>
        /// <param name="address">主地址</param>
        /// <param name="subAddress">子地址</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double GetProtocolCoordinate(int address, int subAddress, double byteLength)
        {
            return address + subAddress * (0.125 / byteLength);
        }

        /// <summary>
        ///     获取字节坐标
        /// </summary>
        /// <param name="address">主地址</param>
        /// <param name="subAddress">子地址</param>
        /// <returns></returns>
        public static double GetAbstractCoordinate(int address, int subAddress)
        {
            return address + subAddress * 0.125;
        }

        /// <summary>
        ///     获取协议坐标下一个数据的位置
        /// </summary>
        /// <param name="protocolAddress">协议坐标地址</param>
        /// <param name="nextPositionBetweenType">间隔的数据类型</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double GetProtocolCoordinateNextPosition(double protocolAddress, Type nextPositionBetweenType,
            double byteLength)
        {
            return protocolAddress +
                   ValueHelper.ByteLength[nextPositionBetweenType.FullName] / byteLength;
        }

        /// <summary>
        ///     获取字节坐标下一个数据的位置
        /// </summary>
        /// <param name="abstractAddress">字节坐标地址</param>
        /// <param name="nextPositionBetweenType">间隔的数据类型</param>
        /// <returns></returns>
        public static double GetAbstractCoordinateNextPosition(double abstractAddress, Type nextPositionBetweenType)
        {
            return abstractAddress +
                   ValueHelper.ByteLength[nextPositionBetweenType.FullName];
        }
    }
}