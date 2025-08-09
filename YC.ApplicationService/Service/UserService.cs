using ApplicationService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.ApplicationService.IService;
using YC.Model;

namespace YC.ApplicationService.Service
{
    public class UserService : IUserService
    {
        public UserService() { }

        public IApiResult RegisterUser(SysUser sysUser) {
            long id = 0;
            try
            {
                var user=SQLiteUtils._freesql.Select<SysUser>().Where(x => x.Account == sysUser.Account).First();
                if (user != null) {
                    return ApiResult.NotOk("用户已经存在！");
                }
                id = SQLiteUtils._freesql.Insert<SysUser>(sysUser).ExecuteIdentity();
                return ApiResult.Ok(id);
            }
            catch (Exception ex)
            {

                return ApiResult.NotOk(ex.ToString());
            }
           
        }

        public IApiResult<SysUser> Login(SysUser sysUser)
        {
            long id = 0;
            var res = new ApiResult<SysUser>();
            try
            {
                var user = SQLiteUtils._freesql.Select<SysUser>().Where(x => x.Account == sysUser.Account&&x.Password==sysUser.Password).First();
                if (user != null)
                {
                    return res.Ok(user);
                }
                else {
                    return res.NotOk("用户名或密码错误！");
                }
               
            }
            catch (Exception ex)
            {

                return res.NotOk(ex.ToString());
            }

        }

        /// <summary>
        /// 初始化创建用户
        /// </summary>
        /// <param name="sysUser"></param>
        /// <returns></returns>
        public IApiResult<SysUser> InitDefalutLoginUser(SysUser sysUser)
        {
            long id = 0;
            var res = new ApiResult<SysUser>();
            try
            {
                var user = SQLiteUtils._freesql.Select<SysUser>().Where(x => x.Account == sysUser.Account && x.Password == sysUser.Password).First();

                if (user != null)
                {
                    if (string.IsNullOrWhiteSpace(user.Name)) {
                        user.Name = sysUser.Name; // 更新用户名称
                        int count = SQLiteUtils._freesql.Update<SysUser>()
                            .Set(x => x.Name, user.Name)
                            .Where(x => x.Id == user.Id)
                            .ExecuteAffrows();
                    }
                    return res.Ok(user);
                }
                else
                {
                    id = SQLiteUtils._freesql.Insert<SysUser>(sysUser).ExecuteIdentity();
                    sysUser.Id = id; // 设置新插入的ID
                    return res.Ok(sysUser); // 返回新插入的用户
                }

            }
            catch (Exception ex)
            {

                return res.NotOk(ex.ToString());
            }

        }
    }
}
