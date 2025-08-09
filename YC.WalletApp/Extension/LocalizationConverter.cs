using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using YC.ApplicationService;

namespace YC.WalletApp.Extension
{
    public class LocalizationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查输入值的数量和类型
            if (values.Length == 2 && values[0] is string key && values[1] is string netWork)
            {
                // 组合绑定的值
                return $"{key}:  {DefaultConfig.ContorlLanguage(netWork)}";
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
