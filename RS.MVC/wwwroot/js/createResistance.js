<script>
function save() {
  
  var params = {};
  params["Email"] = $("#email").val();
  params["FirstName"] = $("#firstName").val();
  params["LastName"] = $("#lastName").val();

  $(".page.dimmer").dimmer("show");
  $.post("/Resistance/Create", params, "json")
  .fail(function (data) {
      $(".page.dimmer").dimmer("hide");
      ShowErrorMessages(["İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."]);
  })
  .done(function (response) {
      if (response.Success) {
          $(".page.dimmer").dimmer("hide");
          ShowSuccessMessages(response.SuccessMessages);
      }
      else {
          $(".page.dimmer").dimmer("hide");
          ShowErrorMessages(response.ErrorMessages);
      }
  }); 
}


    $(function() {
        $("#isOutsource").change(function(){
            if($(this).val()=="1")
            {
            $("#outsource").show();
            }
            else
            {
            $("#outsource").hide();
            }
        });
        $("#HasTradeUnion").change(function(){
            if($(this).val()=="1")
            {
            $(".tradeUnionAuthorityGroup").show();
            
            }
            else
            {
            $(".tradeUnionAuthorityGroup").hide();
            }
        });
        $('.ui.dropdown').dropdown({
            allowAdditions: true,
        });
    });
</script>