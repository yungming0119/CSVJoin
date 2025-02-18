using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using DataGrid = System.Windows.Controls.DataGrid;
using MenuItem = System.Windows.Controls.MenuItem;

namespace CSVJoin
{
    public class ColumnManager
    {
        public static void CreateColumns(string[] headers, DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();
            for (int i = 0; i < headers.Length; i++)
            {
                var column = new DataGridTextColumn
                {
                    Header = headers[i],
                    HeaderTemplate = MainWindow.headerTemplate,                    
                    Binding = new Binding($"[{i}]")
                };
                dataGrid.Columns.Add(column);
            }
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ParkID",
                HeaderTemplate = MainWindow.headerTemplate,
                Binding = new Binding("ParkID")
            });
        }

        public static void CreateColumns(string[] headers, string[] propertyNames, DataGrid dataGrid, RoutedEventHandler func = null, RoutedEventHandler func2 = null, bool createButton=false)
        {   

            if (createButton) {
                //var button = new Wpf.Ui.Controls.SplitButton();
                //button.Content = new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.Open16);
                //button.Foreground = new SolidColorBrush(Colors.White);
                
                //var contextMenu = new ContextMenu();
                //MenuItem menuItem1 = new MenuItem();
                //menuItem1.Header = "輸出GeoJSON";
                //menuItem1.Click += func;
                //MenuItem menuItem2 = new MenuItem();
                //menuItem2.Click += func2;
                //menuItem2.Header = "輸出SHP";
                //contextMenu.Items.Add(menuItem1);
                //contextMenu.Items.Add(menuItem2);

                //button.Flyout = contextMenu;
                //dataGrid.Columns.Add(new DataGridTextColumn
                //{
                //    Header = button
                //});
            }
            else
            {
                dataGrid.Columns.Clear();
                headers = new string[] { "路段ID", "路段名稱", "GIS未對應", "資拓未對應", "GIS須修正", "地須修正" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var column = new DataGridTextColumn
                    {
                        Header = headers[i],
                        HeaderTemplate = MainWindow.headerTemplate,
                        Binding = new Binding(propertyNames[i])
                    };
                    dataGrid.Columns.Add(column);
                }
            }

        }

        public static void CreateColumns(int columnCount, DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();
            for (int i = 0; i < columnCount; i++)
            {
                var column = new DataGridTextColumn
                {
                    HeaderTemplate = MainWindow.headerTemplate,
                    //Header = $"Column {i + 1}",
                    Binding = new Binding($"[{i}]")
                };
                dataGrid.Columns.Add(column);
            }
        }
    }
}

