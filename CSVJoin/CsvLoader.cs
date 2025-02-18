using CSVJoin.ViewModel;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Wpf.Ui.Controls;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace CSVJoin
{
    public class CsvLoader
    {
        public List<TicketViewModel> Tickets { get; private set; }
        public List<GISInfoViewModel> GIS { get; private set; }
        public List<ITInfoViewModel> ITs { get; private set; }
        public List<SegmentViewModel> Segments { get; private set; }
        public List<MagViewModel> Mags { get; private set; }


        public CsvLoader()
        {
            Tickets = new List<TicketViewModel>();
            GIS = new List<GISInfoViewModel>();
            ITs = new List<ITInfoViewModel>();
            Segments = new List<SegmentViewModel>();
            Mags = new List<MagViewModel>();
        }

        public async Task LoadCsvFile<T>(ObservableCollection<T> collection, bool readHeader, int csvType = 0, DataGrid? listView = null) where T : class, new()
        {
            var dialogSettings = new Dictionary<int, (string Title, string FileName)>
            {
                { 1, ("開啟開單資料", "ticket.csv") },
                { 2, ("開啟GIS資料", "gis.csv") },
                { 3, ("開啟資拓資料", "IT.csv") },
                { 4, ("開啟路段資料", "seg.csv") },
                { 5, ("開啟地磁資料", "mag.csv") }
            };

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (dialogSettings.TryGetValue(csvType, out var settings))
            {
                openFileDialog.Title = settings.Title;
                openFileDialog.FileName = settings.FileName;
            }

            if (openFileDialog.ShowDialog() == true)
            {
                collection.Clear(); // Clear existing items

                await LoadCsvInChunksAsync(openFileDialog.FileName, readHeader, collection, listView, csvType);
            }
        }


        private async Task LoadCsvInChunksAsync<T>(string filePath, bool readHeader, ObservableCollection<T> collection, DataGrid? listView, int csvType) where T : class, new()
        {
            int chunkSize = 10000; // Number of lines to read at a time
            List<string[]> csvData = new List<string[]>();

            using (var reader = new StreamReader(filePath))
            {
                string line;
                int lineNumber = 0;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var values = line.Split(',');
                    csvData.Add(values);

                    if (lineNumber == 0 && listView != null)
                    {
                        // Create columns dynamically based on the first row (header)
                        if (readHeader)
                        {
                            ColumnManager.CreateColumns(values, listView);
                        }
                        else
                        {
                            ColumnManager.CreateColumns(values.Length, listView);
                        }
                    }

                    lineNumber++;
                    if (lineNumber % chunkSize == 0)
                    {
                        await Task.Run(() => ProcessData(csvType, collection, csvData.Skip(readHeader ? 1 : 0).ToList()));
                        csvData.Clear();
                    }
                }

                // Process remaining data
                if (csvData.Any())
                {
                    await Task.Run(() => ProcessData(csvType, collection, csvData.Skip(readHeader ? 1 : 0).ToList()));
                }

                //Display the data using the collection
                DisplayData(collection, listView);
            }
        }

        private void ProcessData<T>(int csvType, ObservableCollection<T> collection, List<string[]> data) where T : class, new()
        {
            try
            {
                switch (csvType)
                {
                    case 1:
                        Tickets = data.Select(row => SafeCreateViewModel(() => new TicketViewModel
                        {
                            RoadID = row[0],
                            CellNo = int.Parse(row[1]),
                            TicketNo = int.Parse(row[2]),
                            ParkID = $"{row[0]}_{row[1]}"
                        })).Where(item => item != null).ToList();
                        AddToCollection(collection as ObservableCollection<TicketViewModel>, Tickets);
                        break;
                    case 2:
                        GIS = data.Select(row => SafeCreateViewModel(() => new GISInfoViewModel
                        {
                            Sno = int.TryParse(row[0], out int sno) ? sno : (int?)null,
                            Pkid = transformId(row[1].TrimStart('0')),
                            EditTime = DateTime.TryParse(row[2], out DateTime editTime) ? editTime : (DateTime?)null,
                            PkType = int.TryParse(row[3], out int pkType) ? pkType : (int?)null,
                            RdCode = row[4],
                            PkRoad = row[5],
                            ParkID = $"{row[4]}_{transformId(row[1].TrimStart('0'))}"
                        })).Where(item => item != null).ToList();
                        AddToCollection(collection as ObservableCollection<GISInfoViewModel>, GIS);
                        break;
                    case 3:
                        ITs = data.Select(row => SafeCreateViewModel(() => new ITInfoViewModel
                        {
                            PId = row[0],
                            RoadSegID = row[1],
                            PsId = transformId(row[2].TrimStart('0')),
                            RoadSegName = row[3],
                            ParkID = $"{row[1]}_{transformId(row[2].TrimStart('0'))}"
                        })).Where(item => item != null).ToList();
                        AddToCollection(collection as ObservableCollection<ITInfoViewModel>, ITs);
                        break;
                    case 4:
                        Segments = data.Select(row => SafeCreateViewModel(() => new SegmentViewModel
                        {
                            segID = row[0],
                            segName = row[1]
                        })).Where(item => item != null).ToList();
                        AddToCollection(collection as ObservableCollection<SegmentViewModel>, Segments);
                        break;
                    case 5:
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                        Mags = data.Select(row =>
                        {
                            try
                            {
                                return new MagViewModel
                                {
                                    section_id = row[0],
                                    ps_id_str = row[1],
                                    status = int.Parse(row[2]),
                                    vendorcode = row[3] == "C01" ? "歐特儀" : row[3] == "B01" ? "宏碁" : row[3] == "C03" ? "國雲" : row[3] == "BUS01" ? "昇采科技" : null,
                                    data_dt = DateTime.Parse(row[4]),
                                    receiveTime = DateTime.Parse(row[5]),
                                    ParkID = $"{row[0]}_{transformId(row[1].TrimStart('0'))}"
                                };
                            }
                            catch (Exception ex)
                            {
                                // Log the exception or handle it as necessary
                                Console.WriteLine($"Error processing row: {ex.Message}");
                                return null;
                            }
                        }).Where(item => item != null).ToList();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

                        AddToCollection(collection as ObservableCollection<MagViewModel>, Mags);
                        break;
                    default:
                        throw new ArgumentException("Invalid csvType value", nameof(csvType));
                }
            }
            catch (Exception ex)
            {
                // Show the error in a message box
                MessageBox.Show($"Error processing data: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private T SafeCreateViewModel<T>(Func<T> creator) where T : class
        {
            try
            {
                return creator();
            }
            catch (Exception ex)
            {
                // Show the error in a message box
                MessageBox.Show($"Error creating view model: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }



        private void AddToCollection<T>(ObservableCollection<T> collection, List<T> items)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in items)
                {
                    collection.Add(item);
                }
            });
        }

        public void DisplayData<T>(ObservableCollection<T> collection, DataGrid? listView)
        {
            if (listView != null)
            {
                listView.Dispatcher.Invoke(() =>
                {
                    listView.ItemsSource = collection;
                });
            }
        }

        private string transformId(string idSeries)
        {
            var prefixMap = new Dictionary<char, string>
            {
                { 'A', "1" }, { 'B', "2" }, { 'C', "3" }, { 'D', "4" }, { 'E', "5" }
            };

            if (idSeries.Length == 2)
            {
                return prefixMap.ContainsKey(idSeries[1]) ? $"{prefixMap[idSeries[1]]}00{idSeries[0]}" : idSeries;
            }
            else if (idSeries.Length == 3)
            {
                return prefixMap.ContainsKey(idSeries[2]) ? $"{prefixMap[idSeries[2]]}0{idSeries.Substring(0,2)}" : idSeries;
            }
            else if (idSeries.Length >= 4)
            {
                return prefixMap.ContainsKey(idSeries[3]) ? $"{prefixMap[idSeries[3]]}{idSeries.Substring(0,3)}" : idSeries;
            }
            else
            {
                return idSeries;
            }
        }

    }
}