using System;
using System.Security.Cryptography;

namespace App.Utils
{
    /// <summary>
    /// �������У���ࡣ
    /// �����ϣ���ܣ���󼸸��ֽڱ��潷��ֵ��
    /// </summary>
    public class PasswordHelper
	{
		private const int saltLength = 4;
        public PasswordHelper() { }

        /// <summary>�Ա��û����������Ƿ�ͼ��ܺ�����һ��</summary>
        /// <param name="dbPassword">���ݿ��е�����ܺ������</param>
        /// <param name="planPassword">�û���������</param>
        /// <returns></returns>
		public static bool Compare(string dbPassword, string planPassword)
		{
			byte[] dbPwd = Convert.FromBase64String(dbPassword);
			byte[] hashedPwd = HashString(planPassword);
			if(dbPwd.Length == 0 || hashedPwd.Length == 0 || dbPwd.Length != hashedPwd.Length + saltLength)
				return false;

            // ����ֵ�������ݿ���Կ�����λ
			byte[] saltValue = new byte[saltLength];
			int saltOffset = hashedPwd.Length;
			for (int i = 0; i < saltLength; i++)
				saltValue[i] = dbPwd[saltOffset + i];

            // ���ɼ�����Կ���ٺ����ݿ��е���Կ�Ƚ�
            byte[] saltedPassword = CreateSaltedPassword(hashedPwd, saltValue);
			return CompareByteArray(dbPwd, saltedPassword);
		}

        /// <summary>
        /// �������ݿ����루���ܣ�
        /// </summary>
		public static string CreateDbPassword(string userPassword)
		{
			// �����������ֵ��4λ��
			byte[] saltValue = new byte[saltLength];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(saltValue);

            // ���λ�����
            byte[] unsaltedPassword = HashString(userPassword);
            byte[] saltedPassword = CreateSaltedPassword(unsaltedPassword, saltValue);
			return Convert.ToBase64String(saltedPassword);
		}

        // ���λ��������루�������4λ�ǽ��Σ�
        private static byte[] CreateSaltedPassword(byte[] password, byte[] salt)
        {
            // �ϲ�����ͽ������飬����ȡ��ϣֵ
            byte[] buffer = new byte[password.Length + salt.Length];
            password.CopyTo(buffer, 0);
            salt.CopyTo(buffer, password.Length);
            byte[] hash = SHA1.Create().ComputeHash(buffer);

            // �ϲ���ϣֵ�ͽ������飬��Ϊ��Կ���
            byte[] result = new byte[hash.Length + salt.Length];
            hash.CopyTo(result, 0);
            salt.CopyTo(result, hash.Length);
            return result;
        }

        //----------------------------------------------------
        // ˽�з���
        //----------------------------------------------------
        // ��ȡ�ַ�����ϣֵ
        private static byte[] HashString(string str)
		{
			byte[] pwd = System.Text.Encoding.UTF8.GetBytes(str);
			return SHA1.Create().ComputeHash(pwd);
		}

        // �Ƚ��ֽ�����
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
