using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace CSVtoDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Reading a csv file into memory, inserts into a database and reporting progress
    /// </summary>
    public partial class MainWindow : Window
    { 
        List<string> columnNames;
        List<List<string>> values;
        private BackgroundWorker workerDb;
        private BackgroundWorker workerCsv;
        private OpenFileDialog fileCsv;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void loadCsv_Click(object sender, RoutedEventArgs e)
        {
            progressbar1.Value = 0;
            fileCsv = new OpenFileDialog();
            fileCsv.Multiselect = false;
            fileCsv.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            fileCsv.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
           
            if (fileCsv.ShowDialog() == true)
            {
                loadCsv.IsEnabled = false;
                workerCsv = new BackgroundWorker();
                workerCsv.WorkerReportsProgress = true;
                workerCsv.DoWork += workerCsv_DoWork;
                workerCsv.RunWorkerCompleted += workerCsv_RunWorkerCompleted;

                if (!workerCsv.IsBusy)
                {
                    progressbar1.IsIndeterminate = true;
                    workerCsv.RunWorkerAsync();
                }
            }
        }

        public void pushToDb_Click(object sender, RoutedEventArgs e)
        {
            pushToDb.IsEnabled = false;
            workerDb = new BackgroundWorker();
            workerDb.WorkerReportsProgress = true;
            workerDb.DoWork += workerDb_DoWork;
            workerDb.ProgressChanged += workerDb_ProgressChanged;
            workerDb.RunWorkerCompleted += workerDb_RunWorkerCompleted;
            if (!workerDb.IsBusy)
            {
                workerDb.RunWorkerAsync();
            }
        }

        private void workerCsv_DoWork(object sender, DoWorkEventArgs e)
        {
            string line;
            int count = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(fileCsv.FileName);
            values = new List<List<string>>();

            while ((line = file.ReadLine()) != null)
            {
                if (count == 0)
                {
                    string pattern = "((?:[^\",]|(?:\"(?:\\{2}|\\\"|[^\"])*?\"))*)";
                    string[] splitColumnNames = Regex.Split(line, pattern).Where(value => !string.IsNullOrEmpty(value) && value != "," && value != "\"").ToArray();
                    columnNames = new List<string>(splitColumnNames);
                }
                else
                {
                    string pattern = "((?:[^\",]|(?:\"(?:\\{2}|\\\"|[^\"])*?\"))*)";
                    string[] splitValues = Regex.Split(line, pattern).Where(value => !string.IsNullOrEmpty(value) && value != "," && value != "\"").ToArray();
                    List<string> tempValues = new List<string>(splitValues);
                    values.Add(tempValues);
                }
                count++;
            }
            file.Close();
        }

        void workerCsv_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressbar1.IsIndeterminate = false;
            progressbar1.Value = 0;
            pushToDb.IsEnabled = true;
        }

        private void workerDb_DoWork(object sender, DoWorkEventArgs e)
        {
            jeroenDataSet1TableAdapters.csvtodbTableAdapter csvTableAdapter = new jeroenDataSet1TableAdapters.csvtodbTableAdapter();
            int i = 0;
            foreach (List<String> val in values)
            {
                int percentage = (i + 1) * 100 / values.Count;
                csvTableAdapter.Insert(val[0].Trim(), val[1].Trim(), val[2].Trim());
                i++;
                workerDb.ReportProgress(percentage);
            }

        }

        void workerDb_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressbar1.Value = e.ProgressPercentage;
        }

        void workerDb_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            loadCsv.IsEnabled = true;
            columnNames = null;
            values = null;
            MessageBox.Show("Finished, Cheers!");
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}
