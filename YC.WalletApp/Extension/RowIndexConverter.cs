using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.WalletApp.Extension
{
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    public class RowIndexConverter : IValueConverter
    {
        // 定义静态实例
        public static RowIndexConverter Instance { get; } = new RowIndexConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGridRow row && row.Item != null)
            {
                int index = row.GetIndex();
                return (index + 1).ToString(); // 从1开始
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
