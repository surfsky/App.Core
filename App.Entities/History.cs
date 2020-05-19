using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Web;
using App.Utils;
using Z.EntityFramework.Plus;
//using EntityFramework.Extensions;

namespace App.Entities
{
    /// <summary>处理历史</summary>
    [UI("系统", "处理历史")]
    public class History : EntityBase<History>
    {
        [UI("健值")]           public string Key { get; set; }                // 如Order-123
        [UI("状态ID")]         public int? StatusId { get; set; }
        [UI("状态")]           public string Status { get; set; }
        [UI("备注")]           public string Remark { get; set; }
                               
        [UI("经手人ID")]       public long? UserId { get; set; }
        [UI("经手人")]         public string UserName { get; set; }
        [UI("经手人手机")]     public string UserMobile { get; set; }


        //
        [UI("限制时间")]       public DateTime? RequirDt { get; set; }
        [UI("接收时间")]       public DateTime? AcceptDt { get; set; }
        [UI("完成时间")]       public DateTime? FinishDt { get; set; }

        // 指派逻辑（单独
        [UI("后继处理人ID")]   public long? NextUserId { get; set; }
        [UI("后继处理人")]     public string NextUserName { get; set; }
        [UI("后继处理人手机")] public string NextUserMobile { get; set; }
        [UI("后继处理时限")]   public DateTime? NextRequirDt { get; set; }

        //public virtual User User { get; set; }

        // 获取导出对象
        public override object Export(ExportMode type = ExportMode.Normal)
        {
            return new
            {
                this.ID,
                this.UserId,
                this.UserName,
                this.UserMobile,
                this.StatusId,
                this.Status,
                this.CreateDt,
                this.Remark,
                //this.User?.NickName,
                Images = this.Images?.Cast(t => t.Export(type)),
            };
        }

        //----------------------------------------------------
        // 基础操作
        //----------------------------------------------------
        /// <summary>查找历史记录</summary>
        public static IQueryable<History> Search(
            string key="", 
            string userName = "", string userMobile = "", 
            DateTime? startDt=null
            )
        {
            IQueryable<History> q = IncludeSet;
            if (!key.IsEmpty())        q = q.Where(t => t.Key == key);
            if (startDt != null)       q = q.Where(t => t.CreateDt >= startDt);
            if (!userName.IsEmpty())   q = q.Where(s => s.UserName.Contains(userName));
            if (!userMobile.IsEmpty()) q = q.Where(s => s.UserMobile.Contains(userMobile));
            return q;
        }

        /// <summary>增加历史记录</summary>
        public static History AddHistory(
            string key, 
            long? userId, 
            string status, 
            int? statusId = null,
            string userName = "", 
            string userMobile = "",
            string remark = "", 
            List<string> fileUrls = null
            )
        {
            var item = new History();
            item.Key = key;
            item.Status = status;
            item.UserId = userId;
            item.StatusId = statusId;
            item.Remark = remark;

            //if (userName.IsEmpty())
            //{
            //    var user = User.Get(userId);
            //    item.UserName = user?.NickName;
            //    item.UserMobile = user?.Mobile;
            //}
            //else
            {
                item.UserName = userName;
                item.UserMobile = userMobile;
            }
            item.CreateDt = DateTime.Now;
            item.Save();

            // 添加资源
            if (fileUrls != null)
                item.AddRes(fileUrls);

            return History.GetDetail(item.ID);
        }

        /// <summary>批量删除</summary>
        public static void DeleteBatch(string key)
        {
            Set.Where(t => t.Key == key).Delete();
        }

        /// <summary>指派后继处理人</summary>
        public void AssignNextUser(long? userId, string userName, string userMobile, DateTime? requirDt)
        {
            this.NextUserId = UserId;
            this.NextUserName = userName;
            this.NextUserMobile = userMobile;
            this.NextRequirDt = requirDt;
            this.Save();
        }
    }
}