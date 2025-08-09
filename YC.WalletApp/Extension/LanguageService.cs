using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.ApplicationService;
using YC.Model.Entity;

namespace YC.WalletApp.Extension
{
    public class LanguageService
    {
        public static void SetLanguage(string selectedLanguage)
        {

            DefaultConfig.LocalLanguage = selectedLanguage;
            var config = SQLiteUtils._freesql.Select<SysConfigEntity>().First();
            if (config != null)
            {
                config.LocalLanguage = selectedLanguage;
                var result = SQLiteUtils.Update<SysConfigEntity>(config);
            }
            else
            {
                config = new SysConfigEntity();
                config.LocalLanguage = selectedLanguage;
                var result = SQLiteUtils.Insert<SysConfigEntity>(config);
            }
            // 更新语言并触发界面刷新
            LanguageManager.Instance.ChangeLanguage(); // 设置en/zh-CN等
            // 在这里处理选中事件
            Console.WriteLine($"选中的语言是: {selectedLanguage}");

        }
    }
}
