#pragma checksum "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "092cfb279ad1c4ae2d2ab86ec0f0f9b24a55692c"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Company__List), @"mvc.1.0.view", @"/Views/Company/_List.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Company/_List.cshtml", typeof(AspNetCore.Views_Company__List))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\_ViewImports.cshtml"
using RS.MVC;

#line default
#line hidden
#line 2 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\_ViewImports.cshtml"
using RS.MVC.Models;

#line default
#line hidden
#line 3 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\_ViewImports.cshtml"
using RS.COMMON.DTO;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"092cfb279ad1c4ae2d2ab86ec0f0f9b24a55692c", @"/Views/Company/_List.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ca0c1b4a5de6e1092ef9ea098172652b13d6abd5", @"/Views/_ViewImports.cshtml")]
    public class Views_Company__List : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PagedResult<RS.COMMON.DTO.ComapnyListViewModel, CompanyFilterModel>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("value", "", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("ui fluid search selection dropdown"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("name", "filterCompanyId", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("name", "filterMainCompanyId", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.SelectTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(76, 413, true);
            WriteLiteral(@"
<h4 class=""ui dividing header"">Şirketler</h4>

<div class=""ui attached secondary segment companyFilter"">
    <div class=""ui accordion field"">
        <div class=""title"">
            <i class=""icon dropdown""></i>
            Filtreler
        </div>
        <div class=""content ui form"">
            <div class=""field"">
                <label class=""transition visible"">Şirket</label>
                ");
            EndContext();
            BeginContext(489, 225, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("select", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "2076bc5ab40b42e9af2bc42e48cdc7c3", async() => {
                BeginContext(628, 22, true);
                WriteLiteral("\r\n                    ");
                EndContext();
                BeginContext(650, 37, false);
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "82cf6730f77b4faea31a14ad6e0a74ef", async() => {
                    BeginContext(667, 11, true);
                    WriteLiteral("--Seçiniz--");
                    EndContext();
                }
                );
                __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
                __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper.Value = (string)__tagHelperAttribute_0.Value;
                __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
                await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                if (!__tagHelperExecutionContext.Output.IsContentModified)
                {
                    await __tagHelperExecutionContext.SetOutputContentAsync();
                }
                Write(__tagHelperExecutionContext.Output);
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                EndContext();
                BeginContext(687, 18, true);
                WriteLiteral("\r\n                ");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.SelectTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper);
#line 14 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
__Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.For = ModelExpressionProvider.CreateModelExpression(ViewData, __model => Model.Filter.CompanyId);

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("asp-for", __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.For, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 14 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
__Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.Items = ViewBag.Companies;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("asp-items", __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.Items, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.Name = (string)__tagHelperAttribute_2.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_2);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(714, 141, true);
            WriteLiteral("\r\n            </div>\r\n            <div class=\"field\">\r\n                <label class=\"transition visible\">Ana Şirket</label>\r\n                ");
            EndContext();
            BeginContext(855, 193, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("select", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "a97116afc35d4f2ebad1e7065a9447be", async() => {
                BeginContext(1002, 37, false);
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "d622dc29539641a685070aa7072908f6", async() => {
                    BeginContext(1019, 11, true);
                    WriteLiteral("--Seçiniz--");
                    EndContext();
                }
                );
                __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
                __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper.Value = (string)__tagHelperAttribute_0.Value;
                __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
                await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                if (!__tagHelperExecutionContext.Output.IsContentModified)
                {
                    await __tagHelperExecutionContext.SetOutputContentAsync();
                }
                Write(__tagHelperExecutionContext.Output);
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.SelectTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper);
#line 20 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
__Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.For = ModelExpressionProvider.CreateModelExpression(ViewData, __model => Model.Filter.MainCompanyId);

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("asp-for", __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.For, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 20 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
__Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.Items = ViewBag.Companies;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("asp-items", __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.Items, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.Name = (string)__tagHelperAttribute_3.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_3);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(1048, 637, true);
            WriteLiteral(@"
            </div>
            
            
            <div class=""field"">
                <button id=""btnFilter"" class=""ui button"">Filtrele</button>
                <button id=""btnClearFilter"" class=""ui basic button clear"">Temizle</button>
            </div>
        </div>
    </div>
</div>

    <table class=""ui celled striped table"">
        <thead>
            <tr>
                <th>Şirket</th>
                <th>Ana Şirket</th>
                <th>Tipi</th>
                <th>Ölçek</th>
                <th>İş Kolu</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
");
            EndContext();
#line 44 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
             foreach (var item in Model.Results)
            {

#line default
#line hidden
            BeginContext(1750, 18, true);
            WriteLiteral("            <tr>\r\n");
            EndContext();
#line 47 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                 if (string.IsNullOrEmpty(item.MainCorporationName))
                {

#line default
#line hidden
            BeginContext(1857, 63, true);
            WriteLiteral("                    <td><a href=\"#\" class=\"modalLink\" data-id=\"");
            EndContext();
            BeginContext(1921, 7, false);
#line 49 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                                          Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(1928, 22, true);
            WriteLiteral("\" data-mainCompanyId=\"");
            EndContext();
            BeginContext(1951, 18, false);
#line 49 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                                                                        Write(item.MainCompanyId);

#line default
#line hidden
            EndContext();
            BeginContext(1969, 2, true);
            WriteLiteral("\">");
            EndContext();
            BeginContext(1972, 9, false);
#line 49 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                                                                                             Write(item.Name);

#line default
#line hidden
            EndContext();
            BeginContext(1981, 10, true);
            WriteLiteral("<a></td>\r\n");
            EndContext();
#line 50 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                }
                else
                {

#line default
#line hidden
            BeginContext(2051, 72, true);
            WriteLiteral("                    <td><a href=\"#\" class=\"outsourcemodalLink\" data-id=\"");
            EndContext();
            BeginContext(2124, 7, false);
#line 53 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                                                   Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(2131, 2, true);
            WriteLiteral("\">");
            EndContext();
            BeginContext(2134, 9, false);
#line 53 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                                                             Write(item.Name);

#line default
#line hidden
            EndContext();
            BeginContext(2143, 10, true);
            WriteLiteral("<a></td>\r\n");
            EndContext();
#line 54 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                }

#line default
#line hidden
            BeginContext(2172, 22, true);
            WriteLiteral("\r\n                <td>");
            EndContext();
            BeginContext(2195, 24, false);
#line 56 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
               Write(item.MainCorporationName);

#line default
#line hidden
            EndContext();
            BeginContext(2219, 27, true);
            WriteLiteral("</td>\r\n                <td>");
            EndContext();
            BeginContext(2247, 9, false);
#line 57 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
               Write(item.Type);

#line default
#line hidden
            EndContext();
            BeginContext(2256, 27, true);
            WriteLiteral("</td>\r\n                <td>");
            EndContext();
            BeginContext(2284, 10, false);
#line 58 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
               Write(item.Scale);

#line default
#line hidden
            EndContext();
            BeginContext(2294, 27, true);
            WriteLiteral("</td>\r\n                <td>");
            EndContext();
            BeginContext(2322, 13, false);
#line 59 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
               Write(item.WorkLine);

#line default
#line hidden
            EndContext();
            BeginContext(2335, 56, true);
            WriteLiteral("</td>\r\n                <td width=\"10%\"><button data-id=\"");
            EndContext();
            BeginContext(2392, 7, false);
#line 60 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                            Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(2399, 22, true);
            WriteLiteral("\" data-mainCompanyId=\"");
            EndContext();
            BeginContext(2422, 18, false);
#line 60 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                                                          Write(item.MainCompanyId);

#line default
#line hidden
            EndContext();
            BeginContext(2440, 78, true);
            WriteLiteral("\" class=\"ui basic red button btnDelete\">Sil</button></td>\r\n            </tr>\r\n");
            EndContext();
#line 62 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
            }

#line default
#line hidden
            BeginContext(2533, 112, true);
            WriteLiteral("\r\n        </tbody>\r\n        <tfoot>\r\n            <tr>\r\n                <th colspan=\"5\">\r\n                    <p>");
            EndContext();
            BeginContext(2646, 14, false);
#line 68 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                  Write(Model.RowCount);

#line default
#line hidden
            EndContext();
            BeginContext(2660, 10, true);
            WriteLiteral(" kayıttan ");
            EndContext();
            BeginContext(2671, 10, false);
#line 68 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                           Write(Model.From);

#line default
#line hidden
            EndContext();
            BeginContext(2681, 3, true);
            WriteLiteral(" - ");
            EndContext();
            BeginContext(2685, 8, false);
#line 68 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                                         Write(Model.To);

#line default
#line hidden
            EndContext();
            BeginContext(2693, 265, true);
            WriteLiteral(@" arasındakiler görüntüleniyor.</p>
                </th>
                <th>
                    <div class=""ui right floated pagination menu field"">
                        <select class=""ui compact small dropdown"" id=""pagingDD"">
                            ");
            EndContext();
            BeginContext(2958, 74, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "12a9bfb728644ba8ba89e7abca6cda83", async() => {
                BeginContext(3000, 23, false);
#line 73 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                                                Write(Model.Filter.PageNumber);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 73 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                               WriteLiteral(Model.Filter.PageNumber);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper.Value = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("value", __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper.Value, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(3032, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 74 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                             for (int i = 1; i < @Model.PageCount + 1; i++)
                            {

#line default
#line hidden
            BeginContext(3142, 32, true);
            WriteLiteral("                                ");
            EndContext();
            BeginContext(3174, 30, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "1fc2fe33c4ef43ea98dd3619baa17f88", async() => {
                BeginContext(3194, 1, false);
#line 76 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                              Write(i);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 76 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                                   WriteLiteral(i);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper.Value = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("value", __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper.Value, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(3204, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 77 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Company\_List.cshtml"
                            }

#line default
#line hidden
            BeginContext(3237, 331, true);
            WriteLiteral(@"                        </select>
                    </div>
                </th>
            </tr>
        </tfoot>
    </table>
    <script>
    $(document).ready(function () {
        $('.ui.dropdown').dropdown({
            clearable: true
        });
        $('.ui.accordion').accordion();
    });
    </script>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PagedResult<RS.COMMON.DTO.ComapnyListViewModel, CompanyFilterModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591
