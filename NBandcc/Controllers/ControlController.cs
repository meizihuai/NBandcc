using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NBandcc.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ControlController : ControllerBase
    {
        /// <summary>
        /// 开启切片进程
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> OpenRTSP2TsWork()
        {
            return Task.Run(()=>
            {
                RSTP2TSHelper.Start();
                return new NormalResponse(true, "已开启");
            });
        }
        /// <summary>
        /// 关闭切片进程
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> CloseRTSP2TsWork()
        {
            return Task.Run(() =>
            {
                RSTP2TSHelper.Stop();
                return new NormalResponse(true, "已关闭");
            });
        }
        /// <summary>
        /// 查询切片进程状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> GetRTSPWorkStatus()
        {
            return Task.Run(() =>
            {
               
                return new NormalResponse(true, RSTP2TSHelper.GetStatus()?"开启":"关闭");
            });
        }
        /// <summary>
        /// 重启程序
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> DoRestartApp()
        {
            return Task.Run(() =>
            {
                Task.Run(()=>
                {
                   
                   // Process ps = new Process();
                   // ps.StartInfo.FileName = appName;
                    ProcessStartInfo ps = new ProcessStartInfo($"nohup dotnet NBandcc.dll &");
                   // ps.RedirectStandardOutput = false;
                    Process.Start(ps);
                    Environment.Exit(0);
                });
                return new NormalResponse(true,"");
            });
        }
    }
}