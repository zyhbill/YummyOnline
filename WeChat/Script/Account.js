$(function () {
    var $button = $('#button');
    var $key = $('#key');
    $button.click(function () {
        var phoneNumber = $('#phone').val();
        var psw=$('#psw').val();
        var pswagain=$('#pswagain').val();
        var code=$('#code').val();
        if(psw==pswagain)
        {
            $.post("../Account/Signup",
                {
                    PhoneNumber:phoneNumber,
                    Code:code,
                    Password:psw,
                    PasswordAga:pswagain
                });
        }
       //function (data){
       //     if(data.Succeeded==ture)
       //         alert("注册成功！");
       //     else
       //         alert("注册失败！");
       // }
    });
    $key.click(function(){
        var phoneNumber=$('#phone').val();
        $.post("../Account/generateSmsCodeAndSend",
            { phoneNumber: phoneNumber }
        );
    })})