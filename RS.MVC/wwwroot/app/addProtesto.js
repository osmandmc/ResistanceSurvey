var validationRules =
{
        ProtestoTypeIds: {
            identifier: 'ProtestoTypeIds',
            rules: [
            {
                type   : 'minCount[1]',
                prompt : 'Lütfen en az bir eylem türü seçiniz.'
            }]
        },
        ProtestoPlaceIds: {
            identifier: 'ProtestoPlaceIds',
            rules: [
            {
                type   : 'minCount[1]',
                prompt : 'Lütfen en az bir eylem mekanı seçiniz.'
            }]
        },
        ProtestoStartDate:
        {
            identifier: 'ProtestoStartDate',
            rules: [
            {
                type   : 'empty',
                prompt : 'Lütfen başlangıç tarihi seçiniz.'
            }
        ]
        },
        AnyLegalIntervention: {
            identifier: 'AnyLegalIntervention',
            rules: [
            {
                type   : 'empty',
                prompt : 'Hukuki girişim var mı?'
            }
            ]
        },
        IsAgainstProduction: {
            identifier: 'IsAgainstProduction',
            rules: [
            {
                type   : 'empty',
                prompt : 'Eylem üretim Yönelik mi?'
            }
            ]
        },
        DevelopRight: {
            identifier: 'DevelopRight',
            rules: [
            {
                type   : 'empty',
                prompt : 'Hak Geliştirmeye/Savunma Özelliği'
            }
            ]
        }
};

$('.ui.form').form({
        fields: validationRules,
        inline: true,
        on: 'blur',
        onSuccess: function () {
            submitForm();
            return false;
        }
    }
);



function submitForm() {
    $("#leftDimmer").dimmer("show");
    var dataToPost = $("#protestoForm").serialize()
        $.post("/Resistance/AddProtesto", dataToPost)
            .done(function (response, status, jqxhr) {
                alert("İşlem başarılı");
            })
            .fail(function (jqxhr, status, error) {
                // this is the ""error"" callback
                alert("Bir hata oldu");
            })
            $("#leftDimmer").dimmer("hide");

}

$(".ddmmyyyy").mask("99.99.9999", { placeholder: "gg.aa.yyyy" });