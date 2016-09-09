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
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel;

namespace PhotoOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker backgroundWorker;

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
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            //For the display of operation progress to UI.    
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            //After the completation of operation.    
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
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
            btnStart.IsEnabled = false;
            if (ValidateInput())
            {
                WorkProps props = new WorkProps();
                props.SourceFolder = txtSource.Text.Trim();
                props.DestinationFolder = txtDestination.Text.Trim();
                props.IsByMonth = rdlGroupByMonth.IsChecked.Value;
                backgroundWorker.RunWorkerAsync(props);
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkProps props = (WorkProps)e.Argument;

            string[] files = Directory.GetFiles(props.SourceFolder, "*.*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Count(); i++)
            {
                using (System.Drawing.Image image = new Bitmap(files[i]))
                {
                    RotateImage(image);
                    PropertyItem propertyItem = image.GetPropertyItem(36867);


                    ASCIIEncoding encoding = new ASCIIEncoding();
                    string text = encoding.GetString(propertyItem.Value, 0, propertyItem.Len - 1);

                    // Parse the date and time. 
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime dateTaken = DateTime.ParseExact(text, "yyyy:MM:d H:m:s", provider);

                    FileInfo fileInfo = new FileInfo(files[i]);
                    string folderName = dateTaken.Year.ToString();
                    if (props.IsByMonth)
                    {
                        folderName = dateTaken.ToString("MMMM") + " " + folderName;
                    }
                    if (!Directory.Exists(props.DestinationFolder + "/" + folderName))
                    {
                        Directory.CreateDirectory(props.DestinationFolder + "/" + folderName);
                    }
                    fileInfo.CopyTo(props.DestinationFolder + "/" + folderName + "/" + fileInfo.Name, true);
                }
                backgroundWorker.ReportProgress(i * 100 / files.Count());
            }
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgProgress.Value = e.ProgressPercentage;
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStart.IsEnabled = true;
        }

        private void RotateImage(System.Drawing.Image img)
        {
            if (Array.IndexOf(img.PropertyIdList, 274) > -1)
            {
                var orientation = (int)img.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        return;
                    case 2:
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        img.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        img.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        img.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                img.RemovePropertyItem(274);
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
