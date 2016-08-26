angular.module('wxf', [])
.controller('PayKindsCtrl', function ($scope,$rootScope ,$http) {

    $rootScope.FatherPage = "支付管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "支付方式设置";
    $scope.switch = function (payKind) {
        $http.post('/PayKinds/SwitchPayKinds', {
            Id: payKind.Id
        }).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }
    function refresh() {
        $http.post('/PayKinds/GetPayKinds').then(function (response) {
            $scope.payKinds = response.data;
        });
    }
    $scope.newPayKind = {
        name: null,
        description: null,
        discount: null
    }
    $scope.deletePay = function (payKind) {
        swal({
            title: "确定删除此项?",
            text: "你将不可恢复此项，请谨慎操作!",
            type: "warning",
            showCancelButton: true,

            confirmButtonText: "是, 删除!",
            cancelButtonText: "否, 保留!",
            closeOnConfirm: false
        }, function () {
            $http.post('/PayKinds/DeletePayKind', {
                Id: payKind.Id
            }).then(function (response) {
                if (response.data.succeeded) {
                    refresh();
                    swal("删除成功!", "此项已经删除请重新进入页面.", "success");
                }
                else {
                    alert('失败了!!!');
                    swal("删除失败!", "此项已经删除请重新进入页面.", "error");
                }
            })
        });
       
    }

    $scope.addPayKind = function () {
        $http.post('/PayKinds/AddPayKinds', $scope.newPayKind).then(function (response) {
            if (response.data.succeeded) {
                refresh();
                $scope.newPayKind.name = null;
                $scope.newPayKind.description = null;
                $scope.newPayKind.discount = null;
            }
            else {
                alert('失败了!!!');
            }
        });
    }

    $scope.altPayKind = function (payKind) {
        if (payKind.isAlt) {
            payKind.Name = payKind.ori.Name;
            payKind.Description = payKind.ori.Description;
            payKind.Discount = payKind.ori.Discount;
        } else {
            payKind.ori = {
                Name: payKind.Name,
                Description: payKind.Description,
                Discount: payKind.Discount
            }

        }
        payKind.isAlt = !payKind.isAlt;
    }

    $scope.finishaltPayKind = function (payKind) {
        console.log(payKind);
        $http.post('/PayKinds/AltPayKinds', payKind).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }

    refresh();
})
.controller('PrintersCtrl', function ($scope,$rootScope, $http) {
    $rootScope.FatherPage = "支付管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "打印机设置";
    $scope.switch = function (printer) {
        $http.post('/Printers/SwitchPrinters', {
            Id: printer.Id
        }).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }
    function refresh() {
        $http.post('/Printers/GetPrinters').then(function (response) {
            $scope.printers = response.data;
        });
    }
    $scope.newPrinter = {
        name: null,
        ipaddress: null
    }

    $scope.addPrinter = function () {
        $http.post('/Printers/AddPrinters', $scope.newPrinter).then(function (response) {
            if (response.data.succeeded) {
                refresh();
                $scope.newPrinter.name = null;
                //$scope.newPrinter.ipaddress = null;
            }
            else {
                alert('失败了!!!');
            }
        });
    }

    $scope.altPrinter = function (printer) {
        if (printer.isAlt) {
            printer.Name = printer.ori.Name;
            printer.IpAddress = printer.ori.IpAddress;
        } else {
            printer.ori = {
                Name: printer.Name,
                IpAddress: printer.IpAddress,
            }

        }
        printer.isAlt = !printer.isAlt;
    }

    $scope.finishaltPrinter = function (printer) {
        $http.post('/Printers/AltPrinters', printer).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }

    $scope.DeletePrinter = function (printer) {
        $http.post('/Printers/DeletePrinters', {
            id:printer.Id
        }).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }
    refresh();
})


.controller('TimeDiscountsCtrl', function ($scope, $rootScope, $http) {
    $rootScope.FatherPage = "支付管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "折扣方案设置";
    function refresh() {
        $http.post('/TimeDiscounts/GetTimeDiscounts').then(function (response) {
            $scope.timeDiscounts = response.data;
        });
    }
    $scope.newTimeDiscount = {
        from: null,
        to: null,
        week:null,
        discount: null,
        name: null
    }

    $scope.addTimeDiscount = function () {
        $http.post('/TimeDiscounts/AddTimeDiscounts', $scope.newTimeDiscount).then(function (response) {
            if (response.data.succeeded) {
                refresh();
                $scope.newTimeDiscount.week = null;
                $scope.newTimeDiscount.discount = null;
                $scope.newTimeDiscount.name = null;
            }
            else {
                alert('失败了!!!');
            }
        });
    }

    $scope.altTimeDiscount = function (timeDiscount) {
        if (timeDiscount.isAlt) {
            timeDiscount.From = timeDiscount.ori.From;
            timeDiscount.To = timeDiscount.ori.To;
            timeDiscount.Week = timeDiscount.ori.Week;
            timeDiscount.Discount = timeDiscount.ori.Discount;
            timeDiscount.Name = timeDiscount.ori.Name;
        } else {
            timeDiscount.ori = {
                From: timeDiscount.From,
                To: timeDiscount.To,
                Week: timeDiscount.Week,
                Discount: timeDiscount.Discount,
                Name: timeDiscount.Name
            }

        }
        timeDiscount.isAlt = !timeDiscount.isAlt;
    }

    $scope.finishaltTimeDiscount = function (timeDiscount) {
        $http.post('/TimeDiscounts/AltTimeDiscounts',timeDiscount).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }
   

    $scope.delete = function (timeDiscount) {
        swal({
            title: "确定删除此项?",
            text: "你将不可恢复此项，请谨慎操作!",
            type: "warning",
            showCancelButton: true,
            
            confirmButtonText: "是, 删除!",
            cancelButtonText: "否, 保留!",
            closeOnConfirm: false
        }, function () {
            $http.post('/TimeDiscounts/DelTimeDiscounts', timeDiscount).then(function (response) {
                if (response.data.succeeded) {
                    refresh();
                    swal("删除成功!", "此项已经删除请重新进入页面.", "success");
                }
                else {
                    alert('!!!');
                    swal("删除失败!", "此项已经删除请重新进入页面.", "error");
                }

            });
        });
    }


    refresh();
})


.controller('HotelsCtrl', function ($scope, $rootScope, $http) {
    $rootScope.FatherPage = "酒店基本管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "酒店基本信息";
    function refresh() {
        $http.post('/Hotels/GetHotels').then(function (response) {
            $scope.hotels = response.data;
        });
    }
    $scope.newHotel = {
        name: null,
        address: null,
        tel: null,
        createdate: null,
        opentime: null,
        closetime: null
    }


    $scope.altHotel = function (hotel) {
        if (hotel.isAlt) {
            hotel.Name = hotel.ori.Name;
            hotel.Address = hotel.ori.Address;
            hotel.Tel = hotel.ori.Tel;
            hotel.CreateDate = hotel.ori.CreateDate;
            hotel.OpenTime = hotel.ori.OpenTime;
            hotel.CloseTime = hotel.ori.CloseTime;
        } else {
            hotel.ori = {
                Name: hotel.Name,
                Address: hotel.Address,
                Tel: hotel.Tel,
                CreateDate: hotel.CreateDate,
                OpenTime: hotel.OpenTime,
                CloseTime: hotel.CloseTime
            }

        }
        hotel.isAlt = !hotel.isAlt;
    }


    $scope.finishaltHotel = function (hotel) {
        $http.post('/Hotels/AltHotels', hotel).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        }, function (response) {
            alert('输入格式有误，无法修改！！！格式: 02:20:24');
        });
    }

    refresh();
})

.controller('VipCtrl', function ($scope, $rootScope, $http) {
    $rootScope.FatherPage = "酒店基本管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "会员类别设置";
    function refresh() {
        $http.post('/Vip/GetVip').then(function (response) {
            $scope.vips = response.data;
        });
    }
    $scope.newVip = {
        levelname: null,
        discount: null,
        discountname: null,
    }

    $scope.addVip = function () {
        $http.post('/Vip/AddVip', $scope.newVip).then(function (response) {
            if (response.data.succeeded) {
                refresh();
                $scope.newVip.levelname = null;
                $scope.newVip.discount = null;
                $scope.newVip.discountname = null;
            }
            else {
                alert('失败了！');
            }
        });
    }

    $scope.altVip = function (vip) {
        if (vip.isAlt) {
            vip.levelname = vip.ori.levelname;
            vip.discount = vip.ori.discount;
            vip.discountname = vip.ori.discountname;
        } else {
            vip.ori = {
                levelname: vip.levelname,
                discount: vip.discount,
                discountname: vip.discountname
            }

        }
        vip.isAlt = !vip.isAlt;
    }

    $scope.finishaltVip = function (vip) {
        $http.post('/Vip/AltVip', vip).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }
    $scope.delete = function (vip) {
        swal({
            title: "确定删除此项?",
            text: "你将不可恢复此项，请谨慎操作!",
            type: "warning",
            showCancelButton: true,
            
            confirmButtonText: "是, 删除!",
            cancelButtonText: "否, 保留!",
            closeOnConfirm: false
        }, function () {
            $http.post('/Vip/DelVip', vip).then(function (response) {
                if (response.data.succeeded) {
                    refresh();
                    swal("删除成功!", "此项已经删除请重新进入页面.", "success");
                }
                else {
                    alert('!!!');
                    swal("删除失败!", "此项已经删除请重新进入页面.", "error");
                }

            });
        });
      
    }
    refresh();
})

.controller('DinePaidDetailsCtrl', function ($scope, $rootScope, $http) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "订单查询";
    $scope.begintime = new Date(0, 0, 0, 0, 0);
    $scope.endtime = new Date(0, 0, 0, 23, 59);
    function refresh() {
        $http.post('/DinePaidDetails/GetDinePaidDetails').then(function (response) {
            $scope.dines = response.data.aaa;
            //$scope.sumprice = response.data.bbb;
            $scope.Sum = response.data.Sum;
            
            //$scope.headcount = response.data.headcount;
            //$scope.oriprice = response.data.oriprice;
            //$scope.price = response.data.price;
            //$scope.discount = response.data.discount;
            //$scope.youhui= response.data.youhui;
        });
    }
    function getpaykindname() {
        $http.post('/DinePaidDetails/GetPayKindName').then(function (response) {
            $scope.paykindnames = response.data;
            $scope.paykindnames.forEach(function (x) { x.IsChoose = true; })
            console.log($scope.paykindnames);


        });
    }
        function getstaffname() {
            $http.post('/DinePaidDetails/GetStaffName').then(function (response) {
                $scope.staffnames = response.data;
                console.log($scope.staffnames);

            });
    }
    $scope.setDine = function (dine) {
        $scope.nowDine = dine;
        console.log(dine);
        $http.post('/DinePaidDetails/GetDineDetails', {dineid:dine.Id}).then(function (response) {

            $scope.dinedetails = response.data.aaa;
            $scope.Back = response.data.Back;
            $scope.Gift = response.data.Gift;

        });
        //$http.post('/DinePaidDetails/GetClerkName', { clerkid: dine.ClerkId }).then(function (response) {

        //    $scope.clerkids = response.data;


        //});
        //$http.post('/DinePaidDetails/GetWaiterName', { waiterid: dine.WaiterId }).then(function (response) {

        //    $scope.waiterids = response.data;


        //});
        $http.post('/DinePaidDetails/SearchPayKindName', { dineid: dine.Id }).then(function (response) {

            $scope.paykindnameb = response.data;


        });
        
        
    }
    
    $scope.putInvoice = function () {
        var invoice = $scope.nowDine.Title;
        var price = $scope.nowDine.InvoicePrice;
        var Id = $scope.nowDine.Id;
        console.log(invoice);
        console.log(price);
        $http.post('/DinePaidDetails/putInvoice', { Id: Id, Invoice: invoice,Price:price }).then(function (response) {
            alert('录入成功');
            refresh();
        });
    }

    $scope.search = function (type) {
        var temp = $scope.paykindnames.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id })
        console.log(temp);
        
        
        $http.post('/DinePaidDetails/SearchDine', {
            begindate: $scope.begindate,
            enddate: $scope.enddate,
            begintime: $scope.begintime,
            endtime: $scope.endtime,
         
            waiterid: $scope.waiterid,
            
            payKindIds: temp,
            Type :type
        }).then(function (response) {
            console.log($scope.paykindname);
            
            //$scope.sumprice = response.data.bbb;
            //$scope.headcount = response.data.headcount;
            //$scope.oriprice = response.data.oriprice;
            //$scope.price = response.data.price;
            //$scope.discount = response.data.discount;
           
            if (response.data.succeeded==false) {
                alert('查无记录！！！');
            }
            else {
                $scope.dines = response.data.aaa;
                $scope.Sum = response.data.Sum;
            }

        });
    }

    refresh();
    getpaykindname();
    getstaffname();
})

.controller('MenuSetMealsCtrl', function ($scope, $rootScope, $http) {

    $rootScope.FatherPage = "支付管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "禁用商务套餐";
    function refresh() {
        $http.post('/MenuSetMeals/GetMenuSetMeals').then(function (response) {
            $scope.menuSetMeals = response.data;
        });
    }
    $scope.switch = function (menuSetMeal) {
        $http.post('/MenuSetMeals/SwitchMenuSetMeals', {
            MenuSetId: menuSetMeal.Id
        }).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
            console.log(menuSetMeal);
        });
    }

    $scope.close = function () {
        $http.post('/MenuSetMeals/AllClose').then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
           
        });
    }
    $scope.open = function () {
        $http.post('/MenuSetMeals/AllOpen').then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }

        });
    }
    refresh();
})

.controller('MenuOnSalesCtrl', function ($scope, $rootScope, $http) {
    $rootScope.FatherPage = "支付管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "特价菜设置";
    function refresh() {
        $http.post('/MenuOnSales/GetMenuOnSales').then(function (response) {
            $scope.menuOnSales = response.data;
        });
    }
    $scope.newMenuOnSale = {
        id: null,
        onsaleweek: null,
        MinPrice:null,
        price: null
    }

    $scope.addMenuOnSale = function () {
        $http.post('/MenuOnSales/AddMenuOnSales', $scope.newMenuOnSale).then(function (response) {
            if (response.data.succeeded) {
                refresh();
                $scope.newMenuOnSale = {};
            }
            else {
                alert('请按要求输入！！！');
            }
        });
    }

    $scope.altMenuOnSale = function (menuOnSale) {
        if (menuOnSale.isAlt) {
            menuOnSale.Id = menuOnSale.ori.Id;
            menuOnSale.OnSaleWeek = menuOnSale.ori.OnSaleWeek;
            menuOnSale.Price = menuOnSale.ori.Price;
            menuOnSale.MinPrice = menuOnSale.MinPrice;
        } else {
            menuOnSale.ori = {
                Id: menuOnSale.Id,
                OnSaleWeek: menuOnSale.OnSaleWeek,
                Price: menuOnSale.Price,
                MinPrice:menuOnSale.MinPrice
            }

        }
        menuOnSale.isAlt = !menuOnSale.isAlt;
    }
    $scope.searchbyid = function () {
        $http.post('/MenuOnSales/SearchById',{id:$scope.newMenuOnSale.id}).then(function (response) {
            $scope.idtoname = response.data;
            console.log($scope.newMenuOnSale.id)
            console.log($scope.idtoname)
        });
    }


    $scope.finishaltMenuOnSale = function (menuOnSale) {
        $http.post('/MenuOnSales/AltMenuOnSales', menuOnSale).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }
    

    $scope.delete = function (menuOnSale) {
        swal({
            title: "确定删除此项?",
            text: "你将不可恢复此项，请谨慎操作!",
            type: "warning",
            showCancelButton: true,
            
            confirmButtonText: "是, 删除!",
            cancelButtonText: "否, 保留!",
            closeOnConfirm: false
        }, function () {
            $http.post('/MenuOnSales/DelMenuOnSales', menuOnSale).then(function (response) {
                if (response.data.succeeded) {
                    refresh();
                    swal("删除成功!", "此项已经删除请重新进入页面.", "success");
                }
                else {
                    alert('!!!');
                    swal("删除失败!", "此项已经删除请重新进入页面.", "error");
                }

            });
        });

    }


    refresh();
})

.controller('DailySalesCtrl', function ($scope, $rootScope, $http) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "日营业数据统计";

    $scope.search = function () {
        $http.post('/DailySales/GetDailySales',
           {
               begintime: $scope.begintime,
               endtime: $scope.endtime
           }).then(function (response) {
            
            if (response.data.succeeded == false) {
                alert('查无记录！！！');
            }
            else {
                $scope.dailysales = response.data.aaa;
                $scope.totalprice = response.data.totalprice;
                $scope.totaloriprice = response.data.totaloriprice;
                $scope.totaldiscount = response.data.totaldiscount;
                $scope.totalheadcount = response.data.totalheadcount;
                $scope.totalcpi = response.data.totalcpi;
            }
        });
    }
    
})

.controller('SoldOutCtrl', function ($scope, $rootScope, $http) {

    $rootScope.FatherPage = "店小二营业"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "沽清";
    $scope.switch = function (soldOut) {
        $http.post('/SoldOut/SwitchSoldOut', {
            Id: soldOut.Id
        }).then(function (response) {
            if (response.data.succeeded) {
                refresh();
            }
            else {
                alert('!!!');
            }
        });
    }

    function refresh() {
        $http.post('/SoldOut/GetSoldOut').then(function (response) {
            $scope.soldOut = response.data;
        });
    }
    

    refresh();
})

.controller('PayDetailsCtrl', function ($scope, $rootScope, $http) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/Manage"; $rootScope.ChildPage = "支付明细";
    function getpaykindname() {
        $http.post('/PayDetails/GetPayKindName').then(function (response) {
            $scope.paykindnames = response.data;
            $scope.paykindnames.forEach(function (x) { x.IsChoose = true; })
            console.log($scope.paykindnames);
            
        });
    }
    getpaykindname();
    

    $scope.search = function () {
        
        var temp = $scope.paykindnames.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id })
        console.log(temp);
        $http.post('/PayDetails/GetPayDetails', {
            begintime: $scope.begintime,
            endtime: $scope.endtime,
            payKindIds: temp

        }).then(function (response) {
            if (response.data.succeeded==false) {
                alert('查无记录！！！');
            }
            else {
               
                $scope.paydetails = response.data.aaa;
                $scope.oriprice = response.data.oriprice;
                $scope.price = response.data.price;

                for (var i in $scope.paydetails) {
                    var clerkId = $scope.paydetails[i].Dine.ClerkId;
                    for (var j in response.data.bbb) {
                        if (response.data.bbb[j].Id == clerkId) {
                            $scope.paydetails[i].Dine.ClerkName = response.data.bbb[j].Name;
                            break;
                        }
                    }
                }
                console.log($scope.paydetails);
            }
           
        });
        //$http.post('/PayDetails/GetClerkName', { clerkid: paydetail.Dine.ClerkId }).then(function (response) {

        //    $scope.clerkids = response.data;


        //});
    }
    //$scope.AllSelect = function () {
    //     PayDetails.AllSelect();
    //}
    //$scope.NoneSelect = function () {
    //    PayDetails.NoneSelect();
    //}
    //$scope.ReverseSelect = function () {
    //    PayDetails.ReverseSelect();
    //}

})