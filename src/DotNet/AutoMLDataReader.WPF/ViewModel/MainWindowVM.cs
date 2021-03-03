using AutoMLDataReader.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace AutoMLDataReader.WPF.ViewModel
{
    public class MainWindowVM:INotifyPropertyChanged
    {
        public string SwaggerUrl { get; set; }
        public ObservableCollection<string> SwaggerResultItems { get; set; } = new ObservableCollection<string>();


        private string _status;

        public string StatusText
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }

        private DataTable _result;
        public DataTable ResultTable 
        {
            get
            {
                return _result;
            } private set
            {
                _result = value;
                OnPropertyChanged();
            }
        }
        private float _loadProgress;

        public float LoadProgress
        {
            get { return _loadProgress; }
            set { _loadProgress = value;OnPropertyChanged(); }
        }


        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            { 
                isBusy = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(propertyName:nameof(IsNotBusy)); 
            }
        }
        

        public string APIKey { get; set; } = null;


        public bool IsNotBusy => !IsBusy;

        private string _csvPath;

        public string CSVPath
        {
            get { return _csvPath; }
            set 
            { 
                _csvPath = value; 
                OnPropertyChanged(); 
            }
        }

        public int BatchSize { get; set; } = 200;




        public event PropertyChangedEventHandler PropertyChanged;

        public string EndpointUrl { get; set; }
        public async void RefreshSwagger()
        {
            if (string.IsNullOrEmpty(SwaggerUrl))
            {
                return;
            }
            var result=await EndpointClient.ParseSwaggerAsync(SwaggerUrl);
            SwaggerResultItems.Clear();
            foreach (var item in result)
            {
                SwaggerResultItems.Add(item);
            }
        }


        protected void OnPropertyChanged(bool onUIThread=true, [CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler==null)
            {
                return;
            }
            if (onUIThread)
            {
                Application.Current?.Dispatcher.InvokeAsync(() =>
                {
                    handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
            }
            else
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            
            
        }
        public async void Load()
        {
            IsBusy = true;
            StatusText = string.Empty;
            DataLoader loader = new DataLoader();
            LoadProgress = 0;
            try
            {
                ResultTable = await loader.LoadData(
                CSVPath,
                EndpointUrl,
                APIKey,
                BatchSize,
                new DelegateProgress<float>(x =>
                {
                    LoadProgress = x;
                }));
                StatusText = $"{ResultTable.Rows.Count} rows loaded";
            }
            catch (Exception ex)
            {
                StatusText = string.Empty;
                MessageBox.Show($"failed load data \n\r {ex}", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
            
            
            
        }

        public void ExportToCSV()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.OverwritePrompt = true;
            sfd.Filter = "CSV File|*.CSV";
            sfd.DefaultExt = "CSV";
            if (sfd.ShowDialog()==true)
            {
                try
                {
                    DataLoader.SaveDatatableToCSV(sfd.OpenFile(), ResultTable);
                    MessageBox.Show($"CSV export complete", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Export failed. exception={ex}","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                }
                
            }
            
        }

    }
}
