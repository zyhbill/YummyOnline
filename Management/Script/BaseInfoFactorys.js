angular.module('BaseFactory', [])
.factory('Area', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        AreaElement: {
            Areas: [],
            Departments: [],
            ServiceDepartment: {},
            ReciptDepartment: {},
            newArea: {},
            isAjax: false
        },
        intialize: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Baseinfo/getAreas').success(function (data) {
                _this.AreaElement.Areas = data.Areas;
                _this.AreaElement.Departments = data.Departments;
                _this.AreaElement.ServiceDepartment = {};
                _this.AreaElement.ReciptDepartment = {};
                _this.AreaElement.newArea = {};
                _this.AreaElement.Areas.forEach(function (x) {
                    if (_this.AreaElement.Departments.filter(function (s) { return s.Id == x.DepartmentServeId; }).length > 0) {
                        x.ServiceDepartment = _this.AreaElement.Departments.filter(function (s) { return s.Id == x.DepartmentServeId; })[0].Name;
                        _this.AreaElement.ServiceDepartment = _this.AreaElement.Departments.filter(function (s) { return s.Id == x.DepartmentServeId; })[0];
                    }
                    if (_this.AreaElement.Departments.filter(function (s) { return s.Id == x.DepartmentReciptId; }).length > 0) {
                        x.ReciptDepartment = _this.AreaElement.Departments.filter(function (s) { return s.Id == x.DepartmentReciptId; })[0].Name;
                        _this.AreaElement.ReciptDepartment = _this.AreaElement.Departments.filter(function (s) { return s.Id == x.DepartmentReciptId; })[0];
                    }
                })
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data)
            })
            return deferred.promise;
        },
        AddArea: function () {
            this.AreaElement.isAjax = true;
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Baseinfo/AddArea', { area: _this.AreaElement.newArea }).success(function (data) {
                _this.AreaElement.isAjax = false;
                if (data.Status) {
                    _this.AreaElement.newArea.DepartmentServeId = _this.AreaElement.ServiceDepartment.Id;
                    _this.AreaElement.newArea.DepartmentReciptId = _this.AreaElement.ReciptDepartment.Id;
                    _this.AreaElement.newArea.ReciptDepartment = _this.AreaElement.ReciptDepartment.Name;
                    _this.AreaElement.newArea.ServiceDepartment = _this.AreaElement.ServiceDepartment.Name;
                    _this.AreaElement.Areas.push(_this.AreaElement.newArea);
                    _this.AreaElement.newArea = {};
                } else {
                    alert(data.ErrorMessage);
                }
                deferred.resolve(data);
            }).error(function (data) {
                _this.AreaElement.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        DeleteArea: function (area) {
            var _this = this;
            swal({
                title: "确定删除此区域?",
                text: "你将不可恢复此项区域，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: false
            }, function () {
                if (!_this.AreaElement.isAjax) {
                    _this.AreaElement.isAjax = true;
                    $http.post('../Baseinfo/DeleteArea', { AreaId: area.Id }).success(function (data) {
                        _this.AreaElement.isAjax = false;
                        if (data.Status) {
                            _this.AreaElement.Areas.forEach(function (x, index) {
                                if (x.Id == area.Id) _this.AreaElement.Areas.splice(index, 1);
                            });
                            swal("删除成功!", "此项区域已经删除请重新进入支付页面.", "success");
                        }
                    }).error(function (data) {
                        _this.AreaElement.isAjax = false;
                        console.log(data);
                    });
                }
            });
        },
        EditArea: function (area, OriId) {
            var _this = this;
            var deferred = $q.defer();
            this.AreaElement.isAjax = true;
            $http.post('../Baseinfo/EditArea', { Area: area, OriginAreaId: OriId }).success(function (data) {
                _this.AreaElement.isAjax = false;
                deferred.resolve(data);
            }).error(function (data) {
                _this.AreaElement.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        SelectServiceChange: function (area) {
            area.ServiceDepartment = this.AreaElement.ServiceDepartment.Name;
            area.DepartmentServeId = this.AreaElement.ServiceDepartment.Id;
        },
        SelectReciptChange: function (area) {
            area.ReciptDepartment = this.AreaElement.ReciptDepartment.Name;
            area.DepartmentReciptId = this.AreaElement.ReciptDepartment.Id;
        }
    }
    return service;
}]).factory('Desk', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        DeskElement: {
            Desks: [],
            Areas: [],
            SelectArea: {},
            newDesk: {},
            IdFilter: {},
            isAjax: false
        },
        intialize: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Baseinfo/getDesk').success(function (data) {
                _this.DeskElement.Desks = data.Desks;
                _this.DeskElement.Areas = data.Areas;
                _this.DeskElement.Desks.forEach(function (x) {
                    if (_this.DeskElement.Areas.filter(function (s) { return x.AreaId == s.Id }).length > 0) {
                        x.AreaName = _this.DeskElement.Areas.filter(function (s) { return x.AreaId == s.Id })[0].Name;
                    }
                })
                _this.DeskElement.newDesk = {};
                _this.DeskElement.SelectArea = {};
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        DeleteDesk: function (desk) {
            var _this = this;
            swal({
                title: "确定删除此桌台?",
                text: "你将不可恢复此项桌台，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: true
            }, function () {
                if (!_this.DeskElement.isAjax) {
                    _this.DeskElement.isAjax = true;
                    $http.post('../Baseinfo/DeleteDesk', { DeskId: desk.Id }).success(function (data) {
                        _this.DeskElement.isAjax = false;
                        if (data.Status) {
                            _this.DeskElement.Desks.forEach(function (x, index) {
                                if (x.Id == desk.Id) _this.DeskElement.Desks.splice(index, 1);
                            });
                        } else {
                            alert(data.ErrorMessage);
                        }
                    }).error(function (data) {
                        _this.DeskElement.isAjax = false;
                        console.log(data);
                    })
                }
            });
        },
        AddDesk: function (pic) {
            var _this = this;
            this.DeskElement.isAjax = true;
            var deferred = $q.defer();
            $http.post('../Baseinfo/AddDesk', { Desk: _this.DeskElement.newDesk, PicFile: pic }).success(function (data) {
                _this.DeskElement.isAjax = false;
                if (data.Status) {
                    _this.DeskElement.Desks.push(_this.DeskElement.newDesk);
                } else {
                    alert(data.ErrorMessage);
                }
                deferred.resolve(data);
            }).error(function (data) {
                _this.DeskElement.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        EditDesk: function (desk, file, Id) {
            var _this = this;
            this.DeskElement.isAjax = true;
            $http.post('../Baseinfo/EditDesk', { Desk: desk, PicFile: file, OriginId: Id }).success(function (data) {
                if (data.Status) {

                } else {
                    alert(data.ErrorMessage);
                }
                _this.DeskElement.isAjax = false;
            }).error(function (data) {
                _this.DeskElement.isAjax = false;

            })
        },
        AreaChange: function (desk) {
            desk.AreaId = this.DeskElement.SelectArea.Id;
        },
        NewAreaChange: function () {
            this.DeskElement.newDesk.AreaId = this.DeskElement.SelectArea.Id;
            this.DeskElement.newDesk.AreaName = this.DeskElement.SelectArea.Name;
        }

    }
    return service;
}]).factory('Menu', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        MenuElement: {
            Menus: [],
            Remarks: [],
            Classes: [],
            Departments: [],
            IdFilter: {},
            newMenu: {},
            Supply: [],
            CurrentMenu: {},
            SelectClasses: [],
            SelectRemarks: [],
            SelectDepartment: {},
            BaseUrl: {},
            PicZip: {},
            Excel: {},
            isAjax: false
        },
        intialize: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Baseinfo/getMenu').success(function (data) {
                console.log(data);
                _this.MenuElement.Menus = data.Menus;
                _this.MenuElement.Classes = data.Classes;
                _this.MenuElement.Departments = data.Departments;
                _this.MenuElement.Menus.forEach(function (x, index) {
                    x.MenuPrice.Discount *= 100;
                })
                _this.MenuElement.Remarks = data.Remarks;
                _this.MenuElement.IdFilter = {};
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        InitSelect: function () {
            var _this = this;
            _this.MenuElement.SelectClasses = [];
            _this.MenuElement.SelectRemarks = [];
            _this.MenuElement.SelectDepartment = {};
            if (_this.MenuElement.CurrentMenu.Classes) {
                _this.MenuElement.CurrentMenu.Classes.forEach(function (s) {
                    _this.MenuElement.Classes.forEach(function (x) {
                        if (x.Id == s.Id) _this.MenuElement.SelectClasses.push(x);
                    })
                })
                _this.MenuElement.CurrentMenu.Remarks.forEach(function (s) {
                    _this.MenuElement.Remarks.forEach(function (x) {
                        if (x.Id == s.Id) _this.MenuElement.SelectRemarks.push(x);
                    })
                })
                _this.MenuElement.Departments.forEach(function (x) {
                    if (_this.MenuElement.CurrentMenu.DepartmentId == x.Id) {
                        _this.MenuElement.SelectDepartment = x;
                    }

                })
            }
        },
        CalcSupply: function () {
            for (var i = 0 ; i < 7 ; i++) {
                this.MenuElement.Supply[i] = (parseInt(this.MenuElement.CurrentMenu.SupplyDate).toString(2)[i] == 1);
            }
        },
        DeleteMenu: function (menu) {
            var _this = this;
            swal({
                title: "确定删除" + menu.Name + "?",
                text: "你将不可恢复此项菜品，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: true
            }, function () {
                if (!_this.MenuElement.isAjax) {
                    _this.MenuElement.isAjax = true;
                    $http.post('../Baseinfo/DeleteMenu', { MenuId: menu.Id }).success(function (data) {
                        _this.MenuElement.isAjax = false;
                        _this.MenuElement.Menus.forEach(function (x, index) {
                            if (x.Id == menu.Id) _this.MenuElement.Menus.splice(index, 1);
                        })
                    }).error(function (data) {
                        _this.MenuElement.isAjax = false;
                        console.log(data);
                    })
                }
            });
        },
        AddMenu: function () {
            var _this = this;
            this.MenuElement.isAjax = true;
            this.MenuElement.CurrentMenu.SupplyDate = this.GetSupply();
            this.GetDegree();
            var deferred = $q.defer();
            $http.post('../Baseinfo/AddSingleMenu', {
                Menu: _this.MenuElement.CurrentMenu, PicFile: $rootScope.ImgChange, Department: _this.MenuElement.SelectDepartment
            , Classes: _this.MenuElement.SelectClasses, Remarks: _this.MenuElement.SelectRemarks
            }).success(function (data) {
                _this.MenuElement.isAjax = false;
                _this.MenuElement.CurrentMenu.Classes = _this.MenuElement.SelectClasses;
                _this.MenuElement.CurrentMenu.Remarks = _this.MenuElement.SelectRemarks;
                deferred.resolve(data);
            }).error(function (data) {
                _this.MenuElement.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        EditMenu: function (OriMId) {
            var _this = this;
            this.MenuElement.isAjax = true;
            this.MenuElement.CurrentMenu.SupplyDate = this.GetSupply();
            this.GetDegree();
            $http.post('../Baseinfo/EditMenu', {
                Menu: _this.MenuElement.CurrentMenu, PicFile: $rootScope.ImgChange, Departments: _this.MenuElement.SelectDepartment
            , Classes: _this.MenuElement.SelectClasses, Remarks: _this.MenuElement.SelectRemarks, OriId: OriMId
            }).success(function (data) {
                _this.MenuElement.isAjax = false;
                if (data.Status) {
                    _this.MenuElement.CurrentMenu.Classes = _this.MenuElement.SelectClasses;
                    _this.MenuElement.CurrentMenu.Remarks = _this.MenuElement.SelectRemarks;
                } else {
                    alert(data.ErrorMessage);
                }
            }).error(function (data) {
                _this.MenuElement.isAjax = false;
                console.log(data);
            })
        },
        InitDegree: function () {
            $('#sour').val(this.MenuElement.CurrentMenu.SourDegree);
            $('#sweet').val(this.MenuElement.CurrentMenu.SaltyDegree)
            $('#spicy').val(this.MenuElement.CurrentMenu.SpicyDegree);
            $('#salty').val(this.MenuElement.CurrentMenu.SaltyDegree);
        },
        GetDegree: function () {
            this.MenuElement.CurrentMenu.SweetDegree = $('#sweet').val();
            this.MenuElement.CurrentMenu.SpicyDegree = $('#spicy').val();
            this.MenuElement.CurrentMenu.SourDegree = $('#sour').val();
            this.MenuElement.CurrentMenu.SaltyDegree = $('#salty').val();
        },
        GetSupply: function () {
            var num = 0;
            for (var i = 6; i >= 0 ; i--) {
                num = num << 1;
                num += this.MenuElement.Supply[i] ? 1 : 0;
            }
            return num;
        },
        WatchPath: function () {
            return "../Content/ModelImage/" + $rootScope.HotelId + "/" + this.MenuElement.CurrentMenu.Id + ".png";
        },
        Plus: function () {
            this.MenuElement.CurrentMenu.MinOrderCount++;
        },
        Minus: function () {
            if (this.MenuElement.CurrentMenu.MinOrderCount > 0)
                this.MenuElement.CurrentMenu.MinOrderCount--;
        },
        SelectChange: function () {
            this.MenuElement.CurrentMenu.DepartmentId = this.MenuElement.SelectDepartment.Id;
        },
        AllowAdd: function () {
            if (this.MenuElement.isAjax) {
                return true;
            } else {
                if (this.MenuElement.CurrentMenu.MenuPrice.Price) {
                    if (this.MenuElement.CurrentMenu.Id) {
                        if (this.MenuElement.CurrentMenu.Name) {
                            return false;
                        }
                        else {
                            return true;
                        }
                    }
                    else {
                        return true;
                    }
                }
                else {
                    return true;
                }
            }
        },
        AddMutipleMenu: function () {
            var _this = this;
            this.MenuElement.isAjax = true;
            $http.post('../Baseinfo/FileTrs').success(function (data) {
                _this.MenuElement.isAjax = false;
                if (data.Status) {
                    _this.intialize();
                } else {

                }
            }).error(function (data) {
                _this.MenuElement.isAjax = false;
                console.log(data);
            })
        }
    }
    return service;
}])
.factory('StaffRoles', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        StaffRolesElement: {
            Schemas: [{ Name: "读取会员数据", Id: 0 },{ Name: "提交会员订单", Id: 1 },{ Name: "支付模块", Id: 2 }, { Name: "退菜模块", Id: 3 }, { Name: "修改信息模块", Id: 4 }],
            StaffRoles: [],
            SelectSchemes: [],
            CurRole: {},
            NewRole: {},
            isAjax: false
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Baseinfo/getRoles').success(function (data) {
                _this.StaffRolesElement.StaffRoles = data.Roles;
                _this.StaffRolesElement.StaffRoles.forEach(function (x) {
                    _this.StaffRolesElement.Schemas.forEach(function (s) {
                        x.Schemas.forEach(function (p) {
                            if (p.Schema == s.Id) {
                                p.SchemaName = s.Name;
                            }
                        })
                    })
                })
                _this.StaffRolesElement.StaffRoles.forEach(function (x) {
                    x.Schemas = x.Schemas.filter(function (xx) { return xx.SchemaName });
                })
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        InitSelect: function () {
            var _this = this;
            _this.StaffRolesElement.SelectSchemes = [];
            _this.StaffRolesElement.CurRole.Schemas.forEach(function (x) {
                _this.StaffRolesElement.Schemas.forEach(function (xx) {
                    if (xx.Id == x.Schema) _this.StaffRolesElement.SelectSchemes.push(xx);
                })
            })
        },
        EditRoles: function () {
            var deferred = $q.defer();
            var _this = this;
            var PostSchemas = this.StaffRolesElement.SelectSchemes.map(function (x) {
                return x.Id;
            });
            this.StaffRolesElement.isAjax = true;
            $http.post('../Baseinfo/EditRoles', { RoleId: _this.StaffRolesElement.CurRole.Id, Schemas: PostSchemas }).success(function (data) {
                _this.StaffRolesElement.isAjax = false;
                if (data.Status) {
                    _this.StaffRolesElement.CurRole.Schemas = [];
                    if (_this.StaffRolesElement.SelectSchemes.length > 0) {
                        _this.StaffRolesElement.SelectSchemes.forEach(function (x) {
                            _this.StaffRolesElement.CurRole.Schemas.push({ SchemaName: x.Name, Schema: x.Id });
                        })
                    }
                } else {
                    _this.StaffRolesElement.isAjax = false;
                    alert(data.ErrorMessage);
                }
                deferred.resolve(data);
            }).error(function (data) {
                _this.StaffRolesElement.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        DeleteSchema: function (role) {
            var _this = this;
            swal({
                title: "确定删除此" + role.Name + "?",
                text: "你将不可恢复此项角色，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: false
            }, function () {
                _this.StaffRolesElement.isAjax = true;
                $http.post('../Baseinfo/DeleteRoles', { roleId: role.Id }).success(function (data) {
                    _this.StaffRolesElement.isAjax = false;
                    if (data.Status) {
                        _this.StaffRolesElement.StaffRoles.forEach(function (x, index) {
                            if (x.Id == role.Id) _this.StaffRolesElement.StaffRoles.splice(index, 1);
                        })
                    }
                    swal("删除成功!", "角色已删除.", "success");
                }).error(function (data) {
                    console.log(data);
                })
            });
        },
        AddRole: function () {
            var _this = this;
            this.StaffRolesElement.isAjax = true;
            $http.post('../Baseinfo/AddRole', { Name: _this.StaffRolesElement.NewRole.Name, Schemas: _this.StaffRolesElement.SelectSchemes.map(function (x) { return x.Id; }) }).success(function (data) {
                _this.StaffRolesElement.isAjax = false;
                _this.StaffRolesElement.NewRole.Id = data;
                _this.StaffRolesElement.NewRole.Schemas = _this.StaffRolesElement.SelectSchemes;
                _this.StaffRolesElement.NewRole.Schemas.forEach(function (x) {
                    x.SchemaName = x.Name;
                })
                _this.StaffRolesElement.StaffRoles.push(_this.StaffRolesElement.NewRole);
            }).error(function (data) {
                _this.StaffRolesElement.isAjax = false;
                console.log(data);
            })
        }
    }
    return service;

}])
.factory('Staffs', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        StaffElements: {
            Staffs: [],
            SelectRoles: [],
            Roles: [],
            CurStaff: {},
            isAjax: false
        },
        intialize: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Baseinfo/GetStaffs').success(function (data) {
                console.log(data);
                _this.StaffElements.Staffs = data.Staffs;
                _this.StaffElements.Roles = data.Roles;
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        InitSelect: function () {
            var _this = this;
            _this.StaffElements.SelectRoles = [];
            if (_this.StaffElements.CurStaff.Staff.StaffRoles) {
                _this.StaffElements.CurStaff.Staff.StaffRoles.forEach(function (x) {
                    _this.StaffElements.Roles.forEach(function (xx) {
                        if (xx.Id == x.Id) _this.StaffElements.SelectRoles.push(xx);
                    })
                })
            }
        },
        EditStaffs: function () {
            var deferred = $q.defer();
            var _this = this;
            if (_this.StaffElements.SelectRoles.length > 0) {
                var newRoles = _this.StaffElements.SelectRoles.map(function (x) { return x.Id; });
            }
            this.StaffElements.isAjax = true;
            $http.post('../Baseinfo/EditStaffs', {
                Sf: _this.StaffElements.CurStaff.SysStaff,
                Sfh: _this.StaffElements.CurStaff.Staff,
                roles: newRoles
            }).success(function (data) {
                _this.StaffElements.isAjax = false;
                _this.StaffElements.CurStaff.Staff.StaffRoles = _this.StaffElements.SelectRoles;
                deferred.resolve(data);
            }).error(function (data) {
                _this.StaffElements.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        DeleteStaff: function (staff) {
            var _this = this;
            swal({
                title: "确定删除此" + staff.Staff.Name + "?",
                text: "你将不可恢复此员工，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: false
            }, function () {
                _this.StaffElements.isAjax = true;
                $http.post('../Baseinfo/DeleteStaff', { Id: staff.Staff.Id }).success(function (data) {
                    _this.StaffElements.isAjax = false;
                    if (data.Status) {
                        _this.StaffElements.Staffs.forEach(function (x, index) {
                            if (x.Staff.Id == staff.Staff.Id) {
                                _this.StaffElements.Staffs.splice(index, 1);
                            }
                        })
                    } else {
                        alert(data.ErrorMessage);
                    }
                    swal("删除成功!", "员工已删除.", "success");
                }).error(function (data) {
                    console.log(data);
                })
            });
        },
        AddStaff: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Baseinfo/AddStaff', {
                Sf: _this.StaffElements.CurStaff.SysStaff,
                Sfh: _this.StaffElements.CurStaff.Staff,
                roles: _this.StaffElements.SelectRoles.map(function (x) { return x.Id; })
            }).success(function (data) {
                if (data.Status) {
                    _this.StaffElements.CurStaff.Staff.Id = data.Id;
                    _this.StaffElements.CurStaff.SysStaff.Id = data.Id;
                    _this.StaffElements.CurStaff.Staff.StaffRoles = _this.StaffElements.SelectRoles;
                    _this.StaffElements.Staffs.push(_this.StaffElements.CurStaff);
                }
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        }
    }
    return service;
}])
.factory('MenuRemarks', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        RemarkElement: {
            Remarks: [],
            CurRemark: {},
            isAjax: false
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Baseinfo/getRemarks').success(function (data) {
                _this.RemarkElement.Remarks = data.Remarks;
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        AddRemark: function () {
            var _this = this;
            this.RemarkElement.isAjax = true;
            $http.post('../Baseinfo/AddRemark', {
                Name: _this.RemarkElement.CurRemark.Name,
                Price: _this.RemarkElement.CurRemark.Price
            }).success(function (data) {
                _this.RemarkElement.isAjax = false;
                _this.RemarkElement.CurRemark.Id = data.Id;
                _this.RemarkElement.Remarks.push(_this.RemarkElement.CurRemark);
            }).error(function (data) {
                _this.RemarkElement.isAjax = false;
                console.log(data);
            })
        },
        EditRemark: function () {
            var _this = this;
            var deferred = $q.defer();
            this.RemarkElement.isAjax = true;
            $http.post('../Baseinfo/editRemarks', {
                Id: _this.RemarkElement.CurRemark.Id,
                Name: _this.RemarkElement.CurRemark.Name,
                Price: _this.RemarkElement.CurRemark.Price
            }).success(function (data) {
                _this.RemarkElement.isAjax = false;
                deferred.resolve(data);
            }).error(function (data) {
                _this.RemarkElement.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        DeleteRemark: function (remark) {
            var _this = this;
            swal({
                title: "确定删除此" + remark.Name + "?",
                text: "你将不可恢复此项备注，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: false
            }, function () {
                $http.post('../Baseinfo/DeleteRemark', { Id: remark.Id }).success(function (data) {
                    if (data.Status) {
                        _this.RemarkElement.Remarks.forEach(function (x, index) {
                            if (x.Id == remark.Id) _this.RemarkElement.Remarks.splice(index, 1);
                        })
                    } else {
                        alert(data.ErrorMessage);
                    }
                    swal("删除成功!", "备注已删除.", "success");
                }).error(function (data) {
                    console.log(data);
                })
            });
        }
    }
    return service;
}])
.factory('Department', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        DepartElement: {
            Prints: [],
            Departments: [],
            CurDepartent: {},
            SelectPrinter: {},
            isAjax: false
        },
        Initialize: function () {
            var _this = this;
            var deferred = $q.defer();
            $http.post('../Baseinfo/getDepartment').success(function (data) {
                _this.DepartElement.Departments = data.Departments;
                _this.DepartElement.Prints = data.Prints;
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        InitSelect:function(){
            var _this = this;
            this.DepartElement.Prints.forEach(function (x) {
                if (_this.DepartElement.CurDepartent.Printer.Id = x.Id) {
                    _this.DepartElement.SelectPrinter = x;
                }
            })
        },
        DeleteDepartment: function (department) {
            var _this = this;
            swal({
                title: "确定删除此" + department.Name + "?",
                text: "你将不可恢复此项部门，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: false
            }, function () {
               $http.post('../Baseinfo/DeleteDepartment', { Id: department.Id })
               .success(function (data) {
                   if (data.Status) {
                       _this.DepartElement.Departments.forEach(function (x, index) {
                           if (x.Id == department.Id) _this.DepartElement.Departments.splice(index, 1);
                       })
                   } else {
                       alert(data.ErrorMessage);
                   }
                   swal("删除成功!", "部门已删除.", "success");
               }).error(function (data) {
                   console.log(data);
               })
            });
        },
        EditDepartment: function () {
            var deferred = $q.defer();
            var _this = this;
            _this.DepartElement.isAjax = true;
            $http.post('../Baseinfo/EditDepartment', {
                Id: _this.DepartElement.CurDepartent.Id,
                Name: _this.DepartElement.CurDepartent.Name,
                Description: _this.DepartElement.CurDepartent.Description,
                PrintId: _this.DepartElement.SelectPrinter.Id
            }).success(function (data) {
                _this.DepartElement.isAjax = false;
                if (_this.DepartElement.SelectPrinter.Id) {
                    _this.DepartElement.CurDepartent.Printer = _this.DepartElement.SelectPrinter;
                }
                deferred.resolve(data);
            }).error(function (data) {
                _this.DepartElement.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        AddDepartment: function () {
            var _this = this;
            _this.DepartElement.isAjax = true;
            $http.post('../Baseinfo/AddDepartment', {
                Name: _this.DepartElement.CurDepartent.Name,
                Description: _this.DepartElement.CurDepartent.Description,
                PrintId: _this.DepartElement.SelectPrinter.Id
            }).success(function (data) {
                _this.DepartElement.isAjax = false;
                if (data.Status) {
                    _this.DepartElement.CurDepartent.Id = data.Id;
                    _this.DepartElement.CurDepartent.Printer = _this.DepartElement.SelectPrinter;
                    _this.DepartElement.Departments.push(_this.DepartElement.CurDepartent);
                }
            }).error(function (data) {
                _this.DepartElement.isAjax = false;
                console.log(data);
            })
        },
        InitSelect: function () {
            var _this = this;
            this.DepartElement.SelectPrinter = {};
            this.DepartElement.Prints.forEach(function (x) {
                if (x.Id == _this.DepartElement.CurDepartent.PrintId) {
                    _this.DepartElement.SelectPrinter = x;
                }
            })
        }
    }
    return service;
}])
.factory('MenuClass', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        ClassElement: {
            FirstMenuClasses: [],
            SecondMenuClasses: [],
            ThirdMenuClasses: [],
            NewBigClass:{},
            NewClass: {},
            isAjax: false
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Baseinfo/GetMenuclasses').success(function (data) {
                console.log(data);
                _this.ClassElement.FirstMenuClasses = data.FirstMenuClasses;
                _this.ClassElement.SecondMenuClasses = data.SecondMenuClasses;
                _this.ClassElement.ThirdMenuClasses = data.ThirdMenuClasses;
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        EditMenu: function (menu) {
            if (!this.ClassElement.isAjax) {
                var _this = this;
                var deferred = $q.defer();
                this.ClassElement.isAjax = true;
                $http.post('../Baseinfo/EditMenuClass', {
                    Id: menu.Id, Name: menu.Name
                }).success(function (data) {
                    _this.ClassElement.isAjax = false;
                    deferred.resolve(data);
                }).error(function (data) {
                    _this.ClassElement.isAjax = false;
                    deferred.reject(data);
                })
                return deferred.promise;
            }
        },
        DeleteMenuClass: function (menu, level) {
            var _this = this;
            swal({
                title: "确定删除此" + menu.Name + "?",
                text: "你将不可恢复此项类别，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: false
            }, function () {
                if (!_this.ClassElement.isAjax) {
                    _this.ClassElement.isAjax = true;
                    $http.post('../Baseinfo/DeleteMenuClass', {
                        Id: menu.Id
                    }).success(function (data) {
                        _this.ClassElement.isAjax = false;
                        if (data.Status) {
                            if (level == 0) {
                                _this.ClassElement.FirstMenuClasses.forEach(function (x, i) {
                                    if (x.Id == menu.Id) _this.ClassElement.FirstMenuClasses.splice(i, 1);
                                })
                            } else if (level == 1) {
                                var father = _this.ClassElement.SecondMenuClasses.filter(function (x) {
                                    return x.ParentMenuClassId == menu.ParentMenuClassId;
                                })
                                if (father.length == 1) {
                                    _this.ClassElement.FirstMenuClasses.forEach(function (x) {
                                        if (x.Id == menu.ParentMenuClassId) {
                                            x.IsLeaf = true;
                                        }
                                    })
                                }
                                _this.ClassElement.SecondMenuClasses.forEach(function (x, i) {
                                    if (x.Id == menu.Id) _this.ClassElement.SecondMenuClasses.splice(i, 1);
                                })
                            } else if (level == 2) {
                                var father = _this.ClassElement.ThirdMenuClasses.filter(function (x) {
                                    return x.ParentMenuClassId == menu.ParentMenuClassId;
                                })
                                if (father.length == 1) {
                                    _this.ClassElement.SecondMenuClasses.forEach(function (x) {
                                        if (x.Id == menu.ParentMenuClassId) {
                                            x.IsLeaf = true;
                                        }
                                    })
                                }
                                _this.ClassElement.ThirdMenuClasses.forEach(function (x, i) {
                                    if (x.Id == menu.Id) _this.ClassElement.ThirdMenuClasses.splice(i, 1);
                                })
                            }
                            swal("删除成功!", "类别已删除.", "success");
                        } else {
                            alert(data.ErrorMessage);
                        }
                    }).error(function (data) {
                        _this.ClassElement.isAjax = false;
                        console.log(data);
                    })
                }
            });
        },
        AddClass: function (level, ParentId) {
            var _this = this;
            var deferred = $q.defer();
            if (!this.ClassElement.isAjax) {
                this.ClassElement.isAjax = true;
                $http.post('../Baseinfo/AddMenuClass', {
                    Id: _this.ClassElement.NewClass.Id,
                    Name: _this.ClassElement.NewClass.Name,
                    Description: _this.ClassElement.NewClass.Description,
                    ParentId: ParentId
                }).success(function (data) {
                    _this.ClassElement.isAjax = false;
                    if (data.Status) {
                        _this.ClassElement.NewClass.Level = level;
                        _this.ClassElement.NewClass.IsLeaf = true;
                        _this.ClassElement.NewClass.ParentMenuClassId = ParentId;
                        if (level == 0) {
                            _this.ClassElement.FirstMenuClasses.push(_this.ClassElement.NewClass);
                        } else if (level == 1) {
                            _this.ClassElement.FirstMenuClasses.forEach(function (x) {
                                if (x.Id == ParentId) {
                                    x.IsLeaf = false;
                                }
                            })
                            _this.ClassElement.SecondMenuClasses.push(_this.ClassElement.NewClass);
                        } else if (level == 2) {
                            _this.ClassElement.SecondMenuClasses.forEach(function (x) {
                                if (x.Id == ParentId) {
                                    x.IsLeaf = false;
                                }
                            })
                            _this.ClassElement.ThirdMenuClasses.push(_this.ClassElement.NewClass);
                        }
                        _this.ClassElement.NewClass = {};
                    } else {
                    }
                    deferred.resolve(data);
                }).error(function (data) {
                    _this.ClassElement.isAjax = false;
                    deferred.reject(data);
                })
            }
            return deferred.promise;
        },
        AddBigClass: function (level) {
            var _this = this;
            var deferred = $q.defer();
            if (!this.ClassElement.isAjax) {
                this.ClassElement.isAjax = true;
                $http.post('../Baseinfo/AddMenuClass', {
                    Id: _this.ClassElement.NewBigClass.Id,
                    Name: _this.ClassElement.NewBigClass.Name,
                    Description: _this.ClassElement.NewBigClass.Description,
                    ParentId: null
                }).success(function (data) {
                    _this.ClassElement.isAjax = false;
                    if (data.Status) {
                        _this.ClassElement.NewBigClass.Level = level;
                        _this.ClassElement.NewBigClass.IsLeaf = true;
                        _this.ClassElement.NewBigClass.ParentMenuClassId = null;
                        if (level == 0) {
                            _this.ClassElement.FirstMenuClasses.push(_this.ClassElement.NewBigClass);
                        }
                        _this.ClassElement.NewBigClass = {};
                    } else {
                        alert(data.ErrorMessage);
                    }
                    deferred.resolve(data);
                }).error(function (data) {
                    _this.ClassElement.isAjax = false;
                    deferred.reject(data);
                })
            }
            return deferred.promise;
        },
        CancelAdd: function (menu, level) {
            //this.ClassElement.NewClass.IsAdd = false;
            menu.IsAdd = false;
            if (level == 1) {
                var father = this.ClassElement.SecondMenuClasses.filter(function (x) {
                    return x.ParentMenuClassId == menu.Id;
                })
                if (father.length == 0) {
                    menu.IsLeaf = true;
                }
            } else if (level == 2) {
                var father = this.ClassElement.ThirdMenuClasses.filter(function (x) {
                    return x.ParentMenuClassId == menu.Id;
                })
                if (father.length == 0) {
                    menu.IsLeaf = true;
                }
            }
            this.ClassElement.NewClass = {};
        }
    }
    return service;
}])
.factory('SetMeals', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        MealElement: {
            Meals: [],
            Menus: [],
            CurMeal: {},
            FilterInfo: "",
            AddMenu: {},
            isAjax: false
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Baseinfo/getSetMeal').success(function (data) {
                angular.forEach(data.Menus, function (x) {
                    x.Num = x.MinOrderCount;
                });
                console.log(data);
                _this.MealElement.Meals = data.Meals;
                _this.MealElement.Menus = data.Menus;
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        EditMeal: function () {
            var deferred = $q.defer();
            var _this = this;
            var MenusInMeal = this.MealElement.CurMeal.Menus.map(function (x) {
                return { MenuId: x.Menu.Id, Count: x.Count };
            })
            _this.MealElement.isAjax = true;
            $http.post('../Baseinfo/EditMenusInMeal', {
                MealId: _this.MealElement.CurMeal.MealMenu.Id,
                Menus: MenusInMeal,
                Name: _this.MealElement.CurMeal.MealMenu.Name,
                Price: _this.MealElement.CurMeal.MealMenu.MenuPrice.Price
            }).success(function (data) {
                _this.MealElement.isAjax = false;
                deferred.resolve(data);
            }).error(function (data) {
                _this.MealElement.isAjax = false;
                deferred.reject(data);
            })
            return deferred.promise;
        },
        DeleteMenu: function (menu) {
            var _this = this;
            $http.post('../Baseinfo/deleteMealMenu', {
                MealId: _this.MealElement.CurMeal.MealMenu.Id,
                MenuId: menu.Menu.Id
            }).success(function (data) {
                if (data.Status) {
                    _this.MealElement.CurMeal.Menus.forEach(function (x, index) {
                        if (x.Menu.Id == menu.Menu.Id) _this.MealElement.CurMeal.Menus.splice(index, 1);
                    })
                }
            }).error(function (data) {
                console.log(data);
            })
        },
        DeleteMeal: function (meal) {
            var _this = this;
            var deferred = $q.defer();
            swal({
                title: "确定删除此" + meal.MealMenu.Name + "?",
                text: "你将不可恢复此项套餐，请谨慎操作!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#D0D0D0",
                confirmButtonText: "是, 删除!",
                cancelButtonText: "否, 保留!",
                closeOnConfirm: false
            }, function () {
                $http.post('../Baseinfo/DeleteMeal', {
                    MealId: meal.MealMenu.Id
                }).success(function (data) {
                    if (data.Status) {
                        _this.MealElement.Meals.forEach(function (x, index) {
                            if (x.MealMenu.Id == meal.MealMenu.Id) _this.MealElement.Meals.splice(index, 1);
                        })
                    }
                    swal("删除成功!", "套餐已删除.", "success");
                }).error(function (data) {
                    console.log(data);
                })
            });
        },
        SelectChange: function (menu) {
            this.MealElement.AddMenu = menu;
            this.MealElement.FilterInfo = menu.Name;
            this.MealElement.AddMenu.Num = this.MealElement.AddMenu.MinOrderCount;
        },
        NumChange: function () {
            if (this.MealElement.AddMenu.Num < this.MealElement.AddMenu.MinOrderCount) this.MealElement.AddMenu.Num = this.MealElement.AddMenu.MinOrderCount
        },
        AddMenuInMeal: function () {
            var temp = angular.copy(this.MealElement.AddMenu);
            var AllowAdd = true;
            this.MealElement.CurMeal.Menus.forEach(function (x) {
                if (x.Menu.Id == temp.Id) {
                    AllowAdd = false;
                    alert('已有相同菜品，删除后添加');
                    return;
                }
            })
            if (AllowAdd&&temp.Id) this.MealElement.CurMeal.Menus.push({ Count: temp.Num, Menu: temp });
            this.MealElement.AddMenu = {};
            this.MealElement.FilterInfo = "";
        }
    }
    return service;
}])
.factory('HandPicture', ['$q', function ($q) {
    var service = {
        getPath: function (file) {
            var deferred = $q.defer();
            var reader = new FileReader();
            reader.onloadend = function () {
                deferred.resolve(reader.result);
            }
            reader.readAsDataURL(file);
            return deferred.promise;
        }
    }
    return service;
}])
.factory('Print', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        PrintElement: {
            Printers: [],
            PrinterFormat: {},
            CurrentPrinter: {},
            CurrentFont:{},
            Fonts: [{Name:"宋体"}, {Name:"黑体"}],
            OldFormat: {},
            CurrentFormat:{
                KitchenOrderFontSize:0,
                KitchenOrderSmallFontSize:0,
                PaperSize:0,
                ReciptBigFontSize:0,
                ReciptFontSize:0,
                ReciptSmallFontSize:0,
                ServeOrderFontSize:0,
                ServeOrderSmallFontSize:0,
                ShiftBigFontSize:0,
                ShiftFontSize:0,
                ShiftSmallFontSize:0
            },
            UsePrint: true,
            IsPayFirst:true,
            Rate:0
        },
        Initialize: function () {
            var _this = this;
            $http.post('../Baseinfo/getPrint').success(function (data) {
                _this.PrintElement.Printers = data.Printers;
                _this.PrintElement.PrinterFormat = data.Format;
                _this.PrintElement.Printers.forEach(function (x) {
                    if (x.Id == data.AccountPrint) _this.PrintElement.CurrentPrinter = x;
                })
                console.log(data);
                _this.PrintElement.OldFormat = data.font;
                _this.PrintElement.IsPayFirst = data.IsPayFirst;
                _this.PrintElement.CurrentFont = { Name: data.font.Font };
                _this.PrintElement.CurrentFormat = {
                    KitchenOrderFontSize: data.font.KitchenOrderFontSize,
                    KitchenOrderSmallFontSize: data.font.KitchenOrderSmallFontSize,
                    PaperSize: data.font.PaperSize,
                    ReciptBigFontSize: data.font.ReciptBigFontSize,
                    ReciptFontSize: data.font.ReciptFontSize,
                    ReciptSmallFontSize: data.font.ReciptSmallFontSize,
                    ServeOrderFontSize: data.font.ServeOrderFontSize,
                    ServeOrderSmallFontSize: data.font.ServeOrderSmallFontSize,
                    ShiftBigFontSize: data.font.ShiftBigFontSize,
                    ShiftFontSize: data.font.ShiftFontSize,
                    ShiftSmallFontSize: data.font.ShiftSmallFontSize
                }
                _this.PrintElement.UsePrint = data.IsUsePrinter;
                _this.PrintElement.Rate = data.Rate;
            });
        },
        changeInfo: function () {
            var _this = this;
            $http.post('../Baseinfo/ChangePrintFormat', {
                Format: _this.PrintElement.CurrentFormat,
                Font: _this.PrintElement.CurrentFont,
                Rate: _this.PrintElement.Rate,
                IsUsePrint: _this.PrintElement.UsePrint,
                ShiftPrintId: _this.PrintElement.CurrentPrinter.Id,
                IsPayFirst:_this.PrintElement.IsPayFirst
            }).success(function (data) {
                $rootScope.IsPayFirst = _this.PrintElement.IsPayFirst;
                alert("保存成功");
            })
        }
    }
    return service;
}])