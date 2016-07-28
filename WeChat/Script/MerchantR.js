$(function () {
    var $button = $('#button');
    var $sbutton = $('#sbutton');
    $button.click(function () {
        var name = $('#name').val();
        var phone = $('#phone').val();
        var psw = $('#psw').val();
        var key = $('#key').val();
        $.post("../MerchantRegistration/Register", {
            Name: name,
            Phone: phone,
            PassWord: psw,
            Key: key
        },
        function (data) {
            if (data.Succeeded == true)
                alert("注册成功，请耐心等待审核！");
            else
                alert(data.ErrorMessage);
        });
    })
    $sbutton.click(function () {
        var phone = $('#phone').val();
        $.post("../MerchantRegistration/GetKey", {
            phone: phone
        });
    })
})