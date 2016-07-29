using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeChat.Models;
using YummyOnlineDAO.Models;
using System.Text.RegularExpressions;

namespace WeChat.Controllers
{
    public class MerchantRegistrationController : Controller
    {
        // GET: MerchantRegistration
        public ActionResult Index()
        {
            return View("MerchantRegistration");
        }

        public JsonResult GetKey(string Phone)
        {
            var Rd = new Random();
            int Key = Rd.Next(100000, 1000000);
            Session["Key"] = Key.ToString();
            Method.Send(Phone, Key.ToString());
            return Json(new JsonSuccess());
        }

        public JsonResult Register(string Name, string Phone, string PassWord, string Key)
        {
            if (Key != Session["Key"] as string)
            {
                return Json(new JsonError("验证码不正确"));
            }
            else
            {
                var rs = new YummyOnlineContext();
                var repeat = rs.Hotels.Where(h => h.Tel == Phone && h.Usable == true).FirstOrDefault();
                if (repeat != null) return Json(new JsonError("该号码已被注册"));
                var Staff = rs.Staffs.Where(d => d.SigninName == Phone).FirstOrDefault();
                if (Staff != null) return Json(new JsonError("该号码已被注册"));
                var NewHotel = new Hotel
                {
                    Address = "默认",
                    ConnectionString = null,
                    CreateDate = DateTime.Now,
                    Tel = Phone,
                    AdminConnectionString = null,
                    Usable = false,
                    OpenTime = new TimeSpan(8, 0, 0),
                    CloseTime = new TimeSpan(20, 0, 0),
                    Name = Name,
                    CssThemePath = "default.css",
                    Id = rs.Hotels.Max(h => h.Id) + 1
                };
                rs.Hotels.Add(NewHotel);
                rs.SaveChanges();
                var ps = Method.GetMd5(PassWord);
                rs.Staffs.Add(new YummyOnlineDAO.Models.Staff
                {
                    HotelId = NewHotel.Id,
                    CreateDate = DateTime.Now,
                    PasswordHash = ps,
                    PhoneNumber = Phone,
                    SigninName = Phone,
                    IsHotelAdmin = true
                });
                rs.SaveChanges();
                return Json(new JsonSuccess());
            }
        }

        public JsonResult Verify(string Phone)
        {
            string dianxin = @"^1[3578][01379]\d{8}$";
            Regex dReg = new Regex(dianxin);
            string liantong = @"^1[345678][01256]\d{8}$";
            Regex lReg = new Regex(liantong);
            string yidong = @"^134[012345678]\d{7}|1[34578][0123456789]\d{8}$";
            Regex yReg = new Regex(yidong);

            var P = new Regex("^[0-9]{11,11}$");
            if (P.IsMatch(Phone))
                return Json(new JsonSuccess());
            else
                return Json(new JsonError("号码不对"));
        }
    }
}