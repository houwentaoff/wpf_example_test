using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace asyncui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Task.Factory.StartNew(() =>
                    {
                        //工作任务
                    });
            // UI任务
            Task.Factory.StartNew(() =>
            {
                //第一个UI 异步任务
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    btn.Background = Brushes.Green;
                    //第二个UI 异步任务
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(2000);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            btn.Background = Brushes.Transparent;
                            btn.Background = Brushes.Black;
                        }));
                    });
                    //Thread.Sleep(2000);//投递的UI效果中不能sleep 否则会使UI sleep
                    //btn.Background = Brushes.Transparent;
                }
                )
                );
            });
        }
    }
}
