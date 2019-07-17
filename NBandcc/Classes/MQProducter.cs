using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBandcc
{
    public class MQProducter : IDisposable
    {
        private ConnectionFactory factory;
        private IConnection mConnection;
        private IModel mChannel;
        private IBasicProperties mProperties;
        private string mQueueName;
        public static MQProducter Creat()
        {
            return new MQProducter(Module.AppSetting.MQ_HostName, Module.AppSetting.MQ_UserName, Module.AppSetting.MQ_Password);
        }
        public MQProducter(string hostName, string userName, string password)
        {
            factory = new ConnectionFactory();
            factory.HostName = hostName;
            factory.UserName = userName;
            factory.Password = password;
        }
        public bool Connect(string queueName, bool durable = true, int exp = 0)
        {
            try
            {
                mConnection = factory.CreateConnection();
                mChannel = mConnection.CreateModel();
                //队列声明
                mChannel.QueueDeclare(queueName, durable, false, false, null);
                this.mQueueName = queueName;
                mProperties = mChannel.CreateBasicProperties();
                //properties.SetPersistent(durable);
                if (exp > 0)
                {
                    mProperties.Expiration = $"{exp * 1000}";
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void Dispose()
        {
            if (mChannel != null)
            {
                mChannel.Dispose();
            }
            if (mConnection != null)
            {
                mConnection.Dispose();
            }
            if (factory != null)
            {
                factory = null;
            }
        }

        public bool Send(string msg)
        {
            try
            {
                string message = msg;
                var body = Encoding.UTF8.GetBytes(message);
                mChannel.BasicPublish("", this.mQueueName, mProperties, body);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
