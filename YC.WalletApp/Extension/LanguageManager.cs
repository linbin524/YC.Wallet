using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using YC.ApplicationService;

namespace YC.WalletApp.Extension
{

    public class LanguageManager : INotifyPropertyChanged
    {
        /// <summary>
        /// 资源
        /// </summary>
        private readonly ResourceManager _resourceManager;

        /// <summary>
        /// 懒加载
        /// </summary>
        private static readonly Lazy<LanguageManager> _lazy = new Lazy<LanguageManager>(() => new LanguageManager());
        public static LanguageManager Instance => _lazy.Value;
        public event PropertyChangedEventHandler PropertyChanged;

        public LanguageManager()
        {
            //获取此命名空间下Resources的Lang的资源，Lang可以修改
            _resourceManager = new ResourceManager("WpfApp1.Resources.Lang", typeof(LanguageManager).Assembly);
        }

        /// <summary>
        /// 索引器的写法，传入字符串的下标
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                return DefaultConfig.ContorlLanguage(name);
            }
        }

        public void ChangeLanguage()
        {
            // 触发所有属性的PropertyChanged事件，确保所有绑定都能更新
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        }
    }

}
