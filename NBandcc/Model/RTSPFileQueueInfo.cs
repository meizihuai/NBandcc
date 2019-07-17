using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    [Table("RTSPFileQueueTable")]
    public class RTSPFileQueueInfo
    {
        /// <summary>
        /// 数据库主键
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 入库日期
        /// </summary>
        public string DateTime { get; set; }
        /// <summary>
        /// 真实文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件夹名称，用将同组文件放在一次，给服务器使用 m3u8是文件夹内索引
        /// </summary>
        public string DirName { get; set; }
        /// <summary>
        /// 服务器存放路径
        /// </summary>
        public string ServerFilePath { get; set; }
        /// <summary>
        /// 标识ID
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 状态码 0=未处理，1=转码中，2=已转码，3=上传中，4=已上传
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// 编码后文件大小
        /// </summary>
        public long OutFileLength { get; set; }
        /// <summary>
        /// 编码比率
        /// </summary>
        public double EncodeRate { get; set; }
        /// <summary>
        /// 编码后文件名
        /// </summary>
        public string OutFileName { get; set; }
        /// <summary>
        /// 编码后文件路径
        /// </summary>
        public string OutFilePath { get; set; }
        /// <summary>
        /// 编码日志
        /// </summary>
        public string EncodeLog { get; set; }
        /// <summary>
        /// 编码耗时，单位秒
        /// </summary>
        public double EncodeUseSecond { get; set; }
        /// <summary>
        /// 输入文件在服务器中形成的URL，供下载
        /// </summary>
        public string InputFileUrl { get; set; }
        /// <summary>
        /// 编码后文件在服务器中形成的URL，供下载
        /// </summary>
        public string OutputFileUrl { get; set; }
    }
}
