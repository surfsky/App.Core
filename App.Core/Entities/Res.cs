using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using EntityFramework.Extensions;
using Newtonsoft.Json;
using App.Core;
//using App.Components;

namespace App.Entities
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public  enum ResType : int
    {
        [UI("文件")]  File = 1,
        [UI("图片")]  Image = 2,
    }

    /// <summary>
    /// 资源表（可记录文件、文本等信息）
    /// </summary>
    [UI("系统", "附属资源")]
    public class Res : EntityBase<Res>
    {
        [UI("类型")]                       public ResType? Type { get; set; }  // 如 Text|Image|Others
        [UI("键")]                         public string Key { get; set; }     // 如 Order-3
        [UI("内容")]                       public string Content { get; set; } // 如 /Files/Users/a.jpg
        [UI("备注")]                       public string Remark { get; set; }  // 如 超美的图片

        // 基础属性
        [UI("Mime类型"), JsonIgnore]       public string MimeType { get; set; }
        [UI("顺序")]                       public int?   Seq { get; set; }
        [UI("是否保护")]                   public bool?  Protect { get; set; }

        // 文件相关属性
        [UI("名称")]                       public string FileName { get; set; }    // 如 a.jpg
        [UI("文件大小")]                   public long?  FileSize { get; set; }
        [UI("文件扩展名")]                 public string FileExtension { get; set; }
        [UI("文件MD5值"), JsonIgnore]      public string FileMD5 { get; set; }
        [UI("文件访问次数"), JsonIgnore]   public int?   FileVisitCnt { get; set; }
        [UI("文件大小")]                   public string FileSizeText => FileSize?.ToSizeText();
        [UI("文件快照")]                   public string FileSnap { get; set; }    // 如截图、缩略图等（未实现）

        // 覆盖几个属性，避免递归循环
        [NotMapped, JsonIgnore]            public new List<Res> Reses { get; set; } = null;
        [NotMapped, JsonIgnore]            public new List<Res> Images { get; set; } = null;

        // 计算属性
        [NotMapped, UI("完整URL")]
        public string Url
        {
            get { return Asp.ResolveUrl(this.Content);}
        }

        [NotMapped, JsonIgnore, UI("物理路径")]
        public string PhysicalPath
        {
            get { return HttpContext.Current.Server.MapPath(this.Content); }
        }

        //-----------------------------------------------
        // 公共方法
        //-----------------------------------------------
        /// <summary>导出</summary>
        public override object Export(ExportMode type = ExportMode.Normal)
        {
            return new
            {
                this.FileName,
                this.Url,
                this.FileSizeText,

                ID            = type.HasFlag(ExportMode.Detail) ? (long?)this.ID : null,
                Type          = type.HasFlag(ExportMode.Detail) ? this.Type : null,
                MimeType      = type.HasFlag(ExportMode.Detail) ? this.MimeType : null,
                FileSize      = type.HasFlag(ExportMode.Detail) ? this.FileSize : null,
                CreateDt      = type.HasFlag(ExportMode.Detail) ? this.CreateDt : null,
                FileExtension = type.HasFlag(ExportMode.Detail) ? this.FileExtension : null,
                FileMD5       = type.HasFlag(ExportMode.Detail) ? this.FileMD5 : null,
                FileVisitCnt  = type.HasFlag(ExportMode.Detail) ? this.FileVisitCnt : null,
            };
        }

        // 构造函数
        public Res(){}
        public Res(ResType type, string key, string content, string fileName, bool? protect)
        {
            this.Type = type;
            this.Key = key;
            this.Content = content;
            this.FileName = fileName;
            this.FileVisitCnt = 0;
            this.Protect = protect;
            //HandleFileRes(content);
        }

        public override void BeforeSave(EntityOp op)
        {
            this.FileName = this.FileName.GetEnd("\\").GetEnd("/");
            HandleFileRes(this.Content);
        }

        /// <summary>处理文件资源</summary>
        public void HandleFileRes(string url)
        {
            // 文件扩展名及推断属性
            this.FileExtension = url.GetFileExtension();
            this.MimeType = IO.GetMimeType(this.FileExtension);
            this.Type = this.MimeType.Contains("image") ? ResType.Image : ResType.File;

            // 物理文件属性
            if (url.IsSiteFile())
            {
                try
                {
                    var physicalPath = HttpContext.Current.Server.MapPath(url);
                    var fi = new FileInfo(physicalPath);
                    this.FileSize = fi.Length;
                    this.FileMD5 = IO.GetFileMD5(physicalPath);
                }
                catch (Exception e)
                {
                    CoreConfig.Log("GetFileInfoFail", e.Message);
                };
            }
        }

        // 新增
        public static void Add(ResType type, string key, string content, string fileName = "", bool protect=true)
        {
            Res res = new Res(type, key, content, fileName, protect);
            res.Save();
        }




        // 删除资源文件
        public static void DeleteBatch(string key)
        {
            var items = Set.Where(t => t.Key == key).ToList();
            foreach (var item in items)
            {
                try {File.Delete(item.PhysicalPath);}
                catch { }
            }
            Set.Where(t => t.Key == key).Delete();
        }

        // 批量删除资源文件
        public static void DeleteBatch(List<long> ids)
        {
            var items = Set.Where(t => ids.Contains(t.ID)).ToList();
            foreach (var item in items)
            {
                try { File.Delete(item.PhysicalPath); }
                catch { }
            }
            Set.Where(t => ids.Contains(t.ID)).Delete();
        }

        // 批量删除对应键的资源文件
        public static void DeleteBatch(Type entityType, List<long> ids)
        {
            DeleteBatch(entityType.Name, ids);
        }

        // 批量删除对应键的资源文件
        public static void DeleteBatch(string prefix, List<long> ids)
        {
            foreach (int id in ids)
                Res.DeleteBatch(BuildUniID(prefix, id));
        }

        // 检索
        public static IQueryable<Res> Search(string key, ResType? type = null)
        {
            IQueryable<Res> q = Set;
            if (key.IsNotEmpty()) q = q.Where(t => t.Key == key);
            if (type != null)     q = q.Where(t => t.Type == type);
            return q;
        }


    }
}