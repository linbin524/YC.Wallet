using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.WalletApp
{
    public class WalletAccountDto : BindableBase
    {
        private string _accountName;
        public string AccountName
        {
            get => _accountName;
            set => SetProperty(ref _accountName, value);
        }

        private string _balance;
        public string Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        private List<Transcation> _transactions;
        public List<Transcation> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        private string _tokenDetails;
        public string TokenDetails
        {
            get => _tokenDetails;
            set => SetProperty(ref _tokenDetails, value);
        }

        private string _publicKey;
        public string PublicKey
        {
            get => _publicKey;
            set => SetProperty(ref _publicKey, value);
        }

        private string _owner;
        public string Owner
        {
            get => _owner;
            set => SetProperty(ref _owner, value);
        }

        private bool _isAssociatedTokenAccount;
        public bool IsAssociatedTokenAccount
        {
            get => _isAssociatedTokenAccount;
            set => SetProperty(ref _isAssociatedTokenAccount, value);
        }

        private string _accountType;
        public string AccountType
        {
            get => _accountType;
            set => SetProperty(ref _accountType, value);
        }

        private ulong _lamports;
        public ulong Lamports
        {
            get => _lamports;
            set => SetProperty(ref _lamports, value);
        }

        private double _doubleTypeLamports;
        public double DoubleTypeLamports
        {
            get => _doubleTypeLamports;
            set => SetProperty(ref _doubleTypeLamports, value);
        }

        private int _accountCount;
        public int AccountCount
        {
            get => _accountCount;
            set => SetProperty(ref _accountCount, value);
        }

        private string _tokenMint;
        public string TokenMint
        {
            get => _tokenMint;
            set => SetProperty(ref _tokenMint, value);
        }

        private string _symbol;
        public string Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value);
        }

        private string _tokenName;
        public string TokenName
        {
            get => _tokenName;
            set => SetProperty(ref _tokenName, value);
        }

        private int _decimalPlaces;
        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set => SetProperty(ref _decimalPlaces, value);
        }

        private Double _quantityDecimal;
        public Double QuantityDecimal
        {
            get => _quantityDecimal;
            set => SetProperty(ref _quantityDecimal, value);
        }

        private ulong _quantityRaw;
        public ulong QuantityRaw
        {
            get => _quantityRaw;
            set => SetProperty(ref _quantityRaw, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class Transcation : BindableBase
    {
        private long _id;
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _type;
        public string? Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        private string _payAccount;
        public string? PayAccount
        {
            get => _payAccount;
            set => SetProperty(ref _payAccount, value);
        }

        private string _tokenSymbol;
        public string? TokenSymbol
        {
            get => _tokenSymbol;
            set => SetProperty(ref _tokenSymbol, value);
        }

        private DateTime? _creationTime;
        public DateTime? CreationTime
        {
            get => _creationTime;
            set => SetProperty(ref _creationTime, value);
        }

        private string _tokenMint;
        public string? TokenMint
        {
            get => _tokenMint;
            set => SetProperty(ref _tokenMint, value);
        }

        private string _jsonContent;
        public string? JsonContent
        {
            get => _jsonContent;
            set => SetProperty(ref _jsonContent, value);
        }

        private string _netWork;
        public string? NetWork
        {
            get => _netWork;
            set => SetProperty(ref _netWork, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
