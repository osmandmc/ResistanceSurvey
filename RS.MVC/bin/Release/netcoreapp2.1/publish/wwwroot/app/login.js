var validationRules = {
    email: {
        identifier: 'email',
        rules: [{
            type: 'empty',
            prompt: 'E-posta adresinizi giriniz'
        }, {
            type: 'email',
            prompt: 'E-posta adresi Doğru değil'
        }]
    },
    password: {
        identifier: 'password',
        rules: [{
            type: 'empty',
            prompt: 'Şifre giriniz'
        }, {
            type: 'length[5]',
            prompt: 'Şifre en az 5 karakter olmalıdır'
        }]
    }
}

function login() {
  
    var params = {};
    params["username"] = $("#username").val();
    params["password"] = $("#password").val();
    params["rememberMe"] = $("#cbRememberMe").checkbox("is checked")?"1":"";
    $(".page.dimmer").dimmer("show");
    $.post("/Home/Login", params, "json")
    .fail(function (data) {
        $(".page.dimmer").dimmer("hide");
        ShowErrorMessages(["İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."]);
    })
    .done(function (response) {
        console.log(response);
        if (response.success) {
            window.localStorage.setItem('token', response.response.token);
          location.href ='/Resistance'
        }
        else {

            $(".page.dimmer").dimmer("hide");
            ShowErrorMessages(response.ErrorMessages);
        }

    });

 
}



$(document).ready(function () {
    $('.ui.form').form({
        fields: validationRules,
        inline: false,
        on: 'blur',
        onSuccess: function () {
            login();
            return false;
        }
    });
});

function ShowErrorMessages(msgArray) {
    $("#errorModal > .content").html("");
    $("<ul></ul>").appendTo($("#errorModal > .content"));
    for (i = 0; i < msgArray.length; i++)
        $("<li>" + msgArray[i] + "</il>").appendTo($("#errorModal > .content > ul"));
    $("#errorModal").modal("show");
}
