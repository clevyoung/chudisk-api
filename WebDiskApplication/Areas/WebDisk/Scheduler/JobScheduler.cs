using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDiskApplication.Areas.WebDisk.Scheduler
{
    public class JobScheduler
    {
        public static async System.Threading.Tasks.Task Start()
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler scheduler = await schedFact.GetScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<AutoDeleteJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                      (s => s.WithIntervalInHours(24)
                      .OnEveryDay()
                      .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                      )
                //테스트할 때 
                //.StartNow()
                //.WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}