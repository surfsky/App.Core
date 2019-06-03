using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Security.Principal;

namespace App.Core
{
    /// <summary>�洢�û�������ɫ�б��Principal</summary>
    public class UserRolePrincipal : GenericPrincipal
    {
        public string[] Roles { get; set; }
        public UserRolePrincipal(IIdentity identity, string[] roles)
            : base(identity, roles)
        {
            this.Roles = roles;
        }
    }

    /// <summary>
    /// ����Ȩ�������������û�����ɫ����Ϣ�ü����ַ���������cookie�У���
    /// ��1��Login ������Ʊ�������û���ɫ����ʱ�����Ϣ���ܱ�����cookie�С�
    /// ��2��LoadPrincipal ��cookie������Ʊ�����õ�ǰ��¼����Ϣ��
    /// ��3��Logout ע��
    /// </summary>
    public class AuthHelper
    {
        /// <summary>�Ƿ��¼</summary>
        public static bool IsLogin()
        {
            return (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated);
        }

        /// <summary>��ǰ��¼�û���</summary>
        public static string GetLoginUserName()
        {
            return IsLogin() ? HttpContext.Current.User.Identity.Name : "";
        }

        /// <summary>��ǰ��¼�û��Ƿ����ĳ����ɫ</summary>
        public static bool HasRole(string role)
        {
            if (IsLogin())
                return HttpContext.Current.User.IsInRole(role);
            return false;
        }



        //-----------------------------------------------
        // ��¼
        //-----------------------------------------------
        /// <summary>��¼�����õ�ǰ�û����������û���ƱCookie����</summary>
        /// <param name="userId">�û�</param>
        /// <param name="roles">��ɫ�����б�</param>
        /// <param name="expiration">��Ʊ����ʱ��</param>
        /// <example>AuthHelper.Login("Admin", new string[] { "Admins" }, DateTime.Now.AddDays(1));</example>
        public static IPrincipal Login(string user, string[] roles, DateTime expiration)
        {
            Logout();
            return CreateCookieTicket(user, roles, "", FormsAuthentication.FormsCookieName, expiration);
        }
        public static IPrincipal CreateCookieTicket(string user, string[] roles, string domain, string cookieName, DateTime expiration)
        {
            // ticket
            var ticket = CreateTicket(user, roles, expiration);

            // cookie
            var ticketString = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(cookieName, ticketString);
            cookie.Expires = expiration;
            cookie.Domain = domain;
            HttpContext.Current.Response.Cookies.Add(cookie);

            // current user
            HttpContext.Current.User = new UserRolePrincipal(new FormsIdentity(ticket), roles);
            return HttpContext.Current.User;
        }


        //-----------------------------------------------
        // ��ȡ Cookie ��Ʊ
        //-----------------------------------------------
        /// <summary>��cookie�ж�ȡ��Ʊ�����õ�ǰ�û�</summary>
        public static IPrincipal LoadPrincipalFromCookie()
        {
            // ��ȡ��ȨCookieֵ
            string cookieName = FormsAuthentication.FormsCookieName;
            string cookieValue = CookieHelper.FindCookie(cookieName);

            // ����Cookie
            if (cookieValue.IsNotEmpty())
            {
                FormsAuthenticationTicket authTicket = ParseTicket(cookieValue, out string user, out string[] roles);
                HttpContext.Current.User = new UserRolePrincipal(new FormsIdentity(authTicket), roles);
                return HttpContext.Current.User;
            }
            return null;
        }



        //-----------------------------------------------
        // ע������
        //-----------------------------------------------
        /// <summary>ע����������Ʊ</summary>
        public static void Logout()
        {
            FormsAuthentication.SignOut();
            ClearAuthCookie();
            HttpContext.Current.User = null;
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session.Abandon();
        }

        private static void ClearAuthCookie()
        {
            string cookieName = FormsAuthentication.FormsCookieName;
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie != null)
                cookie.Expires = System.DateTime.Now;
        }

        public static void RediretToLoginPage()
        {
            FormsAuthentication.RedirectToLoginPage();
        }


        //-----------------------------------------------
        // ��Ʊ�ַ�������
        //-----------------------------------------------
        /// <summary>������Ʊ�ַ���</summary>
        /// <param name="user">�û���</param>
        /// <param name="roles">��ɫ�б�</param>
        /// <param name="expiration">����ʱ��</param>
        private static FormsAuthenticationTicket CreateTicket(string user, string[] roles, DateTime expiration)
        {
            // ����ɫ����ת��Ϊ�ַ���
            string userData = "";
            if (roles != null)
                foreach (string role in roles)
                    userData += role + ";";

            // ������Ʊ������֮
            return new FormsAuthenticationTicket(
                1,                          // �汾
                user,                       // �û���
                DateTime.Now,               // ����ʱ��
                expiration,                 // ����ʱ��
                false,                      // ������
                userData                    // �û�����
                );
        }

        /// <summary>������Ʊ�ַ�������ȡ�û��ͽ�ɫ��Ϣ</summary>
        /// <param name="ticket">��Ʊ�ַ���</param>
        /// <param name="user">�û���</param>
        /// <param name="roles">��ɫ�б�</param>
        /// <returns>����֤Ʊ�ݶ���</returns>
        private static FormsAuthenticationTicket ParseTicket(string ticketString, out string user, out string[] roles)
        {
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(ticketString);
            user = authTicket.Name;
            roles = authTicket.UserData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return authTicket;
        }

        /// <summary>��ǰ��¼�û��Ľ�ɫ�б�</summary>
        public static List<string> GetRoles()
        {
            var roles = new List<string>();
            if (IsLogin())
            {
                FormsAuthenticationTicket ticket = ((FormsIdentity)HttpContext.Current.User.Identity).Ticket;
                string userData = ticket.UserData;
                foreach (string role in userData.Split(','))
                {
                    if (!String.IsNullOrEmpty(role))
                        roles.Add(role);
                }
            }
            return roles;
        }
    }
}
