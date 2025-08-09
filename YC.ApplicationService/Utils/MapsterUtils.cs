using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.ApplicationService.Utils
{
    /// <summary>
    /// Mapster映射帮助类 -基础使用
    /// Mapster 版本7.3.0（或7.2.0）
    /// ASP.NET使用时可直接services.AddMapster();
    /// </summary>
    public class MapsterUtils
    {

        public static void InitConfig(Action action) {
            action();
        }
        #region 实体映射
        /// <summary>
        /// 1.1、类型映射_默认字段一一对应
        /// T需要映射后的实体 = 需要映射的实体.Adapt<T需要映射后的实体>();
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="tSource">源数据</param>
        /// <returns></returns>
        public static TDestination MapsterTo<TSource, TDestination>(TSource tSource) where TSource : class where TDestination : class
        {
            if (tSource == null) return default;

            return tSource.Adapt<TDestination>();
        }
        /// <summary>
        /// 1.2、类型映射_默认字段一一对应 (映射到现有对象)
        /// T需要映射后的实体 = 需要映射的实体.Adapt<T需要映射后的实体>();
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="tDestination">目标对象</param>
        /// <param name="tSource">源数据</param>
        /// <returns></returns>
        public static TDestination MapsterTo<TSource, TDestination>(TDestination tDestination, TSource tSource) where TSource : class where TDestination : class
        {
            if (tSource == null) return default;

            return tSource.Adapt(tDestination);
        }

        /// <summary>
        /// 2、类型映射
        /// ① 字段名称不对应
        /// ② 类型转化
        /// ③ 字段省略
        /// ④ 字段名称或类型不对应
        /// ⑤ 条件赋值或null处理
        /// ⑥ 组合赋值
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="tSource">源数据</param>
        /// <param name="configurationExpression">类型</param>
        /// <returns></returns>
        public static TDestination MapsterTo<TSource, TDestination>(TSource tSource, TypeAdapterConfig typeAdapterConfig) where TSource : class where TDestination : class
        {
            if (tSource == null) return default;

            //var typeAdapterConfig = new TypeAdapterConfig();
            //typeAdapterConfig.ForType<MapsterTestTable_ViewModel, MapsterTestTable>()
            //    .Map(member => member.DestName, source => source.Name)  // 指定字段一一对应
            //    .Map(member => member.Birthday, source => source.Birthday.ToString("yy-MM-dd HH:mm"))                              // 指定字段，并转化指定的格式
            //    .Map(member => member.Age, source => source.Age > 5)                                                               // 条件赋值
            //    .Ignore(member => member.A1)                                                                                       // 忽略该字段，不给该字段赋值
            //    .IgnoreNullValues(true)                                                                                            // 忽略空值映射
            //    .IgnoreAttribute(typeof(DataMemberAttribute))                                                                      // 忽略指定特性的字段
            //    .Map(member => member.A3, source => source.Name + source.Age * 3 + source.Birthday.ToString("d"))                  // 可以自己随意组合赋值
            //    .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);                                                            // 忽略字段名称的大小写

            var mapper = new Mapper(typeAdapterConfig);  // 可以自己随意组合赋值
            return mapper.Map<TDestination>(tSource);
        }
        #endregion 实体映射

        #region 列表映射
        /// <summary>
        /// 3、集合列表类型映射,默认字段名字一一对应
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="source">源数据</param>
        /// <returns></returns>
        public static List<TDestination> MapsterToList<TSource, TDestination>(List<TSource> sources) where TSource : class where TDestination : class
        {
            if (sources == null) return new List<TDestination>();

            return sources.Adapt<List<TDestination>>();
        }

        /// <summary>
        /// 3、集合列表类型映射,默认字段名字一一对应
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="source">源数据</param>
        /// <returns></returns>
        public static List<TDestination> MapsterToList<TSource, TDestination>(List<TSource> sources, TypeAdapterConfig typeAdapterConfig) where TSource : class where TDestination : class
        {
            if (sources == null) return new List<TDestination>();

            var mapper = new Mapper(typeAdapterConfig);  // 可以自己随意组合赋值
            return mapper.Map<List<TDestination>>(sources);
        }
        #endregion 列表映射


    }
}
