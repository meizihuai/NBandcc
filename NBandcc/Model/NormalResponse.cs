using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace NBandcc
{
    /// <summary>
    /// 一般返回格式,JSON格式
    /// </summary>
    public class NormalResponse
    {
        /// <summary>
        /// 处理结果，true:成功，false:失败
        /// </summary>
        public bool result { get; set; }
        /// <summary>
        /// 处理消息或处理过程
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string errmsg { get; set; }
        /// <summary>
        /// 数据，可能是string或者json或者json数组
        /// </summary>
        public object data { get; set; }
        public NormalResponse()
        {

        }
        public NormalResponse(bool result, string msg, string errmsg, object data) // 基本构造函数() As _errmsg,string
        {
            this.result = result;
            this.msg = msg;
            this.errmsg = errmsg;
            this.data = data;
        }

        public NormalResponse(bool result, string msg) // 重载构造函数，为了方便写new,很多时候，只需要一个结果和一个参数() As _result,string
        {
            this.result = result;
            this.msg = msg;
            this.errmsg = "";
            this.data = "";
        }
        public NormalResponse(string msg)
        {
            this.result = false;
            if (msg == "success")
            {
                this.result = true;
            }
            this.msg = msg;
            this.errmsg = "";
            this.data = "";
        }
        public T Parse<T>()
        {
            try
            {
                if (data == null) return default(T);
                string json = JsonConvert.SerializeObject(data);
                // string json = data.ToString();
                T t = JsonConvert.DeserializeObject<T>(json);
                return t;
            }
            catch (Exception e)
            {
                return default(T);
            }
           
        }
    }
}
