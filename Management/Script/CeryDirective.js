angular.module('Directives', [])
.directive('setFocus', function () {
    return function (scope, element) {
        $('.PayInput').eq(0).focus();
    };
}).directive('spinnerUp', function () {
    return {
        restrict: 'E',
        template: '<div class="btn btn-success num-spinnerP num-spinnerUp">+</div>'
    };
}).directive('spinnerDown', function () {
    return {
        restrict: 'E',
        template: '<div class="btn btn-danger num-spinnerP num-spinnerDown">-</div>'
    };
}).directive('fileInput', function () {
    return {
        restrict: 'A',
        compile: function (ele, attr) {
            $(ele).ace_file_input({
                style: 'well',
                btn_choose: '拖拽到此，或点击上传',
                btn_change: null,
                no_icon: 'icon-cloud-upload',
                droppable: true,
                thumbnail: 'small'//large | fit
                //,icon_remove:null//set null, to hide remove/reset button
                /**,before_change:function(files, dropped) {
                    //Check an example below
                    //or examples/file-upload.html
                    return true;
                }*/
                /**,before_remove : function() {
                    return true;
                }*/
                     ,
                preview_error: function (filename, error_code) {
                    //name of the file that failed
                    //error_code values
                    //1 = 'FILE_LOAD_FAILED',
                    //2 = 'IMAGE_LOAD_FAILED',
                    //3 = 'THUMBNAIL_FAILED'
                    //alert(error_code);
                }

            }).on('change', function () {
                //console.log($(this).data('ace_input_files'));
                //console.log($(this).data('ace_input_method'));
            });
            console.log($('#id-input-file-3'));
        }
    }
}).directive('setValue', function () {
    return function (scope, element, attrs) {
        $('#sour').val(scope.MenuElement.CurrentMenu.SourDegree);
        $('#sweet').val(scope.MenuElement.CurrentMenu.SaltyDegree)
        $('#spicy').val(scope.MenuElement.CurrentMenu.SpicyDegree);
        $('#salty').val(scope.MenuElement.CurrentMenu.SaltyDegree);
    }
}).directive("fileread", [function () {
    return {
        scope: {
            fileread: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var reader = new FileReader();
                reader.onload = function (loadEvent) {
                    scope.$apply(function () {
                        scope.fileread = loadEvent.target.result;
                    });
                }
                reader.readAsDataURL(changeEvent.target.files[0]);
            });
        }
    }
}])
.directive('timePicker', function () {
    return {
        scope: {
            time: "=",
        },
        link: function (scope, element, attributes) {
            Date.prototype.format = function (format) {
                var date = {
                    "M+": this.getMonth() + 1,
                    "d+": this.getDate(),
                    "h+": this.getHours(),
                    "m+": this.getMinutes(),
                    "s+": this.getSeconds(),
                    "q+": Math.floor((this.getMonth() + 3) / 3),
                    "S+": this.getMilliseconds()
                };
                if (/(y+)/i.test(format)) {
                    format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
                }
                for (var k in date) {
                    if (new RegExp("(" + k + ")").test(format)) {
                        format = format.replace(RegExp.$1, RegExp.$1.length == 1
                               ? date[k] : ("00" + date[k]).substr(("" + date[k]).length));
                    }
                }
                return format;
            }
            if (!scope.time) scope.time = new Date().format('hh:mm:ss');
            element.timepicker({
                minuteStep: 1,
                showSeconds: true,
                defaultTime: scope.time,
                showMeridian: false
            }).next().on(ace.click_event, function () {
                $(this).prev().focus();
            });
            element.bind('change', function (changeEvent) {
                scope.time = changeEvent.target.value
                scope.$apply();
            })
        }
    }
})
.directive('fileUpdate', function () {
    return {
        scope: {
            menus: "="
        },
        link: function (scope, element, attributes) {
            console.log(element);
            element.bind('submit', function () {
                var formData = new FormData(element[0]);
                console.log(formData);
                $.ajax({
                    url: '../Baseinfo/FileTrs',
                    type: 'POST',
                    data: formData,
                    async: false,
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function (returndata) {
                        scope.menus = returndata.Menus;
                        scope.menus.forEach(function (x, index) {
                            x.MenuPrice.Discount *= 100;
                        })
                        scope.$apply();
                        alert("更新成功");
                    },
                    error: function (returndata) {
                        alert("更新失败");
                    }
                });
                return false;
            })
        }
    }
})
.directive('nestable', function () {
    return {
        link: function (scope, element, attributes) {
            element.nestable();
        }
    }
})
.directive('daterange', function () {
    return {
        scope: {
            dateStart: "=",
            dateEnd: "="
        },
        link: function (scope, element, attributes) {
            Date.prototype.format = function (format) {
                var date = {
                    "M+": this.getMonth() + 1,
                    "d+": this.getDate(),
                    "h+": this.getHours(),
                    "m+": this.getMinutes(),
                    "s+": this.getSeconds(),
                    "q+": Math.floor((this.getMonth() + 3) / 3),
                    "S+": this.getMilliseconds()
                };
                if (/(y+)/i.test(format)) {
                    format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
                }
                for (var k in date) {
                    if (new RegExp("(" + k + ")").test(format)) {
                        format = format.replace(RegExp.$1, RegExp.$1.length == 1
                               ? date[k] : ("00" + date[k]).substr(("" + date[k]).length));
                    }
                }
                return format;
            }
            scope.dateStart = new Date().format('yyyy-MM-dd');
            scope.dateEnd = new Date().format('yyyy-MM-dd');

            element.daterangepicker({
                opens: "left",
                startDate: scope.dateStart,
                endDate: scope.dateEnd,
                ranges: {
                    '今天': [moment(), moment()],
                    '昨天': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                    '过去7天': [moment().subtract(6, 'days'), moment()],
                    '过去30天': [moment().subtract(29, 'days'), moment()],
                    '这个月': [moment().startOf('month'), moment().endOf('month')],
                    '上个月': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
                },
            }, function (start, end) {
                scope.dateStart = start.format('YYYY-MM-DD');
                scope.dateEnd = end.format('YYYY-MM-DD');
                scope.$apply();
                console.log(start.format('YYYY-MM-DD'));
                console.log(end.format('YYYY-MM-DD'));
            });
        }
    }
})
.directive('register', function () {
    return {
        link: function (scope, element, attributes) {
            element.ace_wizard();
        }
    }
})
.directive('dateday', function () {
    return {
        scope: {
            time: '=',
            change:'&'
        },
        link: function (scope, element, attributes) {
            Date.prototype.format = function (format) {
                var date = {
                    "M+": this.getMonth() + 1,
                    "d+": this.getDate(),
                    "h+": this.getHours(),
                    "m+": this.getMinutes(),
                    "s+": this.getSeconds(),
                    "q+": Math.floor((this.getMonth() + 3) / 3),
                    "S+": this.getMilliseconds()
                };
                if (/(y+)/i.test(format)) {
                    format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
                }
                for (var k in date) {
                    if (new RegExp("(" + k + ")").test(format)) {
                        format = format.replace(RegExp.$1, RegExp.$1.length == 1
                               ? date[k] : ("00" + date[k]).substr(("" + date[k]).length));
                    }
                }
                return format;
            }
            scope.time = new Date().format('YYYY-MM-dd');
            element.datetimepicker({
                format: 'yyyy-MM-dd',
                minView: '2',
                maxView: '4',
                startView: '2',
                todayBtn: true,
                language: 'zh-CN'
            }).on('changeDate', function (ev) {
                scope.time = ev.date.format('YYYY-MM-dd');
                scope.change();
                scope.$apply();
            });
        }
    }
})
.directive('datetime', function () {
    return {
        scope: {
            time: '='
        },
        link: function (scope, element, attributes) {
            Date.prototype.format = function (format) {
                var date = {
                    "M+": this.getMonth() + 1,
                    "d+": this.getDate(),
                    "h+": this.getHours(),
                    "m+": this.getMinutes(),
                    "s+": this.getSeconds(),
                    "q+": Math.floor((this.getMonth() + 3) / 3),
                    "S+": this.getMilliseconds()
                };
                if (/(y+)/i.test(format)) {
                    format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
                }
                for (var k in date) {
                    if (new RegExp("(" + k + ")").test(format)) {
                        format = format.replace(RegExp.$1, RegExp.$1.length == 1
                               ? date[k] : ("00" + date[k]).substr(("" + date[k]).length));
                    }
                }
                return format;
            }
            scope.time = new Date().format('yyyy-mm');
            element.datetimepicker({
                format: 'yyyy-MM',
                minView: '3',
                maxView: '4',
                startView: '3',
                todayBtn: true,
                language: 'zh-CN'
            }).on('changeDate', function (ev) {
                scope.time = ev.date.format('YYYY-MM');
                console.log(scope.time);
                scope.$apply();
            });
        }
    }
})
.directive('dateyear', function () {
    return {
        scope: {
            time: '='
        },
        link: function (scope, element, attributes) {
            Date.prototype.format = function (format) {
                var date = {
                    "M+": this.getMonth() + 1,
                    "d+": this.getDate(),
                    "h+": this.getHours(),
                    "m+": this.getMinutes(),
                    "s+": this.getSeconds(),
                    "q+": Math.floor((this.getMonth() + 3) / 3),
                    "S+": this.getMilliseconds()
                };
                if (/(y+)/i.test(format)) {
                    format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
                }
                for (var k in date) {
                    if (new RegExp("(" + k + ")").test(format)) {
                        format = format.replace(RegExp.$1, RegExp.$1.length == 1
                               ? date[k] : ("00" + date[k]).substr(("" + date[k]).length));
                    }
                }
                return format;
            }
            scope.time = new Date().format('yyyy');
            element.datetimepicker({
                format: 'yyyy',
                minView: '4',
                maxView: '4',
                startView: '4',
                todayBtn: true,
                language: 'zh-CN'
            }).on('changeDate', function (ev) {
                scope.time = ev.date.format('YYYY');
                console.log(scope.time);
                scope.$apply();
            });
        }
    }
})
.directive('btimePicker', function () {
    return {
        scope: {
            time: "=",
        },
        link: function (scope, element, attributes) {
            Date.prototype.format = function (format) {
                var date = {
                    "M+": this.getMonth() + 1,
                    "d+": this.getDate(),
                    "h+": this.getHours(),
                    "m+": this.getMinutes(),
                    "s+": this.getSeconds(),
                    "q+": Math.floor((this.getMonth() + 3) / 3),
                    "S+": this.getMilliseconds()
                };
                if (/(y+)/i.test(format)) {
                    format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
                }
                for (var k in date) {
                    if (new RegExp("(" + k + ")").test(format)) {
                        format = format.replace(RegExp.$1, RegExp.$1.length == 1
                               ? date[k] : ("00" + date[k]).substr(("" + date[k]).length));
                    }
                }
                return format;
            }
            if (!scope.time) scope.time = "00:00:00";
            element.timepicker({
                minuteStep: 1,
                showSeconds: true,
                defaultTime: scope.time,
                showMeridian: false
            }).next().on(ace.click_event, function () {
                $(this).prev().focus();
            });
            element.bind('change', function (changeEvent) {
                scope.time = changeEvent.target.value
                scope.$apply();
            })
        }
    }
})
.directive('etimePicker', function () {
    return {
        scope: {
            time: "=",
        },
        link: function (scope, element, attributes) {
            Date.prototype.format = function (format) {
                var date = {
                    "M+": this.getMonth() + 1,
                    "d+": this.getDate(),
                    "h+": this.getHours(),
                    "m+": this.getMinutes(),
                    "s+": this.getSeconds(),
                    "q+": Math.floor((this.getMonth() + 3) / 3),
                    "S+": this.getMilliseconds()
                };
                if (/(y+)/i.test(format)) {
                    format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
                }
                for (var k in date) {
                    if (new RegExp("(" + k + ")").test(format)) {
                        format = format.replace(RegExp.$1, RegExp.$1.length == 1
                               ? date[k] : ("00" + date[k]).substr(("" + date[k]).length));
                    }
                }
                return format;
            }
            if (!scope.time) scope.time = "23:59:59";
            element.timepicker({
                minuteStep: 1,
                showSeconds: true,
                defaultTime: scope.time,
                showMeridian: false
            }).next().on(ace.click_event, function () {
                $(this).prev().focus();
            });
            element.bind('change', function (changeEvent) {
                scope.time = changeEvent.target.value
                scope.$apply();
            })
        }
    }
})
.directive('setfirstfocus', function () {
    return {
        scope: {
            index: "=",
        },
        link: function (scope, element, attributes) {
            if (scope.index == 0) {
                element.addClass('active');
            }
        }
    }
})
.directive('nav', function () {
    return {
        scope: {
        },
        link: function (scope, element, attributes) {
            element.find('.submenu>li>a').each(function () {
                $(this).click(function () {
                    $('.nav-list>li').removeClass('active');
                    $('.submenu>li').removeClass('active');
                    $(this).parent().addClass('active');
                    $(this).parent().parent().parent().addClass('active');
                })
            })
        }
    }
})