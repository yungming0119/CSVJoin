using CSVJoin.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using DataGrid = System.Windows.Controls.DataGrid;
using TextBlock = System.Windows.Controls.TextBlock;
using Microsoft.Win32;
using System.Windows.Data;
using System.Text;

namespace CSVJoin
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel;
        private readonly CsvLoader csvLoader;
        private readonly TableJoiner tableJoiner;
        private readonly GroupBy groupBy;
        private readonly GeoJsonIO geoJsonIO;
        private readonly ExcelHelper excelHelper;

        public static DataTemplate headerTemplate { get; private set; }
        public MainWindow()
        {

            InitializeComponent();
            viewModel = new MainViewModel();
            DataContext = viewModel;
            csvLoader = new CsvLoader();
            tableJoiner = new TableJoiner();
            groupBy = new GroupBy();
            geoJsonIO = new GeoJsonIO();
            excelHelper = new ExcelHelper();
            headerTemplate = (DataTemplate)FindResource("FilterButtonTemplate");
            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.SystemThemeWatcher.Watch(
                    this,                                    // Window class
                    Wpf.Ui.Controls.WindowBackdropType.Tabbed, // Background type
                    true                                     // Whether to change accents automatically
                );
            };
        }
        private async void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            JoinButton2.IsEnabled = true;
        }
        private void JoinButton2_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            JoinData();
            Cursor = null;
            HeaderJoin.Visibility = Visibility.Visible;
            HeaderJoin.IsSelected = true;
            GroupButton.IsEnabled = true;
        }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            FilterHelper.HandleFilterButtonClick(sender, this);
            Cursor = null;
        }
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor = Cursors.Wait;
            FilterHelper.HandleFilterComboBoxSelectionChanged(sender, this);
            Cursor = null;
        }
        private async Task LoadDataAsync()
        {
            var loadTasks = new[]
            {
                csvLoader.LoadCsvFile(MainViewModel.Tickets, true, 1, MyDataGrid1),
                csvLoader.LoadCsvFile(MainViewModel.GIS, true, 2, MyDataGrid2),
                csvLoader.LoadCsvFile(MainViewModel.ITs, true, 3, MyDataGrid3),
                csvLoader.LoadCsvFile(MainViewModel.Mags, true, 5, MyDataGrid5),
                csvLoader.LoadCsvFile(MainViewModel.Segments, true, 4, MyDataGridJoined)
            };

            await Task.WhenAll(loadTasks);
        }
        private void JoinData()
        {
            var joinedData = tableJoiner.JoinTables(MainViewModel.Tickets, MainViewModel.GIS, MainViewModel.ITs, MainViewModel.Segments, MainViewModel.Mags);

            var properties = typeof(CombinedResult).GetProperties();
            var headers = properties.Select(p => p.Name).ToArray();

            foreach (var item in joinedData)
            {
                MainViewModel.Combined.Add(item);
            }

            ColumnManager.CreateColumns(headers, headers, MyDataGrid4,ExportButton_Click, ShpButton_Click, true);
            csvLoader.DisplayData(MainViewModel.Combined, MyDataGrid4);
        }
        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            MainViewModel.Grouped = groupBy.TableGroup(MainViewModel.Combined);
            var properties = typeof(GroupedDataItem).GetProperties();
            var headers = properties.Select(p => p.Name).ToArray();
            ColumnManager.CreateColumns(headers, headers, MyDataGridGrouped);
            csvLoader.DisplayData(MainViewModel.Grouped, MyDataGridGrouped);
            HeaderGroup.Visibility = Visibility.Visible;
            HeaderGroup.IsSelected = true;
            Cursor = null;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveActiveTabAsCsv(MyTabControl);
        }
        public void SaveActiveTabAsCsv(TabControl tabControl)
        {
            var activeTab = tabControl.SelectedItem as TabItem;
            if (activeTab == null)
            {
                MessageBox.Show("Active tab not found.");
                return;
            }

            string headerName = activeTab.Header as string;
            DataGrid dataGrid = headerName switch
            {
                "開單資料" => FindName("MyDataGrid1") as DataGrid,
                "GIS資料" => FindName("MyDataGrid2") as DataGrid,
                "資拓資料" => FindName("MyDataGrid3") as DataGrid,
                "地磁資料" => FindName("MyDataGrid5") as DataGrid,
                "路段資料" => FindName("MyDataGridJoined") as DataGrid,
                "合併表格" => FindName("MyDataGrid4") as DataGrid,
                "統計表格" => FindName("MyDataGridGrouped") as DataGrid,
                _ => null
            };

            if (dataGrid == null || dataGrid.Items == null || dataGrid.Items.Count == 0)
            {
                MessageBox.Show("DataGrid not found or is empty.");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = $"{dataGrid.Name}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                {
                    // Write the headers
                    writer.WriteLine(string.Join(",", dataGrid.Columns.Select(c => c.Header.ToString())));

                    // Write the data
                    foreach (var item in dataGrid.Items)
                    {
                        var values = dataGrid.Columns
                            .OfType<DataGridTextColumn>()
                            .Select(col =>
                            {
                                var binding = col.Binding as Binding;
                                return binding != null ? item.GetType().GetProperty(binding.Path.Path)?.GetValue(item, null)?.ToString() : string.Empty;
                            });
                        writer.WriteLine(string.Join(",", values));
                    }
                }
                if (headerName == "合併表格")
                {
                    var new_data = MainViewModel.Combined
                        .AsParallel()
                        .Where(x=> (x.ErrType != 5 && x.ErrType != 0))
                        .Select(x => new List<string>
                    {
                        x.RoadID,
                        x.RoadName,
                        "路邊停車位",
                        x.Cellno == null ? "-" : x.Cellno,
                        x.Ticketno == null ? "-" : x.Ticketno,
                        x.GisOx,
                        x.ItOx,
                        x.Status,
                        x.VendorCode,
                        x.ReceiveTime,
                        x.ParkID
                    }).ToList();
                    excelHelper.AppendDataToTable("form_temp.xlsx", saveFileDialog.FileName.Replace("csv", "xlsx"), new_data);
                }
                MessageBox.Show(String.Format("檔案已成功儲存至{0}", saveFileDialog.FileName));
            }
        }
        public void ExportButton_Click(object sender, EventArgs e)
        {
            geoJsonIO.saveGeoJSON(MainViewModel.Combined);
        }
        public void ShpButton_Click(object sender, EventArgs e)
        {
            geoJsonIO.saveSHP(MainViewModel.Combined);
        }
    }
}
