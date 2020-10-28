$(document).ready(function () {
    $(".dimmer").dimmer("show");
    getTradeUnion();
});
$(document).on("change", "#pagingDD", function () {
    $(".dimmer").dimmer("show");
    getTradeUnion();
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
        url: "/TradeUnion/Save",
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
    let tradeUnionId = $(this).data("id");
    $.ajax({
        url: '/TradeUnion/CheckTradeUnion/' + tradeUnionId,
        success: function (result) {
            console.log(result);

            if (result == true) {
                $("#btnReplaceTradeUnion").data("id", tradeUnionId);
                $("#modalReplaceTradeUnion").modal("show");
            }
            else {
                $("#btnDeleteTradeUnion").data("id", tradeUnionId);
                $("#modalDeleteTradeUnion").modal("show");
            }
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
    $(".ui .dropdown").dropdown();
});

$(document).on("click", "#btnDeleteTradeUnion", function () {
    let tradeUnionId = $(this).data("id");
    $.ajax({
        url: '/TradeUnion/DeleteTradeUnion',
        method: 'POST',
        data: { id: tradeUnionId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceTradeUnion").modal("hide");
            getTradeUnion();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});
$(document).on("click", "#btnReplaceTradeUnion", function () {
    let tradeUnionId = $(this).data("id");
    let newTradeUnionId = $("#tradeUnionDD").val();
    $.ajax({
        url: '/TradeUnion/ReplaceTradeUnion',
        method: 'POST',
        data: { id: tradeUnionId, newId: newTradeUnionId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceTradeUnion").modal("hide");
            getTradeUnion();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});

$(document).on("click", ".checkApproveTradeUnion", function () {
    var checkbox = $(this);
    var approved = checkbox.is(":checked");
    let tradeUnionId = $(this).data("id");
    $.ajax({
        url: '/TradeUnion/SetApproval',
        method: 'POST',
        data: { id: tradeUnionId, approved: approved },
        success: function (result) {
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});


function getTradeUnion() {
    $.ajax({
        url: '/TradeUnion/_List',
        type: "POST",
        data: {
            pageNumber: $("#pagingDD").val()
        },
        success: function (result) {
            $('#tradeUnionList').html(result);
            $(".dimmer").dimmer("hide");
        }
    });
}

