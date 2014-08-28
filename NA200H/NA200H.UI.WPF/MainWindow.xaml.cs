using System;
using ModBus.Net;
using System.Windows;


namespace NA200H.UI.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BaseProtocal wrapper = new ModbusRtuProtocal();
            ReadHoldRegisterModbusProtocal.ReadHoldRegisterInputStruct readHoldRegisterInputStruct = new ReadHoldRegisterModbusProtocal.ReadHoldRegisterInputStruct(2, "0", 4);
            ReadHoldRegisterModbusProtocal.ReadHoldRegisterOutputStruct readHoldRegisterOutputStruct = (ReadHoldRegisterModbusProtocal.ReadHoldRegisterOutputStruct)wrapper.SendReceive(wrapper["ReadHoldRegisterModbusProtocal"], readHoldRegisterInputStruct);
            for (int i = 0; i < readHoldRegisterOutputStruct.HoldRegisterStatus.Length; i++)
            {
                Console.WriteLine(readHoldRegisterOutputStruct.HoldRegisterStatus[i]);
            }
            Console.Read();
        }
    }
}
