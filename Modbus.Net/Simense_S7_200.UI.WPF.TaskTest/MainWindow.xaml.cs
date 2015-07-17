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

namespace Simense_S7_200.UI.WPF.TaskTest
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
            List<AddressUnit> addressUnits = new List<AddressUnit>
            {
                new AddressUnit() {Id = 0, Area = "V", Address = 0, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 1},
                new AddressUnit() {Id = 1, Area = "V", Address = 2, CommunicationTag = "D2", DataType = typeof (float), Zoom = 1}
            };
            TaskManager task = new TaskManager(20, 300, true);
            task.AddMachine(new SimenseMachine(SimenseType.Tcp, "192.168.3.191",SimenseMachineModel.S7_200, addressUnits,
            true));
            task.ReturnValues += (returnValues) =>
            {
                value = new List<string>();
                if (returnValues.Value != null)
                {
                    value = from val in returnValues.Value select val.Key + val.Value;
                    simenseItems.Dispatcher.Invoke(() => simenseItems.ItemsSource = value);
                }
                else
                {
                    Console.WriteLine(String.Format("ip {0} not return value", returnValues.Key));
                }
            };
            task.TaskStart();
        }
    }
}
