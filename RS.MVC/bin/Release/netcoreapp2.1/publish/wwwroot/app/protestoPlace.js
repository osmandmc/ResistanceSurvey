$(document).ready(function () {
    $(".dimmer").dimmer("show");
    getProtestoPlace();
});
$(document).on("change", "#pagingDD", function () {
    $(".dimmer").dimmer("show");
    getProtestoPlace();
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
        url: "/ProtestoPlace/Save",
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
    let protestoPlaceId = $(this).data("id");
    $.ajax({
        url: '/ProtestoPlace/CheckProtestoPlace/' + protestoPlaceId,
        success: function (result) {
            console.log(result);

            if (result == true) {
                $("#btnReplaceProtestoPlace").data("id", protestoPlaceId);
                $("#modalReplaceProtestoPlace").modal("show");
            }
            else {
                $("#btnDeleteProtestoPlace").data("id", protestoPlaceId);
                $("#modalDeleteProtestoPlace").modal("show");
            }
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
    $(".ui .dropdown").dropdown();
});

$(document).on("click", "#btnDeleteProtestoPlace", function () {
    let protestoPlaceId = $(this).data("id");
    $.ajax({
        url: '/ProtestoPlace/DeleteProtestoPlace',
        method: 'POST',
        data: { id: protestoPlaceId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceProtestoPlace").modal("hide");
            getProtestoPlace();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});
$(document).on("click", "#btnReplaceProtestoPlace", function () {
    let protestoPlaceId = $(this).data("id");
    let newProtestoPlaceId = $("#protestoPlaceDD").val();
    $.ajax({
        url: '/ProtestoPlace/ReplaceProtestoPlace',
        method: 'POST',
        data: { id: protestoPlaceId, newId: newProtestoPlaceId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceProtestoPlace").modal("hide");
            getProtestoPlace();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});

$(document).on("click", ".checkApproveProtestoPlace", function () {
    var checkbox = $(this);
    var approved = checkbox.is(":checked");
    let protestoPlaceId = $(this).data("id");
    $.ajax({
        url: '/ProtestoPlace/SetApproval',
        method: 'POST',
        data: { id: protestoPlaceId, approved: approved },
        success: function (result) {
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});


function getProtestoPlace() {
    $.ajax({
        url: '/ProtestoPlace/_List',
        type: "POST",
        data: {
            pageNumber: $("#pagingDD").val()
        },
        success: function (result) {
            $('#protestoPlaceList').html(result);
            $(".dimmer").dimmer("hide");
        }
    });
}

