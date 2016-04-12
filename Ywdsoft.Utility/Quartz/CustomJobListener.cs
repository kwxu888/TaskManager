using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/// <summary>
/// 任务执行监听器  记录任务执行结果
/// </summary>
namespace Ywdsoft.Utility.Quartz
{
    /// <summary>
    /// 自定义触发器监听
    /// </summary>
    public class CustomJobListener : IJobListener
    {
        public string Name
        {
            get
            {
                return "All_JobListener";
            }
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            TaskLogUtil log = new TaskLogUtil();
            log.TaskID = context.JobDetail.Key.Name;
            log.RunTime = DateTime.Now;
            log.IsSuccess = 1;
            string result = Convert.ToString(context.Result);
            if (!String.IsNullOrWhiteSpace(result))
            {
                log.IsSuccess = 0;
                log.Result = result;
            }
            else
            {
                log.Result = "执行完成";
            }
            if (jobException != null)
            {
                log.IsSuccess = 0;
                if (jobException.InnerException != null)
                {
                    log.Result += jobException.ToString();
                }
            }

            TaskHelper.SaveTaskLog(log);
        }
    }
}
