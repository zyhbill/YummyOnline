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
            alert("name:" +data.user.UserName+"\nid:"+data.user.Id+"\nemail:"+data.user.Email+"\ntel:"+data.user.PhoneNumber);
            
        });
    });

})