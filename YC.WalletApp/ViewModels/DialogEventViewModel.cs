using MaterialDesign3Demo.Domain;
using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.ApplicationService;
using YC.Model.Entity;
using YC.SolanaSdkService.DTO;
using YC.WalletApp.Domain;
using YC.WalletApp.Extension;

namespace YC.WalletApp.ViewModels
{

    /// <summary>
    /// 调用说明
    /// 
    /// 1. 给CreateInitFunc 绑定事件执行，主要是构造对应ViewModel，
    ///    将DataContext 传递给打开弹出框做初始化数据源
    /// 2. CancelAction 弹出框取消要处理的事件
    /// 3. SumbmitAction 弹出框提交要处理事件
    /// 
    /// </summary>
    public class DialogEventViewModel: BindableBase
    {
        #region 界面有新增和编辑弹出框的处理

        public DialogEventViewModel(Func<Object> createInit, string showDialogId) { 
         this.CreateInitFunc = createInit;
         this.ShowDialogId = showDialogId;
        }

        /// <summary>
        /// 要打开弹出框，要初始化，给弹出框提供对应的数据源
        /// </summary>
        private Func<Object> CreateInitFunc { get; set; }
        /// <summary>
        /// 展示弹出框 Id，在Mainwindow界面里面
        /// </summary>
        public string ShowDialogId { get; set; }


        /// <summary>
        /// 弹出框取消按钮要进行的实际业务操作
        /// </summary>
        public Action<Object> CancelAction { get; set; }
        /// <summary>
        /// 展示结果提示
        /// </summary>
        public Action ShowResultAction { get; set; }
        /// <summary>
        /// 弹出框提交按钮要进行的实际业务操作
        /// </summary>
        public Func<Object, Object> _submitAction;
        public Func<Object, Object> SubmitAction { get=>_submitAction; set => SetProperty(ref _submitAction, value); }
        /// <summary>
        /// 业务操作的返回结果
        /// </summary>
        private Object _result;
        public Object Result { get => _result; set => SetProperty(ref _result, value); }

        public Object ShowDialogDataContext { get; set; }

        public async void ExecuteRunDialog(object? _)
        {
            Object vm;
            if (CreateInitFunc != null) {
             vm=CreateInitFunc();
                //show the dialog
                var result = await DialogHost.Show(vm, ShowDialogId, ExtendedOpenedEventHandler, ExtendedClosingEventHandler);

                //check the result...
                Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
            }
        }
        private void ExtendedOpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs)
       => Debug.WriteLine("You could intercept the open and affect the dialog using eventArgs.Session.");

        private void ExtendedClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            //var dialogContent = eventArgs.Session.Content as WalletExportDialog;
            if (eventArgs.Parameter is bool parameter &&
            parameter == false)
            {
                CancelAction(eventArgs.Session.Content);
                return;
            }

            if (eventArgs.Parameter is bool parameter_yes &&
               parameter_yes == true)
            {
                Result = SubmitAction?.Invoke(eventArgs);//拿到后转成对应Dialog 获取DataContext
                return;
                // eventArgs.Cancel();
                //ShowResultAction?.Invoke();
                //return;
            }
        }



        #endregion

    }
}
