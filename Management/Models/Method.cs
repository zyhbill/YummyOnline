﻿using Management.ObjectClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ZXing;
using ICSharpCode.SharpZipLib.Zip;
using HotelDAO.Models;
using System.Drawing.Drawing2D;

namespace Management.Models
{
    public class Method
    {
        /// <summary>
        /// 密码加密算法
        /// </summary>
        /// <param name="str">明文密码</param>
        /// <returns>加密后的密码</returns>
        public static string GetMd5(string str)
        {
            string pwd = "";
            if (str != null)
            {
                MD5 md5 = MD5.Create();
                byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                for (int i = 0; i < s.Length; i++)
                {
                    pwd = pwd + s[i].ToString("X");
                }
            }
            return pwd;
        }

        public static async Task<string> postHttp(string url, object postData, string contentType = "application/json")
        {
            HttpClient client = new HttpClient();
            string A = (JsonConvert.SerializeObject(postData));
            StringContent content = new StringContent(JsonConvert.SerializeObject(postData));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            HttpResponseMessage response = await client.PostAsync(url, content);

            if (response != null)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            return null;
        }
        /// <summary>
        /// 图像转码
        /// </summary>
        /// <param name="base64Img"></param>
        /// <returns></returns>
        public static Image Base64ToImg(string base64Img)
        {
            if (base64Img != null && base64Img.Length > 0)
            {
                string[] file = base64Img.Split(new char[] { ',' });
                byte[] imageBytes = Convert.FromBase64String(file[1]);
                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                Image image = Image.FromStream(ms, true);
                return image;
            }
            return null;
        }
        /// <summary>
        /// 获取图片地址
        /// </summary>
        /// <param name="HotelId"></param>
        /// <returns></returns>
        public static string GetBaseUrl(int HotelId)
        {
            string path = HttpRuntime.AppDomainAppPath.ToString();
            DirectoryInfo dr = new DirectoryInfo(path);
            path = dr.Parent.FullName.ToString();
            string dirpath = path + "\\OrderSystem\\Content\\image\\" + HotelId + "\\";
            return dirpath;
        }
        /// <summary>
        /// 获取后台系统路径
        /// </summary>
        /// <param name="HotelId"></param>
        /// <returns></returns>
        public static string MyGetBaseUrl(int HotelId)
        {
            string path = HttpRuntime.AppDomainAppPath.ToString();
            DirectoryInfo dr = new DirectoryInfo(path);
            path = dr.FullName.ToString();
            string dirpath = path + "Content\\ModelImage\\" + HotelId + "\\";
            return dirpath;
        }
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="image"></param>
        /// <param name="path"></param>
        public static void SaveImg(string Name, Image image, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string Url = path + Name + ".jpg";
            image.Save(Url, ImageFormat.Jpeg);
        }

        /// <summary>
        /// 二维码解析
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static string ImgDecoder(Image img)
        {
            BarcodeReader reader = new BarcodeReader();
            Bitmap map = new Bitmap(img);
            Result result = reader.Decode(map);
            if (result != null)
            {
                string[] DeskQrCode = result.Text.Split(new char[] { '/' });
                string QrCode = DeskQrCode[DeskQrCode.Length - 1];
                return QrCode;
            }
            return null;
        }
        public static List<string> GetMenuIdByFathers(List<string> Fathers, HotelContext db)
        {
            List<string> ClassId = new List<string>();
            if (Fathers != null)
            {
                foreach (var i in Fathers)
                {
                    ClassId.Add(i);
                    var childs = db.MenuClasses.Where(m => m.Usable == true && m.ParentMenuClassId == i)
                        .Select(m => m.Id)
                        .ToList();
                    foreach (var j in childs)
                    {
                        ClassId.Add(j);
                    }
                }
            }
            else
            {
                return null;
            }
            ClassId = ClassId.GroupBy(g => g).Select(g => g.Key).ToList();
            var menus = db.Menus
                .Where(d => d.Usable == true)
                .Select(m => new
                {
                    Classes = m.Classes,
                    Id = m.Id
                })
                .ToList();
            List<string> Menus = new List<string>();
            foreach (var i in menus)
            {
                foreach (var j in ClassId)
                {
                    if (i.Classes != null)
                    {
                        if (i.Classes.Select(c => c.Id).Contains(j))
                        {
                            Menus.Add(i.Id);
                            break;
                        }
                    }
                }
            }
            Menus = Menus.GroupBy(m => m).Select(m => m.Key).ToList();
            return Menus;
        }

        public static List<string> GetMenuIdByChilds(List<string> Childs, HotelContext db)
        {
            var MenuIds = new List<string>();
            if (Childs != null)
            {
                var Menus = db.Menus
                    .Where(d => d.Usable == true && d.Status == MenuStatus.Normal)
                    .Select(d => new
                    {
                        Id = d.Id,
                        Classes = d.Classes
                    })
                    .ToList();
                foreach (var i in Menus)
                {
                    if (i.Classes != null)
                    {
                        foreach (var j in Childs)
                        {
                            if (i.Classes.Select(d => d.Id).Contains(j))
                            {
                                MenuIds.Add(i.Id);
                            }
                        }
                    }
                }
                MenuIds.GroupBy(g => g).Select(g => g.Key).ToList();
                return MenuIds;
            }
            else
            {
                return null;
            }
        }

        public static  bool SearchFile(string Path, string Name)
        {
            string filePath = Path;
            string fileName = Name;
            if (System.IO.File.Exists(Path+fileName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static List<string> GetMenuIdByFather(string Fathers, HotelContext db)
        {
            List<string> ClassId = new List<string>();
            if (Fathers != null)
            {
                ClassId.Add(Fathers);
                var childs = db.MenuClasses.Where(m => m.Usable == true && m.ParentMenuClassId == Fathers)
                    .Select(m => m.Id)
                    .ToList();
                foreach (var j in childs)
                {
                    ClassId.Add(j);
                }
            }
            else
            {
                return null;
            }
            ClassId = ClassId.GroupBy(g => g).Select(g => g.Key).ToList();
            var menus = db.Menus
                .Where(d => d.Usable == true)
                .Select(m => new
                {
                    Classes = m.Classes,
                    Id = m.Id
                })
                .ToList();
            List<string> Menus = new List<string>();
            foreach (var i in menus)
            {
                foreach (var j in ClassId)
                {
                    if (i.Classes != null)
                    {
                        if (i.Classes.Select(c => c.Id).Contains(j))
                        {
                            Menus.Add(i.Id);
                            break;
                        }
                    }
                }
            }
            Menus = Menus.GroupBy(m => m).Select(m => m.Key).ToList();
            return Menus;
        }

        public static List<string> GetMenuIdByChild(string Childs, HotelContext db)
        {
            var MenuIds = new List<string>();
            if (Childs != null)
            {
                var Menus = db.Menus
                    .Where(d => d.Usable == true && d.Status == MenuStatus.Normal)
                    .Select(d => new
                    {
                        Id = d.Id,
                        Classes = d.Classes
                    })
                    .ToList();
                foreach (var i in Menus)
                {
                    if (i.Classes != null)
                    {
                        if (i.Classes.Select(d => d.Id).Contains(Childs))
                        {
                            MenuIds.Add(i.Id);
                        }
                    }
                }
                MenuIds.GroupBy(g => g).Select(g => g.Key).ToList();
                return MenuIds;
            }
            else
            {
                return null;
            }
        }


        public static bool Send(string mobile, string code)
        {
            string data = "apikey=2763feb79430838ffecf355f9454e624&";
            data += "mobile=" + mobile + "&";
            data += "text=【上海乔曦信息技术有限公司】您的验证码是" + code;
            string result = PostWebRequest("http://yunpian.com/v1/sms/send.json", data);
            if (result.Contains("OK"))
            {
                return true;
            }
            return false;
        }

        private static string PostWebRequest(string postUrl, string paramData)
        {
            string ret = string.Empty;
            byte[] byteArray = Encoding.UTF8.GetBytes(paramData); //转化
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
            webReq.Method = "POST";
            webReq.ContentType = "application/x-www-form-urlencoded";

            webReq.ContentLength = byteArray.Length;
            Stream newStream = webReq.GetRequestStream();
            newStream.Write(byteArray, 0, byteArray.Length);//写入参数
            newStream.Close();
            HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
            ret = sr.ReadToEnd();
            sr.Close();
            response.Close();
            return ret;
        }

        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sFile">原图片</param>
        /// <param name="dFile">压缩后保存位置</param>
        /// <param name="dHeight">高度</param>
        /// <param name="dWidth"></param>
        /// <param name="flag">压缩质量 1-100</param>
        /// <returns></returns>

        public static bool GetPicThumbnail(string sFile, string dFile, int dHeight, int dWidth, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);

            ImageFormat tFormat = iSource.RawFormat;

            int sW = 0, sH = 0;

            //按比例缩放

            Size tem_size = new Size(iSource.Width, iSource.Height);



            if (tem_size.Width > dHeight || tem_size.Width > dWidth) //将**改成c#中的或者操作符号
            {

                if ((tem_size.Width * dHeight) > (tem_size.Height * dWidth))
                {

                    sW = dWidth;

                    sH = (dWidth * tem_size.Height) / tem_size.Width;

                }

                else
                {

                    sH = dHeight;

                    sW = (tem_size.Width * dHeight) / tem_size.Height;

                }

            }

            else
            {

                sW = tem_size.Width;

                sH = tem_size.Height;

            }

            Bitmap ob = new Bitmap(dWidth, dHeight);

            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);

            g.CompositingQuality = CompositingQuality.HighQuality;

            g.SmoothingMode = SmoothingMode.HighQuality;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();

            //以下代码为保存图片时，设置压缩质量

            EncoderParameters ep = new EncoderParameters();

            long[] qy = new long[1];

            qy[0] = flag;//设置压缩的比例1-100

            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);

            ep.Param[0] = eParam;

            try
            {

                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();

                ImageCodecInfo jpegICIinfo = null;

                for (int x = 0; x < arrayICI.Length; x++)
                {

                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {

                        jpegICIinfo = arrayICI[x];

                        break;

                    }

                }

                if (jpegICIinfo != null)
                {

                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径

                }

                else
                {

                    ob.Save(dFile, tFormat);

                }

                return true;

            }

            catch
            {

                return false;

            }

            finally
            {

                iSource.Dispose();

                ob.Dispose();

            }



        }

        //最后2个括号
    }
}