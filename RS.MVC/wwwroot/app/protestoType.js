$(document).ready(function () {
    $(".dimmer").dimmer("show");
    getProtestoType();
});
$(document).on("change", "#pagingDD", function () {
    $(".dimmer").dimmer("show");
    getProtestoType();
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
        url: "/ProtestoType/Save",
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
    let protestoTypeId = $(this).data("id");
    $.ajax({
        url: '/ProtestoType/CheckProtestoType/' + protestoTypeId,
        success: function (result) {
            console.log(result);

            if (result == true) {
                $("#btnReplaceProtestoType").data("id", protestoTypeId);
                $("#modalReplaceProtestoType").modal("show");
            }
            else {
                $("#btnDeleteProtestoType").data("id", protestoTypeId);
                $("#modalDeleteProtestoType").modal("show");
            }
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
    $(".ui .dropdown").dropdown();
});

$(document).on("click", "#btnDeleteProtestoType", function () {
    let protestoTypeId = $(this).data("id");
    $.ajax({
        url: '/ProtestoType/DeleteProtestoType',
        method: 'POST',
        data: { id: protestoTypeId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceProtestoType").modal("hide");
            getProtestoType();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});
$(document).on("click", "#btnReplaceProtestoType", function () {
    let protestoTypeId = $(this).data("id");
    let newProtestoTypeId = $("#protestoTypeDD").val();
    $.ajax({
        url: '/ProtestoType/ReplaceProtestoType',
        method: 'POST',
        data: { id: protestoTypeId, newId: newProtestoTypeId },
        success: function (result) {
            alert("İşleminiz başarılı");
            $("#modalReplaceProtestoType").modal("hide");
            getProtestoType();
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});

$(document).on("click", ".checkApproveProtestoType", function () {
    var checkbox = $(this);
    var approved = checkbox.is(":checked");
    let protestoTypeId = $(this).data("id");
    $.ajax({
        url: '/ProtestoType/SetApproval',
        method: 'POST',
        data: { id: protestoTypeId, approved: approved },
        success: function (result) {
        },
        error(jqxhr, status, error) {
            alert("Bir hata oldu");
        }
    });
});


function getProtestoType() {
    $.ajax({
        url: '/ProtestoType/_List',
        type: "POST",
        data: {
            pageNumber: $("#pagingDD").val()
        },
        success: function (result) {
            $('#protestoTypeList').html(result);
            $(".dimmer").dimmer("hide");
        }
    });
}

