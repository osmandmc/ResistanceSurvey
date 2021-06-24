$(document).ready(function () {
    $(".dimmer").dimmer("show");
    getCorporation();
});
$(document).on("change", "#pagingDD", function () {
    $(".dimmer").dimmer("show");
    getCorporation();
});

$(document).on("click", ".btnEdit", function () {
    var input = $(this).siblings("input");

    var id = $(this).data("id");
    input.attr("disabled", false);
    $(this).remove();
    input.after("<button data-id=" + id + " class='ui icon primary button btnSave'><i class='save icon'></i></button>" +
        '<div class="ui icon negative button btnCancel"><i class="cancel icon"></i></div>');
});
$(document).on("click", ".btnCancel", function () {
    var input = $(this).siblings("input");
    var saveButton = $(this).siblings(".btnSave");
    var id = saveButton.data("id");
    input.attr("disabled", true);
    saveButton.remove();
    $(this).remove();
    input.after("<button data-id=" + id + " class='ui icon button btnEdit'><i class='edit icon'></i></button>");
});
$(document).on("click", ".btnSave", function () {
    var input = $(this).siblings("input");
    var cancelButton = $(this).siblings(".btnCancel");
    var saveButton = $(this);
    var id = $(this).data("id");
    $.ajax({
        url: "/Corporation/Save",
        method: "POST",
        data: {
            id: id,
            name: input.val()
        },
        success: function (result) {
            input.attr("disabled", true);
            cancelButton.remove();
            saveButton.remove();
            input.after("<button data-id=" + id + " class='ui icon button btnEdit'><i class='edit icon'></i></button>");
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });



});

$(document).on("click", ".btnDelete", function () {
    let corporationId = $(this).data("id");
    $.ajax({
        url: '/Corporation/CheckCorporation/' + corporationId,
        success: function (result) {
            console.log(result);
          
            if (result == true) {
                $("#btnReplaceCorporation").data("id", corporationId);
                $("#modalReplaceCorporation").modal("show");
            }
            else {
                $("#btnDeleteCorporation").data("id", corporationId);
                $("#modalDeleteCorporation").modal("show");
            }
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
    $(".ui .dropdown").dropdown();
});

$(document).on("click", "#btnDeleteCorporation", function () {
    let corporationId = $(this).data("id");
    $.ajax({
        url: '/Corporation/DeleteCorporation',
        method: 'POST',
        data: { id: corporationId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceCorporation").modal("hide");
            getCorporation();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});
$(document).on("click", "#btnReplaceCorporation", function () {
    let corporationId = $(this).data("id");
    let newCorporationId = $("#corporationDD").val();
    $.ajax({
        url: '/Corporation/ReplaceCorporation',
        method: 'POST',
        data: { id: corporationId, newId: newCorporationId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceCorporation").modal("hide");
            getCorporation();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});

$(document).on("click", ".checkApproveCorporation", function () {
    var checkbox = $(this);
    var approved = checkbox.is(":checked");
    let corporationId = $(this).data("id");
    $.ajax({
        url: '/Corporation/SetApproval',
        method: 'POST',
        data: { id: corporationId, approved: approved },
        success: function (result) {
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});

$(document).on("change", ".setCorporationType", function () {
    let corporationId = $(this).data("id");
    let corporationTypeId = $(this).val();
    $.ajax({
        url: '/Corporation/SetCorporationType',
        method: 'POST',
        data: { id: corporationId, corporationTypeId: corporationTypeId },
        success: function (result) {
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});


function getCorporation() {
    $.ajax({
        url: '/Corporation/_List',
        type: "POST",
        data: {
            pageNumber: $("#pagingDD").val()
        },
        success: function (result) {
            $('#corporationList').html(result);
            $(".dimmer").dimmer("hide");
        }
    });
}

