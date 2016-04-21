using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cryptography {
	public class DesCryptography {
		private const string DEFAULT_ENCRYPT_KEY = "isz69Kv*";

		/// <summary>
		/// 使用默认加密
		/// </summary>
		/// <param name="strText">明文</param>
		/// <returns></returns>
		public static string DesEncrypt(string strText) {
			try {
				return DesEncrypt(strText, DEFAULT_ENCRYPT_KEY);
			}
			catch {
				return null;
			}
		}

		/// <summary>
		/// 使用默认解密
		/// </summary>
		/// <param name="strText">密文</param>
		/// <returns></returns>
		public static string DesDecrypt(string strText) {
			try {
				return DesDecrypt(strText, DEFAULT_ENCRYPT_KEY);
			}
			catch {
				return null;
			}
		}

		/// <summary> 
		/// Encrypt the string 
		/// Attention:key must be 8 bits 
		/// </summary> 
		/// <param name="strText">string</param> 
		/// <param name="strEncrKey">key</param> 
		/// <returns></returns> 
		private static string DesEncrypt(string strText, string strEncrKey) {
			byte[] byKey = null;
			byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

			byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();
			byte[] inputByteArray = Encoding.UTF8.GetBytes(strText);
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
			cs.Write(inputByteArray, 0, inputByteArray.Length);
			cs.FlushFinalBlock();
			return Convert.ToBase64String(ms.ToArray());
		}

		/// <summary> 
		/// Decrypt string 
		/// Attention:key must be 8 bits 
		/// </summary> 
		/// <param name="strText">Decrypt string</param> 
		/// <param name="sDecrKey">key</param> 
		/// <returns>output string</returns> 
		private static string DesDecrypt(string strText, string sDecrKey) {
			byte[] byKey = null;
			byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
			byte[] inputByteArray = new Byte[strText.Length];

			byKey = Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8));
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();
			inputByteArray = Convert.FromBase64String(strText);
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
			cs.Write(inputByteArray, 0, inputByteArray.Length);
			cs.FlushFinalBlock();
			Encoding encoding = new UTF8Encoding();
			return encoding.GetString(ms.ToArray());
		}
	}
}