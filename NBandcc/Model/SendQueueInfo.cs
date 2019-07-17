using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    [Table("SendQueueTable")]
    public class SendQueueInfo
    {
        public int ID { get; set; }
        public string DateTime { get; set; }
        [NotMapped]
        public string AID { get; set; }
        public string Guid { get; set; }
        public string FileName { get; set; }
        /// <summary>
        /// 文件夹名称，用将同组文件放在一次，给服务器使用 m3u8是文件夹内索引
        /// </summary>
        public string DirName { get; set; }
        public string FilePath { get; set; }
        public int Index { get; set; }
        public int TotalIndex { get; set; }
        public long Length { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public long TotalLength { get; set; }    
        public int IsSendOver { get; set; }
        public string Status { get; set; }
        public string Buffer { get; set; }
        public string OutputFileUrl { get; set; }
        public string FileExt { get; set; }

        public void ReadBlock()
        {
            if (!File.Exists(FilePath)) return;
            FileStream stream = new FileStream(FilePath, FileMode.Open);
            byte[] by = new byte[Length];
            stream.Position = this.Start;
            stream.Read(by, 0, (int)Length);
            stream.Close();
            this.Buffer = Convert.ToBase64String(by);
        }
        public Task<NormalResponse> Upload()
        {
            this.AID = DeviceClient.AID;
            if (string.IsNullOrEmpty(AID))
            {
                return Task.Run(()=>
                {
                    return new NormalResponse(false, "aid不可为空");
                });
            }
            return API.UploadFileBlock(this);
        }
    }
}
