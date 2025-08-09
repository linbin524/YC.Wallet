using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace YC.WalletApp.Domain
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "内容不能为空.")
                : ValidationResult.ValidResult;
        }
    }
    // 验证 ComboBox 必须选择一项
    public class ComboBoxValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "请选择一项");
            }
            return ValidationResult.ValidResult;
        }
    }

    // 验证 CheckBox 必须勾选
    public class CheckBoxValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is bool isChecked && !isChecked)
            {
                return new ValidationResult(false, "请勾选此选项");
            }
            return ValidationResult.ValidResult;
        }
    }

}
