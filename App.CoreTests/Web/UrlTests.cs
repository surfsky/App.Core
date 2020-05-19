using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils.Tests
{
    [TestClass()]
    public class UrlTests
    {
        [TestMethod()]
        public void UrlTest()
        {
            // 完整url测试
            var url = new Url("http://www.company.com:8080/a/b/c.aspx?mode=new&parentid=1");
            url["mode"] = "edit";
            url["id"] = "5";
            url["more"] = "8";
            url.Remove("parentid");
            var txt = url.QueryString;

            // 仅参数部分
            url = new Url("mode=new&id=1");
            var mode = url["mode"];
            var id = url["id"];
            url["mode"] = "edit";
            txt = url.QueryString;

            // 仅前面部分
            url = new Url("http://www.company.com:8080/a/b/c.aspx");
            mode = url["mode"];
            id = url["id"];
            url["mode"] = "edit";
            txt = url.QueryString;
        }

        [TestMethod()]
        public void UrlTest1()
        {
            var url = new Url("/pages/index/index?inviteStoreId=2");
            var inviteUserId = url["inviteUserId"];
            var inviteStoreId = url["inviteStoreId"];
        }
    }
}