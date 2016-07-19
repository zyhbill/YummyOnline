using HotelDAO.Models;
using Management.ObjectClasses;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Management.Controllers
{
    public class ReportsController : BaseController
    {

        // GET: Reports
        public ActionResult MenuSales()
        {
            return View("MenuSales");
        }

        public ActionResult PayTotal()
        {
            return View("PaykindType");
        }

        public ActionResult SaleClass()
        {
            return View("SaleClass");
        }

        public ActionResult monthSales()
        {
            return View("monthSales");
        }


        public ActionResult Years()
        {
            return View("YearSales");
        }

        public ActionResult SaleRange()
        {
            return View("SaleRange");
        }


        public ActionResult SalesAll()
        {
            return View("SalesAll");
        }

        public ActionResult MenuSaleclass()
        {
            return View("MenuSaleclass");
        }

        public ActionResult Invoice()
        {
            return View("Invoice");
        }
        /// <summary>
        /// 获取菜品销售信息
        /// </summary>
        /// <param name="Begin"></param>
        /// <param name="End"></param>
        /// <param name="Menus"></param>
        /// <param name="Classes"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public async Task<JsonResult> getMenuSales(string Begin, string End ,List<string> Menus ,List<string> Classes,int Type)
        {
            DateTime BeginTime;
            DateTime EndTime;
            if (Begin == null)
            {
                BeginTime = DateTime.Now;
            }
            else
            {
                BeginTime = Convert.ToDateTime(Begin);
            }
            if (End == null)
            {
                EndTime = DateTime.Now;
            }
            else
            {
                EndTime = Convert.ToDateTime(End);
            }
            
            var dines = await db.Dines
                  .Where(d => d.IsPaid == true && SqlFunctions.DateDiff("day", BeginTime,d.BeginTime)>=0&& SqlFunctions.DateDiff("day", EndTime, d.BeginTime) <= 0)
                  .Select(d => d.Id)
                  .ToListAsync();
            var OriMenus = await db.Menus
                .Include(m => m.MenuPrice)
                .Where(m => m.Usable == true)
                .ToListAsync();
            var OriDineMenus = await db.DineMenus
                .Where(d => dines.Contains(d.DineId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift))
                .ToListAsync();
            var OriMenuClasses = await db.MenuClasses
                .Where(d => d.Usable == true)
                .ToListAsync();
            if (Type == 0)
            {
                var menus = OriDineMenus
                  .Where(d => Menus.Contains(d.MenuId))
                  .GroupBy(d => d.MenuId)
                  .Select(d => new
                  {
                      MenuId = d.Key,
                      TotalCount = d.Sum(dd => dd.Count),
                      TotalOriPrice = d.Sum(dd => dd.OriPrice * dd.Count),
                      TotalPrice = d.Sum(dd => dd.Price * dd.Count),
                      TotalSave = d.Sum(dd => dd.OriPrice * dd.Count) - d.Sum(dd => dd.Price * dd.Count)
                  })
                  .OrderByDescending(g => g.TotalPrice)
                  .ToList();
                var Datas = new List<SalePercent>();
                foreach (var i in menus)
                {
                    if(menus.Sum(m => m.TotalSave) != 0)
                    {
                        Datas.Add(new SalePercent
                        {
                            Menu = OriMenus.Where(m => m.Id == i.MenuId).FirstOrDefault(),
                            Count = i.TotalCount,
                            TotalOriPrice = i.TotalOriPrice,
                            TotalPrice = i.TotalPrice,
                            TotalSaveMoney = i.TotalSave,
                            CountPercent = (i.TotalCount * 1.0 / menus.Sum(m => m.TotalCount)) * 100,
                            OriPricePercent = (double)(i.TotalOriPrice / menus.Sum(m => m.TotalOriPrice)) * 100,
                            PricePercent = (double)(i.TotalPrice / menus.Sum(m => m.TotalPrice)) * 100,
                            SaveMoneyPercent = (double)(i.TotalSave / menus.Sum(m => m.TotalSave)) * 100
                        });
                    }
                    else
                    {
                        Datas.Add(new SalePercent
                        {
                            Menu = OriMenus.Where(m => m.Id == i.MenuId).FirstOrDefault(),
                            Count = i.TotalCount,
                            TotalOriPrice = i.TotalOriPrice,
                            TotalPrice = i.TotalPrice,
                            TotalSaveMoney = i.TotalSave,
                            CountPercent = (i.TotalCount * 1.0 / menus.Sum(m => m.TotalCount)) * 100,
                            OriPricePercent = (double)(i.TotalOriPrice / menus.Sum(m => m.TotalOriPrice)) * 100,
                            PricePercent = (double)(i.TotalPrice / menus.Sum(m => m.TotalPrice)) * 100,
                            SaveMoneyPercent = 0
                        });
                    }
                   
                }
                var Sum = new PercentSum
                {
                    Count = Datas.Sum(d => d.Count),
                    TotalOriPrice = Datas.Sum(d => d.TotalOriPrice),
                    TotalPrice = Datas.Sum(d => d.TotalPrice),
                    TotalSaveMoney = Datas.Sum(d => d.TotalSaveMoney),
                    CountPercent = Datas.Sum(d => d.CountPercent),
                    OriPricePercent = Datas.Sum(d => d.OriPricePercent),
                    PricePercent = Datas.Sum(d => d.PricePercent),
                    SaveMoneyPercent = Datas.Sum(d => d.SaveMoneyPercent),
                };
                return Json(new { SalesData = Datas, Sum = Sum });
            }
            else
            {
                var Datas = new List<MenuSaleClassData>();
                if (Classes == null) { return null; }
                foreach (var i in Classes)
                {
                    var menuIds = new List<string>();
                    if (Type == 1) {
                         menuIds = Management.Models.Method.GetMenuIdByFather(i, db);
                    }
                    else
                    {
                        menuIds = Management.Models.Method.GetMenuIdByChild(i, db);
                    }
                    var menus = OriDineMenus
                     .Where(d => menuIds.Contains(d.MenuId))
                     .Select(d => new
                     {
                         Count = d.Count,
                         OriPrice = d.OriPrice * d.Count + d.RemarkPrice,
                         Price = d.Price * d.Count + d.RemarkPrice,
                         SaveMoney = d.OriPrice * d.Count - d.Price * d.Count
                     })
                     .ToList();
                    Datas.Add(new MenuSaleClassData
                    {
                        MenuClass = OriMenuClasses.Where(d => d.Id == i).FirstOrDefault(),
                        Count = menus.Sum(s => s.Count),
                        OriPrice = menus.Sum(s => s.OriPrice),
                        SaveMoney = menus.Sum(s => s.SaveMoney),
                        Price = menus.Sum(s => s.Price)
                    });
                }
                foreach (var i in Datas)
                {
                    if (Datas.Sum(d => d.OriPrice) != 0)
                    {
                        i.OriPrecent = (double)(i.OriPrice / Datas.Sum(d => d.OriPrice)) * 100;
                    }
                    if (Datas.Sum(d => d.SaveMoney) != 0)
                    {
                        i.SavePrecent = (double)(i.SaveMoney / Datas.Sum(d => d.SaveMoney)) * 100;
                    }
                    if (Datas.Sum(d => d.Price) != 0)
                    {
                        i.Precent = (double)(i.Price / Datas.Sum(d => d.Price)) * 100;
                    }
                }
                var Sum = new PercentSum
                {
                    Count = Datas.Sum(d => d.Count),
                    TotalOriPrice = Datas.Sum(d => d.OriPrice),
                    TotalPrice = Datas.Sum(d => d.Price),
                    TotalSaveMoney = Datas.Sum(d => d.SaveMoney),
                    OriPricePercent = Datas.Sum(d => d.OriPrecent),
                    PricePercent = Datas.Sum(d => d.Precent),
                    SaveMoneyPercent = Datas.Sum(d => d.SavePrecent)
                };
                return Json(new { SalesData = Datas, Sum = Sum });
            }
        }
        /// <summary>
        /// 支付种类
        /// </summary>
        /// <param name="Begin"></param>
        /// <param name="End"></param>
        /// <returns></returns>
        public async Task<JsonResult> PaykindType(string Begin, string End)
        {
            var dateBegin = Begin.Split(new char[1] { '-' });
            string beginDate = dateBegin[0].Substring(2, 2) + dateBegin[1] + dateBegin[2];
            var dateEnd = End.Split(new char[1] { '-' });
            string endDate = dateEnd[0].Substring(2, 2) + dateEnd[1] + (Convert.ToInt32(dateEnd[2]) + 1).ToString().PadLeft(2, '0'); ;
            var dines = await db.Dines.Where(d => d.IsPaid == true && string.Compare(d.Id, beginDate) >= 0 && string.Compare(d.Id, endDate) <= 0)
                .Select(d => d.Id)
                .ToListAsync();
            decimal priceAll = 0;
            if (dines.Count() != 0)
            {
                priceAll = await db.DinePaidDetails
                               .Where(d => dines.Contains(d.DineId))
                               .SumAsync(d => d.Price);
            }
            var pays = await db.DinePaidDetails
                .Where(d => dines.Contains(d.DineId))
                .GroupBy(d => d.PayKindId)
                .Select(g => new
                {
                    PayKind = db.PayKinds.FirstOrDefault(p => p.Id == g.Key),
                    Total = g.Sum(gg => gg.Price),
                    Point = g.Sum(gg => gg.Price) * 100 / priceAll
                })
                .ToListAsync();
            var details = await db.DinePaidDetails
                .Include(d => d.Dine)
                .Where(d => dines.Contains(d.DineId))
                .GroupBy(d => d.DineId.Substring(0, 6))
                .Select(g => new
                {
                    Time = g.Select(gg => gg.Dine.BeginTime).FirstOrDefault(),
                    Detail = g.GroupBy(gg => gg.PayKindId).Select(ggg => new
                    {
                        PayKind = ggg.Select(gggg => gggg.PayKind).FirstOrDefault(),
                        Total = ggg.Sum(gggg => gggg.Price)
                    })
                })
                .ToListAsync();
            var PayKinds = await db.PayKinds.Where(p => p.Usable == true).ToListAsync();
            var sumPrice = new List<decimal>();
            foreach (var i in PayKinds)
            {
                var temp = pays.Where(p => p.PayKind.Id == i.Id).FirstOrDefault();
                if (temp == null)
                {
                    sumPrice.Add(0);
                }
                else
                {
                    sumPrice.Add(temp.Total);
                }
            }
            var DailyDetails = new List<DailyDetail>();
            foreach (var i in details)
            {
                var newDay = new DailyDetail();
                newDay.Time = i.Time;
                var newDetails = new List<Detail>();
                foreach (var j in PayKinds)
                {
                    var newPayKind = new Detail();
                    newPayKind.PayKind = j;
                    var temp = i.Detail.Where(d => d.PayKind.Id == newPayKind.PayKind.Id)
                        .FirstOrDefault();
                    if (temp == null)
                    {
                        newPayKind.Total = 0;
                    }
                    else
                    {
                        newPayKind.Total = temp.Total;
                    }
                    newDetails.Add(newPayKind);
                }
                newDay.Detail = newDetails;
                DailyDetails.Add(newDay);
            }
            return Json(new { KindTypeData = pays, Details = DailyDetails, sumPrice = sumPrice });
        }
        
        /// <summary>
        /// 菜品年报表
        /// </summary>
        /// <param name="year"></param>
        /// <param name="Fathers"></param>
        /// <param name="Childs"></param>
        /// <param name="Menus"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public async Task<JsonResult> getMenuYears(string year, List<string> Fathers, List<string> Childs, List<string> Menus, int Type)
        {
            if (year == null||year=="")
            {
                DateTime date = DateTime.Now;
                year = date.Year.ToString().Substring(2);
            }
            else
            {
                year = year.Substring(2);
            }
            string[] Month = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
            List<MenuYearDates> Sales = new List<MenuYearDates>();
            string SaleYearsEnd = (Convert.ToInt32(year) + 1).ToString();
            List<List<monthSale>> monthData = new List<List<monthSale>>();
            var Ydines = await db.Dines.Where(d => d.IsPaid == true && string.Compare(d.Id, year) >= 0 && string.Compare(d.Id, SaleYearsEnd) <= 0)
                  .Select(d => d.Id)
                  .ToListAsync();//年内订单
            List<string> Ymenus = new List<string>();
            if (Type == 0)
            {
                if (Menus != null)
                {
                    Ymenus = await db.DineMenus.Where(d => Ydines.Contains(d.DineId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift) && Menus.Contains(d.MenuId))
                   .GroupBy(d => d.MenuId)
                   .Select(g => g.Key)
                   .ToListAsync();//年内菜品

                    for (var i = 0; i < 12; i++)
                    {
                        string curDate = year + Month[i];
                        string nextDate;
                        if (i != 11)
                        {
                            nextDate = year + Month[i + 1];
                        }
                        else
                        {
                            nextDate = Convert.ToInt32(year + 1).ToString() + "01";
                        }
                        var dines = await db.Dines.Where(d => d.IsPaid == true && string.Compare(d.Id, curDate) >= 0 && string.Compare(d.Id, nextDate) <= 0)
                          .Select(d => d.Id)
                          .ToListAsync();
                        var menus = await db.DineMenus.Where(d => dines.Contains(d.DineId) && d.Status == DineMenuStatus.Normal && Ymenus.Contains(d.MenuId))
                            .GroupBy(d => d.MenuId)
                            .Select(d => new monthSale
                            {
                                Id = d.Key,
                                name = db.Menus.Where(m => m.Id == d.Key).Select(m => m.Name).FirstOrDefault(),
                                TotalPrice = d.Sum(dd => dd.Count)
                            })
                            .ToListAsync();
                        monthData.Add(menus);
                    }
                    var Datas = new List<SalesData>();
                    foreach (var i in Ymenus)
                    {
                        var Sdt = new SalesData();
                        Sdt.name = await db.Menus.Where(m => m.Id == i).Select(m => m.Name).FirstOrDefaultAsync();
                        List<int> Prices = new List<int>();
                        foreach (var j in monthData)
                        {
                            bool flag = false;
                            if (j.Count() == 0) { Prices.Add(0); flag = true; }
                            foreach (var k in j)
                            {
                                if (k.Id == i) { Prices.Add(k.TotalPrice); flag = true; }
                            }
                            if(!flag) Prices.Add(0);
                        }
                        Sdt.data = Prices;
                        Datas.Add(Sdt);
                    }
                    List<decimal> Sum = new List<decimal>();
                    for (var i = 0; i < 12; i++)
                    {
                        decimal priceAll = 0;
                        foreach (var j in Datas)
                        {
                            priceAll += j.data[i];
                        }
                        Sum.Add(priceAll);
                    }
                    foreach(var i in Datas)
                    {
                        i.Count = i.data.Sum(s => s);
                    }
                    var CountAll =  Sum.Sum(s => s);
                return Json(new { Status = true, YearsData = Datas, Sum = Sum, CountAll = CountAll});
                }
                else
                {
                    return Json(new SuccessState());
                }
            }
            else if (Type == 1)
            {
                var tempYear = new MenuYearDates();
                tempYear.Datas = new List<SalesData>();
                //father
                foreach (var p in Fathers)
                {
                    var sales = new SalesData();
                    var MenusId = Management.Models.Method.GetMenuIdByFather(p, db);//根据大类找到菜品
                    Ymenus = await db.DineMenus.Where(d => Ydines.Contains(d.DineId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift) && MenusId.Contains(d.MenuId))
                            .GroupBy(d => d.MenuId)
                            .Select(g => g.Key)
                            .ToListAsync();//年内某类菜品
                    var PriceArr = new List<int>();
                    sales.name = db.MenuClasses.Where(m => m.Usable == true && m.Id == p).FirstOrDefault().Name;

                    for (var i = 0; i < 12; i++)
                    {
                        string curDate = year + Month[i];
                        string nextDate;
                        if (i != 11)
                        {
                            nextDate = year + Month[i + 1];
                        }
                        else
                        {
                            nextDate = Convert.ToInt32(year + 1).ToString() + "01";
                        }
                        var dines = await db.Dines.Where(d => d.IsPaid == true && string.Compare(d.Id, curDate) >= 0 && string.Compare(d.Id, nextDate) <= 0)
                          .Select(d => d.Id)
                          .ToListAsync();
                        var temp = await db.DineMenus.Where(d => dines.Contains(d.DineId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift) && Ymenus.Contains(d.MenuId)).ToListAsync();
                        int Num = 0;
                        if (temp.Count() != 0)
                        {
                            Num =  temp.Sum(d => d.Count);
                        }
                        PriceArr.Add(Num);
                    }
                    sales.data = PriceArr;
                    tempYear.Datas.Add(sales);
                }
                List<int> Sum = new List<int>();
                for (var i = 0; i < 12; i++)
                {
                    int priceAll = 0;
                    for (var j = 0; j<tempYear.Datas.Count();j++)
                    {
                        priceAll += tempYear.Datas[j].data[i];
                    }
                    Sum.Add(priceAll);
                }
                foreach(var i in tempYear.Datas)
                {
                    i.Count = i.data.Sum(s => s);
                }
                int CountAll = Sum.Sum(s => s);
                return Json(new { Status = true, YearsData = tempYear.Datas, Sum = Sum , CountAll = CountAll });
            }
            else
            {
                //child
                var tempYear = new MenuYearDates();
                tempYear.Datas = new List<SalesData>();
                //father
                foreach (var p in Childs)
                {
                    var sales = new SalesData();
                    var MenusId = Management.Models.Method.GetMenuIdByChild(p, db);//根据大类找到菜品
                    Ymenus = await db.DineMenus.Where(d => Ydines.Contains(d.DineId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift) && MenusId.Contains(d.MenuId))
                            .GroupBy(d => d.MenuId)
                            .Select(g => g.Key)
                            .ToListAsync();//年内某类菜品
                    var PriceArr = new List<int>();
                    sales.name = db.MenuClasses.Where(m => m.Usable == true && m.Id == p).FirstOrDefault().Name;

                    for (var i = 0; i < 12; i++)
                    {
                        string curDate = year + Month[i];
                        string nextDate;
                        if (i != 11)
                        {
                            nextDate = year + Month[i + 1];
                        }
                        else
                        {
                            nextDate = Convert.ToInt32(year + 1).ToString() + "01";
                        }
                        var dines = await db.Dines.Where(d => d.IsPaid == true && string.Compare(d.Id, curDate) >= 0 && string.Compare(d.Id, nextDate) <= 0)
                          .Select(d => d.Id)
                          .ToListAsync();
                        var temp = await db.DineMenus.Where(d => dines.Contains(d.DineId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift) && Ymenus.Contains(d.MenuId)).ToListAsync();
                        int Num = 0;
                        if (temp.Count() != 0)
                        {
                            Num = temp.Sum(d => d.Count);
                        }
                        PriceArr.Add(Num);
                    }
                    sales.data = PriceArr;
                    tempYear.Datas.Add(sales);
                }
                List<int> Sum = new List<int>();
                for (var i = 0; i < 12; i++)
                {
                    int priceAll = 0;
                    for (var j = 0; j < tempYear.Datas.Count(); j++)
                    {
                        priceAll += tempYear.Datas[j].data[i];
                    }
                    Sum.Add(priceAll);
                }
                foreach (var i in tempYear.Datas)
                {
                    i.Count = i.data.Sum(s => s);
                }
                int CountAll = Sum.Sum(s => s);
                return Json(new { Status = true, YearsData = tempYear.Datas, Sum = Sum , CountAll = CountAll });
            }
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="HotelId"></param>
        ///// <returns></returns>
        //public async Task<JsonResult> RemoteRepoter(int HotelId)
        //{
        //    var Hotel = await sysdb.Hotels.Where(h => h.Id == HotelId).FirstOrDefaultAsync();
        //    if (Hotel == null) { return Json(new { Succeeded = false, ErrorMessage = "对不起饭店号不正确" }); }
        //    var RDb = new HotelContext(Hotel.ConnectionString);
        //    var dines = await RDb.Dines
        //        .Include(d => d.DineMenus.Select(dd => dd.Menu.MenuPrice))
        //        .Include(d => d.Remarks)
        //        .Where(p => SqlFunctions.DateDiff("day", p.BeginTime, DateTime.Now) == 0)
        //        .ToListAsync();
        //    var dineIds = dines.Select(d => d.Id).ToList();
        //    var pays = await RDb.DinePaidDetails
        //        .Where(d => dineIds.Contains(d.DineId))
        //        .ToListAsync();
        //    return Json(new { Succeeded = true, Dines = dines, PayDetails = pays }, JsonRequestBehavior.AllowGet);
        //}


            /// <summary>
            /// 菜品销售表
            /// </summary>
            /// <returns></returns>
        public async Task<JsonResult> GetClasses()
        {
            var FatherClass = await db.MenuClasses
                .Where(m => m.Usable == true && m.Level == 0)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.ParentMenuClassId
                })
            .ToListAsync();
            var ChildClass = await db.MenuClasses
                .Where(m => m.Usable == true && m.Level == 1)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.ParentMenuClassId
                })
                .ToListAsync();
            var Menus = await db.Menus
                .Include(m => m.Classes)
                .Where(m => m.Usable == true && m.Status == MenuStatus.Normal)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Classes
                })
                .ToListAsync();
            var Areas = await db.Areas.Where(a => a.Usable == true).Select(a => new { a.Id, a.Name }).ToListAsync();
            return Json(new { FatherClass = FatherClass, ChildClass = ChildClass, Menus = Menus , Areas = Areas });
        }
        /// <summary>
        /// 菜品月报表
        /// </summary>
        /// <param name="time"></param>
        /// <param name="Fathers"></param>
        /// <param name="Childs"></param>
        /// <param name="Menus"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetMonthDatas(string time, List<string> Fathers, List<string> Childs, List<string> Menus, int Type)
        {
            string year;
            string month;
            if (time == null)
            {
                year = DateTime.Now.Year.ToString().Substring(2);
                month = DateTime.Now.Month.ToString().PadLeft(2, '0');
            }
            else
            {
                var dates = time.Split('-');
                if (dates.Length < 2) { return Json(new { Status = false, ErrorMessage = "日期格式不对" }); }
                else
                {
                    year = dates[0].Substring(2);
                    month = dates[1];
                }
            }
            var date = year + month;
            var MenuIds = new List<string>();
            var dines = new List<string>();
            var menus = await db.Menus.Where(m => m.Usable == true)
                .ToListAsync();
            var classes = await db.MenuClasses.Where(m => m.Usable == true)
                .ToListAsync();
            var DineMenus = await db.DineMenus
                .Include(d => d.Dine)
                .Where(d => d.DineId.Substring(0, 4) == date && d.Dine.IsPaid == true && d.Status == DineMenuStatus.Normal)
                .ToListAsync();
            int[] days;
            if (Int32.Parse(year) % 4 == 0 && Int32.Parse(year) % 100 != 0)
            {
                //瑞年
                days = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30 };

            }
            else
            {
                days = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30 };
            }
            int MonthIndex = int.Parse(month) - 1;
            string[] Days;
            if (days[MonthIndex] == 31)
            {
                Days = new string[] { "1日", "2日", "3日", "4日", "5日", "6日", "7日", "8日", "9日", "10日", "11日", "12日", "13日", "14日", "15日", "16日", "17日", "18日", "19日", "20日", "21日", "22日", "23日", "24日", "25日", "26日", "27日", "28日", "29日", "30日", "31日" };
            }
            else if (days[MonthIndex] == 30)
            {
                Days = new string[] { "1日", "2日", "3日", "4日", "5日", "6日", "7日", "8日", "9日", "10日", "11日", "12日", "13日", "14日", "15日", "16日", "17日", "18日", "19日", "20日", "21日", "22日", "23日", "24日", "25日", "26日", "27日", "28日", "29日", "30日" };
            }
            else if (days[MonthIndex] == 29)
            {
                Days = new string[] { "1日", "2日", "3日", "4日", "5日", "6日", "7日", "8日", "9日", "10日", "11日", "12日", "13日", "14日", "15日", "16日", "17日", "18日", "19日", "20日", "21日", "22日", "23日", "24日", "25日", "26日", "27日", "28日", "29日" };
            }
            else
            {
                Days = new string[] { "1日", "2日", "3日", "4日", "5日", "6日", "7日", "8日", "9日", "10日", "11日", "12日", "13日", "14日", "15日", "16日", "17日", "18日", "19日", "20日", "21日", "22日", "23日", "24日", "25日", "26日", "27日", "28日" };
            }
            if (Type == 0)
            {
                //menu

                var MonthSales = new List<MonthSale>();
                if (Menus != null)
                {
                   dines = await db.DineMenus
                      .Include(d => d.Dine)
                      .Where(d => d.DineId.Substring(0, 4) == date && d.Dine.IsPaid == true && d.Status == DineMenuStatus.Normal && Menus.Contains(d.MenuId))
                      .GroupBy(d => d.DineId.Substring(0, 6))
                      .Select(g => g.Key)
                      .ToListAsync();


                    MenuIds = db.DineMenus.Include(d => d.Dine)
                        .Where(d => dines.Contains(d.DineId.Substring(0, 6)) && Menus.Contains(d.MenuId))
                        .Select(d => d.MenuId)
                        .GroupBy(d => d)
                        .Select(g => g.Key).ToList();

                    foreach (var i in MenuIds)
                    {
                        var newMonthSale = new MonthSale();
                        newMonthSale.Menu = menus.Where(d => d.Id == i).FirstOrDefault();
                        List<int> Counts = new List<int>();
                        for (var j = 0; j < days[MonthIndex]; j++)
                        {
                            string day = year + month + (j + 1).ToString().PadLeft(2, '0');
                            int num = DineMenus.Where(d => d.DineId.Substring(0, 6) == day && d.MenuId == i && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift))
                                .GroupBy(g => g.MenuId)
                                .Select(g => g.Sum(gg => gg.Count))
                                .FirstOrDefault();
                            Counts.Add(num);
                        }
                        newMonthSale.Counts = Counts;
                        MonthSales.Add(newMonthSale);
                    }
                }
                
                var Sum = new List<int>();
                for (var i = 0; i < days[MonthIndex]; i++)
                {
                    Sum.Add(0);
                }
                for (var i = 0; i < MonthSales.Count(); i++)
                {
                    for (var j = 0; j < MonthSales[i].Counts.Count(); j++)
                    {
                        Sum[j] += MonthSales[i].Counts[j];
                    }
                }
                foreach(var i in MonthSales)
                {
                    i.CountAll = i.Counts.Sum(s => s);
                }
                int CountAll = Sum.Sum(s => s);
                return Json(new { Status = true, MonthSales = MonthSales, Days = Days, Sum = Sum , CountAll = CountAll });
            }
            else if (Type == 1)
            {
                // fatherClass
                var MonthSales = new List<MonthClassSale>();
                foreach (var i in Fathers)
                {
                    var MenusId = Management.Models.Method.GetMenuIdByFather(i, db);//根据大类数组找到菜品
                    var newMonthSale = new MonthClassSale();
                    newMonthSale.Menu = classes
                        .Where(m => m.Id == i)
                        .FirstOrDefault();
                    List<int> Counts = new List<int>();
                    for (var j = 0; j < days[MonthIndex]; j++)
                    {
                        string day = year + month + (j + 1).ToString().PadLeft(2, '0');
                        int num = DineMenus.Where(d => d.DineId.Substring(0, 6) == day && MenusId.Contains(d.MenuId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift))
                            .GroupBy(g => g.DineId.Substring(0, 6))
                            .Select(g => g.Sum(gg => gg.Count))
                            .FirstOrDefault();
                        Counts.Add(num);
                    }
                    newMonthSale.Counts = Counts;
                    MonthSales.Add(newMonthSale);
                }
                var Sum = new List<int>();
                for (var i = 0; i < days[MonthIndex]; i++)
                {
                    Sum.Add(0);
                }
                for (var i = 0; i < MonthSales.Count(); i++)
                {
                    for (var j = 0; j < MonthSales[i].Counts.Count(); j++)
                    {
                        Sum[j] += MonthSales[i].Counts[j];
                    }
                }
                foreach (var i in MonthSales)
                {
                    i.CountAll = i.Counts.Sum(s => s);
                }
                int CountAll = Sum.Sum(s => s);
                return Json(new { Status = true, MonthSales = MonthSales, Days = Days, Sum = Sum, CountAll = CountAll });
            }
            else
            {
                var MonthSales = new List<MonthClassSale>();
                foreach (var i in Childs)
                {
                    var MenusId = Management.Models.Method.GetMenuIdByChild(i, db);//根据大类数组找到菜品
                    var newMonthSale = new MonthClassSale();
                    newMonthSale.Menu = classes
                        .Where(m => m.Id == i)
                        .FirstOrDefault();
                    List<int> Counts = new List<int>();
                    for (var j = 0; j < days[MonthIndex]; j++)
                    {
                        string day = year + month + (j + 1).ToString().PadLeft(2, '0');
                        int num = db.DineMenus.Where(d => d.DineId.Substring(0, 6) == day && MenusId.Contains(d.MenuId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift))
                            .GroupBy(g => g.DineId.Substring(0, 6))
                            .Select(g => g.Sum(gg => gg.Count))
                            .FirstOrDefault();
                        Counts.Add(num);
                    }
                    newMonthSale.Counts = Counts;
                    MonthSales.Add(newMonthSale);
                }
                var Sum = new List<int>();
                for (var i = 0; i < days[MonthIndex]; i++)
                {
                    Sum.Add(0);
                }
                for (var i = 0; i < MonthSales.Count(); i++)
                {
                    for (var j = 0; j < MonthSales[i].Counts.Count(); j++)
                    {
                        Sum[j] += MonthSales[i].Counts[j];
                    }
                }
                foreach (var i in MonthSales)
                {
                    i.CountAll = i.Counts.Sum(s => s);
                }
                int CountAll = Sum.Sum(s => s);
                return Json(new { Status = true, MonthSales = MonthSales, Days = Days, Sum = Sum, CountAll = CountAll });
            }
        }

        public async Task<JsonResult> GetRangeElement()
        {
            var FatherClass = await db.MenuClasses.Where(m => m.Usable == true && m.Level == 0)
                .ToListAsync();
            var ChildClass = await db.MenuClasses.Where(m => m.Usable == true && m.Level != 0)
                .ToListAsync();
            var Areas = await db.Areas.Where(a => a.Usable == true)
                .ToListAsync();
            var Desks = await db.Desks.Where(d => d.Usable == true)
                .ToListAsync();
            var Waiters = await db.Staffs.ToListAsync();
            return Json(new { FatherClass = FatherClass, ChildClass = ChildClass, Areas = Areas, Desks = Desks, Waiters = Waiters });
        }


        public async Task<JsonResult> GetRangeSearch(string Begin, string End, string FatherClassId, string ChildClassId, string WaiterId, string AreaId, string DesksId)
        {
            if (Begin == null)
            {
                Begin = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (End == null)
            {
                End = DateTime.Now.ToString("yyyy-MM-dd");
            }
            var dateBegin = Begin.Split(new char[1] { '-' });
            string beginDate = dateBegin[0].Substring(2, 2) + dateBegin[1] + dateBegin[2];
            var dateEnd = End.Split(new char[1] { '-' });
            string endDate = dateEnd[0].Substring(2, 2) + dateEnd[1] + (Convert.ToInt32(dateEnd[2]) + 1).ToString().PadLeft(2, '0');
            var dines = new List<string>();
            var Waiters = new List<string>();
            var Desks = new List<string>();
            if (AreaId == null)
            {
                Desks = await db.Desks.Where(d => d.Usable == true).Select(d => d.Id).ToListAsync();
            }
            else
            {
                if (DesksId == null)
                {
                    Desks = await db.Desks.Where(d => d.Usable == true && d.AreaId == AreaId).Select(d => d.Id).ToListAsync();
                }
                else
                {
                    Desks.Add(DesksId);
                }
            }
            if (WaiterId == null)
            {
                dines = await db.Dines
                   .Where(d => d.IsPaid == true && string.Compare(d.Id, beginDate) >= 0 && string.Compare(d.Id, endDate) <= 0 && Desks.Contains(d.DeskId))
                   .Select(d => d.Id)
                   .ToListAsync();
            }
            else
            {
                dines = await db.Dines
                  .Where(d => d.IsPaid == true && string.Compare(d.Id, beginDate) >= 0 && string.Compare(d.Id, endDate) <= 0 && Desks.Contains(d.DeskId) && d.WaiterId == WaiterId)
                  .Select(d => d.Id)
                  .ToListAsync();
            }
            var Classes = new List<string>();
            if (FatherClassId == null)
            {
                Classes = await db.MenuClasses
                    .Where(m => m.Usable == true)
                    .Select(m => m.Id)
                    .ToListAsync();
            }
            else
            {
                if (ChildClassId == null)
                {
                    //小类所有
                    Classes.Add(FatherClassId);
                    var sub = await db.MenuClasses.Where(m => m.ParentMenuClassId == FatherClassId)
                        .Select(m => m.Id)
                        .ToListAsync();
                    foreach (var i in sub)
                    {
                        Classes.Add(i);
                        var subsub = await db.MenuClasses.Where(m => m.ParentMenuClassId == i)
                            .Select(m => m.Id)
                            .ToListAsync();
                        foreach (var j in subsub)
                        {
                            Classes.Add(j);
                        }
                    }
                }
                else
                {
                    Classes.Add(FatherClassId);
                    Classes.Add(ChildClassId);
                }
            }
            List<string> Menus = new List<string>();
            foreach (var i in Classes)
            {
                var menus = await db.Menus.Where(m => m.Classes.Select(mm => mm.Id).Contains(i))
                           .Select(m => m.Id)
                           .ToListAsync();
                foreach (var j in menus)
                {
                    Menus.Add(j);
                }
            }
            Menus = Menus.GroupBy(m => m).Select(g => g.Key).ToList();
            var datas = await db.DineMenus.Where(d => dines.Contains(d.DineId) && Menus.Contains(d.MenuId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift))
                .GroupBy(d => d.MenuId)
                .Select(g => new
                {
                    Id = g.Key,
                    Name = db.Menus.FirstOrDefault(m => m.Id == g.Key).Name,
                    Price = db.Menus.Where(m => m.Id == g.Key).Select(m => m.MenuPrice.Price).FirstOrDefault(),
                    Count = g.Sum(gg => gg.Count),
                    PriceAll = g.Sum(gg => (gg.Price + gg.RemarkPrice))
                })
                .OrderByDescending(g => g.Count)
                .ThenByDescending(g => g.PriceAll)
                .ToListAsync();
            return Json(new { Datas = datas });
        }



        public async Task<JsonResult> GetSalesAll(string Begin, string End)
        {
            if (Begin == null)
            {
                Begin = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (End == null)
            {
                End = DateTime.Now.ToString("yyyy-MM-dd");
            }
            var dateBegin = Begin.Split(new char[1] { '-' });
            string beginDate = dateBegin[0].Substring(2, 2) + dateBegin[1] + dateBegin[2];
            var dateEnd = End.Split(new char[1] { '-' });
            string endDate = dateEnd[0].Substring(2, 2) + dateEnd[1] + (Convert.ToInt32(dateEnd[2]) + 1).ToString().PadLeft(2, '0');
            var dines = await db.Dines
                 .Where(d => d.IsPaid == true && string.Compare(d.Id, beginDate) >= 0 && string.Compare(d.Id, endDate) <= 0)
                 .Select(d => d.Id)
                 .ToListAsync();
            var OriDineMenus = await db.DineMenus
                 .Include(d => d.Dine)
                 .Include(d => d.Menu.Classes)
                 .Where(d => (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift) && dines.Contains(d.DineId))
                 .ToListAsync();
            var menus = OriDineMenus
                .Select(d => d.MenuId)
                .ToList();
            var classes = new List<string>();
            var OriMenus = await db.Menus
                .Include(m=>m.Classes)
                .Where(m => m.Usable == true)
                .ToListAsync();
            var OriClasses = await db.MenuClasses.Where(m => m.Usable == true)
                .ToListAsync();
            foreach (var i in menus)
            {
                var menuclasses = OriMenus
                    .Where(m => m.Id == i)
                    .Select(m => m.Classes.Select(mm => mm.Id).ToList())
                    .ToList();
                foreach (var j in menuclasses)
                {
                    foreach (var k in j)
                    {
                        classes.Add(k);
                    }
                }
            }
            classes = classes.GroupBy(m => m)
                .Select(g => g.Key)
                .ToList();
            var Classes = new List<string>();
            var Datas = new List<Sales>();
            var AllMenus = OriDineMenus
                       .GroupBy(g => g.Dine.BeginTime.Year.ToString()+ "-" + g.Dine.BeginTime.Month.ToString() + "-" + g.Dine.BeginTime.Day.ToString())
                       .Select(g => new
                       {
                           Time = g.Key,
                           PriceAll = g.Sum(gg => (gg.Price + gg.RemarkPrice))
                       })
                       .ToList();//获取时间段内 所有类别的 所有销售信息
            if (AllMenus != null)
            {
                foreach (var i in AllMenus)
                {
                    Datas.Add(new Sales
                    {
                        Time = i.Time,
                        Datas = new List<SalesAll>()
                    });
                }
                foreach (var i in Datas)
                {
                    foreach (var j in classes)
                    {
                        i.Datas.Add(new SalesAll { Precent = 0, PriceAll = 0 });
                    }
                }
            }
            else
            {
                return Json(new { Datas = Datas });
            }
            for (var i = 0; i < classes.Count(); i++)
            {
                string Id = classes[i];
                var temp = OriClasses
                    .Where(m => m.Id == Id)
                    .Select(m => m.Name)
                    .FirstOrDefault();
                Classes.Add(temp);
                var Menus =  OriDineMenus
                   .Select(d => new
                   {
                       Id = d.MenuId,
                       Classes = d.Menu.Classes.ToList()
                   })
                   .ToList();// 该段时间内 该类别的菜品
                var dinemenus = new List<string>();
                var tempSale = new Sales();
                if (Menus != null)
                {
                    foreach (var j in Menus)
                    {
                        foreach (var k in j.Classes)
                        {
                            if (k.Id == classes[i])
                            {
                                dinemenus.Add(j.Id);
                            }
                        }
                    }
                    var SaleMenus = OriDineMenus
                        .Where(d =>  dinemenus.Contains(d.MenuId))
                       .GroupBy(g => g.Dine.BeginTime.Year.ToString() + "-" + g.Dine.BeginTime.Month.ToString() + "-" + g.Dine.BeginTime.Day.ToString())
                       .Select(g => new
                       {
                           Time = g.Key,
                           PriceAll = g.Sum(gg => (gg.Price + gg.RemarkPrice))
                       })
                       .ToList();//获取时间段内 某个类别的 所有销售信息
                    if (SaleMenus != null)
                    {
                        foreach (var kk in SaleMenus)
                        {
                            for (var k = 0; k < Datas.Count(); k++)
                            {
                                if (kk.Time == Datas[k].Time)
                                {
                                    Datas[k].Datas[i].PriceAll = kk.PriceAll;
                                }
                            }
                        }
                    }
                }
            }
            for (var i = 0; i < Datas.Count(); i++)
            {
                var sum = Datas[i].Datas.Sum(d => d.PriceAll);
                for (var j = 0; j < Datas[i].Datas.Count(); j++)
                {
                    Datas[i].Datas[j].Precent = (double)(Datas[i].Datas[j].PriceAll / sum);
                }
            }
            var Sum = new List<SaleSum>();
            for(var i = 0; i< Classes.Count(); i++)
            {
                decimal TempPrice = 0;
                foreach(var j in Datas)
                {
                    TempPrice += j.Datas[i].PriceAll;
                }
                Sum.Add(new SaleSum {
                    Price = TempPrice
                });
            }
            foreach(var i in Sum)
            {
                i.Percent = (double) (i.Price / Sum.Sum(s => s.Price))*100;
            }
            return Json(new { Datas = Datas, Classes = Classes ,Sum = Sum});
        }

        public async Task<JsonResult> DailyMenus(string BeginTime,string EndTime,string AreaId,List<string> Fathers,List<string> Childs,List<string> Menus,int Type)
        {
            var  Begin = Convert.ToDateTime(BeginTime);
            var  End = Convert.ToDateTime(EndTime);
            var dines = new List<string>();
            if (AreaId == null)
            {
                dines = await db.Dines
                   .Where(d => d.IsPaid == true && (DateTime.Compare(d.BeginTime, Begin) >= 0) && (DateTime.Compare(d.BeginTime, End) <= 0))
                   .Select(d => d.Id)
                   .ToListAsync();//该段时间内的所有订单
            }
            else
            {
                var Desks = await db.Desks.Where(d => d.Usable == true && d.AreaId == AreaId)
                    .Select(d => d.Id)
                    .ToListAsync();
                dines = await db.Dines
                   .Where(d => d.IsPaid == true && (DateTime.Compare(d.BeginTime, Begin) >= 0) && (DateTime.Compare(d.BeginTime, End) <= 0)&&Desks.Contains(d.DeskId))
                   .Select(d => d.Id)
                   .ToListAsync();//该段时间内的区域订单
            }
            var OrderMenus = await db.DineMenus.Where(d => dines.Contains(d.DineId) && (d.Status == DineMenuStatus.Normal || d.Status == DineMenuStatus.Gift))
                .ToListAsync();
            var Classes = await db.MenuClasses.Where(m => m.Usable == true).ToListAsync();
            var OriMenus = await db.Menus
                .Include(m=>m.MenuPrice)
                .Where(d => d.Usable == true).ToListAsync();
            var Datas = new List<DailyMenu>();
            if (Type == 0)
            {
                //menu
                if (Menus != null)
                {
                    Datas = OrderMenus.Where(d => Menus.Contains(d.MenuId))
                   .GroupBy(d => d.MenuId)
                   .Select(g => new DailyMenu
                   {
                       Id = g.Key,
                       Name = OriMenus.Where(m => m.Usable == true && m.Id == g.Key).FirstOrDefault().Name,
                       Count = g.Sum(gg => gg.Count),
                       OriPrice = g.Sum(gg => gg.OriPrice * gg.Count + gg.RemarkPrice),
                       Price = g.Sum(gg => gg.Price * gg.Count + gg.RemarkPrice),
                       SaveMoney = g.Sum(gg => gg.OriPrice * gg.Count + gg.RemarkPrice) - g.Sum(gg => gg.Price * gg.Count + gg.RemarkPrice)
                   })
                   .ToList();
                }
            }
            else if(Type == 1)
            {
                //father
                foreach(var i in Fathers)
                {
                    var MenuIds = Management.Models.Method.GetMenuIdByFather(i, db);
                    var Data = new DailyMenu();
                    Data.Id = i;
                    Data.Name = Classes.Where(d => d.Id == i).FirstOrDefault().Name;
                    Data.Count = OrderMenus.Where(d => MenuIds.Contains(d.MenuId)).Sum(d => d.Count);
                    Data.OriPrice = OrderMenus.Where(d => MenuIds.Contains(d.MenuId)).Sum(d => d.OriPrice*d.Count + d.RemarkPrice);
                    Data.SaveMoney = OrderMenus.Where(d => MenuIds.Contains(d.MenuId)).Sum(d => d.OriPrice * d.Count - d.Price * d.Count);
                    Data.Price = OrderMenus.Where(d => MenuIds.Contains(d.MenuId)).Sum(d => d.Price*d.Count + d.RemarkPrice);
                    Datas.Add(Data);
                }
            }
            else
            {
                //child
                foreach (var i in Childs)
                {
                    var MenuIds = Management.Models.Method.GetMenuIdByChild(i, db);
                    var Data = new DailyMenu();
                    Data.Id = i;
                    Data.Name = Classes.Where(d => d.Id == i).FirstOrDefault().Name;
                    Data.Count = OrderMenus.Where(d => MenuIds.Contains(d.MenuId)).Sum(d => d.Count);
                    Data.OriPrice = OrderMenus.Where(d => MenuIds.Contains(d.MenuId)).Sum(d => d.OriPrice * d.Count + d.RemarkPrice);
                    Data.SaveMoney = OrderMenus.Where(d => MenuIds.Contains(d.MenuId)).Sum(d => d.OriPrice * d.Count - d.Price * d.Count);
                    Data.Price = OrderMenus.Where(d => MenuIds.Contains(d.MenuId)).Sum(d => d.Price * d.Count + d.RemarkPrice);
                    Datas.Add(Data);
                }
            }
            var Sum = new DailySum();
            Sum.TotalCount = Datas.Sum(d => d.Count);
            Sum.TotalOriPrice = Datas.Sum(d => d.OriPrice);
            Sum.TotalPrice = Datas.Sum(d => d.Price);
            Sum.TotalSaveMoney = Datas.Sum(d => d.SaveMoney);
            return Json(new { Datas = Datas, Sum = Sum });
        }

        public async Task<JsonResult> GetInvoice()
        {
            var Invoices = await db.Invoices
                .Include(d => d.Dine)
                .Where(d => d.Dine.IsInvoiced == true&&
                (SqlFunctions.DateDiff("day", DateTime.Now, d.Dine.BeginTime) >= 0 )&&
                (SqlFunctions.DateDiff("day", DateTime.Now, d.Dine.BeginTime) <= 0))
                .ToListAsync();
            return Json(new SuccessState(Invoices));
        }

        public async Task<JsonResult> SearchTimeInvoice(string Begin,string End)
        {
            DateTime BeginTime, EndTime;
            if (Begin == null)
            {
                BeginTime = DateTime.Now;
            }
            else
            {
                BeginTime = Convert.ToDateTime(Begin);
            }
            if (End == null)
            {
                EndTime = DateTime.Now;
            }
            else
            {
                EndTime = Convert.ToDateTime(End);
            }
            var Invoices =
                await db.Invoices
                .Include(d => d.Dine)
                .Where(d => d.Dine.IsInvoiced == true &&
                  SqlFunctions.DateDiff("day", BeginTime, d.Dine.BeginTime) >= 0 &&
                  SqlFunctions.DateDiff("day", EndTime, d.Dine.BeginTime) <= 0)
                .ToListAsync();
            return Json(new SuccessState(Invoices));

        }
        //最后2个括号
    }
}