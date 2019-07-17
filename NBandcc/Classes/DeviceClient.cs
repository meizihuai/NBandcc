using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBandcc
{
    /// <summary>
    /// 用于设备注册、登陆进远端服务器
    /// </summary>
    public class DeviceClient
    {
        private static MTask mTask;
        private static bool FlagLogined = false;
        public static string AID { get; set; }
        public static string DeviceID { get; set; }
        public static void Start()
        {            
            mTask = new MTask(()=>
            {
                LoopLoginAndAlive();
            });
            mTask.Start();
        }
        private static async void LoopLoginAndAlive()
        {
            int aliveSleepSecond = 15;
            int loginSleepSecond = 5;
            while (!mTask.IsCancelled())
            {
                //没有登陆的话，先发登陆
                if (!FlagLogined)
                {
                   // Module.Log("login");
                    FlagLogined = await Login();
                    if (FlagLogined)
                    {
                        continue;
                    }
                    await Task.Delay(loginSleepSecond * 1000);
                    continue;
                }
                //已登陆，发alive
             //   Module.Log("DoAlive");
                bool flag = await DoAlive();
                if (!flag)
                {
                    FlagLogined = false;
                }
                await Task.Delay(aliveSleepSecond * 1000);
            }
        }
        public static void Stop()
        {
            mTask?.Cancel();
        }
        private static async Task<bool> Login()
        {
            //Module.Log("Login");
            NormalResponse np = await API.Login(DeviceID, Module.Version);
            //Module.Log($"    {JsonConvert.SerializeObject(np)}");
            if (np.result)
            {
                DeviceInfo info = np.Parse<DeviceInfo>();
                AID = info.AID;
                return true;
            }
            return false;
        }
        private static async Task<bool> DoAlive()
        {
            //Module.Log("Alive");
            NormalResponse np = await API.Alive(AID);
            return np.result;
            //Module.Log($"    {JsonConvert.SerializeObject(np)}");
        }
    }
}
