using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using System.Windows.Input;
using System.ComponentModel;
using Solnet.Extensions;
using YC.Model.Entity;
using Mapster;
using YC.ApplicationService;
using System.ComponentModel.DataAnnotations;
using YC.WalletApp.Extension;

namespace YC.WalletApp.ViewModels
{
    public class TokenDefViewModel : BindableBase, IDisposable
    {
        #region 基础定义
       
        private ObservableCollection<TokenDef> allTokenDefs { get {
                var res = SQLiteUtils._freesql.Select<TokenDefEntity>().ToList();
                var data=res.Adapt<ObservableCollection<TokenDef>>();
                for (int i = 0; i < data.Count; i++) {
                    data[i].IsSelected = false;
                    data[i].SetParentViewModel(this); // 设置父ViewModel引用
                }
                return data;
            }
            set {
                allTokenDefs = value;
            }
        }
        private ObservableCollection<TokenDef> tokenDefs;
        private TokenDef selectedTokenDef;
       

        public ObservableCollection<TokenDef> TokenDefs
        {
            get => tokenDefs;
            set => SetProperty(ref tokenDefs, value);
        }

       
        private bool isAddPopupOpen;
        private bool isEditPopupOpen;
        private TokenDef newTokenDef;
        private TokenDef editedTokenDef;
        private string targetPageNumber;
        #endregion

        #region 翻页定义

        private const int PageSize = 10;
        private int currentPage = 1;
        public string TargetPageNumber
        {
            get => targetPageNumber;
            set => SetProperty(ref targetPageNumber, value);
        }

        private int TotalPages => (int)Math.Ceiling((double)allTokenDefs.Count / PageSize);
        public int CurrentPageStart = 1;//需要设置一个，不然CanGoPrevious 无法响应通知，数据不变更
      
        private bool canGoPrevious;
        private long startPage = 1;
        public bool CanGoPrevious//这样还是无效，需要触发设置，常量在这里，数据变化更新无效的
        {
            get
            {
                canGoPrevious = currentPage > startPage;

                return canGoPrevious;
            }
            set => SetProperty(ref canGoPrevious, value);
        }

        public bool CanGoNext => currentPage < TotalPages;

        public string TotalPagesText => string.Format(LanguageManager.Instance["TotalPagesText"], TotalPages);

        public string CurrentPageItemsText => string.Format(LanguageManager.Instance["CurrentPageItemsText"], currentPage, TokenDefs?.Count ?? 0);

        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand GoToPageCommand { get; }
        public ICommand LastPageCommand { get; }

        #endregion

        #region 新增、选中和修改定义

        public TokenDef SelectedTokenDef
        {
            get => selectedTokenDef;
            set
            {
                if (SetProperty(ref selectedTokenDef, value))
                {
                    // 当选中项变化时，通知UI更新编辑和删除按钮状态
                    RaisePropertyChanged(nameof(CanEdit));
                    RaisePropertyChanged(nameof(CanDelete));
                }
            }
        }

        public bool CanEdit => SelectedTokenDef != null;

        public bool CanDelete => TokenDefs != null && TokenDefs.Any(t => t.IsSelected);

        public bool IsAddPopupOpen
        {
            get => isAddPopupOpen;
            set => SetProperty(ref isAddPopupOpen, value);
        }

        public bool IsEditPopupOpen
        {
            get => isEditPopupOpen;
            set => SetProperty(ref isEditPopupOpen, value);
        }

        public TokenDef NewTokenDef
        {
            get => newTokenDef;
            set => SetProperty(ref newTokenDef, value);
        }

        public TokenDef EditedTokenDef
        {
            get => editedTokenDef;
            set => SetProperty(ref editedTokenDef, value);
        }

        public ICommand AddTokenDefCommand { get; }
        public ICommand EditTokenDefCommand { get; }
        public ICommand DeleteTokenDefCommand { get; }
        public ICommand InitializeTokenDefCommand { get; }
        public ICommand AddConfirmCommand { get; }
        public ICommand AddCancelCommand { get; }
        public ICommand EditConfirmCommand { get; }
        public ICommand EditCancelCommand { get; }
        public ICommand CheckAllCommand { get; }
        public ICommand SelectAllCommand { get; }  // 新增命令 
        #endregion

        public TokenDefViewModel()
        {
           
            #region 绑定事件
            
            TokenDefs = new ObservableCollection<TokenDef>();
            NewTokenDef = new TokenDef();
            EditedTokenDef = new TokenDef();

            AddTokenDefCommand = new RelayCommand(OpenAddPopup);
            EditTokenDefCommand = new RelayCommand(OpenEditPopup);
            DeleteTokenDefCommand = new RelayCommand(DeleteTokenDef);
            InitializeTokenDefCommand = new RelayCommand(InitializeTokenDef);
            PreviousPageCommand = new RelayCommand(PreviousPage, () => CanGoPrevious);
            NextPageCommand = new RelayCommand(NextPage, () => CanGoNext);
            GoToPageCommand = new RelayCommand(GoToPage);
            AddConfirmCommand = new RelayCommand(AddConfirm);
            AddCancelCommand = new RelayCommand(AddCancel);
            EditConfirmCommand = new RelayCommand(EditConfirm);
            EditCancelCommand = new RelayCommand(EditCancel);
            LastPageCommand = new RelayCommand(GoToLastPage);
            SelectAllCommand = new RelayCommand(SelectAllCurrentPage); 
            #endregion
          
            UpdatePage();
            
            // 订阅语言切换事件
            LanguageManager.Instance.PropertyChanged += OnLanguageChanged;
        }

        #region 语言切换事件处理
        /// <summary>
        /// 语言切换事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLanguageChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                // 语言变化时重新计算分页文本，触发UI更新
                RaisePropertyChanged(nameof(TotalPagesText));
                RaisePropertyChanged(nameof(CurrentPageItemsText));
                
                // 通知所有TokenDef对象更新Display属性
                if (TokenDefs != null)
                {
                    foreach (var tokenDef in TokenDefs)
                    {
                        tokenDef.NotifyLanguageChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不中断程序
                System.Diagnostics.Debug.WriteLine($"Language change error in TokenDefViewModel: {ex.Message}");
            }
        }
        #endregion

        #region 指定方法

        // 全选当前页逻辑
        public void SelectAllCurrentPage()
        {
            // 获取当前全选状态
            var currentState = IsAllItemsSelected;
            
            // 根据当前状态决定下一步操作
            bool newState;
            if (currentState == true)
            {
                // 当前是全选状态，点击后变为全不选
                newState = false;
            }
            else
            {
                // 当前是部分选中或全不选状态，点击后变为全选
                newState = true;
            }

            // 设置所有项的选中状态
            foreach (var item in TokenDefs)
            {
                item.IsSelected = newState;
            }

            // 强制通知 UI 更新全选状态
            RaisePropertyChanged(nameof(IsAllItemsSelected));
            RaisePropertyChanged(nameof(CanDelete)); // 同时更新删除按钮状态
        }

        public void InitSelectAllCurrentPage()
        {
            // 判断当前页是否已经全选
            bool isAllSelected = TokenDefs.All(t => t.IsSelected);
            if (isAllSelected)
            {
                // 切换所有项的选中状态
                foreach (var item in TokenDefs)
                {
                    item.IsSelected = false;  // 反向设置
                }

                // 强制通知 UI 更新全选状态
                RaisePropertyChanged(nameof(IsAllItemsSelected));
            }

        }

        /// <summary>
        /// 全选标识
        /// </summary>
        public bool? IsAllItemsSelected
        {
            get
            {
                // 计算实际的选中状态
                if (TokenDefs == null || TokenDefs.Count == 0)
                    return false;

                var selectedCount = TokenDefs.Count(t => t.IsSelected);
                if (selectedCount == 0)
                    return false;
                else if (selectedCount == TokenDefs.Count)
                    return true;
                else
                    return false; // 部分选中时显示为未选中，简化用户体验
            }
            set
            {
                // 当通过UI设置全选状态时，同步更新所有项的选中状态
                if (TokenDefs != null && value.HasValue)
                {
                    foreach (var item in TokenDefs)
                    {
                        item.IsSelected = value.Value;
                    }
                    
                    // 直接通知属性变化
                    RaisePropertyChanged(nameof(IsAllItemsSelected));
                    RaisePropertyChanged(nameof(CanDelete));
                }
            }
        }

        /// <summary>
        /// 通知全选状态变化，用于从外部事件处理中调用
        /// </summary>
        public void NotifyAllItemsSelectedChanged()
        {
            RaisePropertyChanged(nameof(IsAllItemsSelected));
            RaisePropertyChanged(nameof(CanDelete));
        }

        /// <summary>
        /// 重新初始化最初数据
        /// </summary>
        public void InitializeTokenDef()
        {
            var wellKnownTokens = WellKnownTokens.All();//加载所有链上已知代币
            var deleleListId = allTokenDefs.Select(x => x.Id).ToList();
            var delectRes = SQLiteUtils._freesql.Delete<TokenDefEntity>(deleleListId).ExecuteAffrows();
            var detailObj = wellKnownTokens.Adapt<List<TokenDefEntity>>();
            if (DefaultConfig.AppConfig.IsDebug)
            {//开发模式下加载对应的测试TokenDef
                detailObj.AddRange(DefaultConfig.TestExpansionTokenDefs);
            }

            detailObj.ForEach(x => {
                x.CreationTime = DateTime.Now;
                //x.CreatorUserId = DefaultConfig.CurrentLoginUser.Id;
            });
            var inserRes = SQLiteUtils._freesql.Insert<TokenDefEntity>(detailObj).ExecuteAffrows();
            currentPage = 1;
            UpdatePage();
        }
        // 统一的 PropertyChanged 事件处理器
        private void TokenDef_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TokenDef.IsSelected))
            {
                // 触发全选状态和删除按钮状态的更新
                RaisePropertyChanged(nameof(IsAllItemsSelected));
                RaisePropertyChanged(nameof(CanDelete));
            }
        }
        #endregion

        #region 添加编辑等事件处理方法
        private void OpenAddPopup()
        {
            NewTokenDef = new TokenDef();
            IsAddPopupOpen = true;
        }

        private void OpenEditPopup()
        {
            if (SelectedTokenDef != null)
            {
                EditedTokenDef = SelectedTokenDef.Adapt<TokenDef>();
                IsEditPopupOpen = true;
            }
        }

        /// <summary>
        /// 添加操作
        /// </summary>
        private void AddConfirm()
        {
            if (int.TryParse(NewTokenDef.DecimalPlacesStr, out int decimalPlaces))
            {
                NewTokenDef.DecimalPlaces = decimalPlaces;
                //allTokenDefs.Add(NewTokenDef);
                var obj = NewTokenDef.Adapt<TokenDefEntity>();
                var res = SQLiteUtils._freesql.Insert<TokenDefEntity>(obj).ExecuteAffrows();
                UpdatePage();
                IsAddPopupOpen = false;
            }
            else
            {
                CommonExtension.ShowDialog(LanguageManager.Instance["DecimalPlacesMustBeInteger"]);
            }
        }

        private void AddCancel()
        {
            IsAddPopupOpen = false;
        }

        private void EditConfirm()
        {
            if (int.TryParse(EditedTokenDef.DecimalPlacesStr, out int decimalPlaces))
            {
                EditedTokenDef.DecimalPlaces = decimalPlaces;
                var list = allTokenDefs.Adapt<List<TokenDef>>();
                bool isExist = list.Any(x => x.Id == selectedTokenDef.Id);
                if (isExist)
                {
                    //allTokenDefs[index] = EditedTokenDef;
                    var obj = EditedTokenDef.Adapt<TokenDefEntity>();
                    var res = SQLiteUtils._freesql.Update<TokenDefEntity>().SetSource(obj).ExecuteAffrows();
                    SelectedTokenDef = EditedTokenDef;
                    UpdatePage();
                }
                IsEditPopupOpen = false;
            }
            else
            {
                CommonExtension.ShowDialog(LanguageManager.Instance["DecimalPlacesMustBeInteger"]);
            }
        }

        private void EditCancel()
        {
            IsEditPopupOpen = false;
        }

        /// <summary>
        /// 删除操作
        /// </summary>
        public void DeleteTokenDef()
        {
            var selectedItems = TokenDefs.Where(t => t.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                CommonExtension.ShowDialog(LanguageManager.Instance["PleaseCheckItemsToDelete"]);
                return;
            }

            var deleleList = selectedItems.Select(x => x.Id).ToList();
            var res = SQLiteUtils._freesql.Delete<TokenDefEntity>(deleleList).ExecuteAffrows();
            UpdatePage();
            CommonExtension.ShowDialog(string.Format(LanguageManager.Instance["DeletedItemsCount"], res));
        } 
        #endregion

        #region 翻页事件

        /// <summary>
        /// 更新数据处理
        /// </summary>
        private void UpdatePage()
        {
            var startIndex = (currentPage - 1) * PageSize;
            var endIndex = Math.Min(startIndex + PageSize, allTokenDefs.Count);
            TokenDefs = new ObservableCollection<TokenDef>(allTokenDefs.Skip(startIndex).Take(PageSize));

            // 为当前页的每个 TokenDef 订阅 PropertyChanged 事件并设置父ViewModel引用
            foreach (var model in TokenDefs)
            {
                model.PropertyChanged -= TokenDef_PropertyChanged; // 防止重复订阅
                model.PropertyChanged += TokenDef_PropertyChanged;
                model.SetParentViewModel(this); // 确保父ViewModel引用正确
            }

            // 触发所有相关属性更新
            RaisePropertyChanged(nameof(TotalPagesText));
            RaisePropertyChanged(nameof(CurrentPageItemsText));
            RaisePropertyChanged(nameof(CanGoPrevious));
            RaisePropertyChanged(nameof(CanGoNext));
            RaisePropertyChanged(nameof(CanDelete));
            RaisePropertyChanged(nameof(IsAllItemsSelected)); // 确保全选状态更新
        }
        /// <summary>
        /// 上一页
        /// </summary>
        public void PreviousPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePage();
            }
        }
        /// <summary>
        /// 下一页
        /// </summary>
        public void NextPage()
        {
            if (currentPage < TotalPages)
            {
                currentPage++;
                UpdatePage();
            }
        }

        /// <summary>
        /// 最后一页
        /// </summary>
        public void GoToLastPage()
        {
            if (currentPage < TotalPages)
            {
                currentPage = TotalPages;
                UpdatePage();
            }
        }

        /// <summary>
        /// 跳转指定页面
        /// </summary>
        public void GoToPage()
        {
            if (int.TryParse(TargetPageNumber, out int pageNumber) && pageNumber >= 1 && pageNumber <= TotalPages)
            {
                currentPage = pageNumber;
                UpdatePage();
            }
            else
            {
                CommonExtension.ShowDialog(LanguageManager.Instance["PleaseEnterValidPageNumber"]);
            }
        }  
       
        #endregion

        /// <summary>
        /// 实现IDisposable接口的Dispose方法
        /// </summary>
        public void Dispose()
        {
            // 取消订阅语言切换事件
            if (LanguageManager.Instance != null)
            {
                LanguageManager.Instance.PropertyChanged -= OnLanguageChanged;
            }

            // 取消订阅所有TokenDef的PropertyChanged事件
            if (TokenDefs != null)
            {
                foreach (var tokenDef in TokenDefs)
                {
                    tokenDef.PropertyChanged -= TokenDef_PropertyChanged;
                }
            }

            // 清理资源
            GC.SuppressFinalize(this);
        }
    }

    public class TokenDef: BindableBase
    {
        private TokenDefViewModel _parentViewModel;
        
        public void SetParentViewModel(TokenDefViewModel parent)
        {
            _parentViewModel = parent;
        }

        /// <summary>
        /// 通知语言变化，用于更新显示属性
        /// </summary>
        public void NotifyLanguageChanged()
        {
            // 当语言变化时，通知UI更新相关属性
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Symbol));
        }

        public long Id { get; set; }
        [Display(Name = "代币账户")]
        public string Mint { get; set; }
        [Display(Name = "代币名称")]
        public string Name { get; set; }
        [Display(Name = "代币标识")]
        public string Symbol { get; set; }
        public int DecimalPlaces { get; set; }
        public string DecimalPlacesStr
        {
            get => DecimalPlaces.ToString();
            set
            {
                if (int.TryParse(value, out int result))
                {
                    DecimalPlaces = result;
                }
            }
        }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set 
            { 
                if (SetProperty(ref isSelected, value))
                {
                    // 当选中状态变化时，通知父ViewModel更新全选状态
                    _parentViewModel?.NotifyAllItemsSelectedChanged();
                }
            }
        }
    }
}