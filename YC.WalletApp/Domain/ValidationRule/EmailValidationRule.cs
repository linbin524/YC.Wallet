using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace YC.WalletApp.Domain
{
    public class EmailValidationRule : ValidationRule
    {
        private static readonly Regex EmailRegex = new Regex(
             @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$",
             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value is string email)
            {
                if (!EmailRegex.IsMatch(email))
                {
                    return new ValidationResult(false, "邮箱验证失败.");
                }
            }
            else
            {
                return new ValidationResult(false, "非法字符.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
