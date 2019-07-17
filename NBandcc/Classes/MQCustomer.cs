using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
namespace NBandcc
{
    public class MQCustomer : IDisposable
    {
        private ConnectionFactory factory;
        private IConnection mConnection;
        private IModel mChannel;
        public delegate void dOnMsg(string msg);
        public event dOnMsg OnMsg;
        public static MQCustomer Creat()
        {
            return new MQCustomer(Module.AppSetting.MQ_HostName, Module.AppSetting.MQ_UserName, Module.AppSetting.MQ_Password);
        }
        public MQCustomer(string hostName, string userName, string password)
        {
            factory = new ConnectionFactory();
            factory.HostName = hostName;
            factory.UserName = userName;
            factory.Password = password;
        }
        public bool Connect(string queueName, bool durable = true)
        {
            try
            {
                mConnection = factory.CreateConnection();
                mChannel = mConnection.CreateModel();
                //队列声明
                mChannel.QueueDeclare(queueName, durable, false, false, null);

                mChannel.BasicQos(0, 10, false);//最多允许同时处理100条消息,需设置手动应答
                var consumer = new EventingBasicConsumer(mChannel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    if (OnMsg != null)
                    {
                        OnMsg.Invoke(message);
                        //应答
                        mChannel.BasicAck(ea.DeliveryTag, false);
                    }
                };
                //true自动应答;false手动应答
                mChannel.BasicConsume(queueName, false, consumer);
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
    }
}
