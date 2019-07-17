using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace NBandcc.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CheckController : ControllerBase
    {
        /// <summary>
        /// 查询服务信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public NormalResponse GetInfo()
        {
            return new NormalResponse(true, $"{Module.APPName} : {Module.Version}", "", DeviceHelper.GetLocalIP());
        }
        /// <summary>
        /// 健康检查
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public NormalResponse Health()
        {
            return new NormalResponse(true, "", "", "");
        }
        /// <summary>
        /// 获取服务器时间
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public NormalResponse GetTime()
        {
            return new NormalResponse(true, "", "", TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss"));
        }
        ///// <summary>
        ///// 获取运行日志
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public NormalResponse GetLog()
        //{
        //    string path = "nohup.out";
        //    if (System.IO.File.Exists(path))
        //    {
        //        return new NormalResponse(false, "", "", System.IO.File.ReadAllText(path).Replace(Environment.NewLine,"<br/>"));
        //    }
        //    return new NormalResponse(false, "", "", "");
        //}
    }
}