using HotelDAO.Models;
using Management.Models;
using Management.ObjectClasses;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;
using YummyOnlineDAO.Models;
using ICSharpCode.SharpZipLib.Zip;
using System.Data.OleDb;
using System.Data;
using System.Data.Entity.SqlServer;

namespace Management.Controllers
{
    public class BaseinfoController : BaseController
    {
        public ActionResult Areas()
        {
            return View("Areas");
        }

        public ActionResult Desks()
        {
            return View("Desks");
        }

        public ActionResult Menus()
        {
            return View("Menus");
        }

        public ActionResult MenuClasses()
        {
            return View("MenuClasses");
        }

        public ActionResult StaffRoles()
        {
            return View("StaffRoles");
        }

        public ActionResult Staffs()
        {
            return View("Staffs");
        }

        public ActionResult MenuRemarks()
        {
            return View("MenuRemarks");
        }

        public ActionResult Department()
        {
            return View("Departments");
        }

        public ActionResult SetMeals()
        {
            return View("SetMeals");
        }


        public ActionResult Reason()
        {
            return View("Reason");
        }

        public ActionResult Printer()
        {
            return View("Printer");
        }

        public ActionResult ReSoldOut()
        {
            return View("ReSoldOut");
        }
        /// <summary>
        /// 获取区域相关信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> getAreas()
        {
            var Areas = await db.Areas.Where(a => a.Usable == true).ToListAsync();
            var Department = await db.Departments.Where(d => d.Usable == true)
                .Select(d => new { d.Id, d.Name }).ToListAsync();
            return Json(new { Areas = Areas, Departments = Department });
        }
        /// <summary>
        /// 删除选中的区域
        /// </summary>
        /// <param name="AreaId"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteArea(string AreaId)
        {
            var area = await db.Areas.FirstOrDefaultAsync(a => a.Usable == true && a.Id == AreaId);
            area.Usable = false;
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 编辑相关的区域
        /// </summary>
        /// <param name="Area"></param>
        /// <param name="OriginAreaId">原先更改的桌台号码</param>
        /// <returns></returns>
        public async Task<JsonResult> EditArea(Area Area, string OriginAreaId,int Type)
        {
            if (Area.Id != OriginAreaId)
            {
                //更改了主键
                var area = await db.Areas.Where(a => a.Id == OriginAreaId).FirstOrDefaultAsync();
                db.Areas.Remove(area);
                db.SaveChanges();
                Area.Type = (AreaType)Type;
                db.Areas.Add(Area);
            }
            else
            {
                var area = await db.Areas.Where(a => a.Id == Area.Id).FirstOrDefaultAsync();
                area.Name = Area.Name;
                area.Description = Area.Description;
                area.DepartmentServeId = Area.DepartmentServeId;
                area.DepartmentReciptId = Area.DepartmentReciptId;
                area.Type = (AreaType)Type;
            }
            db.SaveChanges();
            return Json(new SuccessState());
        }
        /// <summary>
        /// 增加区域
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddArea(newArea area,int Type)
        {
            var Clean = await db.Areas.FirstOrDefaultAsync(a => a.Id == area.Id);
            if (Clean != null) {
                if(Clean.Usable == true) return Json(new ErrorState("已有相同编号"));
                else
                {
                    Clean.Name = area.Name;
                    Clean.Usable = true;
                    Clean.Description = area.Description;
                    Clean.DepartmentReciptId = area.DepartmentReciptId;
                    Clean.DepartmentServeId = area.DepartmentServeId;
                    Clean.Type = (AreaType)Type;
                    db.SaveChanges();
                    return Json(new SuccessState());
                }
            }
            else
            {
                var NewAr = new Area();
                NewAr.Id = area.Id;
                NewAr.Name = area.Name;
                NewAr.Usable = true;
                NewAr.Description = area.Description;
                NewAr.DepartmentReciptId = area.DepartmentReciptId;
                NewAr.DepartmentServeId = area.DepartmentServeId;
                NewAr.Type = (AreaType)Type;
                db.Areas.Add(NewAr);
                db.SaveChanges();
                return Json(new SuccessState()); 
            }
            
        }
        /// <summary>
        /// 获取桌台
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> getDesk()
        {
            var Desks = await db.Desks.Where(d => d.Usable == true)
                        .Select(d => new
                        {
                            d.AreaId,
                            d.Description,
                            d.Name,
                            d.Status,
                            d.Id,
                            d.Order,
                            d.HeadCount,
                            d.MinPrice,
                            d.QrCode
                        }).OrderBy(d => d.Order).ToListAsync();
            var Areas = await db.Areas.Where(a => a.Usable == true)
                .Select(a => new
                {
                    a.Id,
                    a.Name
                })
                .ToListAsync();
            return Json(new { Desks = Desks, Areas = Areas });
        }
        /// <summary>
        /// 编辑桌台
        /// </summary>
        /// <param name="Desk"></param>
        /// <param name="PicFile"></param>
        /// <param name="OriginId"></param>
        /// <returns></returns>
        public async Task<JsonResult> EditDesk(EditDesk Desk, string PicFile, string OriginId)
        {
            //Image image = Method.Base64ToImg(PicFile);
            int HotelId = (int)(Session["User"] as RStatus).HotelId;
            //string dirpath = Method.GetBaseUrl(HotelId);
            //Method.SaveImg("101", image, dirpath);
            var desk = await db.Desks.FirstOrDefaultAsync(d => d.Id == Desk.Id);
            if (desk.Status != DeskStatus.StandBy) { return Json(new { Status = false, ErrorMessage = "当前桌台还有点单，请支付后修改" }); }
            //if (image != null) { desk.QrCode = Method.ImgDecoder(image); }
            if (Desk.Id == OriginId)
            {
                desk.Name = Desk.Name;
                desk.AreaId = Desk.AreaId;
                desk.Description = Desk.Description;
                desk.HeadCount = Desk.HeadCount;
                desk.MinPrice = Desk.MinPrice;
                desk.QrCode = Desk.QrCode;
            }
            else
            {
                db.Desks.Remove(desk);
                db.Desks.Add(new Desk
                {
                    Id = desk.Id,
                    Name = desk.Name,
                    Description = desk.Description,
                    Status = DeskStatus.StandBy,
                    HeadCount = desk.HeadCount,
                    MinPrice = desk.MinPrice,
                    QrCode = desk.QrCode,
                    Usable = true,
                    AreaId = desk.AreaId,
                    Order = desk.Order
                });
            }
            db.SaveChanges();
            return Json(new { Status = true});
        }
        /// <summary>
        /// 添加桌台
        /// </summary>
        /// <param name="Desk"></param>
        /// <param name="PicFile"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddDesk(EditDesk Desk, string PicFile)
        {
            if(Desk.AreaId==null) return Json(new { Status = false, ErrorMessage = "未选择区域" });
            //Image image = Method.Base64ToImg(PicFile);
            int HotelId = (int)(Session["User"] as RStatus).HotelId;
            var desk = await db.Desks.FirstOrDefaultAsync(d => d.Id == Desk.Id && d.Usable == true);
            if (desk != null) { return Json(new { Status = false, ErrorMessage = "当前桌台与已有桌台编号冲突" }); }
            //if (image != null) { QrCode = Method.ImgDecoder(image);}
            else
            {
                //允许添加
               
            }
            var clean = await db.Desks.FirstOrDefaultAsync(d => d.Id == Desk.Id);
            if (clean != null) db.Desks.Remove(clean);
            var newDesk = new Desk();
            newDesk.Usable = true;
            newDesk.Status = DeskStatus.StandBy;
            newDesk.Order = Desk.Order;
            newDesk.HeadCount = Desk.HeadCount;
            newDesk.MinPrice = Desk.MinPrice;
            newDesk.Id = Desk.Id;
            newDesk.Name = Desk.Name;
            newDesk.Description = Desk.Description;
            newDesk.AreaId = Desk.AreaId;
            newDesk.QrCode = Desk.QrCode;
            db.Desks.Add(newDesk);
            db.SaveChanges();
            return Json(new { Status = true});
        }
        /// <summary>
        /// 删除桌台
        /// </summary>
        /// <param name="DeskId"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteDesk(string DeskId)
        {
            var desk = await db.Desks.FirstOrDefaultAsync(d => d.Id == DeskId);
            if (desk == null) return Json(new { Status = false, ErrorMessage = "未找到此桌台" });
            else
            {
                desk.Usable = false;
            }
            db.SaveChanges();
            return Json(new { Status = true });
        }

        /// <summary>
        /// 获取菜品
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> getMenu()
        {
            var Menus = await db.Menus.Where(m => m.Usable == true)
                .Include(m => m.MenuPrice)
                .Include(m => m.Remarks)
                .Include(m => m.Classes)
                .Select(m => new
                {
                    m.Id,
                    m.Code,
                    m.EnglishName,
                    m.MenuPrice,
                    m.Remarks,
                    m.Classes,
                    m.DepartmentId,
                    m.IsFixed,
                    m.IsSetMeal,
                    m.MinOrderCount,
                    m.NameAbbr,
                    m.Ordered,
                    m.PicturePath,
                    m.SaltyDegree,
                    m.SourDegree,
                    m.SpicyDegree,
                    m.Status,
                    m.Name,
                    m.SupplyDate,
                    m.SweetDegree,
                    m.Unit
                }).ToListAsync();
            var Remarks = await db.Remarks.ToListAsync();
            var Classes = await db.MenuClasses.Where(m => m.Usable == true && m.IsLeaf == true).ToListAsync();
            var Departments = await db.Departments.Where(d => d.Usable == true).ToListAsync();
            return Json(new { Menus = Menus, Remarks = Remarks, Classes = Classes, Departments = Departments });
        }
        /// <summary>
        /// 修改菜品
        /// </summary>
        /// <param name="Menu"></param>
        /// <param name="PicFile"></param>
        /// <param name="Departments"></param>
        /// <param name="Classes"></param>
        /// <param name="Remarks"></param>
        /// <param name="OriId"></param>
        /// <returns></returns>
        public async Task<JsonResult> EditMenu(EditMenu Menu, string PicFile, Department Departments, List<MenuClass> Classes, List<Remark> Remarks, string OriId)
        {
            var HotelId = (Session["User"] as RStatus).HotelId;
            var menu = await db.Menus
                .Include(m => m.Classes)
                .Include(m => m.Remarks)
                .FirstOrDefaultAsync(m => m.Id == OriId);
            if (menu == null) return Json(new { Status = false, ErrorMessage = "别逗了，没有这个菜品" });
            Image image = Method.Base64ToImg(PicFile);
            if (Menu.Id != OriId)
            {
                //更改了主键
                return Json(new { Status = false, ErrorMessage = "对不起，不能更改菜品编号" });
            }
            else
            {

                var NewClass = (Classes==null?null:Classes.Select(c => c.Id).ToList());
                var NewRemarks = (Remarks==null?null:Remarks.Select(r => r.Id).ToList());
                var CleanClass = menu.Classes.Select(m => m.Id);
                var cleanC = await db.MenuClasses.Where(m => CleanClass.Contains(m.Id)).ToListAsync();
                var CleanRemark = menu.Remarks.Select(m => m.Id);
                var cleanR = await db.Remarks.Where(m => CleanRemark.Contains(m.Id)).ToListAsync();
                var classes = (Classes == null ? null : await db.MenuClasses.Where(m => NewClass.Contains(m.Id)).ToListAsync());
                var remarks = (Remarks == null ? null : await db.Remarks.Where(r => NewRemarks.Contains(r.Id)).ToListAsync());
                foreach (var i in cleanC)
                {
                    menu.Classes.Remove(i);
                    db.SaveChanges();
                }
                foreach (var i in cleanR)
                {
                    menu.Remarks.Remove(i);
                    db.SaveChanges();
                }
                menu.Name = Menu.Name;
                menu.NameAbbr = Menu.NameAbbr;
                menu.IsFixed = Menu.IsFixed;
                menu.IsSetMeal = Menu.IsSetMeal;
                menu.SupplyDate = Menu.SupplyDate;
                menu.MinOrderCount = Menu.MinOrderCount;
                menu.Code = Menu.Code;
                menu.SaltyDegree = Menu.SaltyDegree;
                menu.SourDegree = Menu.SourDegree;
                menu.SpicyDegree = Menu.SpicyDegree;
                menu.Status = Menu.Status;
                menu.SweetDegree = Menu.SweetDegree;
                menu.EnglishName = Menu.EnglishName;
                menu.Unit = Menu.Unit;
                menu.DepartmentId = Departments.Id;
                var PriceMenu = await db.MenuPrice.FirstOrDefaultAsync(m => m.Id == Menu.Id);
                PriceMenu.Discount = Menu.MenuPrice.Discount / 100;
                PriceMenu.ExcludePayDiscount = Menu.MenuPrice.ExcludePayDiscount;
                PriceMenu.Points = Menu.MenuPrice.Points;
                PriceMenu.Price = Menu.MenuPrice.Price;
                if (image != null)
                {
                    string BaseUrl = Method.GetBaseUrl((int)HotelId);
                    menu.PicturePath = HotelId.ToString() + "/" + Menu.Id + ".jpg";
                    Method.SaveImg(menu.Id, image, BaseUrl);
                    var MenuPlace  = BaseUrl + Menu.Id + ".jpg";
                    var flag = Method.GetPicThumbnail(MenuPlace, MenuPlace, 200, 300, 50);
                    Method.SaveImg(menu.Id, image, Method.MyGetBaseUrl((int)HotelId));
                }
                db.SaveChanges();
                if(classes != null)
                {
                    foreach (var i in classes)
                    {
                        menu.Classes.Add(i);
                        db.SaveChanges();
                    }
                }
                if (remarks != null)
                {
                    foreach (var i in remarks)
                    {
                        menu.Remarks.Add(i);
                        db.SaveChanges();
                    }
                }
            }

            return Json(new { Status = true });
        }
        /// <summary>
        /// 添加单一菜品
        /// </summary>
        /// <param name="Menu"></param>
        /// <param name="PicFile"></param>
        /// <param name="Remarks"></param>
        /// <param name="Classes"></param>
        /// <param name="Department"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddSingleMenu(AddMenu Menu, string PicFile, List<Remark> Remarks, List<MenuClass> Classes, Department Department)
        {
            var menu = await db.Menus.FirstOrDefaultAsync(m => m.Id == Menu.Id && m.Usable == true);
            if (menu != null) return Json(new { Status = false, ErrorMessage = "对不起编号已有菜品请删除完再添加" });
            var depart = await db.Departments.Where(d => d.Id == Department.Id).FirstOrDefaultAsync();
            if(depart==null) return Json(new { Status = false, ErrorMessage = "对不起,请选择出品部门" });
            else
            {
                var HotelId = (Session["User"] as RStatus).HotelId;
                Image image = Method.Base64ToImg(PicFile);
                menu = await db.Menus
                    .Include(m => m.Classes)
                   .Include(m => m.Remarks)
                   .FirstOrDefaultAsync(m => m.Id == Menu.Id);
                if (menu != null)
                {
                    menu.Name = Menu.Name;
                    menu.NameAbbr = Menu.NameAbbr;
                    menu.IsFixed = Menu.IsFixed;
                    menu.IsSetMeal = Menu.IsSetMeal;
                    menu.SupplyDate = Menu.SupplyDate;
                    menu.EnglishName = Menu.EnglishName;
                    menu.MinOrderCount = Menu.MinOrderCount;
                    menu.Code = Menu.Code;
                    menu.SaltyDegree = Menu.SaltyDegree;
                    menu.SourDegree = Menu.SourDegree;
                    menu.SpicyDegree = Menu.SpicyDegree;
                    menu.Status = Menu.Status;
                    menu.SweetDegree = Menu.SweetDegree;
                    menu.Unit = Menu.Unit;
                    menu.DepartmentId = Department.Id;
                    menu.Usable = true;
                    var CleanClass = menu.Classes.Select(m => m.Id);
                    var cleanC = await db.MenuClasses.Where(m => CleanClass.Contains(m.Id)).ToListAsync();
                    var CleanRemark = menu.Remarks.Select(m => m.Id);
                    var cleanR = await db.Remarks.Where(m => CleanRemark.Contains(m.Id)).ToListAsync();
                    //有已经删除过的信息，只能更改
                    foreach (var i in cleanC)
                    {
                        menu.Classes.Remove(i);
                        db.SaveChanges();
                    }
                    foreach (var i in cleanR)
                    {
                        menu.Remarks.Remove(i);
                        db.SaveChanges();
                    }
                    var PriceMenu = await db.MenuPrice.FirstOrDefaultAsync(m => m.Id == Menu.Id);
                    PriceMenu.Discount = Menu.MenuPrice.Discount / 100;
                    PriceMenu.ExcludePayDiscount = Menu.MenuPrice.ExcludePayDiscount;
                    PriceMenu.Points = Menu.MenuPrice.Points;
                    PriceMenu.Price = Menu.MenuPrice.Price;
                    db.SaveChanges();

                }
                else
                {
                    menu = new Menu();
                    menu.Id = Menu.Id;
                    menu.Name = Menu.Name;
                    menu.NameAbbr = Menu.NameAbbr;
                    menu.IsFixed = Menu.IsFixed;
                    menu.IsSetMeal = Menu.IsSetMeal;
                    menu.EnglishName = Menu.EnglishName;
                    menu.SupplyDate = Menu.SupplyDate;
                    menu.MinOrderCount = Menu.MinOrderCount;
                    menu.Code = Menu.Code;
                    menu.SaltyDegree = Menu.SaltyDegree;
                    menu.SourDegree = Menu.SourDegree;
                    menu.SpicyDegree = Menu.SpicyDegree;
                    menu.Status = Menu.Status;
                    menu.SweetDegree = Menu.SweetDegree;
                    menu.Unit = Menu.Unit;
                    menu.DepartmentId = Department.Id;
                    menu.Usable = true;
                    db.Menus.Add(menu);
                    db.SaveChanges();
                    var PriceMenu = new MenuPrice();
                    PriceMenu.Discount = Menu.MenuPrice.Discount / 100;
                    PriceMenu.ExcludePayDiscount = Menu.MenuPrice.ExcludePayDiscount;
                    PriceMenu.Points = Menu.MenuPrice.Points;
                    PriceMenu.Price = Menu.MenuPrice.Price;
                    PriceMenu.Id = Menu.Id;
                    db.MenuPrice.Add(PriceMenu);
                    db.SaveChanges();
                }
                menu = await db.Menus
                   .Include(m => m.Classes)
                   .Include(m => m.Remarks)
                   .FirstOrDefaultAsync(m => m.Id == menu.Id);
                if (image != null)
                {
                    string BaseUrl = Method.GetBaseUrl((int)HotelId);
                    menu.PicturePath = HotelId.ToString() + "/" + Menu.Id + ".jpg";
                    Method.SaveImg(menu.Id, image, BaseUrl);
                    Method.SaveImg(menu.Id, image, Method.MyGetBaseUrl((int)HotelId));
                }else
                {
                    menu.PicturePath = HotelId.ToString() + "/none.jpg";
                }
                db.SaveChanges();
                if (Classes != null)
                {
                    var NewClass = Classes.Select(c => c.Id).ToList();
                    var classes = await db.MenuClasses.Where(m => NewClass.Contains(m.Id)).ToListAsync();
                    foreach (var i in classes)
                    {
                        menu.Classes.Add(i);
                        db.SaveChanges();
                    }

                }
                if (Remarks != null)
                {
                    var NewRemarks = Remarks.Select(r => r.Id).ToList();
                    var remarks = await db.Remarks.Where(r => NewRemarks.Contains(r.Id)).ToListAsync();
                    foreach (var i in remarks)
                    {
                        menu.Remarks.Add(i);
                        db.SaveChanges();
                    }
                }

            }
            return Json(new { Status = true });
        }
        /// <summary>
        /// 删除菜品
        /// </summary>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteMenu(string MenuId)
        {
            var menu = await db.Menus
                .Include(m=>m.Classes)
                .FirstOrDefaultAsync(m => m.Id == MenuId);
            menu.Usable = false;
            foreach(var i in menu.Classes)
            {
                menu.Classes.Remove(i);
            }
            db.SaveChanges();
            return null;
        }
        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> getRoles()
        {
            var Roles = await db.StaffRoles
                .Include(s => s.Schemas)
                .Select(s => new { s.Id, s.Name, Schemas = s.Schemas })
                .ToListAsync();
            return Json(new { Roles = Roles });
        }
        /// <summary>
        /// 编辑角色信息
        /// </summary>
        /// <param name="RoleId"></param>
        /// <param name="Schemas"></param>
        /// <returns></returns>
        public async Task<JsonResult> EditRoles(int RoleId, List<int> Schemas)
        {
            var role = await db.StaffRoles.FirstOrDefaultAsync(d => d.Id == RoleId);
            if (role == null) return Json(new { Status = false, ErrorMessage = "没有相关角色" });
            var roleschema = await db.StaffRoleSchemas.Where(s => s.StaffRoleId == role.Id).ToListAsync();
            foreach (var i in roleschema)
            {
                db.StaffRoleSchemas.Remove(i);
                db.SaveChanges();
            }
            if (Schemas!=null)
            {
                foreach (var s in Schemas)
                {
                    StaffRoleSchema newRole = new StaffRoleSchema();
                    newRole.StaffRoleId = RoleId;
                    newRole.Schema = (Schema)s;
                    db.StaffRoleSchemas.Add(newRole);
                    db.SaveChanges();
                }
            }
            return Json(new { Status = true });

        }
        /// <summary>
        /// 删除角色信息
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteRoles(int roleId)
        {
            var role = await db.StaffRoles.FirstOrDefaultAsync(d => d.Id == roleId);
            db.StaffRoles.Remove(role);
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 添加角色信息
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Schemas"></param>
        /// <returns></returns>
        public async Task<int> AddRole(string Name, List<int> Schemas)
        {
            StaffRole sr = new StaffRole();
            sr.Name = Name;
            db.StaffRoles.Add(sr);
            await db.SaveChangesAsync();
            if (Schemas!=null)
            {
                foreach (var i in Schemas)
                {
                    db.StaffRoleSchemas.Add(new StaffRoleSchema { StaffRoleId = sr.Id, Schema = (Schema)i });
                    await db.SaveChangesAsync();
                }
            }
            return sr.Id;
        }
        /// <summary>
        /// 获取职员信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetStaffs()
        {
            int HotelId = (int)(Session["User"] as RStatus).HotelId;
            var staffs = await sysdb.Staffs.Where(d => d.HotelId == HotelId).ToListAsync();
            List<ConbineStaff> StaffAllInfo = new List<ConbineStaff>();
            foreach (var staff in staffs)
            {
                var hotelStaffInfo = await db.Staffs
                    .Include(d => d.StaffRoles)
                    .FirstOrDefaultAsync(s => s.Id == staff.Id);
                StaffAllInfo.Add(new ConbineStaff
                {
                    Staff = hotelStaffInfo,
                    SysStaff = staff
                });
            }
            var Roles = await db.StaffRoles.ToListAsync();
            return Json(new { Staffs = StaffAllInfo, Roles = Roles });
        }
        /// <summary>
        /// 编辑职员信息
        /// </summary>
        /// <param name="Sf"></param>
        /// <param name="Sfh"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<JsonResult> EditStaffs(EditStaff Sf, EditStaffHotel Sfh, List<int> roles)
        {
            var sysStaff = await sysdb.Staffs.FirstOrDefaultAsync(s => s.Id == Sf.Id);
            sysStaff.SigninName = Sf.SigninName;
            if (Sf.PassWord != null)
            {
                sysStaff.PasswordHash = Method.GetMd5(Sf.PassWord);
            }
            sysStaff.Email = Sf.Email;
            sysStaff.PhoneNumber = Sf.PhoneNumber;
            var staff = await db.Staffs
                .Include(s => s.StaffRoles)
                .FirstOrDefaultAsync(s => s.Id == Sf.Id);
            staff.Name = Sfh.Name;
            staff.WorkTimeFrom = Sfh.WorkTimeFrom;
            staff.WorkTimeTo = Sfh.WorkTimeTo;
            db.SaveChanges();
            if (sysStaff.IsHotelAdmin) { return Json(new ErrorState("不可删除，饭店总管理身份,其他信息已经更改完毕")); }
            var cleanRoles = await db.Staffs.Where(s => s.Id == Sf.Id).FirstOrDefaultAsync();
            var Cr = cleanRoles.StaffRoles.Select(s => s.Id);
            if (Cr.Count() > 0)
            {
                var clean = db.StaffRoles.Where(s => Cr.Contains(s.Id));
                foreach (var role in clean)
                {
                    staff.StaffRoles.Remove(role);
                }
                db.SaveChanges();
            }
            if (roles != null)
            {
                var AddRoles = await db.StaffRoles.Where(s => roles.Contains(s.Id)).ToListAsync();
                foreach (var role in AddRoles)
                {
                    staff.StaffRoles.Add(role);
                    db.SaveChanges();
                }
            }
            return Json(new { Status = true });
        }
        /// <summary>
        /// 删除职员信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteStaff(string Id)
        {
            var staff = await sysdb.Staffs.Where(s => s.Id == Id).FirstOrDefaultAsync();
            if (staff == null) return Json(new { Status = false, ErrorMessage = "没有相关人物" });
            var ReturnMenu = db.DineMenus.Where(d => d.Status == DineMenuStatus.Returned && d.ReturnedWaiterId == Id);
            foreach (var i in ReturnMenu)
            {
                i.ReturnedWaiterId = null;
                db.SaveChanges();
            }

            sysdb.Staffs.Remove(staff);
            sysdb.SaveChanges();
            var Hstaff = await db.Staffs.Where(s => s.Id == Id).FirstOrDefaultAsync();
            db.Staffs.Remove(Hstaff);
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 添加职员信息
        /// </summary>
        /// <param name="Sf"></param>
        /// <param name="Sfh"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddStaff(EditStaff Sf, EditStaffHotel Sfh, List<int> roles)
        {
            var scount = await sysdb.Staffs.Where(s => s.SigninName == Sf.SigninName).FirstOrDefaultAsync();
            if (scount != null) return Json(new { Status = false, ErrorMessage = "账号已存在" });
            var staff = new HotelDAO.Models.Staff();
            var Sstaff = new YummyOnlineDAO.Models.Staff();

            Sstaff.CreateDate = new DateTime();
            Sstaff.Email = Sf.Email;
            Sstaff.HotelId = (int)(Session["User"] as RStatus).HotelId;
            Sstaff.PhoneNumber = Sf.PhoneNumber;
            Sstaff.SigninName = Sf.SigninName;
            Sstaff.PasswordHash = Method.GetMd5(Sf.PassWord);
            sysdb.Staffs.Add(Sstaff);
            sysdb.SaveChanges();
            staff.Id = Sstaff.Id;
            staff.DineCount = 0;
            staff.DinePrice = 0;
            staff.Name = Sfh.Name;
            staff.WorkTimeFrom = Sfh.WorkTimeFrom;
            staff.WorkTimeTo = Sfh.WorkTimeTo;
            db.Staffs.Add(staff);
            db.SaveChanges();
            staff = await db.Staffs.Where(s => s.Id == Sstaff.Id)
                .Include(s => s.StaffRoles).FirstOrDefaultAsync();
            if (roles != null)
            {
                var Roles = await db.StaffRoles.Where(s => roles.Contains(s.Id)).ToListAsync();
                foreach (var i in Roles)
                {
                    staff.StaffRoles.Add(i);
                    db.SaveChanges();
                }
            }
            return Json(new { Status = true ,Id = staff.Id});

        }
        /// <summary>
        /// 获取备注信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> getRemarks()
        {
            var Remarks = await db.Remarks.ToListAsync();
            return Json(new { Remarks = Remarks });
        }
        /// <summary>
        /// 修改备注信息
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Name"></param>
        /// <param name="Price"></param>
        /// <returns></returns>
        public async Task<JsonResult> editRemarks(int Id, string Name, decimal? Price)
        {
            var remark = await db.Remarks.Where(r => r.Id == Id).FirstOrDefaultAsync();
            if (remark == null) return Json(new { Status = false, ErrorMessage = "未找到该备注" });
            remark.Name = Name;
            remark.Price = (decimal)Price;
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 删除备注信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteRemark(int Id)
        {
            var remark = await db.Remarks.Where(r => r.Id == Id).FirstOrDefaultAsync();
            if (remark == null) return Json(new { Status = false, ErrorMessage = "未找到该备注" });
            db.Remarks.Remove(remark);
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 增加备注信息
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Price"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddRemark(string Name, decimal? Price)
        {
            var remark = new Remark();
            remark.Name = Name;
            remark.Price = (decimal)Price;
            db.Remarks.Add(remark);
            await db.SaveChangesAsync();
            return Json(new { Id = remark.Id});
        }
        /// <summary>
        /// 批量导入
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> FileTrs()
        {
            if (Request.Files.Count != 0)
            {
                HttpPostedFileBase Execl = Request.Files["Execl"];
                HttpPostedFileBase Zip = Request.Files["Zip"];
                string baseUrl = Method.MyGetBaseUrl((int)(Session["User"] as RStatus).HotelId);
                string OrderUrl = Method.GetBaseUrl((int)(Session["User"] as RStatus).HotelId);
                if (!Directory.Exists(baseUrl))
                {
                    Directory.CreateDirectory(baseUrl);
                }
                if (Zip != null && Zip.ContentLength > 0)
                {
                    // Unzip file here with for example SharpZipLib
                    Zip.SaveAs(baseUrl + "file.zip");
                    (new FastZip()).ExtractZip(baseUrl + "file.zip", baseUrl, "");
                    (new FastZip()).ExtractZip(baseUrl + "file.zip", OrderUrl, "");
                    var paths = await db.Menus.Where(m => m.Usable == true).ToListAsync();
                    foreach (var path in paths)
                    {
                        path.PicturePath = (Session["User"] as RStatus).HotelId.ToString() + "/" + path.Id + ".jpg";
                        db.SaveChanges();
                    }
                }
                if (Execl != null && Execl.ContentLength > 0)
                {
                    Execl.SaveAs(baseUrl + "excel.xls");
                    string sConnecStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + baseUrl + "excel.xls" + ";" + "Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";
                    OleDbConnection conObj = new OleDbConnection(sConnecStr);
                    conObj.Open();
                    OleDbCommand sqlCommand = new OleDbCommand("SELECT * FROM [Sheet1$]", conObj);
                    OleDbDataAdapter adaObj = new OleDbDataAdapter();
                    adaObj.SelectCommand = sqlCommand;
                    DataSet setObj = new DataSet();
                    adaObj.Fill(setObj);
                    conObj.Close();
                    DataTable dt = setObj.Tables[0];
                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            int hotelId = (int)(Session["User"] as RStatus).HotelId;
                            string Id = row[0].ToString();
                            Menu me = new Menu();
                            MenuPrice Mp = new MenuPrice();
                            var clean = await db.Menus
                                .Include(m => m.MenuPrice)
                                .Where(m => m.Id == Id).FirstOrDefaultAsync();
                            if (clean == null)
                            {
                                //没有信息
                                me.Id = row[0].ToString();
                                me.Status = MenuStatus.Normal;
                                me.Code = row[1].ToString();
                                me.Name = row[2].ToString();
                                me.NameAbbr = row[3].ToString();
                                me.PicturePath = hotelId.ToString() + "/" + row[0].ToString() + ".jpg";
                                me.IsFixed = false;
                                me.SupplyDate = 127;
                                me.Unit = row[4].ToString();
                                me.MinOrderCount = Convert.ToInt32(row[5].ToString());
                                me.Ordered = 0;
                                me.SourDegree = Convert.ToInt32(row[6].ToString());
                                me.SweetDegree = Convert.ToInt32(row[7].ToString());
                                me.SaltyDegree = Convert.ToInt32(row[8].ToString());
                                me.SpicyDegree = Convert.ToInt32(row[9].ToString());
                                me.Usable = true;
                                me.IsSetMeal = false;
                                me.DepartmentId = Convert.ToInt32(row[10].ToString());
                                db.Menus.Add(me);
                                db.SaveChanges();
                                Mp.Id = Id;
                                Mp.Price = Convert.ToDecimal(row[12].ToString());
                                Mp.Points = Convert.ToInt32(row[14].ToString());
                                Mp.Discount = Convert.ToDouble(row[13].ToString()) / 100;
                                Mp.ExcludePayDiscount = Convert.ToInt32(row[11].ToString()) == 1;
                                db.MenuPrice.Add(Mp);
                                db.SaveChanges();
                            }
                            else
                            {
                                clean.Status = MenuStatus.Normal;
                                clean.Code = row[1].ToString();
                                clean.Name = row[2].ToString();
                                clean.NameAbbr = row[3].ToString();
                                clean.PicturePath = hotelId.ToString() + "/" + row[0].ToString() + ".jpg";
                                clean.IsFixed = false;
                                clean.SupplyDate = 127;
                                clean.Unit = row[4].ToString();
                                clean.MinOrderCount = Convert.ToInt32(row[5].ToString());
                                clean.SourDegree = Convert.ToInt32(row[6].ToString());
                                clean.SweetDegree = Convert.ToInt32(row[7].ToString());
                                clean.SaltyDegree = Convert.ToInt32(row[8].ToString());
                                clean.SpicyDegree = Convert.ToInt32(row[9].ToString());
                                clean.Usable = true;
                                clean.IsSetMeal = false;
                                clean.DepartmentId = Convert.ToInt32(row[10].ToString());
                                Mp = await db.MenuPrice.Where(m => m.Id == Id).FirstOrDefaultAsync();
                                Mp.Price = Convert.ToDecimal(row[12].ToString());
                                Mp.Points = Convert.ToInt32(row[14].ToString());
                                Mp.Discount = Convert.ToDouble(row[13].ToString()) / 100;
                                Mp.ExcludePayDiscount = Convert.ToInt32(row[11].ToString()) == 1;
                                db.SaveChanges();
                            }
                        }
                        catch 
                        {

                        }
                    }
                }
            }
            var Menus = await db.Menus.Where(m => m.Usable == true)
                .Include(m => m.MenuPrice)
                .Include(m => m.Remarks)
                .Include(m => m.Classes)
                .Select(m => new
                {
                    m.Id,
                    m.Code,
                    m.MenuPrice,
                    m.Remarks,
                    m.Classes,
                    m.DepartmentId,
                    m.IsFixed,
                    m.IsSetMeal,
                    m.MinOrderCount,
                    m.NameAbbr,
                    m.Ordered,
                    m.PicturePath,
                    m.SaltyDegree,
                    m.SourDegree,
                    m.SpicyDegree,
                    m.Status,
                    m.Name,
                    m.SupplyDate,
                    m.SweetDegree,
                    m.Unit
                }).ToListAsync();
            return Json(new { Menus = Menus});
        }
        /// <summary>
        /// 获取区域信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> getDepartment()
        {
            var Departments = await db.Departments
                .Where(d => d.Usable == true)
                .Include(d => d.Printer)
                .Select(d => new {
                    d.Id,
                    d.Name,
                    d.Printer,
                    d.Description,
                    d.PrinterId
                })
                .ToListAsync();
            var Prints = await db.Printers
                .Where(p => p.Usable == true)
                .ToListAsync();
            return Json(new { Departments = Departments, Prints = Prints });
        }
        /// <summary>
        /// 编辑区域信息
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="PrintId"></param>
        /// <returns></returns>
        public async Task<JsonResult> EditDepartment(int Id, string Name, string Description, int? PrintId)
        {
            var Deparment = await db.Departments.FirstOrDefaultAsync(d => d.Id == Id);
            if (Deparment == null) return Json(new { Status = false, ErrorMessage = "对不起没有相关部门" });
            Deparment.Name = Name;
            Deparment.PrinterId = PrintId;
            Deparment.Description = Description;
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 删除区域信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteDepartment(int Id)
        {
            var department = await db.Departments.FirstOrDefaultAsync(d => d.Id == Id);
            if (department == null) return Json(new { Status = false, ErrorMessage = "对不起没有相关部门" });
            department.Usable = false;
            department.PrinterId = null;
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 增加部门信息
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="PrintId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddDepartment(string Name, string Description, int? PrintId)
        {
            var department = new Department();
            department.Name = Name;
            department.Description = Description;
            department.PrinterId = PrintId;
            department.Usable = true;
            db.Departments.Add(department);
            await db.SaveChangesAsync();
            return Json(new { Status = true ,Id = department.Id});
        }
        /// <summary>
        /// 获取类别信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetMenuclasses()
        {
            var FirstClasses = await db.MenuClasses.Where(m => m.Usable == true && m.Level == 0).ToListAsync();
            var SecondClasses = await db.MenuClasses.Where(m => m.Usable == true && m.Level == 1).ToListAsync();
            var ThirdClasses = await db.MenuClasses.Where(m => m.Usable == true && m.Level == 2).ToListAsync();
            return Json(new { FirstMenuClasses = FirstClasses, SecondMenuClasses = SecondClasses, ThirdMenuClasses = ThirdClasses });
        }
        /// <summary>
        /// 修改类别信息
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public async Task<JsonResult> EditMenuClass(string Id, string Name)
        {
            var menuClass = await db.MenuClasses.Where(m => m.Id == Id).FirstOrDefaultAsync();
            if (menuClass == null) return null;
            menuClass.Name = Name;
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 删除类别信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteMenuClass(string Id)
        {
            var menuclass = await db.MenuClasses.Where(m => m.Id == Id).FirstOrDefaultAsync();
            if (menuclass == null) return Json(new { Status = false ,ErrorMessage = "未找到分类"});
            else
            {
                var menu = await db.Menus.Where(m => m.Classes.Select(mm => mm.Id).FirstOrDefault() == Id).FirstOrDefaultAsync();
                if (menu != null)
                {
                    return Json(new { Status = false, ErrorMessage = "当前分类内还有菜品，请删除本类的所有菜品，再删除" });
                }
                else
                {
                    string ParentId = menuclass.ParentMenuClassId;
                    db.MenuClasses.Remove(menuclass);
                    db.SaveChanges();
                    var brother = await db.MenuClasses.FirstOrDefaultAsync(m => m.ParentMenuClassId == ParentId);
                    if(brother == null)
                    {
                        var father = await db.MenuClasses.FirstOrDefaultAsync(m => m.Id == ParentId);
                        father.IsLeaf = true;
                    }
                    db.SaveChanges();
                }
            }
            return Json(new { Status = true });
        }
        /// <summary>
        /// 增加类别信息
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddMenuClass(string Id, string Name, string Description,string ParentId)
        {
            var menuclass = await db.MenuClasses.Where(m => m.Id == Id).FirstOrDefaultAsync();
            if(menuclass!=null) { return Json(new { Status = false, ErrorMessage = "编号已存在" }); }
            else
            {
                var parent = await db.MenuClasses.FirstOrDefaultAsync(m => m.Id == ParentId);
                var newClass = new MenuClass();
                newClass.Id = Id;
                newClass.Usable = true;
                newClass.IsShow = true;
                newClass.IsLeaf = true;
                newClass.Name = Name;
                newClass.Description = Description;
                newClass.ParentMenuClassId = ParentId;
                if (parent == null)
                {
                    newClass.Level = 0;
                }
                else
                {
                    newClass.Level = parent.Level + 1;
                    parent.IsLeaf = false;
                }
                db.MenuClasses.Add(newClass);
                db.SaveChanges();
            }
            return Json(new { Status = true});
        }
        /// <summary>
        /// 获取套餐信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> getSetMeal()
        {
            var MenuSetIds = await db.Menus.Where(m => m.Usable == true && m.IsSetMeal == true)
                .Select(m => m.Id)
                .ToListAsync();
            var Meals = new List<Meal>();
            foreach(var i in MenuSetIds)
            {
                var newMeal = new Meal();
                var meal = await db.MenuSetMeals
                    .Include(m=>m.Menu.MenuPrice)
                    .Include(m=>m.MenuSet.MenuPrice)
                    .Where(m => m.MenuSetId == i)
                    .ToListAsync();
                var details = new List<MealList>();
                if (meal.Count() > 0)
                {
                    foreach (var j in meal)
                    {
                        var newList = new MealList();
                        newList.Count = j.Count;
                        newList.Menu = j.Menu;
                        details.Add(newList);
                    }
                }
                newMeal.MealMenu = await db.Menus
                    .Include(m=>m.MenuPrice)
                    .FirstOrDefaultAsync(m=>m.Id==i);
                newMeal.Menus = details;
                Meals.Add(newMeal);
            }
            var Menus = await db.Menus.Where(m => m.Usable == true&&m.IsSetMeal==false)
                .Include(m => m.MenuPrice)
                .ToListAsync();
            return Json(new { Meals= Meals, Menus= Menus });
        }
        /// <summary>
        /// 删除套餐信息
        /// </summary>
        /// <param name="MealId"></param>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        public async Task<JsonResult> deleteMealMenu(string MealId,string MenuId)
        {
            var meal = await db.MenuSetMeals.Where(m => m.MenuSetId == MealId && m.MenuId == MenuId)
                            .FirstOrDefaultAsync();
            if(meal==null)  return Json(new { Status = true });
            db.MenuSetMeals.Remove(meal);
            db.SaveChanges();
            return Json(new { Status = true });
        }
        /// <summary>
        /// 修改套餐菜品
        /// </summary>
        /// <param name="MealId"></param>
        /// <param name="Menus"></param>
        /// <param name="Name"></param>
        /// <param name="Price"></param>
        /// <returns></returns>
        public async Task<JsonResult> EditMenusInMeal(string MealId,List<MenuInMeal> Menus,string Name,decimal Price)
        {
            var curmeal = await db.Menus.Where(m => m.Id == MealId)
                .Include(m => m.MenuPrice)
                .FirstOrDefaultAsync();
            if(curmeal==null) return Json(new { Status = false, ErrorMessage = "没有套餐" });
            curmeal.Name = Name;
            curmeal.MenuPrice.Price = Price;
            db.SaveChanges();
            if (Menus==null) return Json(new { Status = false ,ErrorMessage="请直接删除套餐"});
            var listmenus = await db.MenuSetMeals.Where(m => m.MenuSetId == MealId)
                                .ToListAsync();
            if (listmenus != null)
            {
                foreach (var i in listmenus)
                {
                    db.MenuSetMeals.Remove(i);
                    db.SaveChanges();
                }
            }
            foreach(var i in Menus)
            {
                var meal = new MenuSetMeal();
                meal.MenuSetId = MealId;
                meal.Count = i.Count;
                meal.MenuId = i.MenuId;
                db.MenuSetMeals.Add(meal);
                db.SaveChanges();
            }
            return Json(new { Status = true });
        }
        /// <summary>
        /// 删除套餐
        /// </summary>
        /// <param name="MealId"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteMeal(string MealId)
        {
            var meal = await db.Menus.Where(m => m.Id == MealId).FirstOrDefaultAsync();
            meal.Usable = false;
            db.SaveChanges();
            return Json(new { Status = true });
        }

        /// <summary>
        /// 获取打印信息
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> getPrint()
        {
            var rate = await db.HotelConfigs.Select(d => d.PointsRatio).FirstOrDefaultAsync();
            var printers = await db.Printers.Where(d => d.Usable == true).ToListAsync();
            var format = await db.PrinterFormats.FirstOrDefaultAsync();
            var AccountPrint = await db.HotelConfigs.Select(h => h.ShiftPrinterId).FirstOrDefaultAsync();
            var font = await db.PrinterFormats.FirstOrDefaultAsync();
            var IsUsePrinter = await db.HotelConfigs.Select(h => h.HasAutoPrinter).FirstOrDefaultAsync();
            var IsPayFirst = await db.HotelConfigs.Select(h => h.IsPayFirst).FirstOrDefaultAsync();
            int HotelId = (int)(Session["User"] as RStatus).HotelId;
            var Style = await sysdb.Hotels.Where(d => d.Id == HotelId).Select(d => d.CssThemePath).FirstOrDefaultAsync();
            return Json(new { Rate = rate, Printers = printers, Format = format , AccountPrint = AccountPrint , font = font , IsUsePrinter = IsUsePrinter , IsPayFirst = IsPayFirst , Style = Style });
        }
        /// <summary>
        /// 修改打印模式
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="Font"></param>
        /// <param name="Rate"></param>
        /// <param name="IsUsePrint"></param>
        /// <param name="ShiftPrintId"></param>
        /// <param name="IsPayFirst"></param>
        /// <param name="Style"></param>
        /// <returns></returns>
        public async Task<JsonResult> ChangePrintFormat(Format Format,string Font,int Rate,bool IsUsePrint,int ShiftPrintId,bool IsPayFirst,int Style)
        {
            int HotelId = (int)(Session["User"] as RStatus).HotelId;
            var config = await db.HotelConfigs.FirstOrDefaultAsync();
            config.PointsRatio = Rate;
            config.ShiftPrinterId = ShiftPrintId;
            config.HasAutoPrinter = IsUsePrint;
            config.IsPayFirst = IsPayFirst;
            var font = await db.PrinterFormats.FirstOrDefaultAsync();
            font.KitchenOrderFontSize = Format.KitchenOrderFontSize;
            font.KitchenOrderSmallFontSize = Format.KitchenOrderSmallFontSize;
            font.PaperSize = Format.PaperSize;
            font.ReciptBigFontSize = Format.ReciptBigFontSize;
            font.ReciptFontSize = Format.ReciptFontSize;
            font.ReciptSmallFontSize = Format.ReciptSmallFontSize;
            font.ServeOrderFontSize = Format.ServeOrderFontSize;
            font.ServeOrderSmallFontSize = Format.ServeOrderSmallFontSize;
            font.ShiftBigFontSize = Format.ShiftBigFontSize;
            font.ShiftFontSize = Format.ShiftFontSize;
            font.ShiftSmallFontSize = Format.ShiftSmallFontSize;
            font.ColorDepth = Format.ColorDepth;
            db.SaveChanges();
            var hotel = sysdb.Hotels.Where(h => h.Id == HotelId).FirstOrDefault();
            if (Style == 0)
            {
                hotel.CssThemePath = "default.css";
                hotel.OrderSystemStyle = OrderSystemStyle.Simple;
            }else if(Style == 1)
            {
                hotel.OrderSystemStyle = OrderSystemStyle.Fashion;
                hotel.CssThemePath = "cafe.css";
            }
            sysdb.SaveChanges();
            return null;
        }

        [HttpPost]
        public  JsonResult FileUpLoader()
        {
            if (Request.Files.Count != 0)
            {
                HttpPostedFileBase logo = Request.Files["logo"];
                HttpPostedFileBase button = Request.Files["button"];
                HttpPostedFileBase complete = Request.Files["complete"];
                string baseUrl = Method.MyGetBaseUrl((int)(Session["User"] as RStatus).HotelId);
                string OrderUrl = Method.GetBaseUrl((int)(Session["User"] as RStatus).HotelId);
                if (!Directory.Exists(baseUrl))
                {
                    Directory.CreateDirectory(baseUrl);
                }
                if (!Directory.Exists(OrderUrl))
                {
                    Directory.CreateDirectory(OrderUrl);
                }
                if (logo != null)
                {
                    logo.SaveAs(baseUrl + "index.png");
                    logo.SaveAs(OrderUrl + "index.png");
                }
                if (button != null)
                {
                    button.SaveAs(baseUrl + "btn-cart.png");
                    button.SaveAs(OrderUrl + "btn-cart.png");
                }
                if (complete != null)
                {
                    complete.SaveAs(baseUrl + "completed.gif");
                    complete.SaveAs(OrderUrl + "completed.gif");
                }
                return Json(new SuccessState());
            }
            return Json(new ErrorState("请选择文件"));
        }
    }
}