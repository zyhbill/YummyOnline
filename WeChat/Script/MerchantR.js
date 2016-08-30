$(function () {
    var $button = $('#button');
    var $sbutton = $('#sbutton');
    $button.click(function () {
        var name = $('#name').val();
        var phone = $('#phone').val();
        var psw = $('#psw').val();
        var pswagain = $('#pswagain').val();
        var key = $('#key').val();
        if (psw != pswagain)
            alert("密码不一致，请重新输入！");
        else {
            $.post("../MerchantRegistration/Register", {
                Name: name,
                Phone: phone,
                PassWord: psw,
                Key: key
            },
                   function (data) {
                       if (data.Succeeded == true)
                           window.location.href = '../MerchantRegistration/Success'
                           //alert("注册成功，请耐心等待审核！");
                       else
                           alert(data.ErrorMessage);
                   });
        }

    })
    $sbutton.click(function () {
        var phone = $('#phone').val();
        $.post("../MerchantRegistration/Verify", {
            Phone: phone
        },
        function (data) {
            if (data.Succeeded == true) {
                $.post("../MerchantRegistration/GetKey", {
                    Phone: phone
                });
            }
            else
                alert(data.ErrorMessage);
        }
    )
    }
)
})
