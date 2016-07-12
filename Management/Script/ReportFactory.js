angular.module('ReportFactory', [])
.factory('MenuSale', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        SaleElement: {
            DateStart: null,
            DateEnd: null,
            SalesData: [],
            isAjax: false,
            FatherClasses: [],
            ChildClasses: [],
            Menus: [],
            OriMenus: [],
            SelectFather: [],
            SelectChild: [],
            Options: [],
            SelectOption: {},
            Sum:[]
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Reports/GetClasses').success(function (data) {
                _this.SaleElement.FatherClasses = data.FatherClass;
                _this.SaleElement.ChildClasses = data.ChildClass;
                _this.SaleElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
                if (data.ChildClass.length > 0) {
                    _this.SaleElement.ChildClasses.forEach(function (x) {
                        x.IsChoose = true;
                    })
                    _this.SaleElement.Options = [{ Name: '菜品', Id: 0 }, { Name: '一级类', Id: 1 }, { Name: '二级类', Id: 2 }]
                } else {
                    _this.SaleElement.Options = [{ Name: '菜品', Id: 0 }, { Name: '一级类', Id: 1 }]
                }
                _this.SaleElement.Menus = data.Menus;
                _this.SaleElement.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
                _this.SaleElement.OriMenus = _this.SaleElement.Menus;
                _this.SaleElement.SelectOption = _this.SaleElement.Options[0];
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        Search: function () {
            var deferred = $q.defer();
            var _this = this;
            this.SaleElement.isAjax = true;
            var menu = this.SaleElement.Menus.filter(function(x){return x.IsChoose}).map(function(x){return x.Id})
            $http.post('../Reports/getMenuSales', {
                Begin: _this.SaleElement.DateStart,
                End: _this.SaleElement.DateEnd,
                Menus: menu,
                Type:0
            }).success(function (data) {
                _this.SaleElement.isAjax = false;
                _this.SaleElement.SalesData = data.SalesData;
                _this.SaleElement.Sum = data.Sum;
                var datas = [];
                _this.SaleElement.SalesData.forEach(function (x) {
                    datas.push({ name: x.Menu.Name, data: [x.TotalPrice] });
                })
                $('#SaleReport').highcharts({
                    chart: {
                        type: 'column'
                    },
                    title: {
                        text: '菜品销售'
                    },
                    subtitle: {
                        text: '来自: 美味在线'
                    },
                    xAxis: {
                        categories: ['销售数据']
                    },
                    yAxis: {
                        min: 0,
                        title: {
                            text: '单位 (元)'
                        }
                    },
                    tooltip: {
                        headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                        pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                        '<td style="padding:0"><b>{point.y:.1f} 元</b></td></tr>',
                        footerFormat: '</table>',
                        shared: true,
                        useHTML: true
                    },
                    plotOptions: {
                        column: {
                            pointPadding: 0.2,
                            borderWidth: 0
                        }
                    },
                    series: datas
                });
                console.log(data);
            })
        },
        AllSelect: function () {
            if (this.SaleElement.SelectOption.Id == 0) {
                //menu
                this.SaleElement.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else if (this.SaleElement.SelectOption.Id == 1) {
                //father class
                this.SaleElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else {
                this.SaleElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            }
        },
        NoneSelect: function () {
            if (this.SaleElement.SelectOption.Id == 0) {
                //menu
                this.SaleElement.Menus.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else if (this.SaleElement.SelectOption.Id == 1) {
                //father class
                this.SaleElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else {
                this.SaleElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            }
        },
        ReverseSelect: function () {
            if (this.SaleElement.SelectOption.Id == 0) {
                //menu
                this.SaleElement.Menus.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else if (this.SaleElement.SelectOption.Id == 1) {
                //father class
                this.SaleElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else {
                this.SaleElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })

            }
        },
        SelectChange: function () {
            var _this = this;
            this.SaleElement.Menus = this.SaleElement.OriMenus.filter(function (x) {
                x.IsChoose = false;
                x.Classes.forEach(function (xx) {
                    _this.SaleElement.FatherClasses.filter(function (a) { return a.IsChoose; }).forEach(function (xxx) {
                        if (xx.Id == xxx.Id) {
                            x.IsChoose = true;
                            return;
                        }
                    })
                    if (x.IsChoose) return;
                });
                x.Classes.forEach(function (xx) {
                    _this.SaleElement.ChildClasses.filter(function (a) { return a.IsChoose; }).forEach(function (xxx) {
                        if (xx.Id == xxx.Id) {
                            x.IsChoose = true;
                            return;
                        }
                    })
                    if (x.IsChoose) return;
                })
                return x.IsChoose;
            })
        }
    }
    return service;
}])
.factory('MenuSaleClass', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        SaleElement: {
            DateStart: null,
            DateEnd: null,
            SalesData: [],
            isAjax: false,
            FatherClasses: [],
            ChildClasses: [],
            SelectFather: [],
            SelectChild: [],
            Options: [],
            SelectOption: {},
            Sum: []
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Reports/GetClasses').success(function (data) {
                _this.SaleElement.FatherClasses = data.FatherClass;
                _this.SaleElement.ChildClasses = data.ChildClass;
                _this.SaleElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
                if (data.ChildClass.length > 0) {
                    _this.SaleElement.ChildClasses.forEach(function (x) {
                        x.IsChoose = true;
                    })
                    _this.SaleElement.Options = [ { Name: '一级类', Id: 1 }, { Name: '二级类', Id: 2 }]
                } else {
                    _this.SaleElement.Options = [{ Name: '一级类', Id: 1 }]
                }
                _this.SaleElement.SelectOption = _this.SaleElement.Options[0];
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        Search: function () {
            var deferred = $q.defer();
            var _this = this;
            this.SaleElement.isAjax = true;
            var Classes = null;
            if (this.SaleElement.SelectOption.Id == 1) {
                Classes = this.SaleElement.FatherClasses.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id })
            } else if (this.SaleElement.SelectOption.Id == 2)
            {
                Classes = this.SaleElement.ChildClasses.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id })
            }
            $http.post('../Reports/getMenuSales', {
                Begin: _this.SaleElement.DateStart,
                End: _this.SaleElement.DateEnd,
                Classes : Classes,
                Type:_this.SaleElement.SelectOption.Id
            }).success(function (data) {
                _this.SaleElement.isAjax = false;
                _this.SaleElement.SalesData = data.SalesData;
                _this.SaleElement.Sum = data.Sum;
                var datas = [];
                _this.SaleElement.SalesData.forEach(function (x) {
                    datas.push({ name: x.MenuClass.Name, data: [x.Price] });
                })
                $('#SaleReport').highcharts({
                    chart: {
                        type: 'column'
                    },
                    title: {
                        text: '菜品销售'
                    },
                    subtitle: {
                        text: '来自: 美味在线'
                    },
                    xAxis: {
                        categories: ['销售数据']
                    },
                    yAxis: {
                        min: 0,
                        title: {
                            text: '单位 (元)'
                        }
                    },
                    tooltip: {
                        headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                        pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                        '<td style="padding:0"><b>{point.y:.1f} 元</b></td></tr>',
                        footerFormat: '</table>',
                        shared: true,
                        useHTML: true
                    },
                    plotOptions: {
                        column: {
                            pointPadding: 0.2,
                            borderWidth: 0
                        }
                    },
                    series: datas
                });
                console.log(data);
            })
        },
        AllSelect: function () {
            if (this.SaleElement.SelectOption.Id == 0) {
                //menu
                this.SaleElement.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else if (this.SaleElement.SelectOption.Id == 1) {
                //father class
                this.SaleElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else {
                this.SaleElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            }
        },
        NoneSelect: function () {
            if (this.SaleElement.SelectOption.Id == 1) {
                //father class
                this.SaleElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else {
                this.SaleElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            }
        },
        ReverseSelect: function () {
            if (this.SaleElement.SelectOption.Id == 1) {
                //father class
                this.SaleElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else {
                this.SaleElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })

            }
        }
    }
    return service;
}])
.factory('KindType', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        KindElement: {
            DateStart: {},
            DateEnd: {},
            KindTypeData: [],
            Details: [],
            PayKinds: [],
            sumPrice: [],
            isAjax: false
        },
        Search: function () {
            var deferred = $q.defer();
            var _this = this;
            this.KindElement.isAjax = true;
            $http.post('../Reports/PaykindType', {
                Begin: _this.KindElement.DateStart,
                End: _this.KindElement.DateEnd
            }).success(function (data) {
                _this.KindElement.isAjax = false;
                _this.KindElement.KindTypeData = data.KindTypeData.filter(function (x) { return x.Total; });
                _this.KindElement.Details = data.Details;
                _this.KindElement.sumPrice = data.sumPrice;
                if (_this.KindElement.Details.length > 0) {
                    _this.KindElement.PayKinds = _this.KindElement.Details[0].Detail.map(function (x) {
                        return x.PayKind.Name;
                    })
                } else {
                    _this.KindElement.PayKinds = [];
                }
                var datas = _this.KindElement.KindTypeData.map(function (x) {
                    return { name: x.PayKind.Name, y: x.Point };
                })
                $('#DataReport').highcharts({
                    chart: {
                        plotBackgroundColor: null,
                        plotBorderWidth: null,
                        plotShadow: false
                    },
                    title: {
                        text: '美味在线，收银分类汇总表'
                    },
                    tooltip: {
                        pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
                    },
                    plotOptions: {
                        pie: {
                            allowPointSelect: true,
                            cursor: 'pointer',
                            dataLabels: {
                                enabled: true,
                                color: '#000000',
                                connectorColor: '#000000',
                                format: '<b>{point.name}</b>: {point.percentage:.1f} %'
                            }
                        }
                    },
                    series: [{
                        type: 'pie',
                        name: '占比',
                        data: datas
                    }]
                });
                console.log(data);
            }).error(function (data) {
                _this.KindElement.isAjax = false;
                console.log(data);
            })
        }
    }
    return service;
}])
.factory('SaleClass', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        ClassElement: {
            FatherClasses: [],
            ChildClasses: [],
            Menus: [],
            OriMenus: [],
            SelectFather: [],
            SelectChild: [],
            Options: [],
            SelectOption: {},
            SelectArea:{},
            Areas: [{Id:"all",Name:"所有"}],
            Datas: [],
            Sum:[],
            BeginTime: "00:00:00",
            EndTime: "23:59:59"
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Reports/GetClasses').success(function (data) {
                _this.ClassElement.FatherClasses = data.FatherClass;
                _this.ClassElement.ChildClasses = data.ChildClass;
                _this.ClassElement.Areas = [{ Id: "all", Name: "所有" }]
                data.Areas.forEach(function (x) {
                    _this.ClassElement.Areas.push(x);
                })
                _this.ClassElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
                if (data.ChildClass.length > 0) {
                    _this.ClassElement.ChildClasses.forEach(function (x) {
                        x.IsChoose = true;
                    })
                    _this.ClassElement.Options = [{ Name: '菜品', Id: 0 }, { Name: '一级类', Id: 1 }, { Name: '二级类', Id: 2 }]
                } else {
                    _this.ClassElement.Options = [{ Name: '菜品', Id: 0 }, { Name: '一级类', Id: 1 }]
                }
                _this.ClassElement.Menus = data.Menus;
                _this.ClassElement.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
                _this.ClassElement.OriMenus = _this.ClassElement.Menus;
                _this.ClassElement.SelectOption = _this.ClassElement.Options[0];
                _this.ClassElement.SelectArea = _this.ClassElement.Areas[0];
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        AllSelect: function () {
            if (this.ClassElement.SelectOption.Id == 0) {
                //menu
                this.ClassElement.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else if (this.ClassElement.SelectOption.Id == 1) {
                //father class
                this.ClassElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else {
                this.ClassElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            }
        },
        NoneSelect: function () {
            if (this.ClassElement.SelectOption.Id == 0) {
                //menu
                this.ClassElement.Menus.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else if (this.ClassElement.SelectOption.Id == 1) {
                //father class
                this.ClassElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else {
                this.ClassElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            }
        },
        ReverseSelect: function () {
            if (this.ClassElement.SelectOption.Id == 0) {
                //menu
                this.ClassElement.Menus.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else if (this.ClassElement.SelectOption.Id == 1) {
                //father class
                this.ClassElement.FatherClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else {
                this.ClassElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })

            }
        },
        TypeChange: function () {
            var _this = this;
            if (this.ClassElement.SelectOption.Id == 0) {
                this.ClassElement.Menus = this.ClassElement.OriMenus.filter(function (x) {
                    x.IsChoose = false;
                    x.Classes.forEach(function (xx) {
                        _this.ClassElement.FatherClasses.filter(function (a) { return a.IsChoose; }).forEach(function (xxx) {
                            if (xx.Id == xxx.Id) {
                                x.IsChoose = true;
                                return;
                            }
                        })
                        if (x.IsChoose) return;
                    });
                    x.Classes.forEach(function (xx) {
                        _this.ClassElement.ChildClasses.filter(function (a) { return a.IsChoose; }).forEach(function (xxx) {
                            if (xx.Id == xxx.Id) {
                                x.IsChoose = true;
                                return;
                            }
                        })
                        if (x.IsChoose) return;
                    })
                    return x.IsChoose;
                })
            }
        },
        Search: function () {
            var _this = this;
            var deferred = $q.defer();
            var father = null, child = null, menus = null,areas = null;
            if (this.ClassElement.SelectOption.Id == 0) {
                //menu
                menus = this.ClassElement.Menus.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            else if (this.ClassElement.SelectOption.Id == 1) {
                father = this.ClassElement.FatherClasses.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            else {
                this.ClassElement.ChildClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
                this.ClassElement.FatherClasses.filter(function (x) { return x.IsChoose }).forEach(function (x) {
                    _this.ClassElement.ChildClasses.filter(function (xx) { return xx.ParentMenuClassId == x.Id }).forEach(function (xx) {
                        xx.IsChoose = true;
                    })
                })
                child = this.ClassElement.ChildClasses.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            if (this.ClassElement.SelectArea.Id != "all") {
                areas = this.ClassElement.SelectArea.Id;
            }
            $http.post('../Reports/DailyMenus', {
                BeginTime: _this.ClassElement.BeginTime,
                EndTime: _this.ClassElement.EndTime,
                AreaId: areas,
                Fathers: father,
                Childs: child,
                Menus: menus,
                Type:_this.ClassElement.SelectOption.Id
            }).success(function (data) {
                _this.ClassElement.Datas = data.Datas;
                _this.ClassElement.Sum = data.Sum;
                deferred.resolve(data);
            })
            return deferred.promise;
        }
    }
    return service;
}])
.factory('monthSales', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        MonthElements: {
            FatherClasses: [],
            ChildClasses: [],
            Menus: [],
            OriMenus: [],
            SelectFather: [],
            SelectChild: [],
            MonthDatas: [],
            Sum: [],
            Month: null,
            Days: [],
            Options: [],
            CountAll:0,
            SelectOption: {}
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Reports/GetClasses').success(function (data) {
                _this.MonthElements.FatherClasses = data.FatherClass;
                _this.MonthElements.ChildClasses = data.ChildClass;
                _this.MonthElements.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
                if (data.ChildClass.length > 0) {
                    _this.MonthElements.ChildClasses.forEach(function (x) {
                        x.IsChoose = true;
                    })
                    _this.MonthElements.Options = [{ Name: '菜品', Id: 0 }, { Name: '一级类', Id: 1 }, { Name: '二级类', Id: 2 }]
                } else {
                    _this.MonthElements.Options = [{ Name: '菜品', Id: 0 }, { Name: '一级类', Id: 1 }]
                }
                _this.MonthElements.Menus = data.Menus;
                _this.MonthElements.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
                _this.MonthElements.OriMenus = _this.MonthElements.Menus;
                _this.MonthElements.SelectOption = _this.MonthElements.Options[0];
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        Search: function () {
            var _this = this;
            var deferred = $q.defer();
            var father = null, child = null, menus = null;
            if (this.MonthElements.SelectOption.Id == 0) {
                //menu
                menus = this.MonthElements.Menus.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            else if (this.MonthElements.SelectOption.Id == 1) {
                father = this.MonthElements.FatherClasses.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            else {
                this.MonthElements.ChildClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
                this.MonthElements.FatherClasses.filter(function (x) { return x.IsChoose }).forEach(function (x) {
                    _this.MonthElements.ChildClasses.filter(function (xx) { return xx.ParentMenuClassId == x.Id }).forEach(function (xx) {
                        xx.IsChoose = true;
                    })
                })
                child = this.MonthElements.ChildClasses.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            $http.post('../Reports/GetMonthDatas', {
                time: _this.MonthElements.Month,
                Fathers: father,
                Childs: child,
                Menus: menus,
                Type: _this.MonthElements.SelectOption.Id
            }).success(function (data) {
                _this.MonthElements.MonthDatas = data.MonthSales;
                _this.MonthElements.Days = data.Days;
                _this.MonthElements.Sum = data.Sum;
                _this.MonthElements.CountAll = data.CountAll;
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        AllSelect: function () {
            if (this.MonthElements.SelectOption.Id == 0) {
                //menu
                this.MonthElements.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else if (this.MonthElements.SelectOption.Id == 1) {
                //father class
                this.MonthElements.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else {
                this.MonthElements.ChildClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            }
        },
        NoneSelect: function () {
            if (this.MonthElements.SelectOption.Id == 0) {
                //menu
                this.MonthElements.Menus.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else if (this.MonthElements.SelectOption.Id == 1) {
                //father class
                this.MonthElements.FatherClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else {
                this.MonthElements.ChildClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            }
        },
        ReverseSelect: function () {
            if (this.MonthElements.SelectOption.Id == 0) {
                //menu
                this.MonthElements.Menus.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else if (this.MonthElements.SelectOption.Id == 1) {
                //father class
                this.MonthElements.FatherClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else {
                this.MonthElements.ChildClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })

            }
        },
        TypeChange: function () {
            var _this = this;
            if (this.MonthElements.SelectOption.Id == 0) {
                this.MonthElements.Menus = this.MonthElements.OriMenus.filter(function (x) {
                    x.IsChoose = false;
                    x.Classes.forEach(function (xx) {
                        _this.MonthElements.FatherClasses.filter(function (a) { return a.IsChoose; }).forEach(function (xxx) {
                            if (xx.Id == xxx.Id) {
                                x.IsChoose = true;
                                return;
                            }
                        })
                        if (x.IsChoose) return;
                    });
                    x.Classes.forEach(function (xx) {
                        _this.MonthElements.ChildClasses.filter(function (a) { return a.IsChoose; }).forEach(function (xxx) {
                            if (xx.Id == xxx.Id) {
                                x.IsChoose = true;
                                return;
                            }
                        })
                        if (x.IsChoose) return;
                    })
                    return x.IsChoose;
                })
            }
        }
    }
    return service;
}])
.factory('YearSale', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        YearElements: {
            FatherClasses: [],
            ChildClasses: [],
            Menus: [],
            OriMenus: [],
            SelectFather: [],
            SelectChild: [],
            Options: [],
            SelectOption: {},
            YearsData: [],
            CountAll:0,
            Sum: [],
            Year: new Date().getFullYear().toString()
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Reports/GetClasses').success(function (data) {
                _this.YearElements.FatherClasses = data.FatherClass;
                _this.YearElements.ChildClasses = data.ChildClass;
                _this.YearElements.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
                if (data.ChildClass.length > 0) {
                    _this.YearElements.ChildClasses.forEach(function (x) {
                        x.IsChoose = true;
                    })
                    _this.YearElements.Options = [{ Name: '菜品', Id: 0 }, { Name: '一级类', Id: 1 }, { Name: '二级类', Id: 2 }]
                }
                else {
                    _this.YearElements.Options = [{ Name: '菜品', Id: 0 }, { Name: '一级类', Id: 1 }]
                }
                _this.YearElements.Menus = data.Menus;
                _this.YearElements.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
                _this.YearElements.OriMenus = _this.YearElements.Menus;
                _this.YearElements.SelectOption = _this.YearElements.Options[0];
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        Search: function () {
            var _this = this;
            var deferred = $q.defer();
            var father = null, child = null, menus = null;
            if (this.YearElements.SelectOption.Id == 0) {
                //menu
                menus = this.YearElements.Menus.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            else if (this.YearElements.SelectOption.Id == 1) {
                father = this.YearElements.FatherClasses.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            else {
                this.YearElements.ChildClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
                this.YearElements.FatherClasses.filter(function (x) { return x.IsChoose }).forEach(function (x) {
                    _this.YearElements.ChildClasses.filter(function (xx) { return xx.ParentMenuClassId == x.Id }).forEach(function (xx) {
                        xx.IsChoose = true;
                    })
                })
                child = this.YearElements.ChildClasses.filter(function (x) { return x.IsChoose }).map(function (x) { return x.Id });
            }
            $http.post('../Reports/getMenuYears', {
                year: _this.YearElements.Year,
                Fathers: father,
                Childs: child,
                Menus: menus,
                Type: _this.YearElements.SelectOption.Id
            }).success(function (data) {
                console.log(data);
                _this.YearElements.YearsData = data.YearsData;
                _this.YearElements.Sum = data.Sum;
                _this.YearElements.CountAll = data.CountAll;
                $('#YearReport').highcharts({
                    chart: {
                        type: 'column'
                    },
                    title: {
                        text: '每月销售'
                    },
                    subtitle: {
                        text: '美味在线'
                    },
                    xAxis: {
                        categories: [
                            '一月',
                            '二月',
                            '三月',
                            '四月',
                            '五月',
                            '六月',
                            '七月',
                            '八月',
                            '九月',
                            '十月',
                            '十一月',
                            '十二月'
                        ]
                    },
                    yAxis: {
                        min: 0,
                        title: {
                            text: '单位 (份)'
                        }
                    },
                    tooltip: {
                        headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                        pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                            '<td style="padding:0"><b>{point.y} 份</b></td></tr>',
                        footerFormat: '</table>',
                        shared: true,
                        useHTML: true
                    },
                    plotOptions: {
                        column: {
                            pointPadding: 0.2,
                            borderWidth: 0
                        }
                    },
                    series: _this.YearElements.YearsData
                });
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
                console.log(data);
            })
            return deferred.promise;
        },
        AllSelect: function () {
            if (this.YearElements.SelectOption.Id == 0) {
                //menu
                this.YearElements.Menus.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else if (this.YearElements.SelectOption.Id == 1) {
                //father class
                this.YearElements.FatherClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            } else {
                this.YearElements.ChildClasses.forEach(function (x) {
                    x.IsChoose = true;
                })
            }
        },
        NoneSelect: function () {
            if (this.YearElements.SelectOption.Id == 0) {
                //menu
                this.YearElements.Menus.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else if (this.YearElements.SelectOption.Id == 1) {
                //father class
                this.YearElements.FatherClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            } else {
                this.YearElements.ChildClasses.forEach(function (x) {
                    x.IsChoose = false;
                })
            }
        },
        ReverseSelect: function () {
            if (this.YearElements.SelectOption.Id == 0) {
                //menu
                this.YearElements.Menus.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else if (this.YearElements.SelectOption.Id == 1) {
                //father class
                this.YearElements.FatherClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })
            } else {
                this.YearElements.ChildClasses.forEach(function (x) {
                    x.IsChoose = !x.IsChoose;
                })

            }
        },
        TypeChange: function () {
            var _this = this;
            if (this.YearElements.SelectOption.Id == 0) {
                this.YearElements.Menus = this.YearElements.OriMenus.filter(function (x) {
                    x.IsChoose = false;
                    x.Classes.forEach(function (xx) {
                        _this.YearElements.FatherClasses.filter(function (a) { return a.IsChoose; }).forEach(function (xxx) {
                            if (xx.Id == xxx.Id) {
                                x.IsChoose = true;
                                return;
                            }
                        })
                        if (x.IsChoose) return;
                    });
                    x.Classes.forEach(function (xx) {
                        _this.YearElements.ChildClasses.filter(function (a) { return a.IsChoose; }).forEach(function (xxx) {
                            if (xx.Id == xxx.Id) {
                                x.IsChoose = true;
                                return;
                            }
                        })
                        if (x.IsChoose) return;
                    })
                    return x.IsChoose;
                })
            }
        }
    }
    return service;
}])
.factory('SaleRange', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        RangeElement: {
            Areas: [],
            Desks: [],
            Waiters: [],
            FatherClass: [],
            ChildClass: [],
            DateStart: {},
            DateEnd: {},
            SaleData: [],
            isAjax: false,
            SelectFather: {},
            SelectChild: {},
            SelectArea: {},
            SelectDesk: {},
            SelectWaiter: {},
            Count: 0,
            PriceAll: 0,
        },
        Initialize: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Reports/GetRangeElement').success(function (data) {
                _this.RangeElement.FatherClass = data.FatherClass;
                _this.RangeElement.ChildClass = data.ChildClass;
                _this.RangeElement.Areas = data.Areas;
                _this.RangeElement.Waiters = data.Waiters;
                _this.RangeElement.Desks = data.Desks;
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        },
        Search: function () {
            var deferred = $q.defer();
            var _this = this;
            if (!this.RangeElement.DateStart) {

            }
            if (!this.RangeElement.DateEnd) {

            }
            var fa = null, cl = null, wa = null, ds = null, ar = null;
            if (this.RangeElement.SelectFather) {
                fa = this.RangeElement.SelectFather.Id
            }
            if (this.RangeElement.SelectChild) {
                cl = this.RangeElement.SelectChild.Id
            }
            if (this.RangeElement.SelectWaiter) {
                wa = this.RangeElement.SelectWaiter.Id
            }
            if (this.RangeElement.SelectDesk) {
                ds = this.RangeElement.SelectDesk.Id
            }
            if (this.RangeElement.SelectArea) {
                ar = this.RangeElement.SelectArea.Id;
            }
            $http.post('../Reports/GetRangeSearch', {
                Begin: _this.RangeElement.DateStart,
                End: _this.RangeElement.DateEnd,
                FatherClassId: fa,
                ChildClassId: cl,
                WatierId: wa,
                AreaId: ar,
                DesksId: ds
            }).success(function (data) {
                _this.RangeElement.SaleData = data.Datas;
                _this.RangeElement.Count = _this.RangeElement.SaleData.map(function (x) { return x.Count }).reduce(function (a, b) { return +a + +b }, 0);
                _this.RangeElement.PriceAll = _this.RangeElement.SaleData.map(function (x) { return x.PriceAll }).reduce(function (a, b) { return +a + +b }, 0);
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        }
    }
    return service;
}])
.factory('SalesAll', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        SalesElement: {
            DateStart: {},
            DateEnd: {},
            Datas: [],
            Classes: [],
            Sum:[]
        },
        Search: function () {
            var deferred = $q.defer();
            var _this = this;
            $http.post('../Reports/GetSalesAll', {
                Begin: _this.SalesElement.DateStart,
                End: _this.SalesElement.DateEnd
            }).success(function (data) {
                _this.SalesElement.Sum = [];
                _this.SalesElement.Datas = data.Datas;
                _this.SalesElement.Classes = data.Classes;
                data.Sum.forEach(function (x) {
                    _this.SalesElement.Sum.push(x.Price);
                    _this.SalesElement.Sum.push(x.Percent);
                })
                _this.SalesElement.Datas.forEach(function (x) {
                    x.DoubleDatas = [];
                    for (var i = 0 ; i < x.Datas.length; i++) {
                        x.DoubleDatas.push(x.Datas[i].PriceAll);
                        x.DoubleDatas.push(x.Datas[i].Precent);
                    }
                })
                console.log(data);
                deferred.resolve(data);
            }).error(function (data) {
                deferred.reject(data);
            })
            return deferred.promise;
        }
    }
    return service;
}])
.factory('InvoiceInfo', ['$http', '$rootScope', '$q', function ($http, $rootScope, $q) {
    var service = {
        Elements: {
            Invoices: [],
            Begin: {},
            End:{}
        },
        Initialize: function () {
            var _this = this;
            $http.post('../Reports/GetInvoice').then(function (response) {
                _this.Elements.Invoices = response.data.Data;
            })
        },
        Search: function () {
            var _this = this;
            $http.post('../Reports/SearchTimeInvoice', {
                Begin: _this.Elements.Begin,
                End: _this.Elements.End
            }).then(function (response) {
                _this.Elements.Invoices = response.data.Data;
            })
        }
    }
    return service;
}])