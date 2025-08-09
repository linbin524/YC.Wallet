using MaterialDesign3Demo.Domain;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YC.ApplicationService;
using YC.Model.Entity;
using YC.SolanaSdkService.DTO;
using YC.WalletApp.Domain;
using YC.WalletApp.Domain.PartViewControl;



namespace YC.WalletApp.Extension
{
    public class CommonExtension
    {


        #region 导入导出功能

        private const string DefaultImportFilter = "文本文件 (*.txt)|*.txt|JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*";
        private const string DefaultExportFilter = "文本文件 (*.txt)|*.txt";

        /// <summary>
        /// 文件导入方法（默认支持TXT/JSON）
        /// </summary>
        public static string ImportFile(string filter = DefaultImportFilter, string title = "选择导入文件")
        {
            var dialog = new OpenFileDialog();
            try
            {
                dialog.Filter = SanitizeFileFilter(filter);
                dialog.Title = title;
                dialog.ValidateNames = true;
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;

                return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
            }
            catch (Exception ex)
            {
                ShowErrorDialog("文件导入失败", ex);
                return string.Empty;
            }

        }

        /// <summary>
        /// 文件导出方法（默认TXT格式）
        /// </summary>
        public static bool ExportFile(string content,
                                    string filter = DefaultExportFilter,
                                    string title = "保存文件",
                                    string defaultFileName = "")
        {
            var dialog = new SaveFileDialog();
            try
            {
                dialog.Filter = SanitizeFileFilter(filter);
                dialog.Title = title;
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                dialog.ValidateNames = true;
                dialog.FileName = SanitizeFileName(defaultFileName);
                dialog.DefaultExt = GetDefaultExtension(filter);

                if (dialog.ShowDialog() != true) return false;

                File.WriteAllText(dialog.FileName, content);
                return true;
            }
            catch (Exception ex)
            {
                ShowErrorDialog("文件保存失败", ex);
                return false;
            }

        }

        private static string SanitizeFileFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return DefaultImportFilter;

            var items = filter.Split('|');
            for (int i = 0; i < items.Length; i += 2)
            {
                if (i + 1 >= items.Length) continue;

                var extensions = items[i + 1].Split(';')
                    .Select(ext => ext.Contains("*.") ? ext : $"*.{ext.TrimStart('*', '.')}")
                    .Distinct();

                items[i + 1] = string.Join(";", extensions);
            }
            return string.Join("|", items);
        }

        private static string GetDefaultExtension(string filter)
        {
            var validFilter = SanitizeFileFilter(filter);
            var firstPair = validFilter.Split('|').FirstOrDefault();

            if (firstPair?.Contains("*.txt") == true) return "txt";
            if (firstPair?.Contains("*.json") == true) return "json";

            return firstPair?.Split(';')
                .FirstOrDefault()?
                .TrimStart('*', '.')
                ?? "txt";
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "未命名文件.txt";

            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanName = new string(fileName
                .Where(c => !invalidChars.Contains(c))
                .ToArray())
                .Trim();

            return Path.HasExtension(cleanName)
                ? cleanName
                : $"{cleanName}.txt";
        }

        private static void ShowErrorDialog(string baseMessage, Exception ex)
        {
            MessageBox.Show(
                $"{baseMessage}：{ex.Message}",
                "系统错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        #endregion

        #region 公共弹出框
        /// <summary>
        /// 公共弹出框
        /// 
        /// 要在弹出框界面加入这些内容
        /// <materialDesign:DialogHost DialogTheme="Inherit"
        ///Identifier="SecondDialog" 
        /// SnackbarMessageQueue="{Binding ElementName=MainSnackbar, Path=MessageQueue}" >
        /// <!--这里是其他代码-->
        ///</materialDesign:DialogHost>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="container"></param>
        /// <param name="type"></param>
        /// <param name="dialogIdentifier"></param>
        public static void ShowDialog(string content, IContainerExtension container = null, int type = 0, string dialogIdentifier = "SecondDialog")
        {
            ShowDialogViewModel vm;
            if (container != null)
            {
                vm = container.Resolve<ShowDialogViewModel>();
                vm._container = container;
            }
            else
            {
                vm = new ShowDialogViewModel();
                vm.Type = type;
            }
            vm.Content = content;
            vm.DialogIdentifier = dialogIdentifier;
            // 检查命令是否可以执行
            if (vm.RunDialogCommand.CanExecute(null))
            {
                // 执行命令
                vm.RunDialogCommand.Execute(null);
            }
        }
        #endregion

    }
}
