let company = new Object();
let mainCompany = new Object();
$(document).ready(function () {
    let now = new Date();
    getNewsFiltered(now.getDate(), now.getMonth());
    getFiltered();
    $('.ui.dropdown').dropdown({
        clearable: true
    });

});
$(document).on("click", "#btnFilterPeriod", function () {
    getNewsFiltered($("#yearId").val(), $("#monthId").val());
});
$(document).on("change", "#pagingDD", function () {
    getFiltered();
});
$(document).on("click", "#btnFilter", function () {
    getFiltered();
});
$(document).on("click", "#btnExport", function () {
    exportReport();
});
$(document).on("click", "#listCase", function () {
    $("#companyId").val(0);
    getFiltered();
})
$(document).on("click", "#btnClearFilter", function () {
    var selects = $(".resistanceFilter").find("select");
    $.each(selects, function (i, v) {
        console.log($(this));
        $(this).dropdown('clear');
    });
});

$(document).on("click", "#createResistance", function () {
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

$(document).on('click', "#btnAddCompany", function () {
    company = {
        Name: $("input[name=CompanyName]").val(),
        TypeId: $("select[name=TypeId]").val(),
        ScaleId: $("select[name=ScaleId]").val(),
        WorklineId: $("select[name=WorklineId]").val()
    };
    $('#CompanyId').append('<option value="-1">' + company.Name + '<option>');
    $('#CompanyId').dropdown('set selected', -1);
    alert("Þirket eklendi");
    $('#addCompanyModal').modal('hide');
});

$(document).on('click', "#btnAddMainCompany", function () {
    mainCompany = {
        Name: $("input[name=CompanyName]").val(),
        TypeId: $("select[name=TypeId]").val(),
        ScaleId: $("select[name=ScaleId]").val(),
        WorklineId: $("select[name=WorklineId]").val()
    };
    $('#MainCompanyId').append('<option value="-1">' + mainCompany.Name + '<option>');
    $('#MainCompanyId').dropdown('set selected', -1)
    alert("Ana þirket eklendi");
    $('#addCompanyModal').modal('hide');
});



$(document).on('click', '#btnClearTradeUnion', function () {
    $('#TradeUnionId').dropdown('clear');
});
$(document).on("change", ".city", function () {
    var select = $(this);
    console.log(select);
    let id = select.data("id");
    let citySelect = $("select[name='Locations[" + id + "].CityId']");
    console.log(citySelect);
    $.ajax({
        url: "/Resistance/GetDistricts?cityId=" + citySelect.val(),
        success: function (items) {
            console.log(items);
            let districtSelect = $("select[name='Locations[" + id + "].DistrictId']");
            districtSelect.empty();
            districtSelect.append('<option value="">--Seçiniz--</option>');
            $.each(items, function (i, item) {
                districtSelect.append($('<option>', {
                    value: item.id,
                    text: item.name
                }));
            });
        }
    });
});
$(document).on("focusout", "input[name=EmployeeCountInProtesto]", function () {
    var employeecountInProtesto = $(this).val();
    var employeeCountInProtestoId = $("select[name = EmployeeCountInProtestoId]");
    if (employeecountInProtesto == null) return;
    if (employeecountInProtesto > 0 && employeecountInProtesto < 6) employeeCountInProtestoId.val(1);
    if (employeecountInProtesto > 5 && employeecountInProtesto < 26) employeeCountInProtestoId.val(2);
    if (employeecountInProtesto > 25 && employeecountInProtesto < 51) employeeCountInProtestoId.val(3);
    if (employeecountInProtesto > 50 && employeecountInProtesto < 101) employeeCountInProtestoId.val(4);
    if (employeecountInProtesto > 100 && employeecountInProtesto < 251) employeeCountInProtestoId.val(5);
    if (employeecountInProtesto > 250 && employeecountInProtesto < 501) employeeCountInProtestoId.val(6);
    if (employeecountInProtesto > 500 && employeecountInProtesto < 1001) employeeCountInProtestoId.val(7);
    if (employeecountInProtesto > 1000 && employeecountInProtesto < 2501) employeeCountInProtestoId.val(8);
    if (employeecountInProtesto > 2500 && employeecountInProtesto < 5001) employeeCountInProtestoId.val(9);
    if (employeecountInProtesto > 5000) employeeCountInProtestoId.val(10);
    employeeCountInProtestoId.attr("readonly", true);
})
$(document).on("change", "select[name=EmployeeCountInProtestoId]", function () {
    var employeeCountInProtesto = $("input[name = EmployeeCountInProtesto]");
    employeeCountInProtesto.val('');
});

$(document).on("focusout", "input[name=EmployeeCount]", function () {
    var employeecount = $(this).val();
    var employeeCountId = $("select[name = EmployeeCountId]");
    if (employeecount == null) return;
    if (employeecount > 0 && employeecount < 51) employeeCountId.val(1);
    if (employeecount > 50 && employeecount < 251) employeeCountId.val(2);
    if (employeecount > 250) employeeCountId.val(3);
    employeeCountId.attr("readonly", true);
})
$(document).on("change", "select[name=EmployeeCountId]", function () {
    var employeeCount = $("input[name = EmployeeCount]");
    employeeCount.val('');
});
$(document).on("focusout", "input[name=ProtestoEndDate]", function () {
    var endDate = $(this).val();
    if (endDate == '') return;
    var startDate = $("input[name = ProtestoStartDate]").val();
    if (startDate > endDate) {
        alert("Eylemin baþlangýç tarihi bitiþ tarihinden büyük olamaz");
        $(this).val('');
    }
});
$(document).on("focusout", "input[name=ProtestoStartDate]", function () {
    var startDate = $(this).val();
    console.log(startDate);
    var endDate = $("input[name = ProtestoEndDate]").val();
    if (endDate == '') return;
    if (startDate > endDate) {
        alert("Eylemin baþlangýç tarihi bitiþ tarihinden büyük olamaz");
        $(this).val('');
    }
});



$(document).on("change", "#HasTradeUnion", function () {
    if ($(this).val() == "true") $(".tradeUnionAuthorityGroup").show();
    else $(".tradeUnionAuthorityGroup").hide();
});
$(document).on("change", "#isOutsource", function () {
    if ($(this).val() == "True") { $("#outsource").show(); }
    else { $("#outsource").hide(); }
});
$('.ui.dropdown').dropdown({
    allowAdditions: true,
});

function getFiltered() {
    var companyId = $("select[name=filterCompanyId]").val();
    var categoryId = $("select[name=filterCategoryId]").val();
    var filterProtestoYearId = $("select[name=filterProtestoYearId]").val();
    var filterProtestoMonthId = $("select[name=filterProtestoMonthId]").val();
    var personalNote = $("select[name=filterPersonalNote]").val();
    console.log(personalNote);
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/ResistanceList',
        type: "POST",
        data: {
            companyId: companyId,
            categoryId: categoryId,
            monthId: filterProtestoMonthId,
            yearId: filterProtestoYearId,
            personalNote: personalNote,
            pageNumber: $("#pagingDD").val()
        },
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
function exportReport() {
    var companyId = $("select[name=filterCompanyId]").val();
    var categoryId = $("select[name=filterCategoryId]").val();
    var filterProtestoYearId = $("select[name=filterProtestoYearId]").val();
    var filterProtestoMonthId = $("select[name=filterProtestoMonthId]").val();
    var personalNote = $("select[name=filterPersonalNote]").val();
    console.log(personalNote);
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/Export',
        type: "POST",
        data: {
            companyId: companyId,
            categoryId: categoryId,
            monthId: filterProtestoMonthId,
            yearId: filterProtestoYearId,
            personalNote: personalNote,
            pageNumber: $("#pagingDD").val()
        },
        success: function (result) {
            console.log(result);
            $("#leftDimmer").dimmer("hide");
        }
    });
}

function getFormData($form) {
    var unindexed_array = $form.serializeArray();
    console.log(unindexed_array);
    var indexed_array = {};

    $.map(unindexed_array, function (n, i) {
        indexed_array[n['name']] = n['value'];
    });

    return indexed_array;
}