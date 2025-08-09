using MaterialDesign3Demo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YC.ApplicationService;
using YC.Common.ShareUtils;
using YC.Model.Entity;
using YC.WalletApp.Extension;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YC.WalletApp.Domain;

/// <summary>
/// Interaction logic for SampleDialog.xaml
/// </summary>
public partial class WalletExportDialog : UserControl
{

    public WalletExportDialog()
    {
        InitializeComponent();
    }


    private void CollectSelectedItems(IEnumerable<WalletAccountDto> accounts, List<WalletDto> selectedItems)
    {
        //foreach (var account in accounts)
        //{
        //    if (account.IsSelected) selectedItems.Add(account);
        //    if (account.Transactions != null)
        //    {
        //        foreach (var transaction in account.Transactions)
        //        {
        //            if (transaction.IsSelected) selectedItems.Add(transaction);
        //        }
        //    }
        //}
    }
}
