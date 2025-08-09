using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.WalletApp.Attribute
{
    public class NotSpecificValueAttribute : ValidationAttribute
    {
        private readonly int _forbiddenValue;

        public NotSpecificValueAttribute(int forbiddenValue)
        {
            _forbiddenValue = forbiddenValue;
        }

        public override bool IsValid(object value)
        {
            try
            {
                int setValue = int.Parse(value.ToString());

                if (setValue != _forbiddenValue)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
            

            return false;
        }
    }
}
