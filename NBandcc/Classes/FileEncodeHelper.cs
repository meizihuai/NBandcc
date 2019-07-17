using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBandcc
{
    public class FileEncodeHelper
    {
        private static MTask mTask;
       
        public static void Start()
        {
            mTask = new MTask();
            mTask.SetAction(async()=>
            {
                int sleepSecond = 3;
                while (!mTask.IsCancelled())
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
            
            using (var db=new MyDbContext())
            {
                var rt = db.FileQueueTable.Where(a => a.StatusCode == 0).ToList();
                if (rt != null)
                {
                    foreach(var itm in rt)
                    {
                        itm.StatusCode = 1;
                        itm.Status = FileQueueStatus.c1;
                        db.Update(itm, a => a.StatusCode, a => a.Status);
                        await EncodeFileWork(itm);
                    }
                }
            }
        }
        private static Task EncodeFileWork(FileQueueInfo info)
        {
            return Task.Run(async()=>
            {
                Module.CheckDir(Module.FileEncodeOutputDirPath);
                string dirPath = Path.Combine(Module.FileEncodeOutputDirPath, info.Guid);
                Module.CheckDir(dirPath);

                info.OutFileName = "[out]" + info.FileName;
                string filePath = Path.Combine(dirPath, info.OutFileName);
                info.OutFilePath = filePath;
                NormalResponse np=await EncodeFileByFFMpage(info.ServerFilePath, info.OutFilePath, Module.AppSetting.EncodeRate);
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
                            info.EncodeRate = Math.Round(100*(double)info.OutFileLength / (double)info.Length, 1);
                            info.OutputFileUrl = Module.AppSetting.LocalServerUrl + $"/update/fileEncodeOutputDir/{info.Guid}/{info.OutFileName}";
                        }
                    }
                    db.Update(info, a => a.OutFilePath, a => a.OutFileName,a=> a.StatusCode, a => a.Status,a=>a.EncodeLog,
                        a=>a.EncodeUseSecond,a=>a.OutFileLength,a=>a.EncodeRate,a=>a.OutputFileUrl);
                }
            });
        }
        public static Task<NormalResponse> EncodeFileByFFMpage(string input, string output, double rate = 256000)
        {
            return Task.Run(()=>
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
                catch(Exception e)
                {
                    sb.AppendLine(e.ToString());
                    return new NormalResponse(false, "文件编码失败", "", sb.ToString());
                }             
            });                  
        }
    }
}
