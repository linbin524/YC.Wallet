using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace YC.WalletApp.Domain
{
    public class ValidateModelBase : BindableBase, IDataErrorInfo
    {
        public Dictionary<string, string> dataErrors = new Dictionary<string, string>();  //错误信息集合

        private bool _IsValidated;
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValidated
        {
            get { return _IsValidated; }
            set { _IsValidated = value; RaisePropertyChanged(); }
        }


        public string this[string columnName]
        {
            get
            {
                ValidationContext vc = new ValidationContext(this, null, null)
                {
                    MemberName = columnName
                };
                var res = new List<ValidationResult>();
                var temp = GetType().GetProperty(columnName).GetValue(this, null);
                var result = Validator.TryValidateProperty(GetType().GetProperty(columnName).GetValue(this, null), vc, res);

                if (res.Count > 0)
                {
                    string errorInfo = string.Join(Environment.NewLine, res.Select(r => r.ErrorMessage).ToArray());
                    AddError(dataErrors, columnName, errorInfo);
                    return errorInfo;
                }
                RemoveError(dataErrors, columnName);
                return null;
            }
        }

        /// <summary>
        /// 移除错误信息
        /// </summary>
        /// <param name="dataErrors"></param>
        /// <param name="columnName"></param>
        private void RemoveError(Dictionary<string, string> dataErrors, string columnName)
        {
            dataErrors.Remove(columnName);
            IsValidated = !dataErrors.Any();
        }

        /// <summary>
        /// 添加错误信息
        /// </summary>
        /// <param name="dataErrors"></param>
        /// <param name="columnName"></param>
        /// <param name="errorInfo"></param>
        private void AddError(Dictionary<string, string> dataErrors, string columnName, string errorInfo)
        {
            if (!dataErrors.ContainsKey(columnName))
            {
                dataErrors.Add(columnName, errorInfo);
                IsValidated = !dataErrors.Any();
            }
        }

        public string Error { get; set; }

        private bool isFormValid;
        /// <summary>
        /// 是否全局验证
        /// </summary>
        public bool IsFormValid
        {
            get { return isFormValid; }
            set { isFormValid = value; RaisePropertyChanged(); }
        }
    }
}
