using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationService.IService
{
    public interface ISqliteService: IDependencyInjectionSupport
    {
        void QueryAll();
    }
}
