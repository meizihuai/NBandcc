using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    public class Logger
    {
        private static ILog logger;
        private static bool isShowLogOnConsole = false;
        static Logger()
        {
            if (logger == null)
            {
                var repository = LogManager.CreateRepository("NETCoreRepository");
                //log4net从log4net.config文件中读取配置信息
                XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
                logger = LogManager.GetLogger(repository.Name, "InfoLogger");
            }
        }

        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Info(string message, Exception exception = null)
        {

            if (exception == null)
            {
                //if(isShowLogOnConsole) Console.WriteLine(message);
                logger.Info(message);
            }
            else
            {
                // if (isShowLogOnConsole) Console.WriteLine(message);
                // if (isShowLogOnConsole) Console.WriteLine(exception.ToString());
                logger.Info(message, exception);
            }

        }

        /// <summary>
        /// 告警日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Warn(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Warn(message);
            else
                logger.Warn(message, exception);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Error(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Error(message);
            else
                logger.Error(message, exception);
        }
    }
}
