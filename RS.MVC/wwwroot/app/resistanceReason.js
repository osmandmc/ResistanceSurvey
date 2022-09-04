$(document).ready(function () {
    $(".dimmer").dimmer("show");
    getResistanceReason();
});
$(document).on("change", "#pagingDD", function () {
    $(".dimmer").dimmer("show");
    getResistanceReason();
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
        url: "/ResistanceReason/Save",
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
    let resistanceReasonId = $(this).data("id");
    $.ajax({
        url: '/ResistanceReason/CheckResistanceReason/' + resistanceReasonId,
        success: function (result) {
            console.log(result);

            if (result == true) {
                $("#btnReplaceResistanceReason").data("id", resistanceReasonId);
                $("#modalReplaceResistanceReason").modal("show");
            }
            else {
                $("#btnDeleteResistanceReason").data("id", resistanceReasonId);
                $("#modalDeleteResistanceReason").modal("show");
            }
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
    $(".ui .dropdown").dropdown();
});

$(document).on("click", "#btnDeleteResistanceReason", function () {
    let resistanceReasonId = $(this).data("id");
    $.ajax({
        url: '/ResistanceReason/DeleteResistanceReason',
        method: 'POST',
        data: { id: resistanceReasonId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceResistanceReason").modal("hide");
            getResistanceReason();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});
$(document).on("click", "#btnReplaceResistanceReason", function () {
    let resistanceReasonId = $(this).data("id");
    let newResistanceReasonId = $("#resistanceReasonDD").val();
    $.ajax({
        url: '/ResistanceReason/ReplaceResistanceReason',
        method: 'POST',
        data: { id: resistanceReasonId, newId: newResistanceReasonId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceResistanceReason").modal("hide");
            getResistanceReason();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});

$(document).on("click", ".checkApproveResistanceReason", function () {
    var checkbox = $(this);
    var approved = checkbox.is(":checked");
    let resistanceReasonId = $(this).data("id");
    $.ajax({
        url: '/ResistanceReason/SetApproval',
        method: 'POST',
        data: { id: resistanceReasonId, approved: approved },
        success: function (result) {
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});

$(document).on("click", "#btnFilter", function () {
    getResistanceReason();
});
$(document).on("click", "#btnClearFilter", function () {
    $("#Filter_Name").val('')
});

function getResistanceReason() {
    $.ajax({
        url: '/ResistanceReason/_List',
        type: "POST",
        data: {
            pageNumber: $("#pagingDD").val(),
            name: $("#Filter_Name").val()
        },
        success: function (result) {
            $('#resistanceReasonList').html(result);
            $(".dimmer").dimmer("hide");
        }
    });
}

