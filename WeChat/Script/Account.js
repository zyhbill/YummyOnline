$(function () {
    var $button = $('#button');
    var $key = $('#key');
    $button.click(function () {
        var phoneNumber = $('#phone').val();
        var psw = $('#psw').val();
        var pswagain = $('#pswagain').val();
        var code = $('#code').val();
        if (psw == pswagain & psw!=0) {
            $.post("../Account/Signup",
                {
                    PhoneNumber: phoneNumber,
                    Code: code,
                    Password: psw,
                    PasswordAga: pswagain
                },
           function (data) {
               if (data.Succeeded == true)
                   alert("恭喜，注册成功！");
               else
                   alert(data.ErrorMessage);
           })
        }
        else {
            { alert("密码输入错误,请重新填写！"); }
        }
    })
    $key.click(function () {
        var phoneNumber = $('#phone').val();
        $.post("../Account/SendSMS",
            { phoneNumber: phoneNumber }
        ,
        function (data) {
            if (data.Succeeded == false)
                alert(data.ErrorMessage);
        })
    })
})