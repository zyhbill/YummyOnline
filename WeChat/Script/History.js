$(function () {
    var $btn = $('#btn');
    $btn.click(function () {
        $.ajax({
            url: "../select",
            type: "POST",
            datatype: "json",
            success: function (data) {
                console.log(data);
                var $dom = $('#dinelist');
                $dom.children().remove();
                if (data.Data.Data.length==0)
                    $("<p>暂无历史订单</p>").appendTo("#dinelist");
                else
                    data.Data.Data.forEach(function (x) {
                        $("<p>订单号: " + x.Id + "<br/>金额: " + x.Price + "</p>").appendTo("#dinelist")
                    })
            }
        })
    })
    $btn.click();
})