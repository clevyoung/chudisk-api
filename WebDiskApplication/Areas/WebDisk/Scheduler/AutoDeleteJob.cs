using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace WebDiskApplication.Areas.WebDisk.Scheduler
{
    public class AutoDeleteJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            string baseUrl = "https://localhost:44393/";
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(
        //new MediaTypeWithQualityHeaderValue("application/json"));
            await client.GetAsync("api/disk/autoDelete");

        }
    }
}