using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.ObjectClasses;
using YummyOnlineDAO.Models;
using Management.Models;
using System.Threading.Tasks;
using HotelDAO.Models;
using System.Web.Security;
using System.Data.Entity;

namespace Management.Controllers
{
    public class LoginController : BaseController
    {
        // GET: Login
        public ActionResult Index()
        {
            return View("Login");
        }

        public ActionResult SignUp()
        {
            return View("register");
        }
        /// <summary>
        /// 登陆验证
        /// </summary>
        /// <returns>验证信息</returns>
        public async Task<JsonResult> Verification(RemoteUser User)
        {
            RStatus rs;
            string ConnectingStr;
            using (var db = new YummyOnlineContext())
            {
                string PasswordHash = Method.GetMd5(User.password);
                var clerk = db.Staffs.Where(ck => ck.SigninName == User.username && ck.PasswordHash == PasswordHash);
                var ClerkInfo = await clerk.FirstOrDefaultAsync();
                int? Hotel = clerk.FirstOrDefault()?.HotelId;
                ConnectingStr = db.Hotels.Where(ht => ht.Id == Hotel).First().ConnectionString;
                Session["ConnectString"] = ConnectingStr;
                var hotel = new HotelContext(ConnectingStr);
                rs = clerk.Count() > 0 ? new RStatus { Status = true, HotelId = ClerkInfo.HotelId,Name=hotel.Staffs.Where(s => s.Id == ClerkInfo.Id).Select(s=>s.Name).FirstOrDefault(),ClerkId = ClerkInfo.Id } : new RStatus { Status = false };
                var logs = new YummyOnlineDAO.Models.Log();
                logs.DateTime = DateTime.Now;
                logs.Level = YummyOnlineDAO.Models.Log.LogLevel.Success;
                logs.Program = YummyOnlineDAO.Models.Log.LogProgram.Identity;
                logs.Message = $"Manager Signin: {ClerkInfo.Id} (HotelId {ClerkInfo.HotelId}), Host: {Request.UserHostAddress}";
                db.Logs.Add(logs);
                db.SaveChanges();
            }
            using(var db = new HotelContext(ConnectingStr))
            {
                if (rs.Status) {
                    FormsAuthentication.SetAuthCookie(User.username, false);
                    int[] roles = (from s in db.Staffs
                                    from r in s.StaffRoles
                                    where s.Id == rs.ClerkId
                                    select r.Id).ToArray();
                    var models = from r in db.StaffRoles
                                 from s in r.Schemas
                                 where roles.Contains(r.Id)
                                 select s.Schema;
                    int Rate = db.HotelConfigs.Select(h => h.PointsRatio).FirstOrDefault();
                    Session["Rate"] = Rate;
                    rs.IsStaffPay = 0;
                    rs.IsStaffReturn = 0;
                    rs.IsStaffEdit = 0;
                    foreach (var m in models)
                    {
                        if (m == Schema.StaffPay)
                        {
                            rs.IsStaffPay = 1;
                        }
                        else  if (m == Schema.StaffReturn)
                        {
                            rs.IsStaffReturn = 1;
                        }else if (m == Schema.StaffEdit)
                        {
                            rs.IsStaffEdit = 1;
                        }
                        else
                        {
                            
                        }
                    }
                }
                else
                {

                }    
            }
            Session["User"] = rs;
            Session["Username"] = User.username;
            return await Task.Run(()=>Json(rs));
        }

        public JsonResult Logout()
        {
            var username = Session["Username"] as string;
            FormsAuthentication.SetAuthCookie(username, true);
            return Json(new { Status = true});
        }
        

        public JsonResult GetKey(string Phone)
        {
            var Rd = new Random();
            int Key = Rd.Next(100000, 1000000);
            Session["Key"] = Key.ToString();
            Method.Send(Phone, Key.ToString());
            return Json(new SuccessState());
        }

        public JsonResult Register(string Name,string Phone,string PassWord, string Key)
        {
            if(Key != Session["Key"] as string)
            {
                return Json(new ErrorState("验证码不正确"));
            }
            else
            {
                var rs = new YummyOnlineContext();
                var repeat = rs.Hotels.Where(h => h.Tel == Phone && h.Usable==true).FirstOrDefault();
                if(repeat != null) return Json(new ErrorState("该号码已被注册"));
                var Staff = rs.Staffs.Where(d => d.SigninName == Phone).FirstOrDefault();
                if(Staff!=null) return Json(new ErrorState("该号码已被注册"));
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
                    IsHotelAdmin=true
                });
                rs.SaveChanges();
                return Json(new SuccessState());
            }
        }


    }
}