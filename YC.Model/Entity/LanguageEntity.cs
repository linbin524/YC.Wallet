using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    /// <summary>
    /// 配置多语言
    /// </summary>
    public class LanguageEntity
    {
        public string DefaultLanguage { get; set; }
        public List<SupportedLanguage> SupportedLanguages { get; set; }
    }

    public class SupportedLanguage
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string JsonPath { get; set; }
    }
}
