using System;
using System.Collections.Generic;
using System.Threading;
using ModBus.Net;
using System.Windows;
using ModBus.Net.Modbus;
using ModBus.Net.Siemens;


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
            //utility = new ModbusUtility(ModbusType.Tcp, "192.168.3.12");
            //utility.AddressTranslator = new AddressTranslatorNA200H();
            //object[] getNum = utility.GetDatas(0x02, 0x00, "NW 1", new KeyValuePair<Type, int>(typeof(ushort), 4));
            utility = new SiemensUtility(SiemensType.Tcp, "192.168.3.11", SiemensMachineModel.S7_300);
            utility.AddressTranslator = new AddressTranslatorSiemens();
            object[] getNum = utility.GetDatas(0x02, 0x00, "V 1", new KeyValuePair<Type, int>(typeof(ushort), 4));
            ushort[] getNumUshorts = BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(getNum);
            SetValue(getNumUshorts);
        }

        private void GetMachineEnter()
        {
            //machine = new ModbusMachine(ModbusType.Tcp, "192.168.3.12", new List<AddressUnit>()
            //{
                //new AddressUnit() {Id = 1, Area = "NW", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                //new AddressUnit() {Id = 2, Area = "NW", Address = 3, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                //new AddressUnit() {Id = 3, Area = "NW", Address = 5, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                //new AddressUnit() {Id = 4, Area = "NW", Address = 7, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0}
            //});
            //machine.AddressFormater = new AddressFormaterNA200H();
            //machine.AddressTranslator = new AddressTranslatorNA200H();
            machine = new SiemensMachine(SiemensType.Tcp, "192.168.3.11", SiemensMachineModel.S7_300, new List<AddressUnit>()
            {
                new AddressUnit() {Id = 1, Area = "V", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                new AddressUnit() {Id = 2, Area = "V", Address = 3, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                new AddressUnit() {Id = 3, Area = "V", Address = 5, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                new AddressUnit() {Id = 4, Area = "V", Address = 7, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0}
            });
            var result = machine.GetDatas();
            var resultFormat = BaseMachine.MapGetValuesToSetValues(result);
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
            //utility.SetDatas(0x02, 0x00, "NW 1", new object[] { add1, add2, add3 });
            utility.SetDatas(0x02, 0x00, "V 1", new object[] { add1, add2, add3 });
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
