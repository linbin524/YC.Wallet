using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using YC.Model.Entity;
using static MaterialDesignThemes.Wpf.Theme;
using YC.WalletApp.Extension;
using YC.WalletApp.ViewModels;
using YC.WalletApp.Domain.PartViewControl;

namespace YC.WalletApp.Domain.PartViewControl
{
    /// <summary>
    /// DataGridPaginationControl.xaml 的交互逻辑
    /// </summary>
    public partial class DataGridPaginationControl : UserControl
    {

        public DataGridPaginationControl()
        {
            InitializeComponent();
            Loaded += OnLoaded; // 在加载完成后生成列

        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 1. 先检查是否是泛型类型定义
            if (DataContext?.GetType().GetGenericTypeDefinition() == typeof(PaginationViewModel<>))
            {
                // 2. 获取实际泛型参数
                var actualType = DataContext.GetType().GetGenericArguments()[0];

                // 3. 动态创建处理方法
                var method = GetType().GetMethod(nameof(HandlePaginationViewModel),
                                BindingFlags.NonPublic | BindingFlags.Instance);
                method.MakeGenericMethod(actualType).Invoke(this, new[] { DataContext});
            }
        }


        // 泛型处理方法
        private void HandlePaginationViewModel<T>(PaginationViewModel<T> vm) where T : class, new()
        {
            if (!vm.IsCreateColumns)
            {
                GenerateDynamicColumns(vm.Columns);
                vm.IsCreateColumns = true;
            }
        }

        private void GenerateDynamicColumns(ObservableCollection<ColumnDefinition> columns)
        {
            foreach (var columnDef in columns)
            {
                DataGridColumn column;

                if (columnDef.CellTemplate != null)
                {
                    // 使用模板列
                    var templateColumn = new DataGridTemplateColumn
                    {
                        Header = columnDef.DisplayName,
                        CellTemplate = columnDef.CellTemplate
                    };
                    column = templateColumn;
                }
                else
                {
                    // 使用文本列
                    var textColumn = new DataGridTextColumn
                    {
                        Header = columnDef.DisplayName,
                        Binding = new Binding(columnDef.PropertyPath)
                        {
                            Converter = columnDef.Converter // 直接绑定转换器实例
                        }
                    };
                    column = textColumn;
                }

                MainDataGrid.Columns.Add(column);
            }

        }
    }
}
