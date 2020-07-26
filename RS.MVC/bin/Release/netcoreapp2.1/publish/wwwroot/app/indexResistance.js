
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
                $('#leftColumn').html(result);
                $("#leftDimmer").dimmer("hide");
            }
        })
    });
    $('.ui.dropdown').dropdown();
   
    $("#btnFilterPeriod").on("click", function () {
        getNewsFiltered($("#yearId").val(), $("#monthId").val());
    });
    
});
$(document).on("click", "#btnFilter", function () {
    console.log("filter");
    getFiltered($("#companyId").val());
});

function getFiltered(companyId)
{
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/ResistanceList',
        type: "POST",
        data: {companyId : companyId },
        success: function (result) {
            $('#leftColumn').html(result);
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

$(document).on("click", "#listCase", function()
{
    $("#leftDimmer").dimmer("show");
    $.ajax({
        url: '/Resistance/ResistanceList',
        type: "GET",
        success: function (result) {
            $('#leftColumn').html(result);
            $("#leftDimmer").dimmer("hide");
            
        }
    })
})

