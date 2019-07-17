using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    public class FileEncodeInfo
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 文件内容，base64
        /// </summary>
        public string FileData { get; set; }
    }
}
