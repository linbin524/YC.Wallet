using Newtonsoft.Json;
using YC.WalletApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
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
using YC.WalletApp.Domain.Entity;
using YC.ApplicationService;

namespace YC.WalletApp.Controls
{
    /// <summary>
    /// MenuControl.xaml 的交互逻辑
    /// </summary>
    public partial class MenuControl : UserControl
    {
        public MenuControl()
        {
            InitializeComponent();

           Loaded += MenuControl_Loaded;
        }


        private void MenuControl_Loaded(object sender, RoutedEventArgs e)
        {
            ListBox listBox = this.FindName("MenuListBox") as ListBox;

            if (listBox != null)
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    ListBoxItem listBoxItem = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

                    if (listBoxItem != null)
                    {
                        // 获取Expander控件
                        Expander expander = GetChildOfType<Expander>(listBoxItem);

                        Window window = Window.GetWindow(listBox);
                        if (window != null)
                        {
                            OpenTabControl(window.DataContext, DefaultConfig.ContorlLanguage("WalletManage"), "WalletManage");
                        }

                        //if (expander != null)
                        //{
                        //    // 获取Expander绑定的数据源
                        //    object dataContext = expander.DataContext;

                        //    // 在这里你可以对dataContext做进一步的处理
                        //    Console.WriteLine($"Expander at index {i} is bound to: {dataContext}");
                        //    //Debug.WriteLine("我来也："+ JsonConvert.SerializeObject(dataContext));
                        //    Window window = Window.GetWindow(listBox);
                        //    if (window != null)
                        //    {
                        //        OpenTabControl(window.DataContext, DefaultConfig.ContorlLanguage("AccountManage"), "AccountManage");
                        //    }

                        //}
                    }
                }
            }
        }

        /// <summary>
        /// 递归查找指定类型的子控件
        /// </summary>
        /// <typeparam name="T">要查找的控件类型</typeparam>
        /// <param name="obj">从哪个父对象开始查找</param>
        /// <returns>找到的第一个匹配的子控件</returns>
        public static T GetChildOfType<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var result = GetChildOfType<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var foundChild = FindVisualChild<T>(child);
                if (foundChild != null)
                    return foundChild;
            }
            return null;
        }

        /// <summary>
        /// 菜单按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton clickedRadioButton = sender as RadioButton;
            if (clickedRadioButton != null)
            {
                // 获取RadioButton的数据上下文，也就是数据模型
                var dataContext = clickedRadioButton.DataContext;
                if (dataContext != null && dataContext is ModuleInfo model)
                {
                    string selectedText = model.Title;
                    Console.WriteLine($"Selected Text: {selectedText}");

                    Window window = Window.GetWindow(clickedRadioButton);
                    if (window != null)
                    {
                        OpenTabControl(window.DataContext, DefaultConfig.ContorlLanguage(model.ModuleId), model.ModuleId);
                    }

                      
                }
            }
          
        }

        /// <summary>
        /// 打开指定的窗体
        /// </summary>
        /// <param name="clickedRadioButton"></param>
        /// <param name="key"></param>
        public void OpenTabControl(object dataContext, string key,string moduleId)
        {
                // 获取窗口的数据上下文
                object mainWindowDataContext = dataContext;

                // 确保DataContext不是null
                if (mainWindowDataContext != null)
                {
                    // 这里你可以使用mainWindowDataContext来做进一步的操作
                    Console.WriteLine($"Main Window DataContext Type: {mainWindowDataContext.GetType()}");

                    // 假设DataContext是一个ViewModel，你可以访问它的属性
                    if (mainWindowDataContext is MainWindowViewModel vm)
                    {
                        var tabs = vm.Tabs.CustomTabs;
                        if (tabs != null)
                        {
                            var tabObj = tabs.Where(x =>x.CustomTabId.Equals(moduleId)).FirstOrDefault();

                            if (tabObj == null)
                            {
                                var newTab = new CustomTab(vm.Tabs.closeCommand)
                                {
                                    CustomTabId = moduleId,
                                    CustomHeader = key,
                                    CustomContent = GetUserControl(moduleId)

                                };
                                tabs.Add(newTab);

                                //激活
                                vm.Tabs.SelectedTab = newTab;
                            }
                            else
                            {
                                //激活
                                vm.Tabs.SelectedTab = tabObj;
                            }
                        }
                        else
                        {
                            var newTab = new CustomTab(vm.Tabs.closeCommand)
                            {
                                CustomTabId = moduleId,
                                CustomHeader = key,
                                CustomContent = GetUserControl(moduleId)
                            };
                            vm.Tabs.CustomTabs.Add(newTab);//TabsViewModel() 已經實例化了，所以現在只需要添加              
                            //激活
                            vm.Tabs.SelectedTab = newTab;
                           
                        }
                    }
                }
            
        }

        #region 业务处理内部方法
        /// <summary>
        /// 获得返回的控件
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public UserControl GetUserControl(string moduleId)
        {
            var viewModel = (MainWindowViewModel)this.DataContext;
            var moduleList = FindModulesById(viewModel.Modules.ToList(), moduleId);
            //var userControl = moduleList.FirstOrDefault().UserControl;
            var t = moduleList.FirstOrDefault().UserControl.GetType();
            var userControl = viewModel._container.Resolve(t) as UserControl;
            // var userControl= Activator.CreateInstance(t) as UserControl;//通过反射进行实例化
            return userControl;
        }

        /// <summary>
        /// 递归查找符合的菜单控件集合
        /// </summary>
        /// <param name="modules">原始菜单集合</param>
        /// <param name="targetModuleId">目标模块Id</param>
        /// <returns></returns>
        public static List<ModuleInfo> FindModulesById(List<ModuleInfo> modules, string targetModuleId)
        {
            List<ModuleInfo> result = new List<ModuleInfo>();

            foreach (var module in modules)
            {
                if (module.ModuleId == targetModuleId)
                {
                    result.Add(module);
                }

                if (module.Items != null && module.Items.Any())
                {
                    // 递归查找子模块
                    var childModules = FindModulesById(module.Items, targetModuleId);
                    result.AddRange(childModules);
                }
            }

            return result;
        }

        /// <summary>
        /// 通过指定的tab名称，或者id，映射实例化对应的用户控件返回
        /// </summary>
        /// <returns></returns>
        public UserControl MappingTab(string tabTitleName)
        {

            #region 自动映射对应的目录菜单
            var ass = Assembly.GetExecutingAssembly();
            var types = ass.GetTypes();
            UserControl tempTabs = new UserControl();
            foreach (var t in types)
            {
                //这里加上命名空间等Full 路径，才是保险的
                if (t.Name.Contains(tabTitleName))
                {
                    tempTabs = Activator.CreateInstance(t) as UserControl;
                }

            }
            return tempTabs;
            #endregion
        } 
        #endregion
    }
}
