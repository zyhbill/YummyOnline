angular.module('ReportController', [])
.controller('MenuSalesCtrl', ['$scope', '$rootScope', 'MenuSale', function ($scope, $rootScope, MenuSale) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/MenuSales"; $rootScope.ChildPage = "菜品销售";
    $scope.Initialize = function () {
        var promise = MenuSale.Initialize();
        promise.then(function (data) {
            $scope.SaleElement = MenuSale.SaleElement;
        })
    }
    $scope.Search = function () {
        MenuSale.Search();
    }
    $scope.AllSelect = function () {
        MenuSale.AllSelect();
    }
    $scope.NoneSelect = function () {
        MenuSale.NoneSelect();
    }
    $scope.ReverseSelect = function () {
        MenuSale.ReverseSelect();
    }
    $scope.SelectChange = function () {
        MenuSale.SelectChange();
    }
}])
.controller('MenuSaleClassCtrl', ['$scope', '$rootScope', 'MenuSaleClass', function ($scope, $rootScope, MenuSaleClass) {
    $rootScope.FatherPage = "报表管理";  $rootScope.ChildPage = "菜品类别销售统计";
    $scope.Initialize = function () {
        var promise = MenuSaleClass.Initialize();
        promise.then(function (data) {
            $scope.SaleElement = MenuSaleClass.SaleElement;
        })
    }
    $scope.Search = function () {
        MenuSaleClass.Search();
    }
    $scope.AllSelect = function () {
        MenuSaleClass.AllSelect();
    }
    $scope.NoneSelect = function () {
        MenuSaleClass.NoneSelect();
    }
    $scope.ReverseSelect = function () {
        MenuSaleClass.ReverseSelect();
    }
}])
.controller('PaykindTypeCtrl', ['$scope', '$rootScope', 'KindType', function ($scope, $rootScope, KindType) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/PaykindType"; $rootScope.ChildPage = "收银分类汇总表";
    $scope.KindElement = KindType.KindElement;
    $scope.Search = function () { KindType.Search(); }
}])
.controller('MenuSalesClassCtrl', ['$scope', '$rootScope', 'SaleClass', function ($scope, $rootScope, SaleClass) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/MenuSalesClass"; $rootScope.ChildPage = "菜品日报表";
    $scope.Initialize = function () {
        var promise = SaleClass.Initialize();
        promise.then(function (data) {
            $scope.ClassElement = SaleClass.ClassElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.Search = function () {
        var promise = SaleClass.Search();
    }
    $scope.AllSelect = function () {
        SaleClass.AllSelect();
    }
    $scope.NoneSelect = function () {
        SaleClass.NoneSelect();
    }
    $scope.ReverseSelect = function () {
        SaleClass.ReverseSelect();
    }
    $scope.TypeChange = function () {
        SaleClass.TypeChange();
    }
}])
.controller('YearSaleCtrl', ['$scope', '$rootScope', 'YearSale', function ($scope, $rootScope, YearSale) {
    $rootScope.FatherPage = "报表管理"; $rootScope.ChildPage = "年销售数量分析表";
    $scope.Initialize = function () {
        var promise = YearSale.Initialize();
        promise.then(function (data) {
            $scope.YearElements = YearSale.YearElements;
        })
    }

    $scope.Search = function () {
        var promise = YearSale.Search();
        promise.then(function (data) {
            if (data.Status) {

            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.TypeChange = function () {
        YearSale.TypeChange();
    }
    $scope.AllSelect = function () {
        YearSale.AllSelect();
    }
    $scope.NoneSelect = function () {
        YearSale.NoneSelect();
    }
    $scope.ReverseSelect = function () {
        YearSale.ReverseSelect();
    }
}])
.controller('monthSalesCtrl', ['$scope', '$rootScope', 'monthSales', function ($scope, $rootScope, monthSales) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/monthSales"; $rootScope.ChildPage = "月销售数量分析表";
    $scope.Initialize = function () {
        var promise = monthSales.Initialize();
        promise.then(function (data) {
            $scope.MonthElements = monthSales.MonthElements;
        })
    }
    $scope.Search = function () {
        var promise = monthSales.Search();
        promise.then(function (data) {
            if (data.Status) {

            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
    $scope.TypeChange = function () {
        monthSales.TypeChange();
    }
    $scope.AllSelect = function () {
        monthSales.AllSelect();
    }
    $scope.NoneSelect = function () {
        monthSales.NoneSelect();
    }
    $scope.ReverseSelect = function () {
        monthSales.ReverseSelect();
    }
}])
.controller('SaleRangeCtrl', ['$scope', '$rootScope', 'SaleRange', function ($scope, $rootScope, SaleRange) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/monthSales"; $rootScope.ChildPage = "销售排行榜";
    $scope.Initialize = function () {
        var promise = SaleRange.Initialize();
        promise.then(function (data) {
            $scope.RangeElement = SaleRange.RangeElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.Search = function () {
        var promise = SaleRange.Search();
        promise.then(function (data) {

        }, function (data) {
            console.log(data);
        })
    }
}])
.controller('SalesAllCtrl', ['$scope', '$rootScope', 'SalesAll', function ($scope, $rootScope, SalesAll) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/monthSales"; $rootScope.ChildPage = "菜品销售汇总表";
    $scope.SalesElement = SalesAll.SalesElement;
    $scope.Search = function () {
        var promise = SalesAll.Search();
        promise.then(function (data) {

        }, function (data) {
            console.log(data);
        })
    }
}])
.controller('YearsCtrl', ['$scope', '$rootScope', 'YearSale', function ($scope, $rootScope, YearSale) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/monthSales"; $rootScope.ChildPage = "年销售数量分析表";
    $scope.YearElements = YearSale.YearElements;
    $scope.Search = function () {
        var promise = YearSale.Search();
        promise.then(function (data) {
            if (data.Status) {

            } else {
                alert(data.ErrorMessage);
            }
        }, function (data) {
            console.log(data);
        })
    }
}])
.controller('SaleRangeCtrl', ['$scope', '$rootScope', 'SaleRange', function ($scope, $rootScope, SaleRange) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/monthSales"; $rootScope.ChildPage = "销售排行榜";
    $scope.Initialize = function () {
        var promise = SaleRange.Initialize();
        promise.then(function (data) {
            $scope.RangeElement = SaleRange.RangeElement;
        }, function (data) {
            console.log(data);
        })
    }
    $scope.Search = function () {
        var promise = SaleRange.Search();
        promise.then(function (data) {

        }, function (data) {
            console.log(data);
        })
    }
}])
.controller('SalesAllCtrl', ['$scope', '$rootScope', 'SalesAll', function ($scope, $rootScope, SalesAll) {
    $rootScope.FatherPage = "报表管理"; $rootScope.FatherPath = "#/monthSales"; $rootScope.ChildPage = "菜品销售汇总表";
    $scope.SalesElement = SalesAll.SalesElement;
    $scope.Search = function () {
        var promise = SalesAll.Search();
        promise.then(function (data) {

        }, function (data) {
            console.log(data);
        })
    }
}])
