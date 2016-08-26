angular.module('Management', ['ngAnimate', 'ngTouch', 'Config', 'CeryFactory', 'Services', 'Directives', 'Baseinfo', 'ReportController', 'ReportFactory', 'BaseFactory', 'wxf', 'ui.bootstrap', 'localytics.directives'])
.controller('LoginCtrl', ['$scope', '$rootScope', '$window', 'UserLogin', function ($scope, $rootScope, $window, UserLogin) {
    UserLogin.GetCookies();
    $scope.user = UserLogin.User;
    $scope.Hotel = UserLogin.Hotel;
    $scope.KeyInfo = UserLogin.KeyInfo;
    $scope.login = function () {
        if ($scope.user.isSave) UserLogin.SetCookies();
        var promise = UserLogin.Verification();
        promise.then(function (data) {
            if (data.Status) {
                $window.location = "/manage#/CheckOut";
            } else {
                alert("用户名密码错误");
            }
        }, function (reason) {
            alert("用户名密码错误");
        })
    }
    $scope.register = function () {
        var promise = UserLogin.Register();
        promise.then(function (data) {
            console.log(data);
            if (!data.Status) {
                alert(data.ErrorMessage);
            }
            else {
                alert("注册成功，请等待公司确认");
            }
        })
    }
    $scope.GetKey = function () {
        UserLogin.GetKey();
    }
}])
.controller('OrderCtrl', ['$scope', '$rootScope', '$uibModal', 'Order', 'Pay', 'Open', function ($scope, $rootScope, $uibModal, Order, Pay, Open) {
    //点单
    $rootScope.FatherPage = "订单控制"; $rootScope.FatherPath = "#/CheckOut"; $rootScope.ChildPage = "订单管理"; $scope.getId = function (id) { return '/#Home' + id; }; $scope.getIdShow = function (id) { return 'Home' + id; };
    Order.getElements();
    $scope.SysElements = Order.SysElements;
    $scope.unpaid = function (area) { return Order.unpaid(area); }
    $scope.$on('desk', function (event, data) {
        if ($rootScope.HotelId == data.HotelId) {//监视Hotel变化
            console.log(data);
            $scope.SysElements.Desks = data.data;
            $scope.$apply();
        }
    });
    $scope.OpenModel = function (desk, area) {
        var myDesk = {};
        myDesk = { Id: desk.Id, Name: desk.Name };
        if (area.Type == 1) {
            var modalInstance = $uibModal.open({//打开开桌
                animation: $scope.animationsEnabled,
                templateUrl: 'ModelReserve.html',
                controller: 'ModalOpenCtrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    option: { desk: myDesk, method: Open, Pay: Pay }
                }
            });
        }
        else {
            var modalInstance = $uibModal.open({//打开开桌
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalOpen.html',
                controller: 'ModalOpenCtrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    option: { desk: myDesk, method: Open, Pay: Pay }
                }
            });
        }

    }
}])
.controller('ModalPayCtrl', function ($scope, $rootScope, $uibModalInstance, option) {
    //支付控制
    option.method.PayElements.Desk = option.desk;
    $scope.initialize = function () {
        var promise = option.method.getElements();
        promise.then(function (data) {
            $scope.PayElements = option.method.PayElements;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.$on('dine', function (event, data) {
        if ($rootScope.HotelId == data.HotelId) {//监视Hotel变化
            console.log(data);
            if ($scope.PayElements) $scope.PayElements.UnpaidDines = data.data;
            option.method.AddOnSale();
            $scope.$apply();
        }
    });
    $scope.SetDine = function (dine) { option.method.SetDine(dine); }
    $scope.Change = function (kind) { option.method.Change(kind); }
    $scope.Enter = function (cv) {
        if (cv.keyCode !== 13) return; // your code
        if ($scope.AllowPay()) {
            $scope.pay();
        }
    }
    $scope.PriceAll = function () { return option.method.PriceAll(); }
    $scope.OriPriceAll = function () { return option.method.OriPriceAll(); }
    $scope.Unpaid = function () { return option.method.Unpaid(); }
    $scope.TotalPay = function () { return option.method.TotalPay(); }
    $scope.Charge = function () { return option.method.Charge(); }
    $scope.reCalc = function () { option.method.reCalc(); }
    $scope.GetMenuPrice = function (menu) { return option.method.GetMenuPrice(menu); }
    $scope.GetDiscountName = function (dine) { return option.method.GetDiscountName(dine); }
    $scope.Login = function () { option.method.Login(); }
    $scope.ChangeDesk = function () { option.method.ChangeDesk(); }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
    $scope.pay = function () {
        var payPromise = option.method.Pay();
        payPromise.then(function (data) {
            if (option.method.DineCount($scope.PayElements.Desk.Id) == 0) {
                $uibModalInstance.dismiss('cancel');
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.AllowPay = function () { return option.method.AllowPay(); }
    $scope.AllowPrint = function () { return option.method.AllowPrint(); }
    $scope.Recipt = function () { return option.method.Recipt(); }
    $scope.Kitchen = function () { return option.method.Kitchen(); }

})
.controller('ModalOpenCtrl', function ($scope, $rootScope, $uibModal, $uibModalInstance, $q, $timeout, option) {
    //开桌控制
    $scope.initialize = function () {
        option.method.OpenElements.CurrentDesk = option.desk;
        var promise = option.method.getElements();
        promise.then(function (data) {
            $scope.OpenElements = option.method.OpenElements;
            console.log($scope.OpenElements);
        }, function (data) {
            console.log(data);
        })
    }


    $scope.AllowLogin = function () { return option.method.AllowLogin(); }
    $scope.OpenElements = option.method.OpenElements;
    $scope.login = function () { option.method.Login(); }
    $scope.logout = function () { option.method.LogOut(); }
    $scope.NumChange = function () { option.method.NumChange(); }
    $scope.SelectChange = function (menu) { option.method.SelectChange(menu); }
    $scope.AddMenu = function () { option.method.AddMenu(); }
    $scope.SendMenu = function () { option.method.SendMenu(); }
    $scope.CalcMenuPrice = function (menu) { return option.method.CalcMenuPrice(menu); }
    $scope.RemoveMenu = function (index) { option.method.RemoveMenu(index); }
    $scope.FormatDiscount = function () { option.method.FormatDiscount() }
    $scope.account = function () { return option.method.AccountPrice(); }
    $scope.AllowOpen = function () { return option.method.AllowOpen(); }
    $scope.OpenDesk = function () {
        var promise = option.method.OpenDesk();
        promise.then(function (data) {
            if (data.Succeeded) {
                if ($rootScope.IsPayFirst) {
                    var modalInstance = $uibModal.open({//打开支付
                        animation: $scope.animationsEnabled,
                        templateUrl: 'ModalPay.html',
                        controller: 'ModalPayCtrl',
                        backdrop: 'static',
                        size: 'lg',
                        resolve: {
                            option: {
                                desk: angular.copy(option.desk), method: option.Pay
                            }
                        }
                    });
                }
                $uibModalInstance.dismiss('cancel');
            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.OpenReserveModel = function () {
        var modalInstance = $uibModal.open({//打开开桌
            animation: $scope.animationsEnabled,
            templateUrl: 'ModelReservePhoneInfo.html',
            controller: 'ModelReserveCtrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                option: { method: option.method }
            }
        });
    }
    $scope.OpenReserve = function () {
        var promise = option.method.OpenReserve();
        promise.then(function (data) {
            if (data.Status) {
                $uibModalInstance.dismiss('cancel');
            } else {
                alert(data.ErrorMessage);
            }
        })
    }
    $scope.CleanFilter = function () {
        option.method.CleanFilter();
    }
    $scope.ShiftType = function () {
        option.method.ShiftType();
    }
    $scope.EditMenu = function (menu) {
        option.method.EditMenu(menu);
    }
    $scope.EditCheck = function (menu) {
        option.method.EditCheck(menu);
    }
    $scope.EditRemove = function (menu) {
        option.method.EditRemove(menu);
    }
    $scope.CheckAddNum = function (menu) {
        option.method.CheckAddNum(menu);
    }
    $scope.MenuFilter = function (id) {
        option.method.MenuFilter(id);
    }
    $scope.AddSingle = function (menu) {
        option.method.AddSingle(menu);
    }
    $scope.Filter = function () {
        option.method.Filter();
    }
    $scope.Up = function () { option.method.Up(); }
    $scope.Down = function () { option.method.Down(); }
    $scope.cancel = function () {
        option.method.KeepShift();
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ModelReserveCtrl', function ($scope, $rootScope, $uibModal, $uibModalInstance, $q, $timeout, option) {
    $scope.ReserveInfo = option.method.ReserveInfo;
    $scope.OpenElements = option.method.OpenElements;
    $scope.ok = function () {
        $uibModalInstance.dismiss('cancel');
    }
    $scope.SmartChoose = function () {
        option.method.SmartChoose();
    }
    $scope.SetAddress = function (address, type) {
        option.method.SetAddress(address, type);
    }
    $scope.cancel = function () {
        option.method.ReserveInfo ={
            Phone: "",
            Address: "",
            User: {}
        }
        $uibModalInstance.dismiss('cancel');
    }
    $scope.DeleteAddress = function (address) {
        option.method.DeleteAddress(address);
    }

})
.controller('ReturnCtrl', ['$scope', '$rootScope', '$uibModal', 'Returned', function ($scope, $rootScope, $uibModal, Returned) {
    $rootScope.FatherPage = "订单管理"; $rootScope.FatherPath = "#/ReturnMenu"; $rootScope.ChildPage = "退菜管理";
    var promise = Returned.getElements();
    promise.then(function (data) {
        $scope.ReturnElements = Returned.ReturnElements;
    }, function (data) {
        console.log(data);
    });
    $scope.ChangeDesk = function () { Returned.ChangeDesk(); }
    $scope.SetDine = function (dine) { Returned.SetDine(dine); }
    $scope.DeleteDine = function (dine) { Returned.DeleteDine(dine); }
    $scope.DeleteMenu = function (menu) { Returned.DeleteMenu(menu); }
    $scope.DeleteReason = function (reason) {

        Returned.DeleteReason(reason);
    }
    $scope.OpenEditModel = function (reason) {
        var modalInstance = $uibModal.open({//打开支付
            animation: $scope.animationsEnabled,
            templateUrl: 'ModalReasonEdit.html',
            controller: 'ReasoneditCtrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                option: {
                    Returned: Returned, Reason: reason
                }
            }
        });
    }
    $scope.OpenAddModel = function () {
        var modalInstance = $uibModal.open({//打开支付
            animation: $scope.animationsEnabled,
            templateUrl: 'ModalReasonAdd.html',
            controller: 'ReasonaddCtrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                option: {
                    Returned: Returned
                }
            }
        });
    }
}])
.controller('ReasoneditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    var temp = angular.copy(option.Reason);
    console.log(temp);
    option.Returned.ReturnElements.CurrentReason = option.Reason;
    $scope.ReturnElements = option.Returned.ReturnElements;
    $scope.Edit = function () {
        option.Returned.EditReason();
        $uibModalInstance.dismiss('cancel');
    }
    $scope.cancel = function () {
        $scope.ReturnElements.CurrentReason.Description = temp.Description;
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ReasonaddCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.Returned.ReturnElements.CurrentReason = {};
    $scope.ReturnElements = option.Returned.ReturnElements;
    $scope.Add = function () {
        option.Returned.AddReason();
        $uibModalInstance.dismiss('cancel');
    }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('ConbineCtrl', ['$scope', '$rootScope', 'Conbine', function ($scope, $rootScope, Conbine) {
    $rootScope.FatherPage = "订单管理"; $rootScope.FatherPath = "#/Conbine"; $rootScope.ChildPage = "合单管理";
    $scope.initialize = function () {
        var promise = Conbine.initialize();
        promise.then(function (data) {
            $scope.ConbineElements = Conbine.ConbineElements;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.MoveRight = function () { Conbine.MoveRight(); }
    $scope.MoveRightAll = function () { Conbine.MoveRightAll(); }
    $scope.MoveLeft = function () { Conbine.MoveLeft(); }
    $scope.MoveLeftAll = function () { Conbine.MoveLeftAll(); }
    $scope.Conbine = function () { Conbine.Conbine(); }
}])
.controller('ReplaceCtrl', ['$scope', '$rootScope', 'Replace', function ($scope, $rootScope, Replace) {
    $rootScope.FatherPage = "订单管理"; $rootScope.FatherPath = "#/Replace"; $rootScope.ChildPage = "换桌管理";
    $scope.initialize = function () {
        var promise = Replace.getElements();
        promise.then(function (data) {
            $scope.RepalceElements = Replace.RepalceElements;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.changeDesk = function () { Replace.changeDesk(); }
    $scope.getFirstDine = function () { Replace.getFirstDine(); }
    $scope.AllowChange = function () { return Replace.AllowChange(); }
}])
.controller('HandOutCtrl', ['$scope', '$rootScope', '$uibModal', 'HandOut', function ($scope, $rootScope, $uibModal, HandOut) {
    $rootScope.FatherPage = "结账系统管理"; $rootScope.ChildPage = "交接班";
    $scope.initialize = function () {
        var promise = HandOut.getElement();
        promise.then(function (data) {
            $scope.HandElement = HandOut.HandElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.CheckOut = function () {
        var promise = HandOut.CheckOut();
        promise.then(function (data) {
            if (data.Status) {
                $rootScope.Logout();
            }
        })
    }
    $scope.rePrint = function () {
        var modalInstance = $uibModal.open({//打开支付
            animation: $scope.animationsEnabled,
            templateUrl: 'ModelPrint.html',
            controller: 'ModelPrintCtrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                option: {
                    HandOut: HandOut
                }
            }
        });
    }
}])
.controller('HandOutHistoryCtrl', ['$scope', '$rootScope', '$uibModal', 'HandOut', function ($scope, $rootScope, $uibModal, HandOut) {
    $rootScope.FatherPage = "结账系统管理"; $rootScope.ChildPage = "历史交接查询";
    $scope.initialize = function () {
        var promise = HandOut.getElement();
        promise.then(function (data) {
            $scope.HandElement = HandOut.HandElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.getNumbers = function () {
        HandOut.getNumbers();
    }
    $scope.Search = function () {
        HandOut.Search();
    }
    $scope.Print = function () {
        HandOut.Print();
    }
}])
.controller('DailyDetailCtrl', ['$scope', '$rootScope', '$uibModal', 'HandOut', function ($scope, $rootScope, $uibModal, HandOut) {
    $rootScope.FatherPage = "结账系统管理"; $rootScope.ChildPage = "当日交接班明细";
    $scope.initialize = function () {
        var promise = HandOut.getElement();
        promise.then(function (data) {
            $scope.HandElement = HandOut.HandElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.PrintDetail = function () {
        HandOut.PrintDetail();
    }
    $scope.rePrint = function () {
        var modalInstance = $uibModal.open({//打开支付
            animation: $scope.animationsEnabled,
            templateUrl: 'ModelPrint.html',
            controller: 'ModelPrintCtrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                option: {
                    HandOut: HandOut
                }
            }
        });
    }
}])
.controller('ModelPrintCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    }
    $scope.initialize = function () {
        option.HandOut.getNumbers();
    }
    $scope.HandElement = option.HandOut.HandElement;
    $scope.Print = function () {
        option.HandOut.Print();
    }
    $scope.getNumbers = function () {
        option.HandOut.getNumbers();
    }
})
.controller('RePrinterCtrl', ['$scope', '$rootScope', '$uibModal', 'RePrinter', function ($scope, $rootScope, $uibModal, RePrinter) {
    $rootScope.FatherPage = "店小二营业"; $rootScope.ChildPage = "当日订单查询";
    $scope.Initialize = function () {
        RePrinter.Initialize();
    }
    $scope.RePrinterElement = RePrinter.RePrinterElement;
    $scope.Detail = function (dine) {
        RePrinter.RePrinterElement.CurrentDine = dine;
        var modalInstance = $uibModal.open({//打开支付
            animation: $scope.animationsEnabled,
            templateUrl: 'ModalDetail.html',
            controller: 'ModalDetailCtrl',
            backdrop: 'static',
            size: 'sm',
            resolve: {
                option: {
                    method:RePrinter,CurDine:dine
                }
            }
        });
    }
    $scope.GetPaid = function () {
        RePrinter.GetPaid();
    }
    $scope.GetUnpaid = function () {
        RePrinter.GetUnpaid();
    }
}])
.controller('ModalDetailCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    $scope.RePrinterElement = option.method.RePrinterElement;
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    }
    $scope.RePrintDine = function (dine) {
        option.method.RePrintDine(dine);
    }
    $scope.initialize = function () {
        option.method.GetDetail();
    }
})
.controller('TakeOutCtrl', ['$scope', '$rootScope', '$uibModal', 'Order', 'Pay', function ($scope, $rootScope, $uibModal, Order, Pay) {
    //点单
    $rootScope.FatherPage = "店小二营业"; $rootScope.FatherPath = "#/CheckOut"; $rootScope.ChildPage = "结账";
    Order.getElements();
    $scope.SysElements = Order.SysElements;
    $scope.$on('desk', function (event, data) {
        if ($rootScope.HotelId == data.HotelId) {//监视Hotel变化
            console.log(data);
            $scope.SysElements.Desks = data.data;
            $scope.$apply();
        }
    });
    $scope.unpaidAll = function () { return Order.unpaidAll(); }
    $scope.OpenModel = function (desk, area) {
        var myDesk = {};
        myDesk = { Id: desk.Id, Name: desk.Name };
        if (desk.Status == 1) {
            var modalInstance = $uibModal.open({//打开支付
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalPay.html',
                controller: 'ModalPayCtrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    option: {
                        desk: myDesk, method: Pay
                    }
                }
            });
        }
    }
}])
.controller('PayAllCtrl', ['$scope', '$rootScope', '$uibModal', 'SpePay', function ($scope, $rootScope, $uibModal, SpePay) {
    $rootScope.FatherPage = "订单控制"; $rootScope.ChildPage = "外卖结账";
    $scope.Initialize = function () {
        SpePay.Initialize();
    }
    $scope.Element = SpePay.Element;
    $scope.getUnPaidDines = function () {
        SpePay.getUnPaidDines();
    }
    $scope.ChooseDine = function (dine) {
        SpePay.ChooseDine(dine);
    }
    $scope.RemoveDine = function (dine) {
        SpePay.RemoveDine(dine);
    }
    $scope.ChooseAll = function () {
        SpePay.ChooseAll();
    }
    $scope.Account = function () {
        return SpePay.Account();
    }
    $scope.Pay = function () {
        SpePay.Pay();
    }
    $scope.Detail = function () {
        SpePay.Detail();
    }
}])
.controller('NotFound', ['$scope', '$rootScope', function ($scope, $rootScope) {
    //未找到页面
    $rootScope.FatherPage = "页面错误"; $rootScope.FatherPath = "#/404"; $rootScope.ChildPage = "未找到页面";
}])
.controller('BaseCtrl', ['$scope', '$rootScope', '$http', '$window', 'WebSocketService', function ($scope, $rootScope, $http, $window, WebSocketService) {
    //基础控制器
    $scope.initialize = function (id, rate, pu, a, b, c, d, e) {
        $rootScope.HotelId = id; $rootScope.Rate = rate; $rootScope.PayUnder = pu;
        $rootScope.IsStaffPay = a == 1 ? true : false; $rootScope.IsStaffReturn = b == 1 ? true : false; $rootScope.IsStaffEdit = c == 1 ? true : false;
        $rootScope.WebSocketUrl = d;
        $rootScope.IsPayFirst = !!e;
        WebSocketService.Start($rootScope.WebSocketUrl);
    }

    $rootScope.EditTabel = function () {
        //editables on first profile page
        $.fn.editable.defaults.mode = 'inline';
        $.fn.editableform.loading = "<div class='editableform-loading'><i class='light-blue icon-2x icon-spinner icon-spin'></i></div>";
        $.fn.editableform.buttons = '<button type="submit" class="btn btn-info editable-submit"><i class="icon-ok icon-white"></i></button>' +
                                    '<button type="button" class="btn editable-cancel"><i class="icon-remove"></i></button>';

        // *** editable avatar *** //
        try {//ie8 throws some harmless exception, so let's catch it

            //it seems that editable plugin calls appendChild, and as Image doesn't have it, it causes errors on IE at unpredicted points
            //so let's have a fake appendChild for it!
            var last_gritter
            $('#avatar').editable({
                type: 'image',
                name: '图片',
                value: null,
                image: {
                    //specify ace file input plugin's options here
                    btn_choose: '选择图片',
                    droppable: true,
                    /**
                    //this will override the default before_change that only accepts image files
                    before_change: function(files, dropped) {
                        return true;
                    },
                    */

                    //and a few extra ones here
                    name: 'avatar',//put the field name here as well, will be used inside the custom plugin
                    max_size: 1100000,//~1Mb
                    on_error: function (code) {//on_error function will be called when the selected file has a problem
                        if (last_gritter) $.gritter.remove(last_gritter);
                        if (code == 1) {//file format error
                            last_gritter = $.gritter.add({
                                title: '文件不是一个图片!',
                                text: '请选择 jpg|gif|png 图像!',
                                class_name: 'gritter-error gritter-center'
                            });
                        } else if (code == 2) {//file size rror
                            last_gritter = $.gritter.add({
                                title: '文件太大了!',
                                text: '不能超过1Mb!',
                                class_name: 'gritter-error gritter-center'
                            });
                        }
                        else {//other error
                        }
                    },
                    on_success: function () {
                        $.gritter.removeAll();
                    }
                },
                url: function (params) {
                    // ***UPDATE AVATAR HERE*** //
                    //You can replace the contents of this function with examples/profile-avatar-update.js for actual upload

                    var deferred = new $.Deferred

                    //if value is empty, means no valid files were selected
                    //but it may still be submitted by the plugin, because "" (empty string) is different from previous non-empty value whatever it was
                    //so we return just here to prevent problems
                    var value = $('#avatar').next().find('input[type=hidden]:eq(0)').val();
                    if (!value || value.length == 0) {
                        deferred.resolve();
                        return deferred.promise();
                    }


                    //dummy upload
                    setTimeout(function () {
                        if ("FileReader" in window) {
                            //for browsers that have a thumbnail of selected image
                            var thumb = $('#avatar').next().find('img').data('thumb');
                            if (thumb) $('#avatar').get(0).src = thumb;
                        }

                        deferred.resolve({ 'status': 'OK' });

                        if (last_gritter) $.gritter.remove(last_gritter);
                        last_gritter = $.gritter.add({
                            title: '文件更新中!',
                            text: '文件读取完毕，请继续操作',
                            class_name: 'gritter-info gritter-center'
                        });

                    }, parseInt(Math.random() * 800 + 800))

                    return deferred.promise();
                },

                success: function (response, newValue) {
                    $rootScope.ImgChange = $('#avatar').attr('src');
                }
            })
        } catch (e) { }
        $(".knob").knob();
    }

    $rootScope.Logout = function () {
        $http.post('../Login/Logout').success(function (data) {
            $window.location = "/Login";
        }).error(function (data) {
            console.log(data);
        })
    }
}])
.controller('AddMenuCtrl', ['$scope', '$rootScope', '$uibModal', 'AddMenu', 'Open', function ($scope, $rootScope, $uibModal, AddMenu, Open) {
    $rootScope.FatherPage = "店小二营业"; $rootScope.ChildPage = "加菜";
    $scope.Initialize = function () {
        var promise = AddMenu.Initialize();
        promise.then(function (data) {
            $scope.MenuElement = AddMenu.MenuElement;
        })
    }
    $scope.OpenAddMenu = function (dine) {
        var modalInstance = $uibModal.open({//打开支付
            animation: $scope.animationsEnabled,
            templateUrl: 'ModalOpen.html',
            controller: 'ModalAddMenuCtrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                option: {
                    CurDine: dine, method: Open
                }
            }
        });
    }
}])
.controller('ModalAddMenuCtrl', function ($scope, $rootScope, $uibModalInstance, option) {
    $scope.Initialize = function () {
        var promise  =   option.method.getElements();
        promise.then(function (data) {
            $scope.OpenElements = option.method.OpenElements;
            $scope.OpenElements.CurrentDine = option.CurDine;
            $scope.OpenElements.CurrentDiscount.Discount = option.CurDine.Discount * 100;
        })
    }
    $scope.NumChange = function () { option.method.NumChange(); }
    $scope.SelectChange = function (menu) {
        option.method.SelectChange(menu);
    }
    $scope.AddMenu = function () { option.method.AddMenu(); }
    $scope.SendMenu = function () { option.method.SendMenu(); }
    $scope.CalcMenuPrice = function (menu) { return option.method.CalcMenuPrice(menu); }
    $scope.RemoveMenu = function (index) { option.method.RemoveMenu(index); }
    $scope.account = function () { return option.method.AccountPrice(); }
    $scope.AllowOpen = function () { return option.method.AllowOpen(); }
    $scope.Up = function () { option.method.Up(); }
    $scope.Down = function () { option.method.Down(); }
    $scope.AddDineMenu = function () {
        var promise = option.method.AddDineMenu();
        promise.then(function (data) {
            if (data.Status) {
                $scope.cancel();
            } else {
                alert(data.ErrorMessage);
            }
        })
    }
    $scope.ShiftType = function () {
        option.method.ShiftType();
    }
    $scope.MenuFilter = function (id) {
        option.method.MenuFilter(id);
    }
    $scope.cancel = function () {
        option.method.KeepShift();
        $uibModalInstance.dismiss('cancel');
    }
    $scope.AddSingle = function (menu) {
        option.method.AddSingle(menu);
    }
    $scope.Filter = function () {
        option.method.Filter();
    }
    $scope.CleanFilter = function () {
        option.method.CleanFilter();
    }
    $scope.EditMenu = function (menu) {
        option.method.EditMenu(menu);
    }
    $scope.EditCheck = function (menu) {
        option.method.EditCheck(menu);
    }
    $scope.EditRemove = function (menu) {
        option.method.EditRemove(menu);
    }
    $scope.CheckAddNum = function (menu) {
        option.method.CheckAddNum(menu);
    }
})
.filter('menu', function () {
    return function (input, MenuId) {
        return input.filter(function (x) { return x.ParentMenuClassId == MenuId });
    };
})