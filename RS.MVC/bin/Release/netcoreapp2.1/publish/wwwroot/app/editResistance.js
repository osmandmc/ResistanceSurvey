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
    $(".page.dimmer").dimmer("show");
    var resistance = getFormData($("#resistanceForm"));
    console.log(resistance);
    var postData = { resistance: resistance, company: company, mainCompany: mainCompany }
    $.post("/Resistance/Edit", postData)
        .done(function (response) {
            alert("Direniş eklendi");
        })
        .fail(function (jqxhr, status, error) {
            alert("Bir hata oldu");
            $("button[type=submit]").prop("disabled", false);
            // this is the ""error"" callback
        })
    $(".page.dimmer").dimmer("hide");
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
    $('.ui.dropdown').dropdown({
        allowAdditions: true,
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
function openModal(isMain) {
    console.log(isMain);
    $.ajax({
        url: "/Resistance/AddCompany?isMain=" + isMain,
        datatype: "html",
        success: function (response) {
            $("#modal-content").html(response);
            $('#addCompanyModal').modal('show');
        }
    });
}
