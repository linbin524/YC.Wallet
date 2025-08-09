using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationService.Model
{
    public class CombinationCreateDto
    {
      public List<List<int>> DataList { get; set; }
       public int ChooseType { get; set; }
        public string ChooseNumberArrayString { get; set; }
        public string TypeName { get; set; }

        /// <summary>
        /// 选择数字有几个
        /// </summary>
        public int ChooseNumberCount { get; set; }
    }
}
