using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using YC.WalletApp.ViewModels;

namespace YC.WalletApp.Domain.Entity
{
    public class ModuleInfo: ViewModelBase
    {
        public string ModuleId { get; set; }
        public string IconFont { get; set; }

        public string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string IconTitle { get { return IconFont + "  " + Title; } set { IconTitle = value; } }

        public List<ModuleInfo> Items { get; set; }

        public bool IsSelected { get; set; }

        public UserControl UserControl { get; set; }

    }
}
