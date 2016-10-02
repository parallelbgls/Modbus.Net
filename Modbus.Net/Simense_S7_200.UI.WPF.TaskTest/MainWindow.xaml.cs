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
using Modbus.Net;
using Modbus.Net.Siemens;

namespace Siemens_S7_200.UI.WPF.TaskTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEnumerable<string> value = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            //增加需要通信的PLC地址
            List<AddressUnit> addressUnits = new List<AddressUnit>
            {
                new AddressUnit() {Id = "0", Area = "V", Address = 1, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 1},
                new AddressUnit() {Id = "1", Area = "V", Address = 3, CommunicationTag = "D2", DataType = typeof (float), Zoom = 1}
            };
            //初始化任务管理器
            TaskManager task = new TaskManager(10, 300, true);
            //向任务管理器中添加设备
            task.AddMachine(new SiemensMachine(SiemensType.Tcp, "192.168.3.11",SiemensMachineModel.S7_300, addressUnits,
            true, 2, 0));
            //增加值返回时的处理函数
            task.ReturnValues += (returnValues) =>
            {
                //唯一的参数包含返回值，是一个唯一标识符（machine的第二个参数），返回值（类型ReturnUnit）的键值对。
                value = new List<string>();
                if (returnValues.ReturnValues != null)
                {
                    value = from val in returnValues.ReturnValues select val.Key + " " + val.Value.PlcValue;
                    siemensItems.Dispatcher.Invoke(() => siemensItems.ItemsSource = value);
                }
                else
                {
                    Console.WriteLine($"ip {returnValues.MachineId} not return value");
                }
            };
            //启动任务
            task.TaskStart();
        }
    }
}
