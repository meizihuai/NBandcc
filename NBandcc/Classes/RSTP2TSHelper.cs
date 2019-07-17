using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    public class RSTP2TSHelper
    {
        public static bool IsWorking=false;
        private static string workPath;
        private static Process mProcess;
        private static object mProcessLock = new object();
        private static MTask watchProcessTask;
        public static object LocalTsDirLock = new object();
        public static void Start()
        {
            IsWorking = true;
            lock (LocalTsDirLock)
            {
                Module.CheckDir(Path.Combine(Module.FileRTSP2TSDirPath));
            }
           
            StopProcess();
            StartWatchProcess();
        }
        public static bool GetStatus()
        {
            if (mProcess != null)
            {
                if (!mProcess.HasExited)
                {
                    return true;
                }
            }
            return false;
        }
        private static void StartWatchProcess()
        {
            watchProcessTask = new MTask();
            watchProcessTask.SetAction(async() =>
            {
                while (!watchProcessTask.IsCancelled())
                {
                    try
                    {
                        if (mProcess != null)
                        {
                            lock (LocalTsDirLock)
                            {


                                if (Directory.Exists(workPath))
                                {
                                    DirectoryInfo dinfo = new DirectoryInfo(workPath);
                                    if (dinfo.GetFiles().Length >= 900)
                                    {
                                        Module.Log("切片目录文件超过90，退出本次切片进程");
                                        if (!mProcess.HasExited)
                                        {
                                            mProcess.Kill();
                                            Module.Log("ffmpeg切片进程已退出");
                                        }
                                    }
                                }
                            }
                        }
                       
                        StartProcess();
                    }
                    catch (Exception)
                    {
                    }
                   
                    await Task.Delay(3000);
                }
            });
            watchProcessTask.Start();

        }
        private static void StartProcess()
        {
            Task.Run(() =>
            {
                lock (mProcessLock)
                {
                    bool isNeedRestartProcess = false;
                    if (mProcess == null)
                    {
                        isNeedRestartProcess = true;
                    }
                    else
                    {
                        if (mProcess.HasExited)
                        {
                            isNeedRestartProcess = true;
                        }
                    }
                    if (!isNeedRestartProcess) return;
                    DateTime fileTime= TimeUtil.Now(); 
                    while (true)
                    {
                        DateTime now = TimeUtil.Now();
                        fileTime = now;
                        string dayPath = now.ToString("yyyy_MM_dd");
                        Module.CheckDir(Path.Combine(Module.FileRTSP2TSDirPath, dayPath));
                        string sonDirPath = now.ToString("HHmmss");                       
                        workPath = Path.Combine(Module.FileRTSP2TSDirPath, dayPath, sonDirPath);
                        lock (LocalTsDirLock)
                        {
                            if (!Directory.Exists(workPath))
                            {
                                Directory.CreateDirectory(workPath);
                                break;
                            }
                        }
                    }
                    string ffmpegExepath = $"{Module.AppSetting.FFmpegPath}ffmpeg";
                    FileInfo ffmpegExeInfo = new FileInfo(ffmpegExepath);
                    if (!ffmpegExeInfo.Exists)
                    {
                        Module.Log("ffmpeg文件不存在，无法切片");
                        return;
                    }
                    string order = Module.AppSetting.RTSP2TS_cmd;
                    string fileName = fileTime.ToString("yyyy_MM_dd_HH_mm_ss")+ "_p_%03d";
                    order = order.Replace("{RTSPUrl}", Module.AppSetting.RTSPUrl);
                    order = order.Replace("{m3u8Path}", $"{workPath}/{fileName}.m3u8");
                    order = order.Replace("{tsSecond}", Module.AppSetting.TsSecond.ToString());
                    order = order.Replace("{tsPath}", $"{workPath}/{fileName}.ts");

                    ProcessStartInfo ps = new ProcessStartInfo($"{ffmpegExepath}", order);
                    ps.RedirectStandardOutput = false;
                    mProcess = Process.Start(ps);
                    if (mProcess == null)
                    {
                        Module.Log("无法执行ffmpeg切片命令");
                        return;
                    }
                    using (var sr = mProcess.StandardOutput)
                    {
                        Module.Log("ffmpeg切片进程已开启！");
                        while (!sr.EndOfStream)
                        {
                            // string str = sr.ReadLine();
                            
                        }
                      
                        try
                        {
                            if (!mProcess.HasExited)
                            {
                                mProcess.Kill();

                            }
                        }
                        catch (Exception)
                        {

                        }
                        Module.Log("ffmpeg切片进程已退出");
                    }
                }
            });
            
        }
        private static void StopProcess()
        {
            lock (mProcessLock)
            {
                try
                {
                   
                    if (watchProcessTask != null)
                    {
                        watchProcessTask.Cancel();
                    }
                    if (mProcess != null)
                    {
                       
                        if (!mProcess.HasExited)
                        {
                            Module.Log($"正在退出切片进程,进程ID={mProcess.Id}");
                            mProcess.Kill();
                            Module.Log("切片进程已退出!");
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
           
        }
        public static void Stop()
        {        
            StopProcess();
            IsWorking = false;
        }

    }
}
