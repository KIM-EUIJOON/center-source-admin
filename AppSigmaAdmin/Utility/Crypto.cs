using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AppSigmaAdmin.Utility
{
    public class Crypto
    {
        /// <summary>暗号化ベクタ</summary>
        private const string EncryptIV = "lu2w8qVy";

        /// <summary>
        /// 暗号化
        /// </summary>
        /// <param name="applicationId">アプリケーションID</param>
        /// <param name="targetBuff">暗号化対象の文字列</param>
        /// <returns>暗号化した文字列</returns>
        /// <remarks>※ApplicationIdは暗号化キーとしても利用</remarks>
        public static String Encryption(string applicationId, string targetBuff)
        {
            TripleDESCryptoServiceProvider tdesProvider = null;
            MemoryStream ms = null;
            CryptoStream cs = null;

            try
            {
                // TripleDESサービスプロバイダを生成 
                tdesProvider = new TripleDESCryptoServiceProvider();

                // 暗号対象文字列をバイト配列に変換
                byte[] byteBuff = Encoding.UTF8.GetBytes(targetBuff);

                // 暗号化キーをバイト配列に変換（24Byteであること）
                byte[] byteDesKey = Encoding.UTF8.GetBytes(applicationId);

                // ベクターをバイト配列に変換（8Byteであること）
                byte[] byteDesIV = Encoding.UTF8.GetBytes(EncryptIV);

                // CryptoStreamを生成
                ms = new MemoryStream();
                cs = new CryptoStream(ms, tdesProvider.CreateEncryptor(byteDesKey, byteDesIV), CryptoStreamMode.Write);
                //cs = new CryptoStream(ms, tdesProvider.CreateEncryptor(tdesProvider.Key, tdesProvider.IV), CryptoStreamMode.Write);

                // 対象文字列を暗号化してCryptoStreamに格納
                cs.Write(byteBuff, 0, byteBuff.Length);
                cs.FlushFinalBlock();

                // 暗号化されたデータをbyte配列で取得 
                byte[] cryptData = ms.ToArray();

                // 暗号化されたデータを文字列に変換して返却
                return System.Convert.ToBase64String(cryptData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //ガベージコレクションに委ねる
                //if (ms != null) ms.Close();
                //if (cs != null) cs.Close();
                if (tdesProvider != null) tdesProvider.Dispose();
            }
        }
    }
}