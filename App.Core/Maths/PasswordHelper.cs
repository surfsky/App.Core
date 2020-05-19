using System;
using System.Security.Cryptography;

namespace App.Utils
{
    /// <summary>
    /// 密码加密校验类。
    /// 单向哈希加密，最后几个字节保存椒盐值。
    /// </summary>
    public class PasswordHelper
	{
		private const int saltLength = 4;
        public PasswordHelper() { }

        /// <summary>对比用户明文密码是否和加密后密码一致</summary>
        /// <param name="dbPassword">数据库中单向加密后的密码</param>
        /// <param name="planPassword">用户明文密码</param>
        /// <returns></returns>
		public static bool Compare(string dbPassword, string planPassword)
		{
			byte[] dbPwd = Convert.FromBase64String(dbPassword);
			byte[] hashedPwd = HashString(planPassword);
			if(dbPwd.Length == 0 || hashedPwd.Length == 0 || dbPwd.Length != hashedPwd.Length + saltLength)
				return false;

            // 椒盐值来自数据库密钥的最后几位
			byte[] saltValue = new byte[saltLength];
			int saltOffset = hashedPwd.Length;
			for (int i = 0; i < saltLength; i++)
				saltValue[i] = dbPwd[saltOffset + i];

            // 生成加密密钥，再和数据库中的密钥比较
            byte[] saltedPassword = CreateSaltedPassword(hashedPwd, saltValue);
			return CompareByteArray(dbPwd, saltedPassword);
		}

        /// <summary>
        /// 创建数据库密码（加密）
        /// </summary>
		public static string CreateDbPassword(string userPassword)
		{
			// 随机创建椒盐值（4位）
			byte[] saltValue = new byte[saltLength];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(saltValue);

            // 椒盐化处理
            byte[] unsaltedPassword = HashString(userPassword);
            byte[] saltedPassword = CreateSaltedPassword(unsaltedPassword, saltValue);
			return Convert.ToBase64String(saltedPassword);
		}

        // 椒盐化加密密码（单向、最后4位是椒盐）
        private static byte[] CreateSaltedPassword(byte[] password, byte[] salt)
        {
            // 合并密码和椒盐数组，并获取哈希值
            byte[] buffer = new byte[password.Length + salt.Length];
            password.CopyTo(buffer, 0);
            salt.CopyTo(buffer, password.Length);
            byte[] hash = SHA1.Create().ComputeHash(buffer);

            // 合并哈希值和椒盐数组，作为密钥输出
            byte[] result = new byte[hash.Length + salt.Length];
            hash.CopyTo(result, 0);
            salt.CopyTo(result, hash.Length);
            return result;
        }

        //----------------------------------------------------
        // 私有方法
        //----------------------------------------------------
        // 获取字符串哈希值
        private static byte[] HashString(string str)
		{
			byte[] pwd = System.Text.Encoding.UTF8.GetBytes(str);
			return SHA1.Create().ComputeHash(pwd);
		}

        // 比较字节数组
		private static bool CompareByteArray(byte[] array1, byte[] array2)
		{
			if (array1.Length != array2.Length)
				return false;
			for (int i = 0; i < array1.Length; i++)
			{
				if (array1[i] != array2[i])
					return false;
			}
			return true;
		}


	}
}
