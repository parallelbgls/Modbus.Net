using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModBus.Net;

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
            ModbusTCPProtocal wrapper = new ModbusTCPProtocal();
            ReadCoilStatusTCPProtocal.ReadCoilStatusInputStruct readCoilStatusInputStruct = new ReadCoilStatusTCPProtocal.ReadCoilStatusInputStruct(11, "Q1", 3);
            ReadCoilStatusTCPProtocal.ReadCoilStatusOutputStruct readCoilStatusOutputStruct = wrapper.Protocals[ReadCoilStatusTCPProtocal.GetType()].SendReceive(readCoilStatusInputStruct);
            for (int i = 0; i < readCoilStatusOutputStruct.CoilStatus.Length; i++)
            {
                System.Console.WriteLine(readCoilStatusOutputStruct.CoilStatus[i]);
            }
            System.Console.Read();
        }
    }
}
