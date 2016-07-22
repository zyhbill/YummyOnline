$(function () {
    var $btn = $('#btn');
    $btn.click(function () {
        var phone = $('#phonenumber').val();
        var psd = $('#psd').val();
        //$.ajax({
        //    url: '../Login/Login1',
        //    method: 'post',
        //    application:'application/json;charset=utf-8',
        //    data: { UserName: UserName, Paswrd: psd },
        //    success: function (data) {
        //        if (data.Status == true) {
        //            alert(data.Data)
        //        }

        //    }
        //})
        $.post("../Login/Login1", {
            phone: phone,
            Paswrd: psd
        },
        function (data, status) {
            alert("姓名:" +data.user.UserName+"\nId:"+data.user.Id+"\n邮箱:"+data.user.Email+"\n电话号码:"+data.user.PhoneNumber+"\n积分:"+data.points);
            //alert(data.ErrorMessage);
        });
    });

})