using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    public class AppSettingInfo
    {
        public string DeviceID { get; set; }
        public string AppVersion { get; set; }
        public string MysqlConnection { get; set; }
        public string LocalServerUrl { get; set; }
        public bool FlagAutoRSTP2Ts { get; set; }
        public bool FlagDoEncode { get; set; }
        public int EncodeRate { get; set; }
      
        public string MQ_HostName { get; set; }
        public string MQ_UserName { get; set; }
        public string MQ_Password { get; set; }
        public string MQ_File_QueueName { get; set; }
        public string MQ_Block_QueueName { get; set; }
        public string FFmpegPath { get; set; }
        public string ServerUrl { get; set; }
        public int UploadPerlen { get; set; }
        public double ConnectTimeout { get; set; }
        public double WriteTimeOut { get; set; }
        public string RTSPUrl { get; set; }
        public int TsSecond { get; set; }
        public string RTSP2TS_cmd { get; set; }
       
        public static AppSettingInfo ReadConfig()
        {
            string fileName = Module.AppSettingFileName;
            if (!File.Exists(fileName))
            {
                return null;
            }
            try
            {
                string txt = File.ReadAllText(fileName);
                AppSettingInfo info = JsonConvert.DeserializeObject<AppSettingInfo>(txt);
               
                return info;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public void SaveConfig()
        {
            string fileName = "appsettings.json";
            File.WriteAllText(fileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
