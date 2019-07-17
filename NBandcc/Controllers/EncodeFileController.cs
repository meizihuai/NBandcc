using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace NBandcc.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EncodeFileController : ControllerBase
    {
        /// <summary>
        /// 前端上传文件，进行编解码
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public Task<NormalResponse> AddEncodeFileMission(FileEncodeInfo info)
        {
            return Task.Run(()=>
            {
                if (info == null) return new NormalResponse(false, "FileEncodeInfo不可为空");
                if(string.IsNullOrEmpty(info.FileName)) return new NormalResponse(false, "文件名不可为空");
                if(string.IsNullOrEmpty(info.FileData)) return new NormalResponse(false, "文件内容不可为空");
                try
                {
                    byte[] buffer = Convert.FromBase64String(info.FileData);
                    if (buffer == null || buffer.Length == 0)
                    {
                        return new NormalResponse(false, "文件内容格式非法,可能不是base64标准格式");
                    }
                    Module.CheckDir(Module.FileQueueDirPath);
                    info.Length = buffer.Length;
                    string guid = Guid.NewGuid().ToString("N");
                    string dirPath = Path.Combine(Module.FileQueueDirPath, guid);
                    Module.CheckDir(dirPath);
                    string filePath = Path.Combine(dirPath, info.FileName);
                    System.IO.File.WriteAllBytes(filePath, buffer);
                    FileQueueInfo finfo = new FileQueueInfo();
                    finfo.DateTime = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss");
                    finfo.FileName = info.FileName;
                    finfo.ServerFilePath = filePath;
                    finfo.Guid = guid;
                    finfo.StatusCode = 0;
                    finfo.Status = FileQueueStatus.c0;
                    finfo.Length = info.Length;
                    finfo.InputFileUrl = Module.AppSetting.LocalServerUrl + $"/update/fileQueueDir/{guid}/{finfo.FileName}";
                    using(var db=new MyDbContext())
                    {
                        db.FileQueueTable.Add(finfo);
                        int count= db.SaveChanges();
                        if (count > 0)
                        {
                            return new NormalResponse(true, "上传成功");
                        }
                        else
                        {
                            return new NormalResponse(false, "上传失败，数据入库失败");
                        }
                    }
                }
                catch (Exception)
                {
                    return new NormalResponse(false, "文件内容格式非法,可能不是base64标准格式");
                }                
            });
        }
        /// <summary>
        /// 删除文件队列记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> DeleteFileQueue(int id)
        {
            return Task.Run(()=>
            {
                using(var db=new MyDbContext())
                {
                    var rt = db.FileQueueTable.Find(id);
                    if (rt != null)
                    {
                        db.FileQueueTable.Remove(rt);
                        db.SaveChanges();
                    }
                }
                return new NormalResponse(true, "删除成功");
            });
        }
        /// <summary>
        /// 获取文件队列
        /// </summary>
        /// <param name="startTime">入库起始时间</param>
        /// <param name="endTime">入库结束时间</param>
        /// <param name="statusCode">状态码,-1表示不筛选状态码</param>
        /// <param name="getCount">获取数量,0表示全量</param>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> GetFileQueueList(string startTime="",string endTime="",int statusCode=-1,int getCount=0)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var db = new MyDbContext())
                    {
                        var rt = db.FileQueueTable.AsQueryable();
                        if (!string.IsNullOrEmpty(startTime)) rt = rt.Where(a => a.DateTime.CompareTo(startTime) >= 0);
                        if (!string.IsNullOrEmpty(endTime)) rt = rt.Where(a => a.DateTime.CompareTo(endTime) <= 0);
                        if (statusCode > -1) rt = rt.Where(a => a.StatusCode == statusCode);
                        if (getCount > 0) rt = rt.Take(getCount);
                        return new NormalResponse(true, "", "", rt.ToArray());
                    }
                }
                catch (Exception e)
                {
                    return new NormalResponse(false, e.ToString());
                }                
            });
        }
    }
}