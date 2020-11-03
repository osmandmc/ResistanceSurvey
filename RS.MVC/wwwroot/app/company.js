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
$(document).on("click", ".btnDelete", function () {
    let companyId = $(this).data("id");
    $.ajax({
        url: '/Company/CheckCompany/' + companyId,
        success: function (result) {
            console.log(result);

            if (result == true) {
                $("#btnReplaceCompany").data("id", companyId);
                $("#modalReplaceCompany").modal("show");
            }
            else {
                $("#btnDeleteCompany").data("id", companyId);
                $("#modalDeleteCompany").modal("show");
            }
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
    $(".ui .dropdown").dropdown();
});

$(document).on("click", "#btnDeleteCompany", function () {
    let companyId = $(this).data("id");
    $.ajax({
        url: '/Company/DeleteCompany',
        method: 'POST',
        data: { id: companyId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceCompany").modal("hide");
            getCompanies();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});
$(document).on("click", "#btnReplaceCompany", function () {
    let companyId = $(this).data("id");
    let newCompanyId = $("#companyDD").val();
    $.ajax({
        url: '/Company/ReplaceCompany',
        method: 'POST',
        data: { id: companyId, newId: newCompanyId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceCompany").modal("hide");
            getCompanies();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});

$(document).on("click", ".checkApproveCompany", function () {
    var checkbox = $(this);
    var approved = checkbox.is(":checked");
    let companyId = $(this).data("id");
    $.ajax({
        url: '/Company/SetApproval',
        method: 'POST',
        data: { id: companyId, approved: approved },
        success: function (result) {
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});
$(document).on("click", "#btnFilter", function () {
    getCompany();
});
$(document).on("click", "#btnClearFilter", function () {
    var selects = $(".companyFilter").find("select");
    $.each(selects, function (i, v) {
        console.log($(this));
        $(this).dropdown('clear');
    });
});


function getCompany() {
    var companyId = $("select[name=filterCompanyId]").val();
    var mainCompanyId = $("select[name=filterMainCompanyId]").val();
    $.ajax({
        url: '/Company/_List',
        type: "POST",
        data: {
            companyId: companyId,
            mainCompanyId: mainCompanyId,
            pageNumber: $("#pagingDD").val()
        },
        success: function (result) {
            $('#companyList').html(result);
            $(".dimmer").dimmer("hide");
        }
    });
}