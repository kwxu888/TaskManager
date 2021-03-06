﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Ywdsoft.Utility
{
    /// <summary>
    /// 任务实体
    /// </summary>
    public class TaskUtil
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid TaskID { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 任务执行参数
        /// </summary>
        public string TaskParam { get; set; }

        /// <summary>
        /// 运行频率设置
        /// </summary>
        public string CronExpressionString { get; set; }

        /// <summary>
        /// 任务运频率中文说明
        /// </summary>
        public string CronRemark { get; set; }

        /// <summary>
        /// 任务所在DLL对应的程序集名称
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// 任务所在类
        /// </summary>
        public string Class { get; set; }

        public TaskStatus Status { get; set; }

        /// <summary>
        /// 任务状态中文说明
        /// </summary>
        public string StatusCn
        {
            get
            {
                return Status == TaskStatus.STOP ? "停止" : "运行";
            }
        }

        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// 任务修改时间
        /// </summary>
        public DateTime? ModifyOn { get; set; }

        /// <summary>
        /// 任务最近运行时间
        /// </summary>
        public DateTime? RecentRunTime { get; set; }

        /// <summary>
        /// 任务下次运行时间
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        /// 任务备注
        /// </summary>
        public string Remark { get; set; }


    }

    /// <summary>
    /// 任务日志实体
    /// </summary>
    public class TaskLogUtil
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid LogID { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskID { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskName { get; set; }



        /// <summary>
        /// 任务时间
        /// </summary>
        public DateTime? RunTime { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public int? IsSuccess { get; set; }

        /// <summary>
        /// 运行结果
        /// </summary>
        public string Result { get; set; }

    }

    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        RUN = 0,

        /// <summary>
        /// 停止状态
        /// </summary>
        STOP = 1
    }

    /// <summary>
    /// 任务帮助类
    /// </summary>
    public class TaskHelper
    {

        #region 作业
        private static string InsertSQL = @"INSERT INTO dbo.p_Task(TaskID,TaskName,TaskParam,CronExpressionString,Assembly,Class,Status,CronRemark,Remark,LastRunTime)
                            VALUES(@TaskID,@TaskName,@TaskParam,@CronExpressionString,@Assembly,@Class,@Status,@CronRemark,@Remark,@LastRunTime)";

        private static string UpdateSQL = @"UPDATE dbo.p_Task SET TaskName=@TaskName,TaskParam=@TaskParam,CronExpressionString=@CronExpressionString,Assembly=@Assembly,
                                Class=@Class,CronRemark=@CronRemark,Remark=@Remark,LastRunTime=@LastRunTime WHERE TaskID=@TaskID";


        /// <summary>
        /// 获取指定id任务数据
        /// </summary>
        /// <param name="TaskID">任务id</param>
        /// <returns>任务数据</returns>
        public static TaskUtil GetById(string TaskID)
        {
            return SQLHelper.Single<TaskUtil>("SELECT * FROM p_Task WHERE TaskID=@TaskID", new { TaskID = TaskID });
        }

        /// <summary>
        /// 删除指定id任务
        /// </summary>
        /// <param name="TaskID">任务id</param>
        public static void DeleteById(string TaskID)
        {
            TaskUtil taskUtil = GetById(TaskID);
            QuartzHelper.DeleteJob(taskUtil);
            SQLHelper.ExecuteNonQuery("DELETE FROM p_Task WHERE TaskID=@TaskID", new { TaskID = TaskID });
        }

        /// <summary>
        /// 更新任务运行状态
        /// </summary>
        /// <param name="TaskID">任务id</param>
        /// <param name="Status">任务状态</param>
        public static void UpdateTaskStatus(string TaskID, TaskStatus Status)
        {
            TaskUtil taskUtil = GetById(TaskID);
            if (Status == TaskStatus.RUN)
            {
                QuartzHelper.ResumeJob(taskUtil);
            }
            else
            {
                QuartzHelper.PauseJob(taskUtil);
            }
            SQLHelper.ExecuteNonQuery("UPDATE p_Task SET Status=@Status WHERE TaskID=@TaskID", new { TaskID = TaskID, Status = Status });
        }

        /// <summary>
        /// 更新任务下次运行时间
        /// </summary>
        /// <param name="TaskID">任务id</param>
        /// <param name="LastRunTime">下次运行时间</param>
        public static void UpdateLastRunTime(string TaskID, DateTime LastRunTime)
        {
            SQLHelper.ExecuteNonQuery("UPDATE p_Task SET LastRunTime=@LastRunTime WHERE TaskID=@TaskID", new { TaskID = TaskID, LastRunTime = LastRunTime });
        }

        /// <summary>
        /// 更新任务最近运行时间
        /// </summary>
        /// <param name="TaskID">任务id</param>
        public static void UpdateRecentRunTime(string TaskID, DateTime LastRunTime)
        {
            SQLHelper.ExecuteNonQuery("UPDATE p_Task SET RecentRunTime=GETDATE(),LastRunTime=@LastRunTime WHERE TaskID=@TaskID", new { TaskID = TaskID, LastRunTime = LastRunTime });
        }

        /// <summary>
        /// 获取所有启用的任务
        /// </summary>
        /// <returns>所有启用的任务</returns>
        public static List<TaskUtil> ReadConfig()
        {
            return SQLHelper.ToList<TaskUtil>("SELECT * FROM p_Task");
        }

        /// <summary>
        /// 根据条件查询任务
        /// </summary>
        /// <param name="condition">查询条件</param>
        /// <returns>符合条件的任务</returns>
        public static JsonBaseModel<List<TaskUtil>> Query(QueryCondition condition)
        {
            JsonBaseModel<List<TaskUtil>> result = new JsonBaseModel<List<TaskUtil>>();
            if (string.IsNullOrEmpty(condition.SortField))
            {
                condition.SortField = "CreatedOn";
                condition.SortOrder = "DESC";
            }
            Hashtable ht = Pagination.QueryBase<TaskUtil>("SELECT * FROM p_Task", condition);
            result.Result = ht["data"] as List<TaskUtil>;
            result.TotalCount = Convert.ToInt32(ht["total"]);
            result.TotalPage = result.CalculateTotalPage(condition.PageSize, result.TotalCount.Value, condition.IsPagination);
            return result;
        }

        /// <summary>
        /// 保存任务
        /// </summary>
        /// <param name="value">任务</param>
        /// <returns>保存结果</returns>
        public static JsonBaseModel<string> SaveTask(TaskUtil value)
        {
            JsonBaseModel<string> result = new JsonBaseModel<string>();
            result.HasError = true;
            if (value == null)
            {
                result.Message = "参数空异常";
                return result;
            }

            #region "校验"
            if (string.IsNullOrEmpty(value.TaskName))
            {
                result.Message = "任务名称不能为空";
                return result;
            }
            if (string.IsNullOrEmpty(value.Assembly))
            {
                result.Message = "程序集名称不能为空";
                return result;
            }
            if (string.IsNullOrEmpty(value.CronExpressionString))
            {
                result.Message = "Cron表达式不能为空";
                return result;
            }
            if (!QuartzHelper.ValidExpression(value.CronExpressionString))
            {
                result.Message = "Cron表达式格式不正确";
                return result;
            }
            if (string.IsNullOrEmpty(value.CronRemark))
            {
                result.Message = "表达式说明不能为空";
                return result;
            }
            if (string.IsNullOrEmpty(value.Class))
            {
                result.Message = "类名不能为空";
                return result;
            }
            #endregion

            JsonBaseModel<DateTime> cronResult = null;
            try
            {
                //新增
                if (value.TaskID == Guid.Empty)
                {
                    value.TaskID = Guid.NewGuid();
                    //任务状态处理

                    cronResult = GetTaskeLastRunTime(value.CronExpressionString);
                    if (cronResult.HasError)
                    {
                        result.Message = cronResult.Message;
                        return result;
                    }
                    else
                    {
                        value.LastRunTime = cronResult.Result;
                    }
                    //添加新任务
                    QuartzHelper.ScheduleJob(value);

                    SQLHelper.ExecuteNonQuery(InsertSQL, value);
                }
                else
                {
                    value.ModifyOn = DateTime.Now;
                    TaskUtil srcTask = GetById(value.TaskID.ToString());

                    //表达式改变了重新计算下次运行时间
                    if (!value.CronExpressionString.Equals(srcTask.CronExpressionString, StringComparison.OrdinalIgnoreCase))
                    {
                        cronResult = GetTaskeLastRunTime(value.CronExpressionString);
                        if (cronResult.HasError)
                        {
                            result.Message = cronResult.Message;
                            return result;
                        }
                        else
                        {
                            value.LastRunTime = cronResult.Result;
                        }

                        //更新任务
                        QuartzHelper.ScheduleJob(value, true);
                    }
                    else
                    {
                        value.LastRunTime = srcTask.LastRunTime;
                    }

                    SQLHelper.ExecuteNonQuery(UpdateSQL, value);
                }

                result.HasError = false;
                result.Result = value.TaskID.ToString();
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 计算任务下次运行时间
        /// </summary>
        /// <param name="CronExpressionString"></param>
        /// <returns>下次运行时间</returns>
        private static JsonBaseModel<DateTime> GetTaskeLastRunTime(string CronExpressionString)
        {
            JsonBaseModel<DateTime> result = new JsonBaseModel<DateTime>();
            try
            {
                //计算下次任务运行时间
                result.Result = QuartzHelper.GetTaskeFireTime(CronExpressionString, 1)[0];
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Message = "任务Cron表达式设置错误";
                LogHelper.WriteLog("任务Cron表达式设置错误", ex);
            }
            return result;
        }
        #endregion

        #region 日志
        private static string InsertLogSQL = @"INSERT INTO dbo.p_TaskLog(TaskID,RunTime,IsSuccess,Result)
                            VALUES(@TaskID,@RunTime,@IsSuccess,@Result)";


        /// <summary>
        /// 保存日志任务
        /// </summary>
        /// <param name="value">日志</param>
        /// <returns>保存日志结果</returns>
        public static JsonBaseModel<string> SaveTaskLog(TaskLogUtil value)
        {
            JsonBaseModel<string> result = new JsonBaseModel<string>();
            result.HasError = true;
            if (value == null)
            {
                result.Message = "参数空异常";
                return result;
            }
            try
            {
                SQLHelper.ExecuteNonQuery(InsertLogSQL, value);
                result.HasError = false;
                result.Result = value.TaskID.ToString();
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 根据条件查询任务
        /// </summary>
        /// <param name="condition">查询条件</param>
        /// <returns>符合条件的任务</returns>
        public static JsonBaseModel<List<TaskLogUtil>> QueryLog(QueryCondition condition)
        {
            JsonBaseModel<List<TaskLogUtil>> result = new JsonBaseModel<List<TaskLogUtil>>();
            if (string.IsNullOrEmpty(condition.SortField))
            {
                condition.SortField = "RunTime";
                condition.SortOrder = "DESC";
            }
            Hashtable ht = Pagination.QueryBase<TaskLogUtil>(
                @"SELECT p_Tasklog.*,p_Task.TaskName FROM dbo.p_Tasklog
                    LEFT JOIN dbo.p_Task ON dbo.p_Task.TaskID = p_Tasklog.TaskId", condition);
            result.Result = ht["data"] as List<TaskLogUtil>;
            result.TotalCount = Convert.ToInt32(ht["total"]);
            result.TotalPage = result.CalculateTotalPage(condition.PageSize, result.TotalCount.Value, condition.IsPagination);
            return result;
        }

        /// <summary>
        /// 获取指定id任务数据
        /// </summary>
        /// <param name="TaskID">任务id</param>
        /// <returns>任务数据</returns>
        public static TaskLogUtil GetLogById(string LogID)
        {
            return SQLHelper.Single<TaskLogUtil>(
                @"SELECT p_Tasklog.*,p_Task.TaskName FROM dbo.p_Tasklog
                    LEFT JOIN dbo.p_Task ON dbo.p_Task.TaskID = p_Tasklog.TaskId WHERE LogID=@LogID", new { LogID = LogID });
        }

        /// <summary>
        /// 删除指定id日志
        /// </summary>
        /// <param name="LogID">日志id</param>
        public static void DeleteLogById(string LogID)
        {

            SQLHelper.ExecuteNonQuery("DELETE FROM p_TaskLog WHERE LogID=@LogID", new { LogID = LogID });
        }
        /// <summary>
        /// 删除指定天数前的日志
        /// </summary>
        /// <param name="Days">Days</param>
        public static void DeleteLog(int Days)
        {

            SQLHelper.ExecuteNonQuery("DELETE FROM p_TaskLog WHERE Datediff(d,RunTime,GETDate())>@Days", new { Days = Days });
        }
        #endregion
    }
}
