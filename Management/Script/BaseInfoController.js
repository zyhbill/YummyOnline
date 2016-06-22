
angular.module('Baseinfo', [])
.controller('AreasCtrl', ['$scope', '$rootScope', '$uibModal', 'Area', function ($scope, $rootScope, $uibModal, Area) {
    $rootScope.FatherPage = "餐桌管理"; $rootScope.FatherPath = "#/Areas"; $rootScope.ChildPage = "区域设置";
    $scope.intialize = function () {
        var promise = Area.intialize();
        promise.then(function (data) {
            $scope.AreaElement = Area.AreaElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.OpenEditModel = function (area) {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalAreaEdit.html',
                controller: 'ModalAreaEditCtrl',
                backdrop:'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Area, CurArea: area
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.OpenAddModel = function () {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalAreaAdd.html',
                controller: 'ModalAreaAddCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Area
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.DeleteArea = function (area) {
        if ($rootScope.IsStaffEdit) {
            Area.DeleteArea(area);
        } else {
            alert("对不起，你没有相关权限，请联系管理员");
        }
    }
}])
.controller('ModalAreaEditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    $scope.CurrentArea = option.CurArea;
    var temp = angular.copy(option.CurArea);
    var originAreaId = $scope.CurrentArea.Id;
    $scope.AreaElement = option.method.AreaElement;
    $scope.SelectServiceChange = function () { option.method.SelectServiceChange($scope.CurrentArea) };
    $scope.SelectReciptChange = function () { option.method.SelectReciptChange($scope.CurrentArea) };
    $scope.EditArea = function () {
        var promise = option.method.EditArea($scope.CurrentArea, originAreaId);
        promise.then(
            function (data) {
                $scope.AreaElement.isAjax = false;
                $uibModalInstance.dismiss('cancel');
            },
            function (data) {
                console.log(data);
            })
    }
    $scope.cancel = function () {
        for (var i = 0 ; i < option.method.AreaElement.Areas.length; i++) {
            if (option.method.AreaElement.Areas[i].Id == temp.Id)   option.method.AreaElement.Areas[i] = temp;
        }
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ModalAreaAddCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.AreaElement.ServiceDepartment = {};
    option.method.AreaElement.ReciptDepartment = {};
    $scope.AreaElement = option.method.AreaElement;
    $scope.SelectServiceChange = function () { option.method.SelectServiceChange($scope.AreaElement.newArea) };
    $scope.SelectReciptChange = function () { option.method.SelectReciptChange($scope.AreaElement.newArea) };
    $scope.AddArea = function () {
        var promise = option.method.AddArea();
        promise.then(function (data) {
            if (data.Status) {
                $uibModalInstance.dismiss('cancel');
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('DesksCtrl', ['$scope', '$rootScope', '$uibModal', 'Desk', 'HandPicture', function ($scope, $rootScope, $uibModal, Desk, HandPicture) {
    $rootScope.FatherPage = "餐桌管理"; $rootScope.FatherPath = "#/Desks"; $rootScope.ChildPage = "餐桌设置";
    $scope.intialize = function () {
        var promise = Desk.intialize();
        $scope.DeskElement = Desk.DeskElement;
    }
    $scope.OpenEditModel = function (desk) {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalDeskEdit.html',
                controller: 'ModalDeskEditCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Desk, CurrentDesk: desk, handle: HandPicture
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.DeleteDesk = function (desk) {
        if ($rootScope.IsStaffEdit) {
            Desk.DeleteDesk(desk);
        } else {
            alert("对不起，你没有相关权限，请联系管理员");
        }
    }
    $scope.OpenAddModel = function () {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalDeskAdd.html',
                controller: 'ModalDeskAddCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Desk, handle: HandPicture
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }


    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
}])
.controller('ModalDeskEditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    $scope.CurrentDesk = option.CurrentDesk;
    var temp = angular.copy(option.CurrentDesk);
    var OriDeskId = $scope.CurrentDesk.Id;
    option.method.DeskElement.Areas.forEach(function (x) {
        if (x.Id == $scope.CurrentDesk.AreaId) {
            option.method.DeskElement.SelectArea = x;
        }
    })
    $scope.DeskElement = option.method.DeskElement;
    $scope.EditDesk = function () {
        if ($scope.fileChoosed) {
            var promise = option.handle.getPath($scope.fileChoosed[0]);
            promise.then(function (data) {
                option.method.EditDesk($scope.CurrentDesk, data, OriDeskId);
                $uibModalInstance.dismiss('cancel');
            })
        }
        else {
            option.method.EditDesk($scope.CurrentDesk, "", OriDeskId);
            $uibModalInstance.dismiss('cancel');
        }
    }
    $scope.AreaChange = function () { option.method.AreaChange($scope.CurrentDesk); }
    $scope.cancel = function () {
        for (var i = 0; i < option.method.DeskElement.Desks.length;i++){
            if (option.method.DeskElement.Desks[i].Id == temp.Id) option.method.DeskElement.Desks[i] = temp;
        }
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ModalDeskAddCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.DeskElement.SelectArea = {};
    option.method.DeskElement.newDesk = { MinPrice: 0 };
    $scope.DeskElement = option.method.DeskElement;
    $scope.AddDesk = function () {
        if ($scope.fileChoosed) {
            var promise = option.handle.getPath($scope.fileChoosed[0]);
            promise.then(function (data) {
                var AdDk = option.method.AddDesk(data);
                AdDk.then(function (ddata) {
                    $rootScope.fileChoosed = null;
                    if (ddata.Status) {
                        $uibModalInstance.dismiss('cancel');
                    }
                }, function (ddata) {
                    alert(ddata);
                })
            }, function (data) {
                console.log(data);
            })
        }
        else {
            var AdDk = option.method.AddDesk("");
            AdDk.then(function (ddata) {
                $rootScope.fileChoosed = null;
                if (ddata.Status) {
                    $uibModalInstance.dismiss('cancel');
                }
            }, function (ddata) {
                alert(ddata);
            })
        }
    }
    $scope.NewAreaChange = function () { option.method.NewAreaChange(); }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('MenusCtrl', ['$scope', '$rootScope', '$uibModal', 'Menu', 'HandPicture', function ($scope, $rootScope, $uibModal, Menu, HandPicture) {
    $rootScope.FatherPage = "菜品管理"; $rootScope.FatherPath = "#/Menus"; $rootScope.ChildPage = "菜品设置";
    $scope.intialize = function () {
        var promise = Menu.intialize();
        promise.then(function (data) {
            $scope.MenuElement = Menu.MenuElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.OpenEditModel = function (menu) {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalMenuEdit.html',
                controller: 'ModalMenuEditCtrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    option: {
                        method: Menu, handle: HandPicture, CurMenu: menu
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.OpenAddModel = function () {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalMenuAdd.html',
                controller: 'ModalMenuAddCtrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    option: {
                        method: Menu, handle: HandPicture
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.OpenMultipleModel = function () {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalMenuMultiple.html',
                controller: 'ModalMenuMultipleCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Menu, handle: HandPicture
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.DeleteMenu = function (menu) {
        if ($rootScope.IsStaffEdit) {
            Menu.DeleteMenu(menu);
        }
    }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
}])
.controller('ModalMenuEditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.MenuElement.CurrentMenu = option.CurMenu;
    var temp = angular.copy(option.CurMenu);
    var OriMId = option.method.MenuElement.CurrentMenu.Id;
    option.method.CalcSupply();
    option.method.InitSelect();
    option.method.InitDegree();
    $scope.MenuElement = option.method.MenuElement;
    $scope.GetPath = function () {
        return option.method.WatchPath();
    }
    $scope.EditMenu = function () {
        option.method.EditMenu(OriMId);
        $uibModalInstance.dismiss('cancel');
    }
    $scope.SelectChange = function () { option.method.SelectChange(); }
    $scope.GetValue = function () { option.method.GetValue(); }
    $scope.Minus = function () { option.method.Minus(); }
    $scope.Plus = function () { option.method.Plus(); }
    $scope.cancel = function () {
        for (var i = 0; i < option.method.MenuElement.Menus.length; i++) {
            if (option.method.MenuElement.Menus[i].Id == temp.Id) option.method.MenuElement.Menus[i] = temp;
        }
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ModalMenuAddCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.MenuElement.CurrentMenu = { MinOrderCount: 1, IsFixed: false, IsSetMeal: false, Status: 0, SupplyDate: 127, MenuPrice: { Discount: 100, ExcludePayDiscount: false, Points: 0 }, Unit: "份" };
    option.method.CalcSupply();
    option.method.InitSelect();
    option.method.InitDegree();
    $scope.GetPath = function () {
        return option.method.WatchPath();
    }
    $scope.AddMenu = function () {
        var promise = option.method.AddMenu();
        promise.then(function (data) {
            if (data.Status) {
                $scope.MenuElement.Menus.push($scope.MenuElement.CurrentMenu);
                $scope.cancel();
            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.AllowAdd = function () { return option.method.AllowAdd();}
    $scope.SelectChange = function () { option.method.SelectChange();}
    $scope.MenuElement = option.method.MenuElement;
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('ModalMenuMultipleCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    $scope.MenuElement = option.method.MenuElement;
    $scope.AddMutipleMenu = function () {
        option.method.AddMutipleMenu();
    }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('StaffRolesCtrl', ['$scope', '$rootScope', '$uibModal', 'StaffRoles', function ($scope, $rootScope, $uibModal, StaffRoles) {
    $rootScope.FatherPage = "酒店基本管理"; $rootScope.FatherPath = "#/StaffRoles"; $rootScope.ChildPage = "员工角色设置";
    $scope.intialize = function () {
        var promise = StaffRoles.Initialize();
        promise.then(function (data) {
            console.log(data);
            $scope.StaffRolesElement = StaffRoles.StaffRolesElement;
        }, function (data) {
            console.log(data);
        });
    }
    $scope.OpenEditModel = function (role) {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalSchemasEdit.html',
                controller: 'ModalSchemasEditCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: StaffRoles, CurRole: role
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.OpenAddModel = function () {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalSchemasAdd.html',
                controller: 'ModalSchemasAddCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: StaffRoles
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.DeleteSchema = function (role) {
        StaffRoles.DeleteSchema(role);
    }

}])
.controller('ModalSchemasEditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.StaffRolesElement.CurRole = option.CurRole;
    var temp = angular.copy(option.CurRole);
    option.method.InitSelect();
    $scope.StaffRolesElement = option.method.StaffRolesElement;
    $scope.EditRoles = function () {
        var promise = option.method.EditRoles();
        promise.then(function (data) {
            $uibModalInstance.dismiss('cancel');
        }, function (data) {
            console.log(data);
        })
    }
    $scope.cancel = function () {
        for (var i = 0; i < option.method.StaffRolesElement.StaffRoles.length; i++) {
            if (option.method.StaffRolesElement.StaffRoles[i].Id == temp.Id) {
                option.method.StaffRolesElement.StaffRoles[i] = temp;
            }
        }
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ModalSchemasAddCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.StaffRolesElement.SelectSchemes = [];
    option.method.StaffRolesElement.NewRole = {};
    $scope.StaffRolesElement = option.method.StaffRolesElement;
    $scope.AddRole = function () {
        option.method.AddRole();
        $uibModalInstance.dismiss('cancel');
    }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('StaffCtrl', ['$scope', '$rootScope', '$uibModal', 'Staffs', function ($scope, $rootScope, $uibModal, Staffs) {
    $rootScope.FatherPage = "酒店基本管理"; $rootScope.FatherPath = "#/Staffs"; $rootScope.ChildPage = "员工信息设置";
    $scope.intialize = function () {
        var promise = Staffs.intialize();
        promise.then(function (data) {
            $scope.StaffElements = Staffs.StaffElements;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.OpenEditModel = function (staff) {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalStaffsEdit.html',
                controller: 'ModalStaffsEditCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Staffs, CurStaff: staff
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.OpenAddModel = function () {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalStaffsAdd.html',
                controller: 'ModalStaffsAddCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Staffs
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }

    $scope.DeleteStaff = function (staff) {
        if ($rootScope.IsStaffEdit) {
            Staffs.DeleteStaff(staff);
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
}])
.controller('ModalStaffsEditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.StaffElements.CurStaff = option.CurStaff;
    var temp = angular.copy(option.CurStaff);
    $scope.StaffElements = option.method.StaffElements;
    option.method.InitSelect();
    $scope.EditStaffs = function () {
        var promise = option.method.EditStaffs();
        promise.then(function (data) {
            $uibModalInstance.dismiss('cancel');
        }, function (data) {
            console.log(data);
        })
    }
    $scope.cancel = function () {
        for (var i = 0; i < option.method.StaffElements.Staffs.length; i++) {
            if (option.method.StaffElements.Staffs[i].SysStaff.Id == temp.SysStaff.Id) {
                option.method.StaffElements.Staffs[i] = temp;
            }
        }
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ModalStaffsAddCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.StaffElements.CurStaff = { Staff: {}, SysStaff: {} };
    option.method.InitSelect();
    $scope.StaffElements = option.method.StaffElements;
    $scope.AddStaff = function () {
        var promise = option.method.AddStaff();
        promise.then(function (data) {
            if (data.Status) {
                $uibModalInstance.dismiss('cancel');
            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('MenuRemarksCtrl', ['$scope', '$rootScope', '$uibModal', 'MenuRemarks', function ($scope, $rootScope, $uibModal, MenuRemarks) {
    $rootScope.FatherPage = "菜品管理"; $rootScope.FatherPath = "#/MenuRemarks"; $rootScope.ChildPage = "菜品备注设置";
    $scope.intialize = function () {
        var promise = MenuRemarks.Initialize();
        promise.then(function (data) {
            $scope.RemarkElement = MenuRemarks.RemarkElement;
        }, function (data) {
            console.log(data);
        });
    }
    $scope.OpenEditModel = function (remark) {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalRemarksEdit.html',
                controller: 'ModalRemarksEditCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: MenuRemarks, CurRemark: remark
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.OpenAddModel = function () {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalRemarksAdd.html',
                controller: 'ModalRemarksAddCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: MenuRemarks
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.DeleteRemark = function (remark) {
        MenuRemarks.DeleteRemark(remark);
    }
}])
.controller('ModalRemarksEditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.RemarkElement.CurRemark = option.CurRemark;
    var temp = angular.copy(option.CurRemark);
    $scope.RemarkElement = option.method.RemarkElement;
    $scope.EditRemark = function () {
        var promise = option.method.EditRemark();
        promise.then(function (data) {
            if (data.Status) {
                $uibModalInstance.dismiss('cancel');
            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.cancel = function () {
        for (var i = 0; i < option.method.RemarkElement.Remarks.length; i++) {
            if (option.method.RemarkElement.Remarks[i].Id == temp.Id) option.method.RemarkElement.Remarks[i] = temp;
        }
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ModalRemarksAddCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.RemarkElement.CurRemark = { Price: 0 };
    $scope.RemarkElement = option.method.RemarkElement;
    $scope.AddRemark= function () {
        option.method.AddRemark();
        $uibModalInstance.dismiss('cancel');
    }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('DepartmentCtrl', ['$scope', '$rootScope', '$uibModal', 'Department', function ($scope, $rootScope, $uibModal, Department) {
    $rootScope.FatherPage = "酒店基本管理"; $rootScope.FatherPath = "#/Department"; $rootScope.ChildPage = "部门信息设置";
    $scope.intialize = function () {
        var promise = Department.Initialize();
        promise.then(function (data) {
            $scope.DepartElement = Department.DepartElement;
        }, function (data) {
            console.log(data);
        });
    }
    $scope.OpenEditModel = function (department) {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalDepartmentsEdit.html',
                controller: 'ModalDepartmentsEditCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Department, CurDepartent: department
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.OpenAddModel = function () {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalDepartmentsAdd.html',
                controller: 'ModalDepartmentsAddCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: Department
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.DeleteDepartment = function (department) {
        Department.DeleteDepartment(department);
    }
}])
.controller('ModalDepartmentsEditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.DepartElement.CurDepartent = option.CurDepartent;
    var temp = angular.copy(option.CurDepartent);
    $scope.InitSelect = function () {
        option.method.DepartElement.SelectPrinter = {};
        option.method.DepartElement.Prints.forEach(function (x) {
            if (option.method.DepartElement.CurDepartent.Printer.Id == x.Id) {
                option.method.DepartElement.SelectPrinter = x;
            }
        })
    }
    $scope.DepartElement = option.method.DepartElement;
    $scope.EditDepartment = function () {
        var promise = option.method.EditDepartment();
        promise.then(function (data) {
            if (data.Status) {
                $uibModalInstance.dismiss('cancel');
            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.cancel = function () {
        for (var i = 0; i < option.method.DepartElement.Departments.length; i++) {
            if (option.method.DepartElement.Departments[i].Id == temp.Id) option.method.DepartElement.Departments[i] = temp;
        }
        $uibModalInstance.dismiss('cancel');
    }
})
.controller('ModalDepartmentsAddCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.DepartElement.CurDepartent = {};
    option.method.DepartElement.SelectPrinter = {};
    $scope.DepartElement = option.method.DepartElement;
    $scope.AddDepartment = function () {
        option.method.AddDepartment();
        $uibModalInstance.dismiss('cancel');
    }
    $scope.cancel = function () { $uibModalInstance.dismiss('cancel'); }
})
.controller('MenuClassesCtrl', ['$scope', '$rootScope', 'MenuClass', function ($scope, $rootScope, MenuClass) {
    var temp;
    $rootScope.FatherPage = "菜品管理"; $rootScope.FatherPath = "#/MenuClasses"; $rootScope.ChildPage = "类别设置";
    $scope.Initialize = function () {
        var promise = MenuClass.Initialize();
        promise.then(function (data) {
            $scope.ClassElement = MenuClass.ClassElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.Edit = function (menu) {
        temp = angular.copy(menu);
        menu.IsEdit = true;
    }
    $scope.EditMenu = function (menu) {
        var promise = MenuClass.EditMenu(menu);
        promise.then(function (data) {
            if (data.Status) {
                menu.IsEdit = false;
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.DeleteMenuClass = function (menu) {
        MenuClass.DeleteMenuClass(menu,menu.Level);
    }
    $scope.Add = function (menu) {
        $scope.IsSubClass = true;
        $scope.ClassElement.NewClass.IsAdd = true;
        menu.IsAdd = true;
        menu.IsLeaf = false;
    }
    $scope.AddMenu = function (menu, level, parentId) {
        if (!$scope.ClassElement.NewClass.Id) { alert("编号不能为空"); return; }
        if (!$scope.ClassElement.NewClass.Name) { alert("名称不能为空"); return; }
        var promise = MenuClass.AddClass(level, parentId);
        promise.then(function (data) {
            if (data.Status) {
                menu.IsAdd = false;
            } else {
                alert(data.ErrorMessage);
            }
            $scope.IsSubClass = false;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.AddBigClass = function (level) {
        MenuClass.AddBigClass(level);
    }
    $scope.CancelAdd = function (menu, level) {
        MenuClass.CancelAdd(menu, level);
    }
    $scope.CancelEdit = function (menu) {
        menu.Name = temp.Name;
        menu.IsEdit = false;
    }
}])
.controller('SetMealsCtrl', ['$scope', '$rootScope', '$uibModal', 'SetMeals', function ($scope, $rootScope, $uibModal, SetMeals) {
    $rootScope.FatherPage = "菜品管理"; $rootScope.FatherPath = "#/SetMeals"; $rootScope.ChildPage = "套餐设置";
    $scope.Initialize = function () {
        var promise = SetMeals.Initialize();
        promise.then(function (data) {
            $scope.MealElement = SetMeals.MealElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.OpenEditModel = function (meal) {
        if ($rootScope.IsStaffEdit) {
            var modalInstance = $uibModal.open({//打开修改模块
                animation: $scope.animationsEnabled,
                templateUrl: 'ModalMealEdit.html',
                controller: 'ModalMealEditCtrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    option: {
                        method: SetMeals,CurMeal:meal
                    }
                }
            });
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
    $scope.DeleteMeal = function (meal) {
        if ($rootScope.IsStaffEdit) {
            SetMeals.DeleteMeal(meal);
        }
        else {
            alert("对不起，你没有相关权限，请联系管理员")
        }
    }
}])
.controller('ModalMealEditCtrl', function ($scope, $rootScope, $uibModalInstance, $q, $timeout, option) {
    option.method.MealElement.CurMeal = option.CurMeal;
    var temp = angular.copy(option.CurMeal);
    $scope.MealElement = option.method.MealElement;
    $scope.EditMeal = function () {
        var promise = option.method.EditMeal();
        promise.then(function (data) {
            if (data.Status) {
                $uibModalInstance.dismiss('cancel');
            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.DeleteMenu = function (menu) { option.method.DeleteMenu(menu); }
    $scope.SelectChange = function (menu) { option.method.SelectChange(menu); }
    $scope.NumChange = function () { option.method.NumChange(); }
    $scope.AddMenuInMeal = function () { option.method.AddMenuInMeal(); }
    $scope.cancel = function () {
        for (var i = 0; i < option.method.MealElement.Meals.length; i++) {
            if (option.method.MealElement.Meals[i].MealMenu.Id == temp.MealMenu.Id) option.method.MealElement.Meals[i] = temp;
        }
        $uibModalInstance.dismiss('cancel');
    }
})
