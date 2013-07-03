using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Threading;

namespace fontindexer
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class Window1 : Window, INotifyPropertyChanged
    {

        private String _fontfolder;
        public String FontFolder
        {
            set { this._fontfolder = value; this.OnPropertyChanged("FontFolder"); }
            get { return this._fontfolder; }
        }

        private String _csvfile;
        public String CsvFile
        {
            set { this._csvfile = value; this.OnPropertyChanged("CsvFile"); }
            get { return this._csvfile; }
        }

        private int _promax;
        public int ProMax
        {
            set { this._promax = value; this.OnPropertyChanged("ProMax"); }
            get { return this._promax; }
        }

        private int _proval;
        public int ProVal
        {
            set { this._proval = value; this.OnPropertyChanged("ProVal"); }
            get { return this._proval; }
        }

        private String _protext;
        public String ProText
        {
            set { this._protext = value; this.OnPropertyChanged("ProText"); }
            get { return this._protext; }
        }

        BackgroundWorker bgw;

        public Window1()
        {
            this.DataContext = this;
            this.ProVal = 0;
            this.ProMax = 1;
            this.ProText = "0/0";
            InitializeComponent(); 

            // Build font folder path
            DirectoryInfo dirWindowsFolder = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System));
            string strFontsFolder = System.IO.Path.Combine(dirWindowsFolder.FullName, "Fonts");
            this.FontFolder = strFontsFolder;

            // Build csv file path
            DirectoryInfo desktop = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            string csvfile = System.IO.Path.Combine(desktop.FullName, "fontlist-"+DateTime.Now.ToString("yyyyMMddHmmss")+".csv");
            this.CsvFile = csvfile;

        }



        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(
                    this, new PropertyChangedEventArgs(propName));
        }

        private void OnSelectCsvFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";
            dlg.FileName = this.CsvFile;
            dlg.InitialDirectory = (new FileInfo(this.CsvFile)).Directory.FullName;

            if (dlg.ShowDialog(this) == true)
            {
                this.CsvFile = dlg.FileName;
            }

        }

        private void OnSelectFontFolder(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = this.FontFolder;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.FontFolder = dlg.SelectedPath;
            }
        }

        private void OnGenerateCsvFile(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            this.ProVal = 0;
            this.bgw = new BackgroundWorker();
            this.bgw.DoWork += bw_DoWork;
            this.bgw.RunWorkerCompleted += bw_RunWorkerCompleted;
            this.bgw.RunWorkerAsync();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsEnabled = true;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Create table
                DataTable tbl = new DataTable();
                tbl.Columns.Add(new DataColumn() { ColumnName = "Font Name" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Font Path" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Font Filename" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Filesize" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Create Date" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Modification Date" });

                DataRow headline = tbl.NewRow();
                headline["Font Name"] = "Font Name";
                headline["Font Path"] = "Font Path";
                headline["Font Filename"] = "Font Filename";
                headline["Filesize"] = "Filesize";
                headline["Create Date"] = "Create Date";
                headline["Modification Date"] = "Modification Date";
                tbl.Rows.Add(headline);

                // Scan folder
                String fontfolder = "";
                this.Dispatcher.Invoke(new Action(() => { fontfolder = this.FontFolder; }));
                string[] files = Directory.GetFiles(fontfolder);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.ProMax = files.Length;
                    this.ProText = "0/" + files.Length;
                }));

                foreach (String file in files)
                {
                    try
                    {
                        FileInfo fontfile = new FileInfo(file);
                        DataRow newrow = tbl.NewRow();

                        // Font Name
                        System.Drawing.Text.PrivateFontCollection fontCol = new System.Drawing.Text.PrivateFontCollection();
                        fontCol.AddFontFile(file);
                        newrow["Font Name"] = fontCol.Families[0].Name;

                        // Font path
                        newrow["Font Path"] = fontfile.Directory.FullName;

                        // Filename
                        newrow["Font Filename"] = fontfile.Name;

                        // Size
                        newrow["Filesize"] = Convert.ToInt32((fontfile.Length / 1024)) + "KB";

                        // Create Date
                        newrow["Create Date"] = fontfile.CreationTime.ToString("yyyy-MM-dd H:mm:ss");

                        // Mod Date
                        newrow["Modification Date"] = fontfile.LastWriteTime.ToString("yyyy-MM-dd H:mm:ss");

                        tbl.Rows.Add(newrow);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.ProVal++;
                            this.ProText = this.ProVal + "/" + this.ProMax;
                        }));
                    }
                }

                // Generate CSV File
                StringBuilder builder = new StringBuilder();
                foreach (DataRow row in tbl.AsEnumerable())
                {
                    String[] stringArray = row.ItemArray.Cast<string>().ToArray();
                    builder.AppendLine("\"" + string.Join("\";\"", stringArray) + "\"");
                }

                String csvfile = "";
                this.Dispatcher.Invoke(new Action(() => { csvfile = this.CsvFile; }));
                File.WriteAllText(csvfile, builder.ToString());

            } 
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}
