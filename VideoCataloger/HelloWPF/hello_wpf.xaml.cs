#region hello_wpf
//css_ref WindowsBase;
//css_ref PresentationCore;
//css_ref PresentationFramework;
//css_ref System.Runtime
//css_ref System.ObjectModel

using System;
using System.Xaml;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using VideoCataloger;

namespace VideoCataloger
{

    public partial class HelloWindow : Window
    {
        DependencyObject m_RootElement;

        public HelloWindow()
        {
            // setup window properties
            Width = 320;
            Height = 200;
            Title = "Dynamic WPF window";

            // load the xaml code to be used as content for this window
            string current_folder = Directory.GetCurrentDirectory();
            string path = current_folder + "\\scripts\\HelloWPF\\hello_wpf.xaml";
            using (FileStream fs = new FileStream( path, FileMode.Open))
            {
                m_RootElement = (DependencyObject) System.Windows.Markup.XamlReader.Load(fs);
            }

            // setup events for elements in xaml
            Button btn = (Button)LogicalTreeHelper.FindLogicalNode(m_RootElement, "button1");
            btn.Click += button1_Click;

            // use the loaded scene
            this.Content = m_RootElement;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox) LogicalTreeHelper.FindLogicalNode(m_RootElement, "input1");
            MessageBox.Show("Message is: " + textbox.Text);
        }



        [STAThread]
        public static void Main()
        {
            // not used as entry point for script but needed for compilation
        }
    }

    class Script
    {
        static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
        {
            HelloWindow wnd = new HelloWindow();
            wnd.ShowDialog();
        }
    }

}
#endregion
