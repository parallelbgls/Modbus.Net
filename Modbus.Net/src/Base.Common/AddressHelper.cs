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
        public static double MapAbstractCoordinateToProtocalCoordinate(double abstractAddress, int startAddress,
            double byteLength)
        {
            return abstractAddress / byteLength + startAddress;
        }

        /// <summary>
        ///     将协议坐标变为字节坐标
        /// </summary>
        /// <param name="protocalAddress">协议坐标地址</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double MapProtocalCoordinateToAbstractCoordinate(double protocalAddress, int startAddress,
            double byteLength)
        {
            return (protocalAddress - startAddress) * byteLength;
        }

        /// <summary>
        ///     将协议获取数变为字节获取数
        /// </summary>
        /// <param name="protocalGetCount">协议坐标获取个数</param>
        /// <param name="areaLength">协议坐标区域与字节之间的防缩倍数</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double MapProtocalGetCountToAbstractByteCount(double protocalGetCount, double areaLength,
            double byteLength)
        {
            return protocalGetCount * areaLength + byteLength;
        }

        /// <summary>
        ///     获取协议坐标
        /// </summary>
        /// <param name="address">主地址</param>
        /// <param name="subAddress">子地址</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double GetProtocalCoordinate(int address, int subAddress, double byteLength)
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
        /// <param name="protocalAddress">协议坐标地址</param>
        /// <param name="nextPositionBetweenType">间隔的数据类型</param>
        /// <param name="byteLength">协议坐标单个地址的字节长度</param>
        /// <returns></returns>
        public static double GetProtocalCoordinateNextPosition(double protocalAddress, Type nextPositionBetweenType,
            double byteLength)
        {
            return protocalAddress +
                   BigEndianValueHelper.Instance.ByteLength[nextPositionBetweenType.FullName] / byteLength;
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
                   BigEndianValueHelper.Instance.ByteLength[nextPositionBetweenType.FullName];
        }
    }
}