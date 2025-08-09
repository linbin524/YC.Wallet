using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Prism.Commands;
using Prism.Mvvm;
using Solnet.Extensions.TokenMint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using YC.WalletApp.Extension;

namespace YC.WalletApp.Domain.PartViewControl
{
    public class PaginationViewModel<T>  : BindableBase  where T : class,new()
    {
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalItems;
        private int _targetPage;


        
        public int[] PageSizes { get; } = { 10, 50, 100 };

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                var clamped = value < 1 ? 1 : (value > TotalPages ? TotalPages : value);
                if (SetProperty(ref _currentPage, clamped))
                {
                    TargetPage = clamped;
                    LoadPageData?.Invoke();
                }
               
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    CurrentPage = 1;
                    RaisePropertyChanged(nameof(TotalPages));
                    LoadPageData?.Invoke();
                }
            }
        }

        public int TotalItems
        {
            get => _totalItems;
            set
            {
                SetProperty(ref _totalItems, value);
                RaisePropertyChanged(nameof(TotalPages));
            }
        }

        public int TotalPages => (TotalItems == 0 || PageSize == 0) ? 1 : (int)Math.Ceiling((double)TotalItems / PageSize);

        public int TargetPage
        {
            get => _targetPage;
            set => SetProperty(ref _targetPage, value);
        }
        public Action LoadPageData { get; set; }

        private bool? _isAllSelected;
        public bool? IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                if (SetProperty(ref _isAllSelected, value) && Items != null)
                {
                    foreach (var item in Items)
                    {

                        Type type = item.GetType();
                        PropertyInfo prop = type.GetProperty("IsSelected");

                        // 检查属性是否存在、可写且类型为bool或可空bool
                        if (prop != null &&
                            prop.CanWrite &&
                            (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?)))
                        {
                            try
                            {
                                prop.SetValue(item, value);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error setting IsSelected: {ex.Message}");
                            }
                        }

                    }
                }
            }
        }

    

        private ObservableCollection<T> _items;
        // 监听项选中状态变化
        public ObservableCollection<T> Items 
        {
            get => _items;
            set
            {
               SetProperty( ref _items,value);
                
                //if (_items != null)
                //{
                //    foreach (var item in _items.OfType<INotifyPropertyChanged>())
                //    {
                //        item.PropertyChanged += (s, e) =>
                //        {
                //            if (e.PropertyName == nameof(ISelectable.IsSelected))
                //            {
                //                UpdateAllSelectedState();
                //            }
                //        };
                //    }
                //}
                CanNext = _currentPage < TotalPages ? true : false;
                CanPrevious = _currentPage > 1 ? true : false;
            }
        }

        private void UpdateAllSelectedState()
        {
            if (Items == null || !Items.Any())
            {
                IsAllSelected = false;
                return;
            }
            IsAllSelected = Items.OfType<ISelectable>().All(i => i.IsSelected);
        }

        public bool IsCreateColumns { get; set; }
        public void InitializeColumns<T>() where T : class
        {
            // 动态从模型属性读取DisplayName
            var userType = typeof(T);
            this.Columns = new ObservableCollection<ColumnDefinition>();
            foreach (var prop in userType.GetProperties())
            {
                var displayName = prop.GetCustomAttribute<DisplayAttribute>()?.Name ?? prop.Name;
                this.Columns.Add(new Domain.PartViewControl.ColumnDefinition
                {
                    DisplayName = displayName,
                    PropertyPath = prop.Name,
                   Converter = prop.PropertyType == typeof(bool) ? new BoolToYesNoConverter() : null
                });
            }
        }

        // 在ViewModel中配置列
        private ObservableCollection<ColumnDefinition> _columns;
        public ObservableCollection<ColumnDefinition> Columns { get => _columns; set => SetProperty(ref _columns, value); } 

        public DelegateCommand NextPageCommand => new DelegateCommand(() => CurrentPage++);
        public DelegateCommand PreviousPageCommand => new DelegateCommand(() => CurrentPage--);
        public DelegateCommand GoToPageCommand => new DelegateCommand(() => CurrentPage = TargetPage);

        private bool _canPrevious;
        public bool CanPrevious { get {
                return _canPrevious;
            } set => SetProperty(ref _canPrevious, value); }
        private bool _canNext;
        public bool CanNext
        {
            get
            {
                return _canNext;
            }
            set => SetProperty(ref _canNext, value);
        }
        
    }

    public class ColumnDefinition : BindableBase
    {
        private string _displayName;
        private string _propertyPath;
        private IValueConverter _converter;
        private DataTemplate _cellTemplate;

        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        public string PropertyPath { get => _propertyPath; set => _propertyPath = value; }
        public IValueConverter Converter { get => _converter; set => _converter = value; }
        public DataTemplate CellTemplate { get => _cellTemplate; set => _cellTemplate = value; } // 可选：自定义单元格模板
    }

}
