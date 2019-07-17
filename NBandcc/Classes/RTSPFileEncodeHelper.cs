using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NBandcc
{
    public class RTSPFileEncodeHelper
    {
        private static MTask mTask;

        public static void Start()
        {
            mTask = new MTask();
            mTask.SetAction(async () =>
            {
                try
                {
                    using (var db = new MyDbContext())
                    {
                        var rt = db.RTSPFileQueueTable.Where(a => a.StatusCode == 1).ToList();
                        if (rt != null)
                        {
                            db.RTSPFileQueueTable.RemoveRange(rt);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception)
                {
                 
                }
                
                int sleepSecond = 3;
                while (true)
                {
                    try
                    {
                        
                        await DoWork();
                    }
                    catch (Exception)
                    {

                    }
                   
                    await Task.Delay(sleepSecond * 1000);
                }
            });
            mTask.Start();
        }
        public static void Stop()
        {
            if (mTask != null)
            {
                mTask.Cancel();
            }
        }

        private static async Task DoWork()
        {

            using (var db = new MyDbContext())
            {
                var rt = db.RTSPFileQueueTable.Where(a => a.StatusCode == 0).ToList();
                if (rt != null)
                {
                    bool isNeedSave = false;
                    foreach (var itm in rt)
                    {
                        if (itm.Length == 0)
                        {
                            isNeedSave = true;
                            db.RTSPFileQueueTable.Remove(itm);
                        }
                        else
                        {
                            itm.StatusCode = 1;
                            itm.Status = FileQueueStatus.c1;
                            db.Update(itm, a => a.StatusCode, a => a.Status);
                            try
                            {
                               NormalResponse np= await EncodeFileWork(itm);
                                if(np.result && itm.StatusCode == 2)
                                {
                                    //转码成功
                                }
                                else
                                {
                                    isNeedSave = true;
                                    db.RTSPFileQueueTable.Remove(itm);
                                    //itm.EncodeLog = np.msg;
                                    //db.Update(itm, a => a.StatusCode, a => a.Status,a=>a.EncodeLog);
                                }
                            }
                            catch (Exception e)
                            {
                                itm.EncodeLog =Environment.NewLine+e.ToString();
                                db.Update(itm, a => a.StatusCode, a => a.Status, a => a.EncodeLog);
                            }
                           
                        }
                      
                    }
                    if (isNeedSave)
                    {                       
                        db.SaveChanges();
                    }
                }
            }
        }
        private static async Task<NormalResponse> EncodeFileWork(RTSPFileQueueInfo info)
        {
            Module.CheckDir(Module.RTSPFileEncodeOutputDirPath);
            string dirPath = Module.RTSPFileEncodeOutputDirPath;
            Module.CheckDir(dirPath);

          
            info.OutFileName = "[out]" + info.FileName;
            string filePath = Path.Combine(dirPath, info.OutFileName);
            info.OutFilePath = filePath;
            NormalResponse np = null;
            bool flagDoEncode = Module.AppSetting.FlagDoEncode;
            FileInfo inputFileInfo = new FileInfo(info.ServerFilePath);
            if (!inputFileInfo.Exists)
            {
                return new NormalResponse(false, "输入文件不存在，type=1");
            }
            if (inputFileInfo.Extension == ".m3u8")
            {
                flagDoEncode = false;
            }
            if (flagDoEncode)
            {
               // Module.Log("编码");
                np = await EncodeFileByFFMpage(info.ServerFilePath, info.OutFilePath, Module.AppSetting.EncodeRate);
            }
            else
            {
                np = new NormalResponse(true, "", "0", "直复制");
               // Module.Log("直复制");
                if (File.Exists(info.ServerFilePath))
                {
                    info.Length = new FileInfo(info.ServerFilePath).Length;
                    File.Copy(info.ServerFilePath, info.OutFilePath, true);
                }
                else
                {
                    //  Module.Log("info.ServerFilePath文件不存在");
                    return new NormalResponse(false, "输入文件不存在，type=2");
                }
            }
            // Module.Log("入库");
            using (var db = new MyDbContext())
            {            
                info.StatusCode = 2;
                info.Status = FileQueueStatus.c2;
                info.EncodeLog = np.data.ToString();
                if (Utils.IsNumberic(np.errmsg))
                {
                    info.EncodeUseSecond = Math.Ceiling(double.Parse(np.errmsg));
                }
                if (np.result)
                {
                    if (File.Exists(info.OutFilePath))
                    {
                        info.OutFileLength = new FileInfo(info.OutFilePath).Length;
                        
                        info.EncodeRate = Math.Round(100 * (double)info.OutFileLength / (double)info.Length, 1);
                        info.OutputFileUrl = Module.AppSetting.LocalServerUrl + $"/update/rtspFileEncodeOutputDirPath/{info.OutFileName}";
                      //  Module.Log($"info.Length={ info.Length}, info.EncodeRate={ info.EncodeRate}");
                    }
                }
                try
                {
                    int updateResult = db.Update(info, a => a.OutFilePath, a => a.OutFileName, a => a.StatusCode, a => a.Status, a => a.EncodeLog,
                    a => a.EncodeUseSecond, a => a.OutFileLength, a => a.EncodeRate, a => a.OutputFileUrl);
                    return new NormalResponse(true, "已转码");
                    //  Module.Log($"updateResult={updateResult}");
                }
                catch (Exception e)
                {
                    // Module.Log($"updateResult error {e.ToString()}");
                }

            }
            return new NormalResponse(true, "已转码");           
        }
        public static Task<NormalResponse> EncodeFileByFFMpage(string input, string output, double rate = 256000)
        {
            return Task.Run(() =>
            {
                DateTime startTime = TimeUtil.Now();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("开始视频文件编码...");
                FileInfo inputInfo = new FileInfo(input);
                FileInfo outputInfo = new FileInfo(output);
                if (!inputInfo.Exists)
                {
                    sb.AppendLine("输入文件不存在");
                    return new NormalResponse(false, "输入文件不存在", "", sb.ToString());
                }
                sb.AppendLine($"输入文件路径:{inputInfo.FullName}");
                sb.AppendLine($"输出文件路径:{outputInfo.FullName}");
                string ffmpegExepath = $"{Module.AppSetting.FFmpegPath}ffmpeg";
                FileInfo ffmpegExeInfo = new FileInfo(ffmpegExepath);
                if (!ffmpegExeInfo.Exists)
                {
                    sb.AppendLine("ffmpeg文件不存在");
                    return new NormalResponse(false, "ffmpeg文件不存在", "", sb.ToString());
                }
                try
                {
                    ProcessStartInfo ps = new ProcessStartInfo($"{ffmpegExepath}", $"-y -i {input} -c:v libx264 -b:v {rate} -f mp4 {output}");
                    ps.RedirectStandardOutput = true;
                    var proc = Process.Start(ps);
                    if (proc == null)
                    {
                        sb.AppendLine("无法执行该命令");
                        return new NormalResponse(false, "无法执行ffmpeg命令", "", sb.ToString());
                    }
                    else
                    {
                        sb.AppendLine($"开始进行编码，文件名 {inputInfo.Name}");
                        using (var sr = proc.StandardOutput)
                        {
                            while (!sr.EndOfStream)
                            {
                                string str = sr.ReadLine();
                            }
                            try
                            {
                                if (!proc.HasExited)
                                {
                                    proc.Kill();
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        if (File.Exists(output))
                        {
                            long len = new FileInfo(output).Length;
                            DateTime endTime = TimeUtil.Now();
                            TimeSpan ts = endTime - startTime;
                            double workSecond = ts.TotalSeconds;
                            sb.AppendLine($"[ok]文件编码完成,耗时{workSecond}秒,文件大小{len}");
                            return new NormalResponse(true, "编码成功,文件大小{len}", workSecond.ToString(), sb.ToString());
                        }
                        else
                        {
                            sb.AppendLine($"[fail]文件编码失败");
                            return new NormalResponse(false, "文件编码失败", "", sb.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    sb.AppendLine(e.ToString());
                    return new NormalResponse(false, "文件编码失败", "", sb.ToString());
                }
            });
        }
    }
}
