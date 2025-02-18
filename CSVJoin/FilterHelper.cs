using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CSVJoin
{
    public static class FilterHelper
    {
        public static void HandleFilterButtonClick(object sender, MainWindow mainWindow)
        {
            if (sender is Button button)
            {
                var contextMenu = mainWindow.FindResource("FilterContextMenu") as ContextMenu;
                if (contextMenu != null)
                {
                    string columnTag = (button.Parent as StackPanel)?.Children.OfType<TextBlock>().FirstOrDefault()?.Text;
                    var dataGrid = FindParent<DataGrid>(button);
                    columnTag = DataGridHelper.GetBindingByHeader(columnTag);
                    int columnIndex = FindColumnIndex(dataGrid, columnTag);

                    var stackPanel = (StackPanel)contextMenu.Items[0];
                    var comboBox = stackPanel.Children.OfType<ComboBox>().FirstOrDefault();

                    if (comboBox != null && dataGrid != null && columnIndex != -1)
                    {
                        var uniqueValues = GetUniqueValuesFromColumn(dataGrid, columnTag);
                        comboBox.ItemsSource = uniqueValues.Select(x => new KeyValuePair<string, string>(x, x)).ToList().Prepend(new KeyValuePair<string, string>("all", "全部"));
                        comboBox.Name = $"{dataGrid.Name}_{columnTag}";
                        comboBox.SelectedIndex = 0;
                    }

                    contextMenu.PlacementTarget = button;
                    contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    contextMenu.IsOpen = true;
                }
            }
        }

        public static void HandleFilterComboBoxSelectionChanged(object sender, MainWindow mainWindow)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                string selectedValue = comboBox.SelectedValue.ToString();
                string[] tags = comboBox.Name.Split('_');
                var dataGrid = (DataGrid)mainWindow.FindName(tags[0]);
                var view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);

                if (selectedValue == "all")
                {
                    view.Filter = null;
                }
                else
                {
                    string propertyName = tags[1];
                    view.Filter = item =>
                    {
                        var property = item.GetType().GetProperty(propertyName);
                        var propertyValue = property?.GetValue(item)?.ToString();
                        return propertyValue == selectedValue;
                    };
                }
                view.Refresh();
            }
        }

        private static int FindColumnIndex(DataGrid dataGrid, string columnName)
        {
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                var column = dataGrid.Columns[i] as DataGridTextColumn;
                var binding = column.Binding as Binding;
                if (binding != null)
                if (binding.Path.Path == columnName)
                {
                    return i;
                }
            }
            return -1;
        }


        private static IEnumerable<string> GetUniqueValuesFromColumn(DataGrid dataGrid, string columnName)
        {
            var uniqueValues = new HashSet<string>();

            Parallel.ForEach(dataGrid.Items.Cast<object>(), item =>
            {
                var property = item.GetType().GetProperty(columnName);
                var value = property?.GetValue(item)?.ToString();
                if (value != null)
                {
                    lock (uniqueValues)
                    {
                        uniqueValues.Add(value);
                    }
                }
            });

            return uniqueValues.ToList();
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            return parentObject as T ?? FindParent<T>(parentObject);
        }
    }
}
public class DataGridHelper
{
    public static Dictionary<string, string> GetHeaderBindingMap()
    {
        return new Dictionary<string, string>
        {
            { "rcID", "RoadID" },
            { "路段代碼", "RoadName" },
            { "Park ID", "ParkID" },
            { "格位號", "Cellno" },
            { "開單數", "Ticketno" },
            { "GIS對應代碼", "GisParkID" },
            { "資拓對應代碼", "ItsParkID" },
            { "地磁對應代碼", "MagID" },
            { "地磁對應狀態", "Status" },
            { "地磁廠商", "VendorCode" },
            { "地磁接收時間", "ReceiveTime" },
            { "GIS正確", "GisYn" },
            { "資拓正確", "ItYn" },
            { "GIS對應", "GisOx" },
            { "IT對應", "ItOx" },
            { "錯誤樣態代碼", "ErrType" },
            { "路段ID", "RoadID" },
            { "路段名稱", "RoadName" },
            { "GIS未對應", "TotalGISYn" },
            { "資拓未對應", "TotalItYn" },
            { "GIS須修正", "GisParkIDCount" },
            { "資拓須修正", "ItsParkIDCount" }
        };
    }

    public static string GetBindingByHeader(string header)
    {
        var headerBindingMap = GetHeaderBindingMap();
        return headerBindingMap.TryGetValue(header, out string binding) ? binding : header;
    }
}
