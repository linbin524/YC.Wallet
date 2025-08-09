using ApplicationService.IService;
using ApplicationService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.Model;

namespace ApplicationService.Service
{
    public class SqliteService: ISqliteService
    {
        public SqliteService() { }

        public static IApiResult SaveCombinationList(CombinationCreateDto input) {

            if (input.DataList == null || input.DataList.Count == 0) {

                return ApiResult.NotOk("没有需要保存的数据！");
            }
            try
            {
                SQLiteUtils.ExecuteTransaction(() =>
                {
                    CombinationDto combinationDto = new CombinationDto();
                    combinationDto.PermutationType = input.ChooseType;
                    combinationDto.UUID=Guid.NewGuid().ToString();
                    combinationDto.PermutationGroupCount=input.DataList.Count;
                    combinationDto.ChooseNumberAarrayString = input.ChooseNumberArrayString;
                    combinationDto.ChooseNumberCount = input.ChooseNumberCount;
                    combinationDto.Amount = input.DataList.Count * 2;
                    combinationDto.CreatedTime = DateTime.Now;
                    combinationDto.TypeName = input.TypeName;
                  var id=  SQLiteUtils._freesql.Insert<CombinationDto>(combinationDto).ExecuteIdentity();

                   List<Happy8Dto> excelList = new List<Happy8Dto>();
                    List<CombinationDetailDto> list = new List<CombinationDetailDto>();
                    int i = 1;
                    input.DataList.ForEach(x =>
                    {
                        CombinationDetailDto combinationDetailDto = new CombinationDetailDto();
                        combinationDetailDto.CombinationString= string.Join(", ", x);
                        combinationDetailDto.CombinationId = id;
                        combinationDetailDto.IndexId = i;
                        combinationDetailDto.CreatedTime= DateTime.Now;
                        list.Add(combinationDetailDto);
                        i++;
                        
                    });
                    SQLiteUtils._freesql.Insert(list).ExecuteAffrows();//批量插入

                });

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {

                return ApiResult.NotOk(ex.ToString());
            }


           

        }

        public void QueryAll() {
           var users= SQLiteUtils._freesql.Select<User>().ToList();
           var list= SQLiteUtils._freesql.Select<CombinationDto>().ToList();
        }

    }
}
