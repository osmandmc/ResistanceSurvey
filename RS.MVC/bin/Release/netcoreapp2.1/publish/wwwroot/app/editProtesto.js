var validationRules =
{
    ProtestoTypeIds: {
        identifier: 'ProtestoTypeIds',
        rules: [
            {
                type: 'minCount[1]',
                prompt: 'Lütfen en az bir eylem türü seçiniz.'
            }]
    },
    ProtestoPlaceIds: {
        identifier: 'ProtestoPlaceIds',
        rules: [
            {
                type: 'minCount[1]',
                prompt: 'Lütfen en az bir eylem mekanı seçiniz.'
            }]
    },
    ProtestoStartDate:
    {
        identifier: 'ProtestoStartDate',
        rules: [
            {
                type: 'empty',
                prompt: 'Lütfen başlangıç tarihi seçiniz.'
            }
        ]
    },
    AnyLegalIntervention: {
        identifier: 'AnyLegalIntervention',
        rules: [
            {
                type: 'empty',
                prompt: 'Hukuki girişim var mı?'
            }
        ]
    },
    GenderId: {
        identifier: 'GenderId',
        rules: [
            {
                type: 'empty',
                prompt: 'Lütfen cinsiyet seçiniz'
            }
        ]
    },
    DevelopRight: {
        identifier: 'DevelopRight',
        rules: [
            {
                type: 'empty',
                prompt: 'Hak Geliştirmeye/Savunma Özelliği'
            }
        ]
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


$(function () {
    interventionTypeVisibility();
    $("#HasTradeUnion").change(function () {
        if ($(this).val() == "true") { $(".tradeUnionAuthorityGroup").show(); }
        else { $(".tradeUnionAuthorityGroup").hide(); }
    });
    $('.ui.dropdown').dropdown({
        allowAdditions: true,
    });
});

$("#InterventionTypeIds").change(function () {
    interventionTypeVisibility();
});
$("#isOutsource").change(function () {
    console.log("outsource");
    if ($(this).val() == "1") { $("#outsource").show(); }
    else { $("#outsource").hide(); }
});
function interventionTypeVisibility() {
    if ($("#InterventionTypeIds").dropdown("get value") != null && $("#InterventionTypeIds").dropdown("get value").indexOf("7") < 0) {
        console.log($("#InterventionTypeIds").dropdown("get value").indexOf("7"));
        $("#InterventionTypeArea").show();
    }
    else {
        $("#InterventionTypeArea").hide();
    }
}

function submitForm() {

    var dataToPost = $("#protestoForm").serialize()
    $.post("Resistance/EditProtesto", dataToPost)
        .done(function (response) {
            if (response == '') {
                alert("İşleminiz başarılı");
            }
            else {
                alert("Eksik alanlar var");
            }

        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
            alert("Bir hata alındı");
        })

}

function deleteProtesto() {
    $.post("Resistance/DeleteProtesto/", { ProtestoId: $("#ProtestoId").val() })
        .done(function (response) {
            alert("Silme İşleminiz başarılı");
            $('.mini.modal').modal('hide');
        })
        .fail(function (jqxhr, status, error) {
            // this is the ""error"" callback
            alert("Bir hata alındı");
        })

}

$(document).on("click", "#btnCancelProtesto", function () {
    $('#modalprotestoDelete').modal('show');
    deleteProtesto();
});
$(document).on("click", "#btnCancelProtestoModal", function () {
    $('#modalprotestoDelete').modal('show');
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
            let districtSelect = $("select[name='Locations["+id+"].DistrictId']");
            districtSelect.empty();
            $.each(items, function (i, item) {
                districtSelect.append($('<option>', {
                    value: item.id,
                    text: item.name
                }));
            });
        }
    });
});
$(document).on("click", "#btnDeleteLocation", function () {
    var location = $(this).closest(".fields");
    console.log(location);
    var deleted = location.find(":first");
    console.log(deleted);
    deleted.val(true);
    location.hide();
});
let id = $("select[name $='CityId']").length;
console.log(id);
$("#addLocation").click(function () {


    let locationHtml = '<div class="fields">' +
        ' <input type="hidden" name ="Locations[' + id + '].Deleted" value="false" />'+
        '<div class="four field">' +
        '<select name="Locations[' + id + '].CityId" class="ui selection dropdown city" data-id="' + id + '">' +
        '<option value="">--Seçiniz--</option>' +
        '<option value="1">Adana</option>' +
        '<option value="2">Adiyaman</option>' +
        '<option value="3">Afyonkarahisar</option>' +
        '<option value="4">Agri</option>' +
        '<option value="5">Amasya</option>' +
        '<option value="6">Ankara</option>' +
        '<option value="7">Antalya</option>' +
        '<option value="8">Artvin</option>' +
        '<option value="9">Aydin</option>' +
        '<option value="10">Balikesir</option>' +
        '<option value="11">Bilecik</option>' +
        '<option value="12">Bingöl</option>' +
        '<option value="13">Bitlis</option>' +
        '<option value="14">Bolu</option>' +
        '<option value="15">Burdur</option>' +
        '<option value="16">Bursa</option>' +
        '<option value="17">Çanakkale</option>' +
        '<option value="18">Çankiri</option>' +
        '<option value="19">Çorum</option>' +
        '<option value="20">Denizli</option>' +
        '<option value="21">Diyarbakir</option>' +
        '<option value="22">Edirne</option>' +
        '<option value="23">Elazig</option>' +
        '<option value="24">Erzincan</option>' +
        '<option value="25">Erzurum</option>' +
        '<option value="26">Eskisehir</option>' +
        '<option value="27">Gaziantep</option>' +
        '<option value="28">Giresun</option>' +
        '<option value="29">Gümüshane</option>' +
        '<option value="30">Hakkâri</option>' +
        '<option value="31">Hatay</option>' +
        '<option value="32">Isparta</option>' +
        '<option value="33">Mersin</option>' +
        '<option value="34">Istanbul</option>' +
        '<option value="35">Izmir</option>' +
        '<option value="36">Kars</option>' +
        '<option value="37">Kastamonu</option>' +
        '<option value="38">Kayseri</option>' +
        '<option value="39">Kirklareli</option>' +
        '<option value="40">Kirsehir</option>' +
        '<option value="41">Kocaeli</option>' +
        '<option value="42">Konya</option>' +
        '<option value="43">Kütahya</option>' +
        '<option value="44">Malatya</option>' +
        '<option value="45">Manisa</option>' +
        '<option value="46">Kahramanmaras</option>' +
        '<option value="47">Mardin</option>' +
        '<option value="48">Mugla</option>' +
        '<option value="49">Mus</option>' +
        '<option value="50">Nevsehir</option>' +
        '<option value="51">Nigde</option>' +
        '<option value="52">Ordu</option>' +
        '<option value="53">Rize</option>' +
        '<option value="54">Sakarya</option>' +
        '<option value="55">Samsun</option>' +
        '<option value="56">Siirt</option>' +
        '<option value="57">Sinop</option>' +
        '<option value="58">Sivas</option>' +
        '<option value="59">Tekirdag</option>' +
        '<option value="60">Tokat</option>' +
        '<option value="61">Trabzon</option>' +
        '<option value="62">Tunceli</option>' +
        '<option value="63">Sanliurfa</option>' +
        '<option value="64">Usak</option>' +
        '<option value="65">Van</option>' +
        '<option value="66">Yozgat</option>' +
        '<option value="67">Zonguldak</option>' +
        '<option value="68">Aksaray</option>' +
        '<option value="69">Bayburt</option>' +
        '<option value="70">Karaman</option>' +
        '<option value="71">Kirikkale</option>' +
        '<option value="72">Batman</option>' +
        '<option value="73">Sirnak</option>' +
        '<option value="74">Bartin</option>' +
        '<option value="75">Ardahan</option>' +
        '<option value="76">Igdir</option>' +
        '<option value="77">Yalova</option>' +
        '<option value="78">Karabük</option>' +
        '<option value="79">Kilis</option>' +
        '<option value="80">Osmaniye</option>' +
        '<option value="81">Düzce</option>' +
        '</select>' +
        '</div>' +
        '<div class="four field">' +
        '<select name="Locations[' + id + '].DistrictId"  class="ui selection dropdown" data-id="' + id + '" data-deleted="false">' +
        '<option value="">--Seçiniz--</option>' +
        '</select>' +
        '</div>' +
        '<div class="two field">' +
        '<input class="text-box single-line" id="Locations_0__Place" name="Locations['+id+'].Place" type="text">' +
        '</div>' +
        '<div class="four field">'+
        '<button id="btnDeleteLocation" type="button" class="ui icon button red basic"><i class="trash icon"></i></button>'+
        '</div>' +
        '</div>';
    $("#location").append(locationHtml);
    id++;
    
});


function openModal() {
    console.log("open");
    $.ajax({
        url: "/Resistance/AddCompany",
        datatype: "html",
        success: function (response) {
            $("#modal-content").html(response);
            $('#addCompanyModal').modal('show');
        }
    });

}
function openOutsourceModal() {
    $.ajax({
        url: "/Resistance/AddOutsourceCompany",
        datatype: "html",
        success: function (response) {
            $("#modalOutsource-content").html(response);
            $('#addOutsourceCompanyModal').modal('show');
        }
    });
}
function checkExisiting() {

    var companyId = $("#CompanyId").val();
    var categoryId = $("#CategoryId").val();
    console.log(companyId);
    if (companyId != null && categoryId != null) {
        ExistingResistance(companyId, categoryId);
    }
}
function ExistingResistance(companyId, categoryId) {
    $.ajax({
        url: "/Resistance/ExistingResistance?companyId=" + companyId + "&categoryId=" + categoryId,
        success: function (response) {
            if (response != null) {
                alert("mevcut");
                console.log(response);
            }

        }
    });
}
