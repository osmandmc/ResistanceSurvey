$(document).ready(function () {
    $(".dimmer").dimmer("show");
    getCompanies();
});
$(document).on("change", "#pagingDD", function () {
    $(".dimmer").dimmer("show");
    getCompanies();
});

$(document).on("click", ".modalLink", function () {
    var companyId = $(this).data("id");
    $.ajax({
        url: "/Company/_Edit/" + companyId,
        datatype: "html",
        success: function (result) {
            $("#modalCompany-content").html(result);
            $("#modalCompany").modal("show");
        }
    });
});

$(document).on("click", "#btnEditCompany", function () {
    var companyData = {
        Id: $("input[name=Id]").val(),
        Name: $("input[name=Name]").val(),
        TypeId: $("select[name=TypeId]").val(),
        WorkLineId: $("select[name=WorklineId]").val(),
        ScaleId: $("select[name=ScaleId]").val()
    }
    if (companyData.Name == "") {
        alert("Şirket ismi boş olamaz");
        return;
    }
    $.ajax({
        url: "/Company/Edit",
        method: "POST",
        data: companyData,
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalCompany").modal("hide");
            $(".dimmer").dimmer("show");
            getCompanies();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }

    });
});
$(document).on("click", ".outsourcemodalLink", function () {
    var companyId = $(this).data("id");
    $.ajax({
        url: "/Company/_EditOutsource/" + companyId,
        datatype: "html",
        success: function (result) {
            $("#modalCompany-content").html(result);
            $("#modalCompany").modal("show");
        }
    });
});
$(document).on("click", "#btnEditOutsourceCompany", function () {
    var companyData = {
        Id: $("input[name=Id]").val(),
        MainCompanyId: $("select[name=MainCompanyId]").val(),
        Name: $("input[name=Name]").val(),
        TypeId: $("select[name=TypeId]").val(),
        WorkLineId: $("select[name=WorklineId]").val(),
        ScaleId: $("select[name=ScaleId]").val()
    }
    if (companyData.Name == "") {
        alert("Şirket ismi boş olamaz");
        return;
    }
    $.ajax({
        url: "/Company/EditOutsource",
        method: "POST",
        data: companyData,
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalOutsourceCompany").modal("hide");
            $(".dimmer").dimmer("show");
            getCompanies();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }

    });
});

function getCompanies() {
    $.ajax({
        url: '/Company/_List',
        type: "POST",
        data: {
            pageNumber: $("#pagingDD").val()
        },
        success: function (result) {
            $('#companyList').html(result);
            $(".dimmer").dimmer("hide");
        }
    });
}