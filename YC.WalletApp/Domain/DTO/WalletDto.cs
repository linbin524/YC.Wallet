using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using YC.SolanaSdkService.DTO;

namespace YC.WalletApp
{
    public class WalletDto:BindableBase
    {
        private string _walletName;
        public string WalletName
        {
            get => _walletName;
            set => SetProperty(ref _walletName, value);
        }

        private string _creationTime;
        public string CreationTime
        {
            get => _creationTime;
            set => SetProperty(ref _creationTime, value);
        }

        private List<WalletAccountDto> _accounts;
        public List<WalletAccountDto> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        private string _walletAddress;
        public string WalletAddress
        {
            get => _walletAddress;
            set => SetProperty(ref _walletAddress, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set {
                if (SetProperty(ref _isSelected, value))
                {
                    // 同步选择所有账户
                    if (Accounts != null)
                    {
                        foreach (var account in Accounts)
                        {
                            account.IsSelected = value;
                        }
                    }
                }
            }
        }

        private string _publicKey;
        public string PublicKey
        {
            get => _publicKey;
            set => SetProperty(ref _publicKey, value);
        }

        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        private string _lamportsBalance;
        public string LamportsBalance
        {
            get {
                if (!string.IsNullOrWhiteSpace(_lamportsBalance))
                {
                    return (ulong.Parse(_lamportsBalance) / Math.Pow(10, 9)).ToString() + " SOL";
                }
                else {
                    return "";
                }
            }

            set => SetProperty(ref _lamportsBalance, value);
        }
    }

}
