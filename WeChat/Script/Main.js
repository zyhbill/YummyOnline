$(function () {
    var $btn = $('#btn');
    $btn.click(function () {
        var phone = $('#phonenumber').val();
        var psd = $('#psd').val();
        $.post("../query", {
            phone: phone,
            Paswrd: psd
        },
        function (data) {
            if(data.Succeeded==true)
                alert("绑定成功！")
            else
                alert("手机号或密码错误，请重新输入！")
        });
    });
})