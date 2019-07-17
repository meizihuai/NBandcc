using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace NBandcc
{
    public class Module
    {
        public static readonly string APPName = "NBandcc";
        public static readonly bool IsLogger = false;
        public static string Version = "1.0.0";
        public static ServiceProvider ServiceProvider;
    
        
        public static string FileQueueDirPath = Path.Combine("update", "fileQueueDir");
        public static string FileEncodeOutputDirPath = Path.Combine("update", "fileEncodeOutputDir");
        public static string RTSPFileEncodeOutputDirPath = Path.Combine("update", "rtspFileEncodeOutputDirPath");

        public static string FileRTSP2TSDirPath= Path.Combine("update", "fileRTSP2TSDir");

        public static string AppSettingFileName = "appsettings.json";
        public static string AppSettingFileNameBak = "appsettings.json.bak";
        public static AppSettingInfo AppSetting;
        public static void Init(IConfiguration Configuration, ServiceProvider serviceProvider)
        {
            if (File.Exists(AppSettingFileName) && !File.Exists(AppSettingFileNameBak))
            {
                File.Copy(AppSettingFileName, AppSettingFileNameBak);
            }
            AppSetting = AppSettingInfo.ReadConfig();

            Module.Version = AppSetting.AppVersion;
            Console.Title = $"{APPName} {Version}";
            Module.ServiceProvider = serviceProvider;
            //MysqlConnstr = Configuration.GetSection("MysqlConnection").Value;

            //EncodeRate= int.Parse(Configuration.GetSection("EncodeRate").Value);

            //LocalServerUrl= Configuration.GetSection("LocalServerUrl").Value;

            //Configuration.GetSection("ServerUrl").Value = "123";


            //ServerUrl = Configuration.GetSection("ServerUrl").Value;
            //FFmpegPath = Configuration.GetSection("FFmpegPath").Value;
            //UploadPerlen = int.Parse(Configuration.GetSection("UploadPerlen").Value);
            //ConnectTimeout= int.Parse(Configuration.GetSection("ConnectTimeout").Value);
            //WriteTimeout = int.Parse(Configuration.GetSection("WriteTimeout").Value);


            //MQHostName = Configuration.GetSection("MQ-HostName").Value;
            //MQUserName = Configuration.GetSection("MQ-UserName").Value;
            //MQPassword = Configuration.GetSection("MQ-Password").Value;

            //MQ_File_QueueName = Configuration.GetSection("MQ-File-QueueNamee").Value;
            //MQ_Block_QueueName = Configuration.GetSection("MQ-Block-QueueName").Value;       
            Start();

        }
        public static void Start()
        {         
            if (File.Exists("nohup.out"))
            {
                File.WriteAllText("nohup.out", "");
            }
            Log("================程序启动================");
            // DeviceClient.MAC = DeviceHelper.GetMacAddress();
            DeviceClient.DeviceID = AppSetting.DeviceID;
            Log($"本机DeviceId：{DeviceClient.DeviceID}");
            DeviceClient.Start();
            //文件分块、上传服务
            FileUploadHelper.Start();
            //文件编码服务，适用于接口，网页接口，用于编码文件
            FileEncodeHelper.Start();
            //文件编码服务，适用于实时RTSP切片文件，从数据库读取未编码记录，进行编码
            RTSPFileEncodeHelper.Start();
            //RTSP流切片服务，从RTSP流切成文件，存放文件夹
            if (AppSetting.FlagAutoRSTP2Ts) RSTP2TSHelper.Start();
            //本地RTSP切片后，TS文件编码服务，从文件夹读取文件入库记录
            LocalTSFile2DbHelper.Start();
            //空文件夹和0大小文件清理
            EmptyDirAndZeroFileCleaner.Start();
        }
        public static void Stop()
        {
            Log("================程序关闭================");
            DeviceClient.Stop();
            //文件分块、上传服务
            FileUploadHelper.Stop();
            //文件编码服务，适用于接口，网页接口，用于编码文件
            FileEncodeHelper.Stop();
            //文件编码服务，适用于实时RTSP切片文件
            RTSPFileEncodeHelper.Stop();
            //RTSP流切片服务
            RSTP2TSHelper.Stop();
            //本地RTSP切片后，TS文件编码服务
            LocalTSFile2DbHelper.Stop();
            //空文件夹和0大小文件清理
            EmptyDirAndZeroFileCleaner.Stop();
        }
        public static void Log(string str)
        {
            Console.WriteLine(TimeUtil.Now().ToString("[HH:mm:ss] ") + str);
            if (IsLogger)
            {
                Logger.Info(str);
            }
        }
        public static void CheckDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
