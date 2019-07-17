using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    /// <summary>
    /// 空文件夹和0大小文件清理
    /// </summary>
    public class EmptyDirAndZeroFileCleaner
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
                //程序启动，先等待5分钟
                await Task.Delay(5 * 60 * 1000);
                while (!mTask.IsCancelled())
                {
                    lock (RSTP2TSHelper.LocalTsDirLock)
                    {
                        DoWork();
                    }
                      
                    //30分钟
                    await Task.Delay(30*60*1000);
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
        public static void DoWork()
        {
            string path = Module.FileRTSP2TSDirPath;
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists) return;
            foreach (var dir in info.GetDirectories())
            {
                if (dir.GetDirectories().Length == 0)
                {
                    dir.Delete();
                    continue;
                }

                foreach (var sonDir in dir.GetDirectories())
                {
                    List<FileInfo> list = new List<FileInfo>();
                    if (sonDir.GetFiles().Length == 0)
                    {
                        sonDir.Delete();
                        continue;
                    }
                    list = sonDir.GetFiles().ToList();
                    if (list == null || list.Count == 0) return;
                    foreach (var itm in list)
                    {
                        if (itm.Length == 0)
                        {
                            try
                            {
                                //如果切片线程不在工作，则删除0文件； 切片线程会先生成0文件再写入，防止抢占资源
                                if (!RSTP2TSHelper.GetStatus())
                                {
                                    itm.Delete();
                                }

                            }
                            catch (Exception e)
                            {

                            }
                            continue;
                        }
                    }
                }
            }
        }
    }
}
