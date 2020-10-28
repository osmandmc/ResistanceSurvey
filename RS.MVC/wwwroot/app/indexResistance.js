$(document).ready(function () {
    let now = new Date();
    getNewsFiltered(now.getDate(), now.getMonth());
    getFiltered();
    $('.ui.dropdown').dropdown();
});
$(document).on("click","#btnFilterPeriod", function () {
    getNewsFiltered($("#yearId").val(), $("#monthId").val());
});
$(document).on("change", "#pagingDD", function () {
    getFiltered();
});
$(document).on("click", "#btnFilter", function () {
    getFiltered();
});
$(document).on("click", "#listCase", function () {
    $("#companyId").val(0);
    getFiltered();
})


$(document).on("click","#createResistance", function()
{
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/Create',
        type: "GET",
        success: function (result) {
            $('#leftColumn').html(result);
            $("#leftDimmer").dimmer("hide");
            
        }
    })
})

$(document).on('click', "#btnAddOutsourceCompany", function () {
    var company = { Name: $("input[name=Name]").val(), TypeId: $("select[name=TypeId]").val(), ScaleId: $("select[name=ScaleId]").val(), WorklineId: $("select[name=WorklineId]").val(), MainCompanyId : $("select[name=MainCompanyId]").val()  };
    if (company.MainCompanyId == 0) {
        alert("Ana þirket seçilmesi zorunludur");
        return;
    }
    $.ajax({
        url: window.origin + "/Resistance/AddOutsourceCompany",
        method: "POST",
        data: company,
        success(result) {
            console.log(result);
            $('#OutsourceCompanyId').append($('<option>', {
                value: result.companyId,
                text: result.companyName
            }));
            alert("Kurum eklendi");
            $('#OutsourceCompanyId').dropdown('set selected', result.companyId)
            $('#CompanyId').dropdown('set selected', result.mainCompanyId)
           
            $(".ui.modal").modal("hide");
            $('#CompanyId').addClass("disabled");
        },
        error(jqxhr, status, error) {
            alert(error);
        }

    })
});

$(document).on('click', "#btnAddCompany", function () {
    var company = { Name: $("input[name=CompanyName]").val(), TypeId: $("select[name=TypeId]").val(), ScaleId: $("select[name=ScaleId]").val(), WorklineId: $("select[name=WorklineId]").val() };
    $.ajax({
        url: window.origin + "/Resistance/AddCompany",
        method: "POST",
        data: company,
        success(result) {
            if (result.success == true) {
                $('#CompanyId').append($('<option>', {
                    value: result.response.companyId,
                    text: result.response.companyName
                }));
                $("#CompanyId").dropdown("clear");
                $('#CompanyId').dropdown('set selected', result.response.companyId);
                alert("Kurum eklendi");

            }
            else {
                alert("Bu kurum daha önce eklenmiþ. Lütfen listeden seçin.")
            }
            $(".ui.modal").modal("hide");
        },
        error(jqxhr, status, error) {
            alert(error);
        }

    });
});

function getFiltered() {
    var companyId = $("#companyId").val();
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/ResistanceList',
        type: "POST",
        data: { companyId: companyId, pageNumber: $("#pagingDD").val() },
        success: function (result) {
            $('#leftColumn').html(result);
            $("#leftDimmer").dimmer("hide");
        }
    });
}
function getNewsFiltered(year, month) {
    $("#rightDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/NewsList',
        type: "POST",
        data: { year: year, month: month },
        success: function (result) {
            $('#news').html(result);
            $("#rightDimmer").dimmer("hide");
        }
    })
}
function editResistance(id) {
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/EditResistance/' + id,
        type: "GET",
        success: function (result) {
            $('#leftColumn').html(result);
            $("#leftDimmer").dimmer("hide");
        }
    })
}
