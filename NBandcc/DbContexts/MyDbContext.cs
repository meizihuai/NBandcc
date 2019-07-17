using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NBandcc
{
    public class MyDbContext : DbContext
    {
        //增加 DbSet
        public DbSet<FileQueueInfo> FileQueueTable { get; set; }
        public DbSet<RTSPFileQueueInfo> RTSPFileQueueTable { get; set; }
        public DbSet<SendQueueInfo> SendQueueTable { get; set; }

        //public MyDbContext(int k)
        //{

        //}
        //public MyDbContext GetNew()
        //{
        //    try
        //    {
        //        return new MyDbContext(0);
        //    }
        //    catch (Exception )
        //    {
        //        return null;
        //    }
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            /// 除了startup里面通过appsetting配置之外，还可以直接在此配置 如下：
            /// server = 123.207.31.37; port = 3306; database = efTest; uid = root; password = Mei19931129; sslmode = none
            string conn = Module.AppSetting.MysqlConnection;
            optionsBuilder.UseMySQL(conn);//配置连接字符串 必须加sslmode=none
        }

        public int Update<TEntity>(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties) where TEntity : class
        {
            try
            {
                var dbEntityEntry = this.Entry(entity);
                if (updatedProperties.Any())
                {
                    foreach (var property in updatedProperties)
                    {
                        dbEntityEntry.Property(property).IsModified = true;
                    }
                }
                else
                {
                    foreach (var property in dbEntityEntry.OriginalValues.Properties)
                    {
                        string pName = property.Name;
                        var original = dbEntityEntry.OriginalValues.GetValue<object>(pName);
                        var current = dbEntityEntry.CurrentValues.GetValue<object>(pName);
                        if (original != null && !original.Equals(current))
                        {
                            dbEntityEntry.Property(pName).IsModified = true;
                        }
                    }
                }
                return this.SaveChanges();
            }
            catch (Exception e)
            {
               // Module.Log(e.ToString());
              
            }
            return 0;
        }
    }
}
