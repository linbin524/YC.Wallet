using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using YC.WalletApp.Domain.Entity;

namespace YC.WalletApp.Views
{

    public class ItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NonEmptyTemplate { get; set; }
        public DataTemplate EmptyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // 假设你的item是一个具有Items属性的ViewModel  
            var viewModel = item as ModuleInfo; // YourViewModel是包含Items属性的类型  
            if (viewModel?.Items != null && viewModel.Items.Any())
            {
                return NonEmptyTemplate;
            }
            else
            {
                return EmptyTemplate;
            }
        }
    }
}
