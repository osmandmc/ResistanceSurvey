var validationRules =
{
        CategoryId: {
            identifier: 'CategoryId',
            rules: [
                {
                type   : 'empty',
                prompt : 'Lütfen bir kategori giriniz.'
            }]
        },
        Code: {
            identifier: 'Code',
            rules: [
            {
                type   : 'empty',
                prompt : 'Lütfen bir Kod giriniz.'
            }
            ]
        },
        CompanyId: {
            identifier: 'CompanyId',
            rules: [
            {
                type   : 'minCount[1]',
                prompt : 'Lütfen bir şirket seçiniz.'
            }]
        },
        EmploymentTypeIds: {
            identifier: 'EmploymentTypeIds',
            rules: [
            {
                type   : 'minCount[1]',
                prompt : 'Lütfen en az bir istihdam türü seçiniz.'
            }]
        },
        GenderId: {
            identifier: 'GenderId',
            rules: [
            {
                type   : 'empty',
                prompt : 'Lütfen bir Cinsiyet giriniz.'
            }
            ]
        },
        ResistanceReasonIds: {
            identifier: 'ResistanceReasonIds',
            rules: [
            {
                type   : 'minCount[1]',
                prompt : 'Lütfen en az bir vaka nedeni giriniz.'
            }]
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
    
    var dataToPost = $("#resistanceForm").serialize()
        $.post("/Resistance/Edit", dataToPost)
            .done(function (response, status, jqxhr) {
                alert("İşlem başarılı");
                counter = 0;
            })
            .fail(function (jqxhr, status, error) {
                // this is the ""error"" callback
            })

}

function addProtesto(id)
{
    var resistanceName = $("#CompanyId option:selected").text();
    $("#leftDimmer").dimmer("show");
    $('#leftColumn').load('/Resistance/AddProtesto?id='+id);
    $("#leftDimmer").dimmer("hide");
}
function editProtesto(id)
{
    $("#leftDimmer").dimmer("show");
    $("#leftColumn").load('/Resistance/EditProtesto?id='+id);
    $("#leftDimmer").dimmer("hide");
}
function back(id)
{
    $('#leftColumn').load('/Resistance/EditResistance/'+id);
}
$(function () {
    newsCounter = $("#resistanceNews .item").length;
    console.log(newsCounter);
    $("#HasTradeUnion").change(function () {
        if ($(this).val() == "true") {
            $(".tradeUnionAuthorityGroup").show();
        }
        else {
            $(".tradeUnionAuthorityGroup").hide();
        }
    });
    $('.ui.dropdown').dropdown({
        allowAdditions: true,
    });
    $("#isOutsource").change(function () {
        console.log("outsource");
        if ($(this).val() == "True") {
            $("#outsource").show();
        }
        else {
            $("#outsource").hide();
        }
    });
    
    
    
   
});

$(document).on("click", "#btnCancelResistance", function () {
    deleteResistance();
});
$(document).on("click", "#btnCancelResistanceModal", function () {
    $('#modalResistanceDelete').modal('show');
});

function deleteResistance() {
    $.post("Resistance/DeleteResistance/", { ResistanceId: $("#Id").val() })
        .done(function (response) {
            alert("Silme İşleminiz başarılı");
            $('.mini.modal').modal('hide');
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
            alert("Bir hata alındı");
        })

}