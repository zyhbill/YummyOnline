$(function () {
    var $btn = $('#btn');
    $btn.click(function () {
        $.ajax({
            url: "../select",
            type: "POST",
            datatype: "json",
            success: function (data) {
                if (data.data == null)
                    alert("暂无订单！");
                else
                    alert("~~~")
                }
            })
    })
})