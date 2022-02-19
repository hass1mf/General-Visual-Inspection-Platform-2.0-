using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOS.Common.Helper
{
    /// <summary>
    /// 十六进制 和string转换
    /// </summary>
    public class HexTool
    {
        #region HexToByte
        /// <summary>
        /// method to convert hex string into a byte array msg 格式为 68 74 74 70 3A 2F 2F 77 77 
        /// </summary>
        /// <param name="msg">string to convert</param>
        /// <returns>a byte array</returns>
        public static byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            return comBuffer;
        }
        #endregion

        public static string StrToHexStr(string mStr) //返回处理后的十六进制字符串
        {
            //string str = BitConverter.ToString(
            //ASCIIEncoding.Default.GetBytes(mStr));
            //string str = BitConverter.ToString(
            //ASCIIEncoding.Default.GetBytes(mStr)).Replace("-", " ");

            //string str = string.Empty;
            try
            {
                int num = mStr.IndexOf(" ");
                if (num >= 1)
                {
                    StringBuilder _str = new StringBuilder();
                    mStr = new System.Text.RegularExpressions.Regex("[\\s]+").Replace(mStr.Trim(), ",");
                    string[] arr = mStr.Split(',');
                    for (int i = 0; i < arr.Length; i++)    //这里实现把单位的前面补零
                    {
                        if (arr[i].Length == 1)
                        {
                            arr[i] = "0" + arr[i];
                        }
                        _str.Append(arr[i] + " ");
                    }
                    return _str.ToString().Trim();  //这里只需要返回全部带空格的字符就行
                }
                else
                {
                    string str2;
                    str2 = BitConverter.ToString(ASCIIEncoding.Default.GetBytes(mStr)).Replace("-", " ");
                    return str2;
                }
            }
            catch
            {
                string str = BitConverter.ToString(ASCIIEncoding.Default.GetBytes(mStr)).Replace("-", " ");
                return str;
            }


            //return str;
        } /* StrToHex */

        /// <summary>
        /// 16进制转int
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        static public int HexStringToInt(string hex)
        {
            int num1 = 0;
            int num2 = 0;
            char[] nums = hex.ToCharArray();
            if (hex.Length == 2)
            {
                for (int i = 0; i < nums.Length; i++)
                {
                    string strNum = nums[i].ToString().ToUpper();
                    switch (strNum)
                    {
                        case "A":
                            strNum = "10";
                            break;
                        case "B":
                            strNum = "11";
                            break;
                        case "C":
                            strNum = "12";
                            break;
                        case "D":
                            strNum = "13";
                            break;
                        case "E":
                            strNum = "14";
                            break;
                        case "F":
                            strNum = "15";
                            break;
                        default:

                            break;
                    }
                    if (i == 0)
                    {
                        num1 = int.Parse(strNum) * 16;
                    }
                    if (i == 1)
                    {
                        num2 = int.Parse(strNum);
                    }
                }
            }

            return num1 + num2;
        }

        public static string HexStrToStr(string mHex) // 返回十六进制代表的字符串
        {
            try
            {
                mHex = mHex.Replace(" ", "");
                if (mHex.Length <= 0) return "";
                byte[] vBytes = new byte[mHex.Length / 2];
                for (int i = 0; i < mHex.Length; i += 2)
                    if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2]))
                        vBytes[i / 2] = 0;
                return ASCIIEncoding.Default.GetString(vBytes);
            }
            catch (Exception)
            {
                 Debug.WriteLine($"无法将十六进制的[{mHex}]转换为string");
                return "";
            }
      
        } /* HexToStr */

        #region ByteToHex
        /// <summary>
        /// method to convert a byte array into a hex string
        /// </summary>
        /// <param name="comByte">byte array to convert</param>
        /// <returns>a hex string</returns>
        public static string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }
        #endregion
    }
}
