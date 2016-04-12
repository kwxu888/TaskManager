/*
 * 模块名: 任务日志
 * 描述: 查看管理任务日志
 * 作者: 徐凯威
 * 创建日期: 2016/04/12 
 * 邮箱地址：http://kaiwei.cnblogs.com
 */
using Ywdsoft.Utility;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ywdsoft.Modules
{
    public class TaskLogModule : NancyModule
    {
        public TaskLogModule() : base("TaskLog")
        {
            //任务列表
            Get["/Grid"] = r =>
            {
                return View["Grid"];
            };
            //任务日志查看界面
            Get["/View"] = r =>
            {
                return View["View"];
            };
            //取数接口API
            #region
            Get["/GetById/{Id}"] = r =>
            {
                JsonBaseModel<TaskLogUtil> result = new JsonBaseModel<TaskLogUtil>();
                try
                {
                    //取出单条记录数据
                    string LogID = r.Id;
                    result.Result = TaskHelper.GetLogById(LogID);
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.Message = ex.Message;
                }
                return Response.AsJson(result);
            };

            //列表查询接口
            Post["/PostQuery"] = r =>
            {
                QueryCondition condition = this.Bind<QueryCondition>();
                return Response.AsJson(TaskHelper.QueryLog(condition));
            };
            //删除7天前任务日志接口
            Delete["/Delete7DayAgoLog"] = r =>
            {
                JsonBaseModel<string> result = new JsonBaseModel<string>();
                try
                {
                   
                    TaskHelper.DeleteLog(7);
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.Message = ex.Message;
                }
                return Response.AsJson(result);
            };
            //删除任务接口
            Delete["/{Id}"] = r =>
            {
                JsonBaseModel<string> result = new JsonBaseModel<string>();
                try
                {
                    string LogId = r.Id;
                    TaskHelper.DeleteLogById(LogId);
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.Message = ex.Message;
                }
                return Response.AsJson(result);
            };

            #endregion
        }
    }
}