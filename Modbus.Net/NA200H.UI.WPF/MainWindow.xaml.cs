using System;
using System.Collections.Generic;
using System.Threading;
using ModBus.Net;
using System.Windows;


namespace NA200H.UI.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private BaseUtility utility;
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //utility = new ModbusUtility(ModbusType.Tcp, "192.168.3.247");
            //utility.AddressTranslator = new AddressTranslatorNA200H();
            //object[] getNum = utility.GetDatas(0x02, 0x00, "NW1", new KeyValuePair<Type, int>(typeof(ushort), 4));
            utility = new SimenseUtility(SimenseType.Tcp, "192.168.3.191", SimenseMachineModel.S7_200);
            utility.AddressTranslator = new AddressTranslatorSimense();
            object[] getNum = utility.GetDatas(0x02, 0x00, "V1", new KeyValuePair<Type, int>(typeof(ushort), 4));
            ushort[] getNumUshorts = ValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(getNum);
            SetValue(getNumUshorts);
        }

        private void SetValue(ushort[] getNum)
        {
            Add1.Text = getNum[0].ToString();
            Add2.Text = getNum[1].ToString();
            Add3.Text = getNum[2].ToString();
            AddAns.Text = getNum[3].ToString();
        }

        private void Calc_OnClick(object sender, RoutedEventArgs e)
        {
            ushort add1 = 0, add2 = 0, add3 = 0;
            ushort.TryParse(Add1.Text, out add1);
            ushort.TryParse(Add2.Text, out add2);
            ushort.TryParse(Add3.Text, out add3);
            //utility.SetDatas(0x02, 0x00, "NW1", new object[] { add1, add2, add3 });
            utility.SetDatas(0x02, 0x00, "V1", new object[] { add1, add2, add3 });
            Thread.Sleep(100);
            //object[] getNum = utility.GetDatas(0x02, 0x00, "NW1", new KeyValuePair<Type, int>(typeof(ushort), 4));
            object[] getNum = utility.GetDatas(0x02, 0x00, "V1", new KeyValuePair<Type, int>(typeof(ushort), 4));
            ushort[] getNumUshorts = ValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(getNum);
            SetValue(getNumUshorts);
        }
    }
}
