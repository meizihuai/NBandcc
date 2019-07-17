using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    public class API
    {
        public static Task<NormalResponse>Login(string deviceId,string version)
        {
            Dictionary<string, object> dik = new Dictionary<string, object>();
            dik.Add("deviceId", deviceId);
            dik.Add("version", version);
            return HttpHelper.Get("/api/Device/login", dik);
        }
        public static Task<NormalResponse> Alive(string aid)
        {
            Dictionary<string, object> dik = new Dictionary<string, object>();
            dik.Add("aid", aid);
            return HttpHelper.Get("/api/Device/Alive", dik);
        }
        public static  Task<NormalResponse> UploadFileBlock(SendQueueInfo send)
        {         
            return HttpHelper.Post($"/api/file/UploadFileBlock", send);
        }
    }
}
