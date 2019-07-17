using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBandcc
{
    /// <summary>
    /// 针对RTSP切片后生成的文件入库，遍历文件夹
    /// </summary>
    public class LocalTSFile2DbHelper
    {
        public static MTask mTask;

        public static void Start()
        {
            if (mTask != null)
            {
                mTask.Cancel();
            }
            mTask = new MTask();
            mTask.SetAction(async () =>
            {
                while (!mTask.IsCancelled())
                {

                    DoWork();
                    await Task.Delay(30000);
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
        private static void DoWork()
        {
            string path = Module.FileRTSP2TSDirPath;
            DirectoryInfo info = new DirectoryInfo(path);
            lock (RSTP2TSHelper.LocalTsDirLock)
            {
                if (!info.Exists) return;
            }

            foreach (var dir in info.GetDirectories())
            {
                lock (RSTP2TSHelper.LocalTsDirLock)
                {
                    if (dir.GetDirectories().Length == 0)
                    {
                        //由EmptyDirAndZeroFileCleaner统一清理
                        //dir.Delete();
                        continue;
                    }
                    foreach (var sonDir in dir.GetDirectories())
                    {
                        List<FileInfo> list = new List<FileInfo>();
                        if (sonDir.Exists)
                        {
                            if (sonDir.GetFiles().Length == 0)
                            {
                                //由EmptyDirAndZeroFileCleaner统一清理
                                //sonDir.Delete();
                                continue;
                            }
                            list = sonDir.GetFiles().ToList();
                        }
                        AddDoEncodeFiles2Db(list);
                    }
                }

               
            }
        }
        class FileInfoAndFileRand
        {
            public  FileInfo fInfo;
            public double OrderIndex;
        }
        private static void AddDoEncodeFiles2Db(List<FileInfo> list)
        {
            if (list == null || list.Count == 0) return;
            // list = list.OrderBy(a => a.Name).ToList();
            //list = list.OrderBy(a => a.CreationTime).ToList();
            List<FileInfoAndFileRand> frlist = new List<FileInfoAndFileRand>();
            list.ForEach(a =>
            {
                FileInfoAndFileRand fr = new FileInfoAndFileRand();
                fr.fInfo = a;
                string fileName = a.Name;
                fileName = fileName.Replace("p", "").Replace("_", "").Replace(".ts", "").Replace("%03d.m3u8", ""); ;
                if (Utils.IsNumberic(fileName))
                {
                    fr.OrderIndex = double.Parse(fileName);
                }
                else
                {
                    fr.OrderIndex = 0;
                }
                frlist.Add(fr); 
            });
            frlist = frlist.OrderBy(a => a.OrderIndex).ToList();
            list = new List<FileInfo>();
            frlist.ForEach(a =>
            {
                list.Add(a.fInfo);
            });
            using (var db=new MyDbContext())
            {
                bool isAdd = false;
                foreach(var itm in list)
                {
                    if (itm.Length == 0)
                    {
                        //由EmptyDirAndZeroFileCleaner统一清理
                        //itm.Delete();
                        continue;
                    }
                    var rt = db.RTSPFileQueueTable.Where(a => a.FileName == itm.Name).Count();
                    if (rt >0) continue;
                    isAdd = true;
                     
                    string guid = Guid.NewGuid().ToString("N");
                    RTSPFileQueueInfo finfo = new RTSPFileQueueInfo();
                    finfo.DateTime = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss");
                    finfo.FileName = itm.Name;
                    finfo.DirName =$"{itm.Directory.Parent.Name}_{itm.Directory.Name}" ;
                    finfo.ServerFilePath = itm.FullName;
                    finfo.Guid = guid;
                    finfo.StatusCode = 0;
                    finfo.Status = FileQueueStatus.c0;
                    finfo.Length = itm.Length;                   
                    db.RTSPFileQueueTable.Add(finfo);
                }
                if (isAdd)
                {
                    db.SaveChanges();
                }
            }
        }
    }

}
