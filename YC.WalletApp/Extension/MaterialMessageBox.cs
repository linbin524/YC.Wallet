using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace YC.WalletApp.Extension
{
  
        public static class MaterialMessageBox
        {
            public static async Task<MessageBoxResult> Show(string message, string title = "提示", MessageBoxButton button = MessageBoxButton.OK)
            {
                var dialogContent = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Width = 300,
                    Height = 200,
                    Background = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(10),
                    Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(10)
                    },
                    new TextBlock
                    {
                        Text = message,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10)
                    },
                    CreateButtonPanel(button)
                }
                };

                var result = await DialogHost.Show(dialogContent);
                return (MessageBoxResult)result;
            }

            private static StackPanel CreateButtonPanel(MessageBoxButton button)
            {
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10)
                };

                switch (button)
                {
                    case MessageBoxButton.OK:
                        buttonPanel.Children.Add(CreateButton("确定", MessageBoxResult.OK));
                        break;
                    case MessageBoxButton.OKCancel:
                        buttonPanel.Children.Add(CreateButton("取消", MessageBoxResult.Cancel));
                        buttonPanel.Children.Add(CreateButton("确定", MessageBoxResult.OK));
                        break;
                    case MessageBoxButton.YesNo:
                        buttonPanel.Children.Add(CreateButton("否", MessageBoxResult.No));
                        buttonPanel.Children.Add(CreateButton("是", MessageBoxResult.Yes));
                        break;
                    case MessageBoxButton.YesNoCancel:
                        buttonPanel.Children.Add(CreateButton("取消", MessageBoxResult.Cancel));
                        buttonPanel.Children.Add(CreateButton("否", MessageBoxResult.No));
                        buttonPanel.Children.Add(CreateButton("是", MessageBoxResult.Yes));
                        break;
                }

                return buttonPanel;
            }

            private static Button CreateButton(string content, MessageBoxResult result)
            {
                var button = new Button
                {
                    Content = content,
                    Margin = new Thickness(5),
                    Tag = result
                };
                button.Click += (sender, args) =>
                {
                    DialogHost.CloseDialogCommand.Execute(result, null);
                };
                return button;
            }
        }
   
}
