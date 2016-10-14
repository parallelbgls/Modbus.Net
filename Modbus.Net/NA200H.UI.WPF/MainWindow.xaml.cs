using System;
using System.Collections.Generic;
using System.Threading;
using Modbus.Net;
using System.Windows;
using Modbus.Net.Modbus;
using Modbus.Net.Siemens;


namespace NA200H.UI.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private BaseUtility utility;
        private BaseMachine machine;
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //GetUtilityEnter();
            GetMachineEnter();
        }

        private void GetUtilityEnter()
        {
            if (utility == null)
            {
                utility = new ModbusUtility(ModbusType.Tcp, "192.168.3.12", 2, 0);
                utility.AddressTranslator = new AddressTranslatorNA200H();
                //utility = new SiemensUtility(SiemensType.Tcp, "192.168.3.11", SiemensMachineModel.S7_300, 2, 0);
                //utility.AddressTranslator = new AddressTranslatorSiemens();
            }
            object[] getNum = utility.GetDatas("NW 1", new KeyValuePair<Type, int>(typeof(ushort), 4));           
            //object[] getNum = utility.GetDatas("V 1", new KeyValuePair<Type, int>(typeof(ushort), 4));
            ushort[] getNumUshorts = BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(getNum);
            SetValue(getNumUshorts);
        }

        private void GetMachineEnter()
        {
            if (machine == null)
            { 
                //machine = new ModbusMachine(ModbusType.Rtu, "COM3", new List<AddressUnit>()
                //{
                   //new AddressUnit() {Id = "1", Area = "4X", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    //new AddressUnit() {Id = "2", Area = "4X", Address = 2, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    //new AddressUnit() {Id = "3", Area = "4X", Address = 3, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    //new AddressUnit() {Id = "4", Area = "4X", Address = 4, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                //}, 2, 0);
                //machine.AddressCombiner = new AddressCombinerContinus(machine.AddressTranslator);
                //machine.AddressCombinerSet = new AddressCombinerContinus(machine.AddressTranslator);
                machine = new SiemensMachine(SiemensType.Tcp, "192.168.3.11", SiemensMachineModel.S7_300, new List<AddressUnit>()
                {
                    new AddressUnit() {Id = "1", Area = "V", Address = 0, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "2", Area = "V", Address = 2, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "3", Area = "V", Address = 4, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "4", Area = "V", Address = 6, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0}
                }, 2, 0);
                machine.AddressCombiner = new AddressCombinerContinus(machine.AddressTranslator);
                machine.AddressCombinerSet = new AddressCombinerContinus(machine.AddressTranslator);
            }
            var resultFormat = machine.GetDatas(MachineGetDataType.CommunicationTag).MapGetValuesToSetValues();
            SetValue(new ushort[4] {(ushort)resultFormat["Add1"], (ushort)resultFormat["Add2"], (ushort)resultFormat["Add3"], (ushort)resultFormat["Ans"]});           
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
            //SetUtilityEnter();
            SetMachineEnter();
        }

        private void SetUtilityEnter()
        {
            ushort add1 = 0, add2 = 0, add3 = 0;
            ushort.TryParse(Add1.Text, out add1);
            ushort.TryParse(Add2.Text, out add2);
            ushort.TryParse(Add3.Text, out add3);
            //utility.SetDatas("NW 1", new object[] { add1, add2, add3 });
            utility.SetDatas("V 1", new object[] { add1, add2, add3 });
            Thread.Sleep(100);
            GetUtilityEnter();
        }

        private void SetMachineEnter()
        {
            ushort add1 = 0, add2 = 0, add3 = 0;
            ushort.TryParse(Add1.Text, out add1);
            ushort.TryParse(Add2.Text, out add2);
            ushort.TryParse(Add3.Text, out add3);
            var setDic = new Dictionary<string, double> {{"Add1", add1}, {"Add2", add2}, {"Add3", add3}};
            machine.SetDatas(MachineSetDataType.CommunicationTag, setDic);
            //var setDic = new Dictionary<string, double>{{"V 1", add1}, {"V 3", add2}, {"V 5", add3}};
            //machine.SetDatas(MachineSetDataType.Address, setDic);
            Thread.Sleep(100);
            GetMachineEnter();
        }
    }
}
