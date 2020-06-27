using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
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
using System.Windows.Threading;

namespace test_progressbar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        private int percent = 0;
        public int Percent
        {
            set
            {
                if (percent != value)
                {
                    percent = value;
                    Notify("Percent");
                }
            }
            get { return percent; }
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void begin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Add event handler implementation here.
            //异步更新UI
            Task.Factory.StartNew(() =>
            {
                 Dispatcher.BeginInvoke(new Action(() =>
                {
                    pb.Value = 0;
                }));
               
                for (int i = 0; i <= 100; i++)
                {
                    //当前进度，最大值默认100
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        pb.Value = i;
                    }));
                    Thread.Sleep(100);//do some work
                }
            });
        }
        //winform
        private void begin2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            //同步更新UI
            pb.Value = 0;
            for (int i = 0; i <= 100; i++)
            {
                /*需要引用system.windows.forms : referenece */
                /*
                 * DoEvent 的原理是execution loop，也就是以轮训的方式來查詢是否有要更新的UI(message)
                 * 循环太多CPU会飙升
                 * 
                 */
                System.Windows.Forms.Application.DoEvents();
                pb.Value = i;
              
                Thread.Sleep(100);//do some work
            }
        }
        //wpf
        private void begin3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Add event handler implementation here.
            //同步更新UI
            pb.Value = 0;
            for (int i = 0; i <= 100; i++)
            {
                /* 不需要引用system.windows.forms : referenece */
                /* https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcherframe?redirectedfrom=MSDN&view=netframework-4.7.2
                 * DoEvent 的原理是execution loop，也就是以轮训的方式來查詢是否有要更新的UI(message)
                 * 循环太多CPU会飙升
                 * 
                 */
                UI.DoEvents();
                pb.Value = i;

                Thread.Sleep(100);//do some work
            }
        }

        private void Notify_Click(object sender, RoutedEventArgs e)
        {
            // 异步更新UI 多线程中建议使用这种方法进行UI界面的更新，
            // wpf的UI线程只有一个，如果UI线程没有返回，则界面不能得到更新
            // UI主线程也就是执行该Notify_Click的线程，比如这里要传送文件，
            // 需要个过程才返回，则不会更新UI。这时如果需要更新UI要使用
            // 上面2个例子中的System.Windows.Forms.Application.DoEvents();强制刷新所有UI
            this.DataContext = this;
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(100);
                    Percent = i/100;
                }
            });
        }
    }

    //The following example shows how to use a DispatcherFrame to achieve similar results as the Windows Forms DoEvents method. 
    public class UI
    {
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        static public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        static public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
    }
}
