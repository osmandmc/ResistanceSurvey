
function editResistance(id)
{
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/EditResistance/'+id,
        type: "GET",
        success: function (result) {
            $('#leftColumn').html(result);
            $("#leftDimmer").dimmer("hide");
        }
    })
}

$(document).ready(function () {
  


    let now = new Date();
  getNewsFiltered(now.getDate(), now.getMonth());
  getFiltered();
    $(document).on("change", "#pagingDD", function () {
        $("#leftDimmer").dimmer("show");
        $.ajax({
            url: '/Resistance/ResistanceList',
            type: "POST",
            data: {pageNumber : $(this).val() },
            success: function (result) {
                $('#results').html(result);
                $("#leftDimmer").dimmer("hide");
            }
        })
    });
    $('.ui.dropdown').dropdown();
    $("#btnFilter").on("click", function () {
        getFiltered($("#companyId").val());
    });
    $("#btnFilterPeriod").on("click", function () {
        getNewsFiltered($("#yearId").val(), $("#monthId").val());
    });
    
});

function getFiltered(companyId)
{
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/ResistanceList',
        type: "POST",
        data: {companyId : companyId },
        success: function (result) {
            $('#results').html(result);
            $("#leftDimmer").dimmer("hide");
        }
    })
}
function getNewsFiltered(year, month)
{
    $("#rightDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/NewsList',
        type: "POST",
        data: {year : year, month: month},
        success: function (result) {
            $('#news').html(result);
            $("#rightDimmer").dimmer("hide");
        }
    })
}

$("#createResistance").on("click", function()
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
