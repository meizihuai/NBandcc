using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NBandcc.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppsettingController : ControllerBase
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> GetAppsetting()
        {
            return Task.Run(()=>
            {
                return new NormalResponse(true, "", "", AppSettingInfo.ReadConfig());
            });
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public Task<NormalResponse> SaveAppsetting(AppSettingInfo info)
        {
            return Task.Run(() =>
            {
                //if (Module.AppSetting == null)
                //{
                //    return new NormalResponse(false, "Module中配置对象为空");
                //}
                if (info != null)
                {
                    Module.AppSetting = info;
                }
                Module.AppSetting.SaveConfig();
                return new NormalResponse(true, "");
            });
        }
    }
}