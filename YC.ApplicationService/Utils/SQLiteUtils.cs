using System;
using System.Collections.Generic;
using FreeSql;
using System.IO;
using System.Data.SQLite;
using FreeSql.DataAnnotations;
using ApplicationService.Model;
using YC.Model;
using System.Linq.Expressions;

public  class SQLiteUtils
{
    public static IFreeSql _freesql;

    public static void SqliteInit(string connectionString="")
    {

        if (!string.IsNullOrEmpty(connectionString))
        {
            if (_freesql == null) {
                // 初始化FreeSql
                _freesql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.Sqlite, connectionString)
                    .UseAutoSyncStructure(true) // 自动同步实体结构
                    .Build();
            }
           
        }
        
        //CreateDatabaseAndTables<User>();
        //CreateDatabaseAndTables<CombinationDto>();
        //CreateDatabaseAndTables<CombinationDetailDto>();
        CreateDatabaseAndTables<SysUser>();
    }

    private static void CreateDatabaseAndTables<T>() where T : class
    {
        // 创建数据库（如果文件不存在，SQLite 会自动创建）
        // 创建表（使用 CodeFirst）
        _freesql.CodeFirst.SyncStructure<T>(); // 假设 YourEntity 是你的数据模型
    }

    // 创建表
    public static void CreateTable<T>() where T : class
    {
        _freesql.CodeFirst.SyncStructure<T>();
    }

    // 插入数据
    public static int Insert<T>(T entity) where T : class
    {
        return _freesql.Insert(entity).ExecuteAffrows();
    }

    // 插入数据
    public static long InsertById<T>(T entity) where T : class
    {
       return _freesql.Insert(entity).ExecuteIdentity();
    }

    // 插入数据
    public static int Insert<T>(List<T> entity) where T : class
    {
        return _freesql.Insert(entity).ExecuteAffrows();
    }

    // 查询数据
    public static List<T> Query<T>() where T : class
    {
        return _freesql.Select<T>().ToList();
    }

    // 更新数据
    public static int Update<T>(T entity) where T : class
    {
        return _freesql.Update<T>().SetSource(entity).ExecuteAffrows();
    }
   
    // 删除数据
    public static int Delete<T>(T entity) where T : class
    {
        return _freesql.Delete<T>().WhereDynamic(entity).ExecuteAffrows();
    }

    // 事务处理,采用全局事务处理，同线程事务，
    // 由 fsql.Transaction 管理事务提交回滚（缺点：不支持异步），比较适合 WinForm/WPF UI 主线程使用事务的场景。
    public static void ExecuteTransaction(Action action)
    {
        _freesql.Transaction(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        });

        //using (var uow = fsql.CreateUnitOfWork())// 这种需要配合Repository，同时手动对每个这种需要配合Repository
        //的UnitOfWork进行工作单元传递绑定
        //{
        //    var songRepo = fsql.GetRepository<Song>();
        //    var userRepo = fsql.GetRepository<User>();
        //    songRepo.UnitOfWork = uow; //手工绑定工作单元
        //    userRepo.UnitOfWork = uow;

        //    songRepo.Insert(new Song());
        //    userRepo.Update(...);

        //    uow.Orm.Insert(new Song()).ExecuteAffrows();
        //    //注意：uow.Orm 和 fsql 都是 IFreeSql
        //    //uow.Orm CRUD 与 uow 是一个事务（理解为临时 IFreeSql）
        //    //fsql CRUD 与 uow 不在一个事务

        //    uow.Commit();
        //}

    }
}

// 示例实体类
public class User
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}