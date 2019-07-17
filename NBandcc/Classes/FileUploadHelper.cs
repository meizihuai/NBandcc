using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    public class FileUploadHelper
    {
        private static MTask mTask;
        private static MTask loopUploadTask;
        public static void Start()
        {
            mTask = new MTask();
            mTask.SetAction(async () =>
            {
                while (!mTask.IsCancelled())
                {
                    int result = 0;
                    try
                    {
                        result = DoWork();
                    }
                    catch (Exception e)
                    {
                       // Module.Log(e.ToString());
                    }
                    if (result == 1)
                    {
                        await Task.Delay(3000);
                    }
                }
            });
            mTask.Start();
            loopUploadTask = new MTask();
            loopUploadTask.SetAction(async () =>
            {
                while (!loopUploadTask.IsCancelled())
                {
                    int result = 0;
                    try
                    {
                        result = await SendQueueTableUpload();
                    }
                    catch (Exception)
                    {

                    }
                    if (result == 1)
                    {
                        await Task.Delay(3000);
                    }
                }
            });
            loopUploadTask.Start();
        }
        public static void Stop()
        {
            if (mTask != null)
            {
                mTask.Cancel();
            }

            if (loopUploadTask != null)
            {
                loopUploadTask.Cancel();
            }
        }
        /// <summary>
        /// 查询数据库待传文件队列
        /// </summary>
        /// <returns>1表示没有查到任何需要上传的文件，2表示查到了，但是文件不存在，0表示成功</returns>
        private static int DoWork()
        {
          
            using (var db = new MyDbContext())
            {
                var rt = db.RTSPFileQueueTable.Where(a => a.StatusCode == 2 && !string.IsNullOrEmpty(a.OutFilePath)).FirstOrDefault();
                if (rt == null) return 1;
                if (!File.Exists(rt.OutFilePath))
                {
                   // Module.Log("查询出记录，但是OutFilePath不存在");
                    db.RTSPFileQueueTable.Remove(rt);
                    db.SaveChanges();
                    return 2;
                }
                int per = Module.AppSetting.UploadPerlen * 1024;
                List<SendQueueInfo> list = new List<SendQueueInfo>();
                long readIndex = 0;
                FileInfo finfo = new FileInfo(rt.OutFilePath);
                long sumLength = finfo.Length;
                string outputFileUrl = rt.OutputFileUrl;
              
                while (readIndex < sumLength)
                {
                    long readLen = sumLength - readIndex;
                    if (readLen > per) readLen = per;
                    SendQueueInfo send = new SendQueueInfo();
                    send.DateTime = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss");
                    send.Guid = rt.Guid;
                    send.DirName = rt.DirName;
                    send.Index = list.Count + 1;
                    send.Start = readIndex;
                    send.End = readIndex + readLen;
                    send.Length = readLen;
                    send.TotalLength = sumLength;
                    send.FileName = finfo.Name;
                    send.FilePath = finfo.FullName;
                    send.IsSendOver = 0;
                    send.OutputFileUrl = outputFileUrl;
                    lock (RSTP2TSHelper.LocalTsDirLock)
                    {
                        send.ReadBlock();
                    }
                    send.FileExt = finfo.Extension.Substring(1, finfo.Extension.Length - 1);
                    list.Add(send);
                    readIndex += readLen;
                }
                foreach (var itm in list)
                {
                    itm.TotalIndex = list.Count;
                }
                if (list.Count > 0)
                {
                    db.SendQueueTable.AddRange(list.ToArray());
                }
                lock (RSTP2TSHelper.LocalTsDirLock)
                {
                    if (File.Exists(rt.ServerFilePath))
                    {
                        File.Delete(rt.ServerFilePath);
                    }
                    if (File.Exists(rt.OutFilePath))
                    {
                        File.Delete(rt.OutFilePath);
                    }
                }
                db.RTSPFileQueueTable.Remove(rt);
                db.SaveChanges();
                return 0;
            }
        }
        private static async Task<int> SendQueueTableUpload()
        {
           
            using (var db = new MyDbContext())
            {
               // Module.Log("查询待删除记录...");
                var readyToDeleteList = db.SendQueueTable.Where(a => a.IsSendOver == 1 && a.Index == a.TotalIndex).ToList();
                if (readyToDeleteList != null)
                {
                    foreach (var itm in readyToDeleteList)
                    {
                        var dlist = db.SendQueueTable.Where(a => a.IsSendOver == 1 && a.Guid == itm.Guid).ToList();
                        if (dlist != null)
                        {
                            db.SendQueueTable.RemoveRange(dlist);
                        }
                    }
                    db.SaveChanges();
                }
               // Module.Log("查询待传记录...");
                var rt = db.SendQueueTable.Where(a => a.IsSendOver == 0).OrderBy(a => a.ID).FirstOrDefault();
                if (rt == null)
                {
                   // Module.Log("没有待传记录");
                    return 1;
                }
              
                //Module.Log("开始传输...");
                NormalResponse np = await rt.Upload();
               // Module.Log("传输完毕");
                if (np.result)
                {
                    rt.IsSendOver = 1;
                    db.Update(rt, a => a.IsSendOver);
                    if (rt.Index == rt.TotalIndex)
                    {
                        //本文件所有块都传输完毕，进行清理工作
                       // Module.Log("本文件所有块都传输完毕，进行清理工作");
                        var list = db.SendQueueTable.Where(a => a.IsSendOver == 1 && a.Guid == rt.Guid).ToList();
                        if (list != null)
                        {
                            db.SendQueueTable.RemoveRange(list);
                            db.SaveChanges();
                       //     Module.Log("清理完毕");
                        }
                    }
                }
            }
            return 0;
        }
    }
}
