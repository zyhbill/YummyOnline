using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HotelDAO.Models;
namespace Management.ObjectClasses
{
    /// <summary>
    /// 远程登陆用户
    /// </summary>
    public class RemoteUser
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public enum ManagePrintType
    {
        //厨房
        Kitchen = 0 ,
        //收银
        Recipt = 1
    }

    /// <summary>
    /// 返回的状态
    /// </summary>
    /// 
    public class RStatus
    {
        public bool Status { get; set; }
        public int? HotelId { get; set; }
        public string Name { get; set; }
        public string ClerkId { get; set; }
        public int IsStaffPay { get; set; }
        public int IsStaffReturn { get; set; }
        public int IsStaffEdit { get; set; }
    }

    public class PayMoney
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public decimal Number { get; set; }
    }

    public class PayDine
    {
        public string Id { get; set; }
        public int Discount { get; set; }
        public string DiscountName { get; set; }

    }

    public class OpenInfo
    {
        public int HeadCount { get; set; }
        public float Price { get; set; }
        public OpenDeskInfo Desk { get; set; }
        public List<OpenOrderMenus> OrderedMenus { get; set; }
        public List<OpenOrderMenus> SendMenus { get; set; }
    }

    public class OpenDeskInfo
    {
        public string Id { get; set; }
    }

    public class OpenOrderMenus
    {
        public string Id { get; set; }
        public int Ordered { get; set; }
        public List<int> Remarks { get; set; }
    }

    public class OpenDiscount
    {
        public string Name { get; set; }
        public int Discount { get; set; }
        public bool? IsSet { get; set; }
    }

    public class ConbineDine
    {
        public string Id { get; set; }
        public string DeskId { get; set; }
    }

    public class newArea
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? DepartmentReciptId { get; set; }
        public int? DepartmentServeId { get; set; }
        public string Description { get; set; }
    }

    public class EditDesk
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public int HeadCount { get; set; }
        public decimal MinPrice { get; set; }
        public string AreaId { get; set; }
        public string QrCode { get; set; }
    }
    public class EditMenu
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string EnglishName { get; set; }
        public List<Remark> Remarks { get; set; }
        public List<MenuClass> Classes { get; set; }
        public MenuPrice MenuPrice { get; set; }
        public int DepartmentId { get; set; }
        public bool IsFixed { get; set; }
        public bool IsSetMeal { get; set; }
        public int MinOrderCount { get; set; }
        public string NameAbbr { get; set; }
        public int Ordered { get; set; }
        public string PicturePath { get; set; }
        public int SaltyDegree { get; set; }
        public int SourDegree { get; set; }
        public int SpicyDegree { get; set; }
        public int SweetDegree { get; set; }
        public int SupplyDate { get; set; }
        public string Name { get; set; }
        public MenuStatus Status { get; set; }
        public string Unit { get; set; }
    }
    public class AddMenu
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public string Code { get; set; }
        public string NameAbbr { get; set; }
        public int DepartmentId { get; set; }
        public bool IsFixed { get; set; }
        public AddMenuPrice MenuPrice { get; set; }
        public int SaltyDegree { get; set; }
        public int SourDegree { get; set; }
        public int SpicyDegree { get; set; }
        public int SweetDegree { get; set; }
        public MenuStatus Status { get; set; }
        public bool IsSetMeal { get; set; }
        public int MinOrderCount { get; set; }
        public int SupplyDate { get; set; }
        public string Unit { get; set; }

    }

    public class AddMenuPrice
    {
        public double Discount { get; set; }
        public bool ExcludePayDiscount { get; set; }
        public string Id { get; set; }
        public int Points { get; set; }
        public decimal Price { get; set; }
    }

    public class ConbineStaff
    {
        public Staff Staff { get; set; }
        public YummyOnlineDAO.Models.Staff SysStaff { get; set; }
    }

    public class EditStaff
    {
        public string Id { get; set; }
        public string SigninName { get; set; }
        public string PhoneNumber { get; set; }
        public string PassWord { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class EditStaffHotel
    {
        public string Name { get; set; }
        public TimeSpan WorkTimeFrom { get; set; }
        public TimeSpan WorkTimeTo { get; set; }
    }

    public class SalesData
    {
        public string name { get; set; }
        public int Count { get; set; }
        public List<int> data { get; set; }
    }

    public class MenuYearDates
    {
        public List<SalesData> Datas { get; set; }
    }

    public class monthSale
    {
        public string Id { get; set; }
        public string name { get; set; }
        public int TotalPrice { get; set; }
    }

    public class Meal
    {
        public List<MealList> Menus { get; set; }
        public Menu MealMenu { get; set; }
    }

    public class MealList
    {
        public int Count { get; set; }
        public Menu Menu { get; set; }
    }

    public class MenuInMeal
    {
        public string MenuId { get; set; }
        public int Count { get; set; }
    }
    
    public class DailyDetail
    {
        public DateTime Time { get; set; }
        public List<Detail> Detail { get; set; }
    }

    public class Detail
    {
        public PayKind PayKind { get; set; }
        public decimal Total { get; set; }
    }


    public class MonthSale
    {
        public Menu Menu { get; set; }
        public int CountAll { get; set; } = 0;
        public List<int> Counts { get; set; }
    }

    public class MonthClassSale
    {
        public MenuClass Menu { get; set; }
        public int CountAll { get; set; } = 0;
        public List<int> Counts { get; set; }
    }

    public class Sales
    {
        public string Time { get; set; }
        public List<SalesAll> Datas { get; set; }
    }

    public class SalesAll
    {
        public decimal PriceAll { get; set; } = 0;
        public double Precent { get; set; } = 0;
    }

    public class AddDineMenu
    {
        public string DineId { get; set; }
        public List<myMenu> Menus { get; set; }
    }

    public class myMenu
    {
        public string Id { get; set; }
        public int Num { get; set; }
        public List<int> Remarks { get; set; }
        public bool IsSend { get; set; }
    }

    public class monthDine
    {
        public string Time { get; set; }
        public Menu Menu { get; set; }
        public int TotalNum { get; set; }
    }

    public class DailyMenu
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Count { get;set;}
        public decimal OriPrice { get; set; }
        public decimal SaveMoney { get; set; }
        public decimal Price { get; set; }
    }

    public class DailySum
    {
        public int TotalCount { get; set; } = 0;
        public decimal TotalOriPrice { get; set; } = 0;
        public decimal TotalPrice { get; set; } = 0;
        public decimal TotalSaveMoney { get; set; } = 0;
    }

    public class SalePercent
    {
        public Menu Menu { get; set; }
        public int Count { get; set; }
        public decimal TotalOriPrice { get; set; } = 0;
        public decimal TotalPrice { get; set; } = 0;
        public decimal TotalSaveMoney { get; set; } = 0;

        public double CountPercent { get; set; }
        public double OriPricePercent { get; set; }
        public double PricePercent { get; set; }
        public double SaveMoneyPercent { get; set; }
    }

    public class PercentSum
    {
        public int Count { get; set; } = 0;
        public decimal TotalOriPrice { get; set; } = 0;
        public decimal TotalPrice { get; set; } = 0;
        public decimal TotalSaveMoney { get; set; } = 0;

        public double CountPercent { get; set; }
        public double OriPricePercent { get; set; }
        public double PricePercent { get; set; }
        public double SaveMoneyPercent { get; set; }

    }

    public class SaleSum
    {
        public decimal Price { get; set; }
        public double Percent { get; set; } = 0;
    }

    public class MenuSaleClassData
    {
        public MenuClass MenuClass { get; set; }
        public int Count{get; set;} = 0;
        public decimal OriPrice { get; set; } = 0;
        public decimal SaveMoney { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public double OriPrecent { get; set; } = 0;
        public double SavePrecent { get; set; } = 0;
        public double Precent { get; set; } = 0;
    }

    public class ErrorState
    {
        public bool Status { get; set; } = false;
        public string ErrorMessage { get; set; }
        public ErrorState() { }
        public ErrorState(string Message = "")
        {
            ErrorMessage = Message;
        }
    }

    public class SuccessState
    {
        public bool Status { get; set; } = true;
        public object Data { get; set; }
        public SuccessState() { }
        public SuccessState(object obj)
        {
            Data = obj;
        }
    }

    public  class PostData
    {
        public bool Succeeded { get; set; }
        public string Data { get; set; }
    }

    public class Profit
    {
        public int Id { get; set; }
        public decimal Num { get; set; }
    }

    public class Format
    {
        public int KitchenOrderFontSize { get; set; }
        public int KitchenOrderSmallFontSize { get; set; }
        public int PaperSize { get; set; }
        public int ReciptBigFontSize { get; set; }
        public int ReciptFontSize { get; set; }
        public int ReciptSmallFontSize { get; set; }
        public int ServeOrderFontSize { get; set; }
        public int ServeOrderSmallFontSize { get; set; }
        public int ShiftBigFontSize { get; set; }
        public int ShiftFontSize { get; set; }
        public int ShiftSmallFontSize { get; set; }
        public int ColorDepth { get; set; }
    }

    public class MyMeal
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAbbr { get; set; }
        public List<MealClass> SetMealClasses { get; set; }
        public MenuPrice Price { get; set; }
    }

    public class MealClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public List<MealClassMenu> SetMealClassMenus { get; set; }
    }

    public class MealClassMenu
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAbbr { get; set; }
        public int Count { get; set; }
        public int OrderNum { get; set; } = 0;
    }

    public class AddSetMeal
    {
        public string Id { get; set; }
        public int Count { get; set; }
    }
}