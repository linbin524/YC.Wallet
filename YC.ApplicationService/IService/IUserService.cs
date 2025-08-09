using ApplicationService.IService;
using ApplicationService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.Model;

namespace YC.ApplicationService.IService
{
    public interface IUserService: IDependencyInjectionSupport
    {
        IApiResult RegisterUser(SysUser sysUser);
        IApiResult<SysUser> Login(SysUser sysUser);

        IApiResult<SysUser> InitDefalutLoginUser(SysUser sysUser);
    }
}
