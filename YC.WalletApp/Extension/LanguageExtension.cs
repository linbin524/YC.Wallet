using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using YC.ApplicationService;
using System.Windows.Media;

namespace YC.WalletApp.Extension
{
    //public static class LanguageExtension
    //{
    //    public static readonly DependencyProperty LanguageKeyProperty =
    //        DependencyProperty.RegisterAttached("LanguageKey", typeof(string), typeof(LanguageExtension), new PropertyMetadata(null, OnLanguageKeyChanged));

    //    public static string GetLanguageKey(DependencyObject obj)
    //    {
    //        return (string)obj.GetValue(LanguageKeyProperty);
    //    }

    //    public static void SetLanguageKey(DependencyObject obj, string value)
    //    {
    //        obj.SetValue(LanguageKeyProperty, value);
    //    }

    //    private static void OnLanguageKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is FrameworkElement element && e.NewValue is string key)
    //        {
    //            element.SetValue(ContentControl.ContentProperty, GetLocalizedString(key));
    //        }
    //    }

    //   /// <summary>
    //   /// 改变触发文字
    //   /// </summary>
    //   /// <param name="key"></param>
    //   /// <returns></returns>
    //    private static string GetLocalizedString(string key)
    //    {
    //        // 这里实现根据语言和键获取本地化字符串的逻辑
    //        // 示例中简单返回键名，实际应用中应根据当前语言从资源文件中获取对应字符串
    //        return DefaultConfig.ContorlLanguage(key);
    //    }

    //    static LanguageExtension()
    //    {
    //        // 订阅语言变更事件
    //        LanguageManager.Instance.LanguageChanged += OnLanguageChanged;
    //    }

    //    private static void OnLanguageChanged(object sender, EventArgs e)
    //    {
    //        //// 遍历所有使用了 LanguageKey 附加属性的控件并更新文本
    //        //var visualTree = VisualTreeHelper.GetOpenPopups().Cast<Window>().SelectMany(x => FlattenVisualTree(x));
    //        //foreach (var element in visualTree)
    //        //{
    //        //    var key = GetLanguageKey(element);
    //        //    if (!string.IsNullOrEmpty(key))
    //        //    {
    //        //        element.SetValue(ContentControl.ContentProperty, GetLocalizedString(key));
    //        //    }
    //        //}
    //    }

    //    private static IEnumerable<FrameworkElement> FlattenVisualTree(Visual parent)
    //    {
    //        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
    //        {
    //            var child = VisualTreeHelper.GetChild(parent, i) as Visual;
    //            if (child != null)
    //            {
    //                if (child is FrameworkElement frameworkElement)
    //                {
    //                    yield return frameworkElement;
    //                }
    //                foreach (var descendant in FlattenVisualTree(child))
    //                {
    //                    yield return descendant;
    //                }
    //            }
    //        }
    //    }
    //}
}
