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
using System.Text.RegularExpressions;

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

        private bool _includeunknown;
        public bool IncludeUnknown
        {
            set { this._includeunknown = value; this.OnPropertyChanged("IncludeUnknown"); }
            get { return this._includeunknown; }
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
            this.ProText += " Done!";
            this.ProVal = 0;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Create table
                DataTable tbl = new DataTable();
                tbl.Columns.Add(new DataColumn() { ColumnName = "Name" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Type" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Path" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Filename" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Filesize" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Create" });
                tbl.Columns.Add(new DataColumn() { ColumnName = "Modification" });

                DataRow headline = tbl.NewRow();
                headline["Name"] = "Font Name";
                headline["Type"] = "Font Type";
                headline["Path"] = "Font Path";
                headline["Filename"] = "Font Filename";
                headline["Filesize"] = "Filesize";
                headline["Create"] = "Create Date";
                headline["Modification"] = "Modification Date";
                tbl.Rows.Add(headline);

                // Scan folder
                String fontfolder = "";
                bool includeunknown = false;
                this.Dispatcher.Invoke(new Action(() => { 
                    fontfolder = this.FontFolder; 
                    includeunknown = this.IncludeUnknown;
                }));
                string[] files = Directory.GetFiles(fontfolder);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.ProMax = files.Length;
                    this.ProText = "0/" + files.Length;
                }));

                foreach (String file in files)
                {
                    FileInfo fontfile = new FileInfo(file);
                    String extension = fontfile.Extension.ToLower();
                    DataRow newrow = tbl.NewRow();

                    if ((extension.Equals(".pfa") || extension.Equals(".pfb") ||
                        extension.Equals(".afm") || extension.Equals(".ttf")) == false && 
                        includeunknown == false)
                    {
                        this.ProVal++;
                        continue;
                    }
                    

                    // Default values
                    newrow["Name"] = "ERR: File extension not supported";
                    newrow["Type"] = extension.Trim(new char[] { '.' }).ToUpper();
                    newrow["Path"] = fontfile.Directory.FullName;
                    newrow["Filename"] = fontfile.Name;
                    newrow["Filesize"] = "ERR";
                    newrow["Create"] = "ERR";
                    newrow["Modification"] = "ERR";

                    // Get real values
                    try
                    {
                        // Size
                        newrow["Filesize"] = Convert.ToInt32((fontfile.Length / 1024)) + "KB";

                        // Create Date
                        newrow["Create"] = fontfile.CreationTime.ToString("yyyy-MM-dd H:mm:ss");

                        // Mod Date
                        newrow["Modification"] = fontfile.LastWriteTime.ToString("yyyy-MM-dd H:mm:ss");

                        // Font Name
                        if (extension.Equals(".ttf"))
                        {
                            System.Drawing.Text.PrivateFontCollection fontCol = new System.Drawing.Text.PrivateFontCollection();
                            try
                            {
                                fontCol.AddFontFile(file);
                                newrow["Name"] = fontCol.Families[0].Name;
                            }
                            catch (Exception)
                            {
                                newrow["Name"] = "ERR: No valid font file";
                            }
                        }
                        else
                        {
                            Regex rgx = null;
                            if (extension.Equals(".pfa"))
                            {
                                rgx = new Regex(@"%%FontName: ([^\s]*)", RegexOptions.Multiline);
                            }
                            else if(extension.Equals(".pfb"))
                            {
                                rgx = new Regex(@"/FontName /([^\s]*)", RegexOptions.Multiline);
                            }
                            else if (extension.Equals(".afm"))
                            {
                                rgx = new Regex(@"FullName ([^\s]*)", RegexOptions.Multiline);
                            }

                            if (rgx != null)
                            {
                                String content = File.ReadAllText(fontfile.FullName);
                                if (rgx.IsMatch(content))
                                {
                                    Match match = rgx.Match(content);
                                    newrow["Name"] = match.Groups[1].Value.ToString();
                                }
                                else
                                {
                                    newrow["Name"] = "ERR: Name not found";
                                }
                            }
                            else
                            {
                                newrow["Name"] = "ERR: Unexpected file extension";
                            }
                        }
                        

                        
                    }
                    catch (Exception exx)
                    {
                        newrow["Name"] = "Exception: "+exx.Message.Replace("\"", "'");
                    }
                    finally
                    {
                        tbl.Rows.Add(newrow);
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
