using System;
using System.Collections.Generic;
using System.IO;
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

namespace PhotoOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SourceDirectory
        {
            get
            {
                return txtSource.Text.Trim();
            }
        }

        private string DestinationDirectory
        {
            get
            {
                return txtDestination.Text.Trim();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSource_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog(((Control)sender).Parent, "txtSource");
        }

        private void btnDestination_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog(((Control)sender).Parent, "txtDestination");
        }

        private void OpenFileDialog(DependencyObject parent, string controlName)
        {
            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //Nullable<bool> result = dlg.ShowDialog();

            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TextBox txt = UIHelper.FindChild<TextBox>(parent, controlName);
                txt.Text = dialog.SelectedPath;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string [] files = Directory.GetFiles(txtSource.Text.Trim(), "*.*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    string folderName = fileInfo.LastWriteTime.Year.ToString();
                    if (rdlGroupByMonth.IsChecked.Value)
                    {
                        folderName = fileInfo.LastWriteTime.ToString("MMMM") + " " + folderName;
                    }
                    if (!Directory.Exists(DestinationDirectory + "/" + folderName))
                    {
                        Directory.CreateDirectory(DestinationDirectory + "/" + folderName);
                    }
                    fileInfo.CopyTo(DestinationDirectory + "/" + folderName + "/" + fileInfo.Name, true);
                }
            }
        }

        private bool ValidateInput()
        {
            if (!Directory.Exists(SourceDirectory))
            {
                return false;
            }
            if (!Directory.Exists(DestinationDirectory))
            {
                return false;
            }
            return true;
        }
    }
}
