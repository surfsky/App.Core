using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

namespace App.Utils
{
    /// <summary>
    /// Javascript 相关的辅助类
    /// </summary>
    public class ScriptHelper
    {
        // Page
        public static System.Web.UI.Page Page
        {
            get { return System.Web.HttpContext.Current.Handler as System.Web.UI.Page; }
        }

        ///////////////////////////////////////////////////////////////////////////////
        // 客户端Script应用
        ///////////////////////////////////////////////////////////////////////////////
        private static string ToScriptSafeString(string text)
        {
            if (text == null) return "";
            text = text.Replace("'", "\\'");
            text = text.Replace("\"", "\\\"");
            return text;
        }

        // 告警对话框
        public static void Alert(string text)
        {
            text = ToScriptSafeString(text);
            var str = string.Format("window.alert('{0}');", text);
            Page.ClientScript.RegisterStartupScript(typeof(string), "Alert", str, true);
        }

        /// <summary>
        /// 弹出信息并返回到指定页
        /// </summary>
        /// <param name="msg">弹出的消息</param>
        /// <param name="url">指定转向的页面</param>
        public static void Alert(string msg, string url)
        {
            string str = "alert(\"" + msg + "\");location.href=\"" + url + "\";";
            Page.ClientScript.RegisterStartupScript(typeof(string), "Alert", str, true);
        }

        // 确认对话框
        public static void Confirm(string text)
        {
            text = ToScriptSafeString(text);
            var str = string.Format("return window.confirm('{0}');", text);
            Page.ClientScript.RegisterStartupScript(typeof(string), "Confirm", str, true);
        }

        // 点击按钮弹出确认对话框
        public static void Confirm(WebControl control, string msg)
        {
            control.Attributes.Add("onclick", "return confirm('" + msg + "');");
        }

        // 输入对话框
        public static void Prompt(string text)
        {
            text = ToScriptSafeString(text);
            var str = string.Format("return window.prompt('{0}', '');", text);
            Page.ClientScript.RegisterStartupScript(typeof(string), "Confirm", str, true);
        }

        // 模态对话框
        public static void ShowModalDialog(Page page, string url, int width, int height, bool showScroll)
        {
            var str = string.Format("window.showModalDialog('{0}', window, 'dialogWidth:{1}px;dialogHeight:{2}px;center:Yes;help:No;resizable:Yes;status:No;scroll:{3}');",
                url,
                width,
                height,
                showScroll ? "Yes" : "No"
                );
            page.ClientScript.RegisterStartupScript(typeof(string), "ModalDialog", str, true);
        }


        // 非模态对话框
        public static void ShowModelessDialog(Page page, string url, int width, int height, bool showScroll)
        {
            var str = string.Format("window.showModelessDialog('{0}', window, 'dialogWidth:{1}px;dialogHeight:{2}px;center:Yes;help:No;resizable:Yes;status:No;scroll:{3}');",
                url,
                width,
                height,
                showScroll ? "Yes" : "No"
                );
            page.ClientScript.RegisterStartupScript(typeof(string), "ModalessDialog", str, true);
        }

        // 关闭窗口
        public static void Close(Page page)
        {
            var js = new StringBuilder();
            js.Append("<script language=\"JavaScript\">");
            js.Append("window.close();");
            js.Append("</script>");
            page.Response.Write(js.ToString());
        }

        /// <summary>
        /// 为了防止常时间不刷新页面造成会话超时
        /// 每隔一分钟向本页发送一个XmlHttp无刷新请求以维持会话不被超时
        /// 这个方法也在Page.OnInit方法里调用
        /// </summary>
        public static void KeepClientActive()
        {
            var sb = new StringBuilder();
            sb.Append("<SCRIPT LANGUAGE=\"JavaScript\">");
            sb.Append("function GetMessage(){");
            sb.Append("  var xh = new ActiveXObject(\"Microsoft.XMLHTTP\");");
            sb.Append("  xh.open(\"get\", window.location, false);");
            sb.Append("  xh.send();");
            sb.Append("  window.setTimeout(\"GetMessage()\", 60000);");
            sb.Append("}");
            sb.Append("window.onload = GetMessage();");
            sb.Append("</SCRIPT>       ");
            if (!Page.ClientScript.IsStartupScriptRegistered("xmlreload"))
                Page.ClientScript.RegisterStartupScript(typeof(string), "xmlreload", sb.ToString());
        }

    }
}
