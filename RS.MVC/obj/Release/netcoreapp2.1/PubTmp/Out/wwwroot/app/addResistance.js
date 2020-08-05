var validationRules =
{
    CategoryId: {
        identifier: 'CategoryId',
        rules: [
            {
                type: 'empty',
                prompt: 'Lütfen bir kategori giriniz.'
            }]
    },
    Code: {
        identifier: 'Code',
        rules: [
            {
                type: 'empty',
                prompt: 'Lütfen bir Kod giriniz.'
            }
        ]
    },
    CompanyId: {
        identifier: 'CompanyId',
        rules: [
            {
                type: 'minCount[1]',
                prompt: 'Lütfen bir şirket seçiniz.'
            }]
    },
    CorporationIds: {
        identifier: 'CorporationIds',
        rules: [
            {
                type: 'minCount[1]',
                prompt: 'Lütfen en az bir kurumsallık seçiniz.'
            }]
    },
    EmploymentTypeIds: {
        identifier: 'EmploymentTypeIds',
        rules: [
            {
                type: 'minCount[1]',
                prompt: 'Lütfen en az bir istihdam türü seçiniz.'
            }]
    },
    GenderId: {
        identifier: 'GenderId',
        rules: [
            {
                type: 'empty',
                prompt: 'Lütfen bir cinsiyet giriniz.'
            }
        ]
    },
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
    InterventionTypeIds: {
        identifier: 'InterventionTypeIds',
        rules: [
            {
                type: 'minCount[1]',
                prompt: 'Lütfen en az bir müdahale tipi seçiniz.'
            }]
    },
    ProtestoCityIds: {
        identifier: 'ProtestoCityIds[0]',
        rules: [
            {
                type: 'minCount[1]',
                prompt: 'Lütfen en az bir il seçiniz.'
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
    IsAgainstProduction: {
        identifier: 'IsAgainstProduction',
        rules: [
            {
                type: 'empty',
                prompt: 'Eylem üretim Yönelik mi?'
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
    on: 'blur',
    onSuccess: function () {
        $("button[type=submit]").prop("disabled", true);
        submitForm();
        return false;
    }
}
);



function submitForm() {
    $(".page.dimmer").dimmer("show");
    var dataToPost = $("#resistanceForm").serialize();
    console.log(dataToPost);
    $.post("/Resistance/Create", dataToPost)
        .done(function (response) {
            console.log(response);
            alert("Direniş eklendi");
        })
        .fail(function (jqxhr, status, error) {
            alert("Bir hata oldu");
            $("button[type=submit]").prop("disabled", false);
            // this is the ""error"" callback
        })
    $(".page.dimmer").dimmer("hide");
}

//$(".ddmmyyyy").mask("99.99.9999", { placeholder: "gg.aa.yyyy" });

$("#cities").dropdown({
    onChange: function (value, text, $selectedItem) {
        loadDistricts(value);
    }
});
$(document).on("change", ".city", function () {
    var select = $(this);
    let id = select.data("id");
    let citySelect = $("#ProtestoCityIds_" + id + "_");

    $.ajax({
        url: "/Resistance/GetDistricts?cityId=" + citySelect.val(),
        success: function (items) {
            console.log(items);
            let districtSelect = $("#ProtestoDistrictIds_" + id + "_");
            districtSelect.empty();
            districtSelect.append($('<option>', {
                value: "",
                text: "--Seçiniz--"
            }));
            $.each(items, function (i, item) {
                districtSelect.append($('<option>', {
                    value: item.id,
                    text: item.name
                }));
            });
        }
    });
});
$("#InterventionTypeIds").change(function () {
    console.log($("#InterventionTypeIds").dropdown("get value"));

    if ($("#InterventionTypeIds").dropdown("get value") != null && $("#InterventionTypeIds").dropdown("get value").indexOf("7") < 0) {
        console.log($("#InterventionTypeIds").dropdown("get value").indexOf("7"));
        $("#InterventionTypeArea").show();
    }
    else {
        $("#InterventionTypeArea").hide();
    }
});
let id = 1;
$("#addLocation").click(function () {


    let locationHtml = '<div class="two fields">' +
        '<div class="field">' +
        '<label for="ProtestoCityIds[' + id + ']">İl</label>' +
        '<select id="ProtestoCityIds_' + id + '_" name="ProtestoCityIds[' + id + ']" class="ui fluid search selection dropdown city" data-id="' + id + '">' +
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
        '<div class="field">' +
        '<label for="ProtestoDistrictIds[' + id + ']">İlçe</label>' +
        '<select id="ProtestoDistrictIds_' + id + '_" name="ProtestoDistrictIds[' + id + ']"  class="ui fluid search dropdown" data-id="' + id + '">' +
        '<option value="">--Seçiniz--</option>' +
        '</select>' +
        '</div>' +
        '</div>';
    $("#location").append(locationHtml);
    id++;
});
