angular.module('CeryFactory', ['ngCookies'])
.factory('UserLogin', ['$cookies', '$http', '$q', '$interval', function ($cookies, $http, $q, $interval) {
    //用户登录
    var service = {
        User: {},
        Hotel: {
            Name: "",
            Phone: "",
            PassWord: ""
        },
        KeyInfo:{
            Time: 60,
            Key: null,
            IsReget: true,
            IsFirst:true
        },
        GetCookies: function () {
            //获取cookies
            this.User.isSave = ($cookies.get('isSave') === "true");
            if (this.User.isSave) {
                this.User.username = $cookies.get('username');
                this.User.password = $cookies.get('password');
            }
        },
        SetCookies: function () {
            //设置cookies
            var expireDate = new Date();
            expireDate.setDate(expireDate.getDate() + 30);
            $cookies.put('isSave', this.User.isSave, { 'expires': expireDate });
            if (this.User.isSave) {
                $cookies.put('username', this.User.username, { 'expires': expireDate });
                $cookies.put('password', this.User.password, { 'expires': expireDate });
            }
        },
        Verification: function () {
            //登陆验证
            var deferred = $q.defer();//声明承诺
            $(".fakeloader").fakeLoader({
                timeToHide: 5000,
                bgColor: "#34495e",
                spinner: "spinner3"
            });
            $http.post('../Login/Verification', { User: this.User }).success(function (data) {
                deferred.resolve(data);//请求成功
            }).error(function (data) {
                deferred.reject(data); //请求失败
            });
            return deferred.promise;   // 返回承诺，这里返回的不是数据，而是API
        },
        GetKey: function(){
            var _this = this;
            $http.post('../Login/GetKey', {
                Phone: _this.Hotel.Phone
            }).success(function (data) {
                _this.KeyInfo.IsFirst = false;
                _this.KeyInfo.IsReget = false;
                if (data.Status) {
                    var timer = null;
                    timer = $interval(function () {
                        _this.KeyInfo.Time--;
                        if (_this.KeyInfo.Time == 0) {
                            _this.KeyInfo.Time = 60;
                            _this.KeyInfo.IsReget = true;
                            $interval.cancel(timer);
                        }
                    }, 1000);
                } else {
                    alert(data.ErrorMessage);
                }
            }).error(function (data) {
                console.log(data);
            })
        },
        Register: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Login/Register', {
                Name: _this.Hotel.Name,
                Phone: _this.Hotel.Phone,
                Password:_this.Hotel.PassWord,
                Key:_this.KeyInfo.Key
            }).success(function (data) {
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
                console.log(data);
            })
            return deferred.promise;
        }
    }
    return service;
}])
.factory('Order', ['$http', '$filter', '$q', function ($http, $filter, $q) {
    //订单管理
    var service = {
        SysElements: {
            Areas: [],
            Desks: []
        },
        getElements: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Templates/getArea').success(function (data) {
                _this.SysElements.Areas = data.Areas;
                _this.SysElements.Desks = data.Desks;
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        unpaid: function (area) {
            var temp = $filter('filter')(this.SysElements.Desks, { AreaId: area.Id }).filter(function (x) { return x.Status == 1 });
            return temp.length;//获取未处理的桌台数量
        },
        unpaidAll: function () {
            var temp = this.SysElements.Desks.filter(function (x) { return x.Status; });
            return temp.length;
        }
    }
    return service;
}])
.factory('Pay', ['$http', '$q', '$filter', '$rootScope', function ($http, $q, $filter, $rootScope) {
    var temp = 0;
    var last_gritter;
    var service = {
        PayElements: {
            Desk: {},
            PayMethods: [],
            UnpaidDines: [],
            Discounts: [{ Name: "自定义", Discount: 1, IsSet: true }],
            OnSaleMenus: [],
            CurrentUser: {},
            CurrentDiscount: {},
            CurrentDine: {}
        },
        PayClean: function () {
            for (var i = 0; i < this.PayElements.PayMethods.length; i++) {
                delete this.PayElements.PayMethods[i].Number;
            }
        },
        getElements: function () {
            //获取打折信息和订单初始化
            var _this = this;
            var date = new Date();
            var deferred = $q.defer();
            $http.post('../Templates/getPay').success(function (data) {
                _this.PayElements.UnpaidDines = data.Dines;
                _this.PayElements.Discounts = [{ Name: "自定义", Discount: 1, IsSet: true }];
                for (var i = 0; i < _this.PayElements.UnpaidDines.length; i++) {
                    _this.PayElements.UnpaidDines[i].Discount *= 100;
                }
                _this.PayElements.PayMethods = data.Pays;
                for (var i = 0; i < data.Discounts.length; i++) {
                    if (data.Discounts[i].Week == date.getDay()) _this.PayElements.Discounts.push(data.Discounts[i]);
                }
                for (var i = 0; i < _this.PayElements.Discounts.length; i++) {
                    if (_this.PayElements.Discounts[i].Discount > 1) _this.PayElements.Discounts[i].Discount = 100;
                    else if (_this.PayElements.Discounts[i].Discount < 0) _this.PayElements.Discounts[i].Discount = 100;
                    else {
                        _this.PayElements.Discounts[i].Discount *= 100;
                    }
                }
                _this.PayElements.OnSaleMenus = data.OnSaleMenus;
                _this.AddOnSale();
                temp = _this.PayElements.Discounts.length;
                _this.PayElements.CurrentDiscount = _this.PayElements.Discounts[0];
                _this.PayElements.CurrentDine = {};
                _this.PayElements.CurrentUser = {};
                _this.getNowDine();
                _this.getUser();
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        SetDine: function (dine) {
            //设置当前点单
            if (dine == this.PayElements.CurrentDine) return;
            this.PayElements.CurrentDine = dine;
            //当前用户注销  打折方案为自定义  获取订单用户登录
            this.PayElements.CurrentUser.Id = this.PayElements.CurrentDine.UserId;
            this.getUser();
            this.PayElements.Discounts.splice(temp, (this.PayElements.Discounts.length - temp));
            //输入框清空 自定义 折扣率设为 不打折 重新计算价钱 输入框自动聚焦
            this.Login();
            this.PayClean();
            this.DiscountClean();
            if (this.PayElements.CurrentDine.Discount == 100) { this.reCalc(); }
            this.AutoFocus();
        },
        CalcPrice: function () {
            //计算价格
            var _this = this;
            this.PayElements.CurrentDine.Price = this.PayElements.CurrentDine.DineMenus.filter(function (x) { return x.Status == 0 }).map(function (x) {
                if (x.Status == 2) return 0;
                if (x.IsOnSale) return (x.OnSalePrice * x.Count + x.RemarkPrice)
                if (x.Menu.MenuPrice.ExcludePayDiscount) return (x.OriPrice * x.Count + x.RemarkPrice)
                else return (x.OriPrice * x.Count * _this.PayElements.CurrentDiscount.Discount / 100 + x.RemarkPrice)
            }).reduce(function (a, b) { return +a + +b }, 0);
            angular.forEach(this.PayElements.CurrentDine.DineMenus, function (x) {
                if (x.Status == 2) x.Price = 0;
                if (x.IsOnSale) {
                    //特价
                    x.Price = x.OnSalePrice;
                } else {
                    if (x.Menu.MenuPrice.ExcludePayDiscount) {
                        //不打折
                    } else {
                        x.Price = x.OriPrice * _this.PayElements.CurrentDiscount.Discount / 100;
                    }
                }
            })
        },
        GetMenuPrice: function (menu) {
            if (this.PayElements.CurrentDiscount.IsSet) {
                //如果是自定义按自定义来
                if (this.PayElements.CurrentDiscount.Discount > 0 && this.PayElements.CurrentDiscount.Discount < 100) {
                    if (menu.IsOnSale) return menu.OnSalePrice * menu.Count;
                    return menu.Menu.MenuPrice.ExcludePayDiscount ? menu.OriPrice * menu.Count : menu.OriPrice * menu.Count * this.PayElements.CurrentDiscount.Discount / 100;
                } else {
                    return menu.OriPrice * menu.Count
                }

            } else {
                //非自定义 按折扣优惠大的取
                if (this.PayElements.CurrentDiscount.Discount <= this.PayElements.CurrentDine.Discount) {
                    if (this.PayElements.CurrentDiscount.Discount > 0 && this.PayElements.CurrentDiscount.Discount < 100) {
                        if (menu.IsOnSale) return menu.OnSalePrice * menu.Count;
                        return menu.Menu.MenuPrice.ExcludePayDiscount ? menu.OriPrice * menu.Count : menu.OriPrice * menu.Count * this.PayElements.CurrentDiscount.Discount / 100;
                    }
                } else {
                    return menu.Menu.MenuPrice.ExcludePayDiscount ? menu.OriPrice * menu.Count : menu.OriPrice * menu.Count * this.PayElements.CurrentDine.Discount / 100;
                }
            }
        },
        AddOnSale: function () {
            var _this = this;
            this.PayElements.UnpaidDines.forEach(function (x) {
                x.DineMenus.forEach(function (xx) {
                    _this.PayElements.OnSaleMenus.forEach(function (xxx) {
                        if (xx.MenuId == xxx.Id) {
                            xx.IsOnSale = true;
                            xx.OnSalePrice = xxx.Price;
                        }
                    })
                })
            })
        },
        reCalc: function () {
            //重新计算价钱
            if (this.PayElements.CurrentDiscount.Discount > 0 && this.PayElements.CurrentDiscount.Discount < 100) {
                this.PayElements.CurrentDine.DiscountName = this.PayElements.CurrentDiscount.Name;
                this.PayElements.CurrentDine.Discount = this.PayElements.CurrentDiscount.Discount;
            } else {
                this.PayElements.CurrentDiscount.Discount = 100
            }
            this.CalcPrice();
        },
        getNowDine: function () {//灵魂函数 获取当前的桌台订单和用户信息
            if (this.DineCount(this.PayElements.Desk.Id) == 0) { this.PayElements.CurrentDine = { Id: '当前桌台没有点单' }; this.PayElements.CurrentUser = { Id: '当前没有点单' }; }
            else {
                if (this.PayElements.CurrentDine.Id == '当前桌台没有点单') {//当前是空单，默认是找到
                    this.PayElements.CurrentDine = $filter('filter')(this.PayElements.UnpaidDines, { DeskId: this.PayElements.Desk.Id })[0];
                    this.PayElements.CurrentUser.Id = this.PayElements.CurrentDine.UserId;
                    this.Login();
                } else if (this.PayElements.CurrentDine.DeskId != this.PayElements.Desk.Id) {//换桌之后默认自动匹配
                    this.PayElements.CurrentDine = $filter('filter')(this.PayElements.UnpaidDines, { DeskId: this.PayElements.Desk.Id })[0];
                    this.PayElements.Discounts.splice(temp, (this.PayElements.Discounts.length - temp));
                    this.DiscountClean();
                    this.PayElements.CurrentUser.Id = this.PayElements.CurrentDine.UserId;
                    this.Login();
                }
            }
        },
        getUser: function () {
            //获取登录用户
            if (this.PayElements.CurrentUser.Id == null) {
                this.PayElements.CurrentUser.Id = '匿名用户';
            }
            if (this.PayElements.CurrentDine.Id == '当前桌台没有点单') {
                //如果没有订单
                this.PayElements.CurrentUser.Id = '当前没有点单'
            }
        },
        DineCount: function (DeskId) {
            //统计当前桌台未支付订单数
            return $filter('filter')(this.PayElements.UnpaidDines, { DeskId: DeskId }).length;
        },
        Change: function (kind) {
            //输入金额发生变化是判断不能为负
            if (kind.Type == 2) {
                if (kind.Number > parseInt(this.PayElements.CurrentUser.Points / $rootScope.Rate)) {
                    kind.Number = parseInt(this.PayElements.CurrentUser.Points / $rootScope.Rate)
                } else if (kind.Number < 0) {
                    kind.Number = 0;
                }
            }
            else if (kind.Type != 4) {
                if (kind.Number > this.PriceAll()) {
                    kind.Number = this.PriceAll();
                }
                if (kind.Number < 0) kind.Number = 0;
            }
            else {
                if (kind.Number < 0) kind.Number = 0;
            }
        },
        TotalPay: function () {
            //已支付金额
            if (this.PayElements.CurrentDine.Id != '当前桌台没有点单') {
                //有订单
                return this.PayElements.PayMethods.filter(function (x) { return x.Number }).map(function (x) { return x.Number }).reduce(function (a, b) { return +a + +b; }, 0)//获取输入框总价
            } else {
                //没有订单
                return 0.00;
            }
        },
        Charge: function () {
            //找零金额 已付金额 - 订单金额
            if (this.PayElements.CurrentDine.Id != '当前桌台没有点单') {
                //有订单
                if ($rootScope.PayUnder == 0) {
                    //如果不需要抹零
                    return (this.TotalPay() - this.PayElements.CurrentDine.Price).toFixed(2) >= 0 ? (this.TotalPay() - this.PayElements.CurrentDine.Price).toFixed(2) : 0;
                } else if ($rootScope.PayUnder == 1) {
                    //如果需要抹零
                    return (this.TotalPay() - parseInt(this.PayElements.CurrentDine.Price)) >= 0 ? (this.TotalPay() - parseInt(this.PayElements.CurrentDine.Price)) : 0;
                } else if ($rootScope.PayUnder == 2) {
                    //需要四舍五入的
                    return (this.TotalPay() - this.PayElements.CurrentDine.Price.toFixed(0)) >= 0 ? (this.TotalPay() - this.PayElements.CurrentDine.Price.toFixed(0)) : 0;
                }
            } else {
                //没有订单
                return 0;
            }
        },
        Unpaid: function () {
            //未支付金额  订单金额 - 已付金额
            if (this.PayElements.CurrentDine.Id != '当前桌台没有点单') {
                //有订单
                if (!this.PayElements.CurrentDine.Price) return 0;
                if ($rootScope.PayUnder == 0) {
                    //如果不需要抹零
                    return ((this.PayElements.CurrentDine.Price).toFixed(2) - this.TotalPay()) > 0 ? ((this.PayElements.CurrentDine.Price).toFixed(2) - this.TotalPay()) : 0;
                } else if ($rootScope.PayUnder == 1) {
                    //如果需要抹零
                    return (parseInt(this.PayElements.CurrentDine.Price) - this.TotalPay()) > 0 ? (parseInt(this.PayElements.CurrentDine.Price) - this.TotalPay()) : 0;
                } else if ($rootScope.PayUnder == 2) {
                    //需要四舍五入的
                    return (this.PayElements.CurrentDine.Price.toFixed(0) - this.TotalPay()) ? (this.PayElements.CurrentDine.Price.toFixed(0) - this.TotalPay()) : 0;
                }
            } else {
                //没有订单
                return 0;
            }
        },
        PriceAll: function () {
            //返回当前点单价格
            return this.PayElements.CurrentDine.Price ? this.PayElements.CurrentDine.Price : 0;
        },
        OriPriceAll: function () {
            return this.PayElements.CurrentDine.OriPrice ? this.PayElements.CurrentDine.OriPrice : 0;
        },
        Pay: function () {
            var deferred = $q.defer();
            var _this = this;
            var PayList = _this.PayElements.PayMethods.filter(function (x) { return x.Number }).map(function (x) { return { Id: x.Id, Type: x.Type, Number: x.Number } });
            var DineInfo = { Id: _this.PayElements.CurrentDine.Id, Discount: _this.PayElements.CurrentDine.Discount, DiscountName: _this.PayElements.CurrentDine.DiscountName };
            $http.post('../Templates/PayDine', { PayList: PayList, Dine: DineInfo, UserId: _this.PayElements.CurrentUser.Id }).success(function (data) {
                if (data.Status) {
                    _this.PayElements.UnpaidDines = data.Dines;
                    for (var i = 0; i < _this.PayElements.UnpaidDines.length; i++) {
                        _this.PayElements.UnpaidDines[i].Discount *= 100;
                    }
                    swal("支付已完成!", "原价:" + $filter('currency')(_this.OriPriceAll(), "￥", 0) + ",已付:" + $filter('currency')(_this.TotalPay(), "￥", 0) + ",找零:" + $filter('currency')(_this.Charge(), "￥", 0) + ",优惠" + $filter('currency')(_this.OriPriceAll() - _this.PriceAll(), "￥", 0), "success")
                    //注销当前订单 和当前用户
                    _this.PayElements.CurrentDine = {};
                    _this.PayElements.CurrentUser = {};
                    _this.getNowDine();
                    _this.getUser();
                    _this.PayClean();
                    _this.DiscountClean();
                    if (_this.PayElements.CurrentDine.Discount == 100) { _this.reCalc(); }
                    _this.AutoFocus();

                } else {
                    alert(data.ErrorMessage);
                }
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        DiscountClean: function () {
            this.PayElements.CurrentDiscount = this.PayElements.Discounts[0];
            this.PayElements.CurrentDiscount.Discount = 100;
        },
        Login: function () {
            var _this = this;
            $http.post('../Templates/getUser', { CustomerId: _this.PayElements.CurrentUser.Id, Phone: _this.PayElements.CurrentUser.PhoneNumber, Password: _this.PayElements.CurrentUser.Password }).success(function (data) {
                if (data.Status) {
                    _this.PayElements.CurrentUser = data.data;
                    _this.PayElements.CurrentUser.IsLogin = true;
                    if (_this.PayElements.CurrentUser.VipLevel) _this.PayElements.Discounts.push(_this.PayElements.CurrentUser.VipLevel.VipDiscount);
                    if (_this.PayElements.Discounts.length > temp) {
                        _this.PayElements.CurrentDiscount = _this.PayElements.Discounts[_this.PayElements.Discounts.length - 1];
                        _this.PayElements.CurrentDiscount.Discount *= 100;
                    }
                    _this.PayElements.CurrentDine.UserId = _this.PayElements.CurrentUser.Id;
                    _this.reCalc();
                } else {
                    if (_this.PayElements.CurrentUser.PhoneNumber) alert('账号密码错误');
                }
            }).error(function (data) {
                console.log(data);
            })
        },
        AutoFocus: function () {
            //输入框自动聚焦
            $('.PayInput').eq(0).focus();
        },
        AllowPay: function () {
            if (this.PayElements.CurrentDine.Id != '当前桌台没有点单') {
                if (this.Unpaid() == 0) { return true; }
                else { return false; }
            }
            else {
                return false;
            }
        },
        AllowPrint: function () {
            if (this.PayElements.CurrentDine.Id != '当前桌台没有点单') {
                return true;
            }
            return false;
        },
        ChangeDesk: function () {
            this.getNowDine();
            this.getUser();
            this.reCalc();
        },
        Recipt: function () {
            var _this = this;
            $http.post('../Templates/RePrint',{
                DineId:_this.PayElements.CurrentDine.Id,
                Type:0
            } ).success(function (data) {
                $.gritter.add({
                    title: '提醒',
                    text: '收银已打印！',
                    sticky: false,
                    time: 1000,
                    speed: 500,
                    position: 'top-right',
                    class_name: 'gritter-success'//gritter-center   
                });
            })
        },
        Kitchen: function () {
            var _this = this;
            $http.post('../Templates/RePrint',{
                DineId:_this.PayElements.CurrentDine.Id,
                Type:0
            } ).success(function (data) {
                $.gritter.add({
                    title: '提醒',
                    text: '厨房已打印！',
                    sticky: false,
                    time: 1000,
                    speed: 500,
                    position: 'top-right',
                    class_name: 'gritter-success'//gritter-center   
                });
            })
        }
    }
    return service;
}])
.factory('AddMenu', ['$http', '$q', '$filter', '$rootScope', function ($http, $q, $filter, $rootScope) {
    var service = {
        MenuElement: {
            UnpaidDines: [],
            Menus: [],
            CurrentDine: {},
            OrderMenus: [],
            FilterDesk:""
        },
        Initialize: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Templates/GetAddMenuEle').success(function (data) {
                _this.MenuElement.UnpaidDines = data.UnpaidDines;
                _this.MenuElement.Menus = data.Menus;
                deferred.resolve(data);
            })
            return deferred.promise;
        }
    }
    return service;
}])
.factory('Open', ['$http', '$q', '$filter', '$rootScope', '$cookies', function ($http, $q, $filter, $rootScope, $cookies) {
    var temp = 0;
    var service = {
        OpenElements: {
            Type: $cookies.get('Type')||0,
            Discounts: [{ Name: "自定义", Discount: 1, IsSet: true }],
            Classes: [],
            OriMenus:[],
            Menus: [],
            CurrentUser: { Number: 1 },
            CurrentDesk: {},
            CurrentDiscount: {},
            OrderMenus: [],
            CurrentMenu: {},
            CurrentMenuRemarks: [],
            CurrentDine:{},
            FilterInfo: "",
            CurrentFilter: "",
            allchoose:true
        },
        getElements: function () {
            var _this = this;
            var date = new Date();
            var deferred = $q.defer();
            $http.post('../Templates/GetOpenElements').success(function (data) {
                _this.OpenElements.Discounts = [{ Name: "自定义", Discount: 1, IsSet: true }];
                angular.forEach(data.Menus, function (x) {
                    x.Num = x.MinOrderCount;
                });
                _this.OpenElements.Menus = data.Menus;
                _this.OpenElements.OriMenus = data.Menus;
                _this.OpenElements.Classes = data.Classes;
                for (var i = 0; i < data.Discounts.length; i++) {
                    if (data.Discounts[i].Week == date.getDay()) _this.OpenElements.Discounts.push(data.Discounts[i]);
                }

                for (var i = 0; i < _this.OpenElements.Discounts.length; i++) {
                    if (_this.OpenElements.Discounts[i].Discount > 1) _this.OpenElements.Discounts[i].Discount = 100;
                    else if (_this.OpenElements.Discounts[i].Discount < 0) _this.OpenElements.Discounts[i].Discount = 100;
                    else {
                        _this.OpenElements.Discounts[i].Discount *= 100;
                    }
                }
                _this.OpenElements.CurrentDiscount = _this.OpenElements.Discounts[0];
                temp = _this.OpenElements.Discounts.length;
                _this.OpenElements.OrderMenus = [];
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        Login: function () {
            var _this = this;
            $http.post('../Templates/OpenLogin', { username: _this.OpenElements.CurrentUser.UserName, password: _this.OpenElements.CurrentUser.Password }).success(function (data) {
                if (data.Status) {
                    //登陆成功获取打折信息
                    _this.OpenElements.CurrentUser.isLogin = true;
                    _this.OpenElements.CurrentUser.Id = data.data.Id;
                    if (data.data.VipLevel.VipDiscount) {
                        //避免错误信息
                        _this.OpenElements.Discounts.push(data.data.VipLevel.VipDiscount);
                        _this.OpenElements.CurrentDiscount = _this.OpenElements.Discounts[_this.OpenElements.Discounts.length - 1];
                        _this.OpenElements.CurrentDiscount.Discount *= 100;
                    }
                } else {
                    alert('用户名密码错误');
                }
            }).error(function (data) {
                console.log(data);
            })
        },
        LogOut: function () {
            delete this.OpenElements.CurrentUser.Id;
            delete this.OpenElements.CurrentUser.isLogin;
            delete this.OpenElements.CurrentUser.UserName;
            delete this.OpenElements.CurrentUser.Password;
            this.OpenElements.CurrentDiscount = this.OpenElements.Discounts[0];
            this.OpenElements.CurrentDiscount.Discount = 100;
            this.OpenElements.Discounts.splice(temp, (this.OpenElements.Discounts.length - temp));
        },
        AllowLogin: function () {
            if (!this.OpenElements.CurrentUser.UserName) { return true; }
            if (!this.OpenElements.CurrentUser.Password) { return true; }
            return false;
        },
        AllowOpen: function () {
            return this.OpenElements.OrderMenus.length == 0;
        },
        NumChange: function () {
            if (this.OpenElements.CurrentMenu.Num < this.OpenElements.CurrentMenu.MinOrderCount) this.OpenElements.CurrentMenu.Num = this.OpenElements.CurrentMenu.MinOrderCount
        },
        SelectChange: function (menu) {
            this.OpenElements.CurrentMenu = menu;
            this.OpenElements.FilterInfo = menu.Name;
            this.OpenElements.CurrentMenu.Num = this.OpenElements.CurrentMenu.MinOrderCount;
        },
        AddMenu: function () {
            var _this = this;
            var temp = angular.copy(this.OpenElements.CurrentMenu);
            temp.Remarks = this.OpenElements.CurrentMenuRemarks;
            if (temp.Id) this.OpenElements.OrderMenus.push(temp);
            this.OpenElements.OriMenus.forEach(function (x) {
                if (x.Id == _this.OpenElements.CurrentMenu.Id) x.Class = true;
            });
            this.OpenElements.Menus.forEach(function (x) {
                if (x.Id == _this.OpenElements.CurrentMenu.Id) x.Class = true;
            })
            this.OpenElements.CurrentMenu = {};
            this.OpenElements.CurrentMenuRemarks = [];
            this.OpenElements.FilterInfo = "";
            this.OpenElements.CurrentFilter = "";
        },
        SendMenu: function () {
            var _this = this;
            var temp = angular.copy(this.OpenElements.CurrentMenu);
            temp.Remarks = this.OpenElements.CurrentMenuRemarks;
            if (temp.Id) {
                temp.IsSend = true;
                this.OpenElements.OrderMenus.push(temp);
                this.OpenElements.OriMenus.forEach(function (x) {
                    if (x.Id == _this.OpenElements.CurrentMenu.Id) x.Class = true;
                });
                this.OpenElements.Menus.forEach(function (x) {
                    if (x.Id == _this.OpenElements.CurrentMenu.Id) x.Class = true;
                })
            }
            this.OpenElements.CurrentMenu = {};
            this.OpenElements.CurrentMenuRemarks = [];
            this.OpenElements.FilterInfo = "";
            this.OpenElements.CurrentFilter = "";
        },
        CalcMenuPrice: function (menu) {
            if (menu.IsSend) {
                //赠送
                return 0;
            }
            else {
                //正常
                if (menu.MenuPrice.ExcludePayDiscount) {
                    //包含在整单打折之外 不打折
                    if ($rootScope.PayUnder == 0) {
                        //如果不需要抹零
                        return (menu.MenuPrice.Price * menu.Num).toFixed(2);
                    } else if ($rootScope.PayUnder == 1) {
                        //如果需要抹零
                        return parseInt(menu.MenuPrice.Price * menu.Num)
                    } else if ($rootScope.PayUnder == 2) {
                        //需要四舍五入的
                        return (menu.MenuPrice.Price * menu.Num).toFixed(0);
                    }
                    //return menu.MenuPrice.Price * menu.Num;
                } else {
                    //打折
                    if ($rootScope.PayUnder == 0) {
                        //如果不需要抹零
                        return (menu.MenuPrice.Price * this.OpenElements.CurrentDiscount.Discount / 100 * menu.Num).toFixed(2);
                    } else if ($rootScope.PayUnder == 1) {
                        //如果需要抹零
                        return parseInt(menu.MenuPrice.Price * this.OpenElements.CurrentDiscount.Discount / 100) * menu.Num;
                    } else if ($rootScope.PayUnder == 2) {
                        //需要四舍五入的
                        return (menu.MenuPrice.Price * this.OpenElements.CurrentDiscount.Discount / 100 * menu.Num).toFixed(0);
                    }
                }
            }
        },
        RemoveMenu: function (index) {
            var Id = this.OpenElements.OrderMenus[index].Id;
            this.OpenElements.OrderMenus.splice(index, 1);
            if (this.OpenElements.OrderMenus.filter(function (x) { return x.Id == Id }).length == 0) {
                this.OpenElements.OriMenus.forEach(function (x) {
                    if (x.Id == Id) x.Class = false;
                });
                this.OpenElements.Menus.forEach(function (x) {
                    if (x.Id == Id) x.Class = false;
                })
            }
        },
        FormatDiscount: function () {
            if (this.OpenElements.CurrentDiscount.Discount <= 0 || this.OpenElements.CurrentDiscount.Discount > 100)
                this.OpenElements.CurrentDiscount.Discount = 100;
        },
        AccountPrice: function () {
            var _this = this;
            if (this.OpenElements.OrderMenus.length == 0) { return 0; }
            else {
                //有点菜
                var priceAll = this.OpenElements.OrderMenus.map(function (x) {
                    if (x.IsSend) return 0;
                    else return x.MenuPrice.ExcludePayDiscount ? x.MenuPrice.Price * x.Num + x.Remarks.filter(function (r) { return r.Price }).map(function (r) { return r.Price }).reduce(function (a, b) { return +a + +b }, 0)
                    : _this.OpenElements.CurrentDiscount.Discount / 100 * x.MenuPrice.Price * x.Num + x.Remarks.filter(function (r) { return r.Price }).map(function (r) { return r.Price }).reduce(function (a, b) { return +a + +b }, 0);
                }).reduce(function (a, b) { return +a + +b }, 0);
                if ($rootScope.PayUnder == 0) {
                    //如果不需要抹零
                    return priceAll.toFixed(2);
                } else if ($rootScope.PayUnder == 1) {
                    //如果需要抹零
                    return parseInt(priceAll);
                } else if ($rootScope.PayUnder == 2) {
                    //需要四舍五入的
                    return priceAll.toFixed(0);
                }
            }
        },
        AddDineMenu:function(){
            var _this = this;
            var deferred = $q.defer();
            var temp = {
                DineId: this.OpenElements.CurrentDine.Id,
                Menus: this.OpenElements.OrderMenus.map(function (x) {
                    return { Id: x.Id, Num: x.Num, Remarks: x.Remarks.map(function (x) { return x.Id }), IsSend: !!x.IsSend }
                })
            }
            $http.post('../Templates/AddDineMenu', {
                Menus:temp
            }).success(function (data) {
                if (data.Status) {
                    if (data.OriPrice) _this.OpenElements.CurrentDine.OriPrice = data.OriPrice;
                    if (data.Price) _this.OpenElements.CurrentDine.Price = data.Price;
                }
              
                deferred.resolve(data);
            })
            return deferred.promise;
        },
        OpenDesk: function () {
            $(".fakeloader").fakeLoader({
                timeToHide: 3000,
                bgColor: "#34495e",
                spinner: "spinner3"
            });
            var _this = this;
            var deferred = $q.defer();
            var tempOrderedMenus = this.OpenElements.OrderMenus.filter(function (x) { return !x.IsSend }).map(function (x) {
                return { Id: x.Id, Ordered: x.Num, Remarks: x.Remarks.map(function (x) { return x.Id }) }
            })
            var tempSendMenus = this.OpenElements.OrderMenus.filter(function (x) { return x.IsSend }).map(function (x) {
                return { Id: x.Id, Ordered: x.Num, Remarks: x.Remarks.map(function (x) { return x.Id }) }
            })
            var OrderInfo = {
                HeadCount: this.OpenElements.CurrentUser.Number,
                Price: this.AccountPrice(),
                Desk: this.OpenElements.CurrentDesk,
                OrderedMenus: tempOrderedMenus,
                SendMenus: tempSendMenus
            };
            $http.post('../Templates/OpenDesk', { OrderInfo: OrderInfo, UserId: _this.OpenElements.CurrentUser.Id, OpenDiscount: _this.OpenElements.CurrentDiscount }).success(function (data) {
                data = JSON.parse(data);
                _this.OpenElements.OrderMenus = [];
                deferred.resolve(data);
            }).error(function (data) {
                data = JSON.parse(data);
                deferred.reject(data);
            })
            return deferred.promise;
        },
        Up: function () {
            if (this.OpenElements.CurrentMenu.Num) this.OpenElements.CurrentMenu.Num++;
        },
        Down: function () {
            if (this.OpenElements.CurrentMenu.Num > this.OpenElements.CurrentMenu.MinOrderCount) this.OpenElements.CurrentMenu.Num--;
        },
        MenuFilter: function (ClassId) {
            this.OpenElements.Menus = this.OpenElements.OriMenus.filter(function (x) {
                var flag = false;
                x.Classes.forEach(function (xx) {
                    if (xx.Id == ClassId) { flag = true; return;}
                })
                return flag;
            })
            this.OpenElements.allchoose = false;
            this.OpenElements.Classes.forEach(function (x) {
                x.Current = false;
                if (x.Id == ClassId) { x.Current = true;}
            })
        },
        ShiftType: function () {
            var _this = this;
            if (this.OpenElements.Type == 0) {
                this.OpenElements.Type = 1;
            } else {
                this.OpenElements.Type = 0;
            }
        },
        KeepShift: function () {
            var _this = this;
            var expireDate = new Date();
            if (typeof($cookies.get('Type')) != undefined) {
                if ($cookies.get('Type') != this.OpenElements.Type) {
                    bootbox.confirm({
                        buttons: {
                            confirm: {
                                label: '确认'
                            },
                            cancel: {
                                label: '取消'
                            }
                        },
                        message: '是否保留当前模式',
                        callback: function (result) {
                            if (result) {
                                expireDate.setDate(expireDate.getDate() + 30);
                                $cookies.put('Type', _this.OpenElements.Type, { 'expires': expireDate });
                            }
                        },
                        //title: "bootbox confirm也可以添加标题哦",  
                    });
                }
            } else {
                $cookies.put('Type', _this.OpenElements.Type, { 'expires': expireDate });
            }
        },
        Filter: function () {
            this.OpenElements.CurrentFilter = this.OpenElements.FilterInfo;
        },
        CleanFilter: function () {
            this.OpenElements.CurrentFilter = "";
            this.OpenElements.FilterInfo = "";
            this.OpenElements.CurrentMenu = {};
        },
        ChooseAll: function () {
            this.OpenElements.Classes.forEach(function (x) {
                x.Current = false;
            });
            this.OpenElements.allchoose = true;
        }
    }
    return service;
}])
.factory('Returned', ['$http', '$q', '$filter', '$rootScope', function ($http, $q, $filter, $rootScope) {
    var service = {
        ReturnElements: {
            Desks: [],
            CurrentDesk: {},
            UnpaidDines: [],
            CurrentDine: {},
            CurrentReason: {},
            isAjax:false,
            Reasons:[]
        },
        getElements: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Templates/getReturn').success(function (data) {
                if (data.Desks.length > 0) {
                    _this.ReturnElements.Desks = data.Desks.filter(function (x) { return x.Status });
                }
                _this.ReturnElements.UnpaidDines = data.Dines;
                _this.ReturnElements.Reasons = data.Reasons;
                _this.getFirstDesk();
                _this.getFirstDine();
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        getFirstDesk: function () {
            if (this.ReturnElements.Desks.length > 0) {
                this.ReturnElements.CurrentDesk = this.ReturnElements.Desks[0];
            }
        },
        getFirstDine: function () {
            var _this = this;
            if (this.ReturnElements.Desks.length == 0) {
                this.ReturnElements.CurrentDine = {}
            } else {
                this.ReturnElements.CurrentDine = $filter('filter')(this.ReturnElements.UnpaidDines, { DeskId: this.ReturnElements.CurrentDesk.Id })[0];
            }
        },
        ChangeDesk: function () {
            this.getFirstDine();
        },
        SetDine: function (dine) {
            if (this.ReturnElements.CurrentDine = dine) return;
            this.ReturnElements.CurrentDine = dine;
        },
        DeleteMenu: function (menu) {
            var _this = this;
            bootbox.confirm({
                buttons: {
                    confirm: {
                        label: '确认'
                    },
                    cancel: {
                        label: '取消'
                    }
                },
                message: GetReason(_this.ReturnElements.Reasons),
                callback: function (result) {
                    var reason = $('.radio').find('input:checked').attr('data-description');
                    if (result) {
                        swal({
                           title: "确定删除此菜品?",
                           text: "你将不可恢复此项删除菜品，请谨慎操作!",
                           type: "warning",
                           showCancelButton: true,
                           confirmButtonColor: "#D0D0D0",
                           confirmButtonText: "是, 删除!",
                           cancelButtonText: "否, 保留!",
                           closeOnConfirm: false
                        }, function () {
                           $http.post('../Templates/ReturnMenu', {
                               DineId: _this.ReturnElements.CurrentDine.Id,
                               Id: menu.Id,
                               Reason: reason
                           }).success(function (data) {
                               if (data.Status) {
                                   _this.ReturnElements.Desks = data.Desks.filter(function (x) { return x.Status });
                                   _this.ReturnElements.UnpaidDines = data.Dines;
                                   _this.getFirstDesk();
                                   _this.getFirstDine();
                                   swal("删除成功!", "此项菜品已经删除请重新进入支付页面.", "success");
                               } else {
                                   console.log("数据错误");
                               }
                           }).error(function (data) {
                               console.log(data);
                           });
                       }
                       );
                    }
                },
                //title: "bootbox confirm也可以添加标题哦",  
            });
        },
        DeleteDine: function (dine) {
            var _this = this;
            swal(
            {
                title: "确定删除此订单?",
                text: "你将不可恢复此项删除订单，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: false
            }, function () {
                $http.post('../Templates/ReturnDine', { DineId: dine.Id }).success(function (data) {
                    _this.ReturnElements.UnpaidDines.forEach(function (x, index) {
                        if (x.Id == dine.Id) _this.ReturnElements.UnpaidDines.splice(index, 1);
                    })
                    _this.getFirstDine();
                }).error(function (data) {
                    console.log(data);
                })
                swal("删除成功!", "此项订单已经删除请重新进入支付页面.", "success");
            });
        },
        EditReason: function () {
            var _this = this;
            _this.ReturnElements.isAjax = true;
            $http.post('../Templates/EditReason', {
                Id: _this.ReturnElements.CurrentReason.Id,
                Reason: _this.ReturnElements.CurrentReason.Description
            }).success(function (data) {
                _this.ReturnElements.isAjax = false;
            }).error(function (data) {
                _this.ReturnElements.isAjax = false;
                console.log(data);
            })
        },
        AddReason: function () {
            var _this = this;
            if (!_this.ReturnElements.isAjax) {
                _this.ReturnElements.isAjax = true;
                $http.post('../Templates/AddReason', {
                    Reason: _this.ReturnElements.CurrentReason.Description
                }).success(function (data) {
                    _this.ReturnElements.isAjax = false;
                    _this.ReturnElements.CurrentReason.Id = data.Id;
                    _this.ReturnElements.Reasons.push(_this.ReturnElements.CurrentReason);
                }).error(function (data) {
                    _this.ReturnElements.isAjax = false;
                    console.log(data);
                })
            }
        },
        DeleteReason: function (reason) {
            var _this = this;
            swal({
                title: "确定删除此原因?",
                text: "你将不可恢复此项原因，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: true
            }, function () {
                if (!_this.ReturnElements.isAjax) {
                    _this.ReturnElements.isAjax = true;
                    $http.post('../Templates/DeleteReason', {
                        Id: reason.Id
                    }).success(function (data) {
                        _this.ReturnElements.isAjax = false;
                        _this.ReturnElements.Reasons.forEach(function (x, index) {
                            if (x.Id == reason.Id) _this.ReturnElements.Reasons.splice(index, 1);
                        })
                    }).error(function (data) {
                        _this.ReturnElements.isAjax = false;
                        console.log(data);
                    })
                }
            });
           
        }
    }
    return service;
}])
.factory('Conbine', ['$http', '$q', '$filter', '$rootScope', function ($http, $q, $filter, $rootScope) {
    var service = {
        ConbineElements: {
            CurrentDesk: {},
            UnpaidDesk: [],
            UnpaidDines: [],
            ConbineDines: [],
            SelectDines: [],
            SelectConbineDines: []
        },
        initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Templates/GetConbine').success(function (data) {
                _this.ConbineElements.UnpaidDesk = data.Desks;
                _this.ConbineElements.UnpaidDines = data.Dines;
                _this.ConbineElements.ConbineDines = [];
                _this.getFirstDesk();
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        getFirstDesk: function () {
            if (this.ConbineElements.UnpaidDesk.length > 0) {
                this.ConbineElements.CurrentDesk = this.ConbineElements.UnpaidDesk[0];
            }
        },
        MoveRight: function () {
            var _this = this;
            if (this.ConbineElements.SelectDines.length > 0) {
                this.ConbineElements.SelectDines.forEach(function (s) {
                    _this.ConbineElements.ConbineDines.push(angular.copy(s));
                    _this.ConbineElements.UnpaidDines.forEach(function (x, index) {
                        if (x.Id == s.Id) { _this.ConbineElements.UnpaidDines.splice(index, 1); }
                    })
                })
            }
            _this.ConbineElements.SelectDines = [];
        },
        MoveRightAll: function () {
            var _this = this;
            this.ConbineElements.UnpaidDines.filter(function (x) {
                return x.DeskId == _this.ConbineElements.CurrentDesk.Id;
            }).forEach(function (x) {
                _this.ConbineElements.ConbineDines.push(angular.copy(x));
                _this.ConbineElements.UnpaidDines.forEach(function (s, index) {
                    if (x.Id == s.Id) _this.ConbineElements.UnpaidDines.splice(index, 1);
                })
            })
            _this.ConbineElements.SelectDines = [];
        },
        MoveLeft: function () {
            if (this.ConbineElements.SelectConbineDines.length > 0) {
                var _this = this;
                this.ConbineElements.SelectConbineDines.forEach(function (s) {
                    _this.ConbineElements.UnpaidDines.push(angular.copy(s));
                    _this.ConbineElements.ConbineDines.forEach(function (x, index) {
                        if (x.Id == s.Id) { _this.ConbineElements.ConbineDines.splice(index, 1); }
                    })
                })
            }
        },
        MoveLeftAll: function () {
            var _this = this;
            this.ConbineElements.ConbineDines.forEach(function (x) {
                _this.ConbineElements.UnpaidDines.push(angular.copy(x));
            })
            this.ConbineElements.ConbineDines = [];
            this.ConbineElements.SelectDines = [];
        },
        Conbine: function () {
            var _this = this;
            var temp = this.ConbineElements.ConbineDines.map(function (x) { return { Id: x.Id, DeskId: x.DeskId } });
            swal(
           {
               title: "确定合并此订单?",
               text: "你将不可恢复此项合并订单，请谨慎操作!",
               type: "warning",
               showCancelButton: true,
               confirmButtonColor: "#D0D0D0",
               confirmButtonText: "是, 删除!",
               cancelButtonText: "否, 保留!",
               closeOnConfirm: false
           }, function () {
               $http.post('../Templates/ConbineDine', { ConbineDines: temp }).success(function (data) {
                   _this.ConbineElements.UnpaidDesk = data.Desks;
                   _this.ConbineElements.UnpaidDines = data.Dines;
                   _this.getFirstDesk();
                   _this.ConbineElements.ConbineDines = [];
                   _this.ConbineElements.SelectDines = [];
                   swal("合并成功!", "已经成功合并订单.", "success");
               }).error(function (data) {
                   console.log(data);
               })
           });
        }
    }
    return service;
}])
.factory('Replace', ['$http', '$q', '$filter', '$rootScope', function ($http, $q, $filter, $rootScope) {
    var service = {
        RepalceElements: {
            TotalDesks: [],
            UnpaidDesk: [],
            UnpaidDines: [],
            CurrentDine: {},
            CurrentDesk: {},
            TagetDesk: {},
        },
        getElements: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Templates/getReplace').success(function (data) {
                _this.RepalceElements.UnpaidDesk = data.Desks;
                _this.RepalceElements.UnpaidDines = data.Dines;
                _this.RepalceElements.TotalDesks = data.TotalDesk;
                _this.getFirstDesk();
                _this.getFirstDine();
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        getFirstDine: function () {
            this.RepalceElements.CurrentDine = $filter('filter')(this.RepalceElements.UnpaidDines, { DeskId: this.RepalceElements.CurrentDesk.Id })[0];
        },
        getFirstDesk: function () {
            if (this.RepalceElements.UnpaidDesk.length > 0) {
                this.RepalceElements.CurrentDesk = this.RepalceElements.UnpaidDesk[0];
            } else {
                this.RepalceElements.CurrentDesk = {};
            }
        },
        changeDesk: function () {
            var _this = this;
            swal(
          {
              title: "确定更换桌台号?",
              text: "更换后桌台信息会改变，请谨慎操作!",
              type: "warning",
              showCancelButton: true,
              confirmButtonColor: "#D0D0D0",
              confirmButtonText: "是, 更换!",
              cancelButtonText: "否, 保留!",
              closeOnConfirm: false
          }, function () {
              $http.post('../Templates/ChangeDesk', { DineId: _this.RepalceElements.CurrentDine.Id, DeskId: _this.RepalceElements.TagetDesk.Id }).success(function (data) {
                  if (data.Status) {
                      _this.RepalceElements.UnpaidDesk = data.Desks;
                      _this.RepalceElements.UnpaidDines = data.Dines;
                      _this.RepalceElements.TotalDesks = data.TotalDesk;
                      _this.getFirstDesk();
                      _this.getFirstDine();
                      _this.RepalceElements.TagetDesk = {};
                      swal("更换成功!", "此项订单已经更换至目标页面.", "success");
                  } else {
                      alert(data.Message);
                  }
              }).error(function (data) {
                  console.log(data);
              })
          });
        },
        AllowChange: function () {
            if (this.RepalceElements.CurrentDine.Id && this.RepalceElements.TagetDesk.Id) {
                if (this.RepalceElements.CurrentDesk.Id == this.RepalceElements.TagetDesk.Id) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
    }
    return service;
}])
.factory('HandOut', ['$http', '$q', '$filter', '$rootScope', function ($http, $q, $filter, $rootScope) {
    var service = {
        HandElement: {
            PayList: [],
            PayKinds: [],
            Time: null,
            NumberStart: 1,
            NumberBegin: 1,
            Numbers:[]
        },
        getElement: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Templates/GetHandOut').success(function (data) {
                console.log(data);
                _this.HandElement.PayList = data.PayList;
                _this.HandElement.PayKinds = data.PayKinds;
                _this.getNumbers();
                _this.HandElement.PayKinds.forEach(function (x) {
                    x.Num = 0;
                    x.Gain = 0;
                    _this.HandElement.PayList.forEach(function (s) {
                        if (x.Id == s.Id) x.Num = s.Total;
                    });
                })
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        getNumbers:function(){
            var _this = this;
            $http.post('../Templates/GetNumbers', {
                Time:_this.HandElement.Time
            }).success(function (data) {
                data.Numbers.forEach(function (x) { x.IsChoose = true; });
                _this.HandElement.Numbers = data.Numbers;
            })
        },
        CheckOut: function () {
            var _this = this;
            var price = this.HandElement.PayKinds.filter(function (x) { return x.Num }).map(function (x) { return x.Num }).reduce(function (a, b) { return +a + +b }, 0);
            if (price > 0) {
                var profit = this.HandElement.PayKinds.filter(function (x) { return x.Gain }).map(function (x) { return { Id: x.Id, Num: x.Gain } });
                $http.post('../Templates/CheckOut', {
                    Profit: profit
                }).success(function (data) {
                    _this.HandElement.PayKinds.forEach(function (x) {
                        x.Num = 0;
                        x.Gain = 0;
                    })
                    alert("交接成功");
                }).error(function (data) {
                    console.log(data);
                })
            }
            else {
                alert("金额为0不用交接");
            }
        },
        Print: function () {
            var _this = this;
            var frequencies = _this.HandElement.Numbers.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            $http.post('../Templates/RePrintCheck', {
                Time: _this.HandElement.Time,
                frequencies: frequencies
            }).success(function (data) {
            }).error(function (data) {
                console.log(data);
            })
        }
    }
    return service;
}])


var GetReason = function (Reasons) {
    var result = '<div class ="control-group"><p>请选择退菜理由</p>';
    for (var i = 0; i < Reasons.length; i++) {
         result += '<div class="radio">'
						+ '<label>'
						    + '<input name="form-field-radio" type="radio" class="ace" data-description="' + Reasons[i].Description + '" />'
						    + '<span class="lbl">' + Reasons[i].Description + '</span>'
						+ '</label>'
					+ '</div>'
    }
    result += '</div>';
    return result;
}