#pragma checksum "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e7a99eb70748c483c4f164bea450129dfde11aa3"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Resistance__ResistanceList), @"mvc.1.0.view", @"/Views/Resistance/_ResistanceList.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Resistance/_ResistanceList.cshtml", typeof(AspNetCore.Views_Resistance__ResistanceList))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e7a99eb70748c483c4f164bea450129dfde11aa3", @"/Views/Resistance/_ResistanceList.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ca0c1b4a5de6e1092ef9ea098172652b13d6abd5", @"/Views/_ViewImports.cshtml")]
    public class Views_Resistance__ResistanceList : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PagedResult<RS.COMMON.DTO.ResistanceIndexDto>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("value", "", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("ui fluid search selection dropdown"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("id", new global::Microsoft.AspNetCore.Html.HtmlString("companyId"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
            BeginContext(0, 1, true);
            WriteLiteral(" ");
            EndContext();
            BeginContext(55, 396, true);
            WriteLiteral(@"
<h4 class=""ui dividing header"">Vakalar</h4>
                 
<div class=""ui attached secondary segment"">
    <div class=""ui small filter form"">
        <div class=""fields"">
            <div class=""six wide field"">
                <a id=""createResistance"" class=""ui button""><i class=""plus icon""></i>Ekle</a>
            </div>
            <div class=""nine wide field"">
                ");
            EndContext();
            BeginContext(451, 184, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("select", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "0cbbdd86e80c4c5a804dbc60830b459e", async() => {
                BeginContext(548, 23, true);
                WriteLiteral(" \r\n                    ");
                EndContext();
                BeginContext(571, 37, false);
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ffb13153c89141c6af9dc87520162930", async() => {
                    BeginContext(588, 11, true);
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
                BeginContext(608, 18, true);
                WriteLiteral("\r\n                ");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.SelectTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper);
#line 12 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
__Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.Items = ViewBag.Companies;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("asp-items", __Microsoft_AspNetCore_Mvc_TagHelpers_SelectTagHelper.Items, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(635, 352, true);
            WriteLiteral(@"
            </div>
            <div class=""six wide field"">
                <button id=""btnFilter"" class=""ui left labeled icon button""><i class=""filter icon""></i>Filtrele</button>
            </div>
        </div>
    </div>
</div>
<div style=""overflow-y: scroll; height:700px;"">
    <table class=""ui celled striped table"">
        <tbody>
");
            EndContext();
#line 25 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
             foreach (var item in Model.Results)
            {

#line default
#line hidden
            BeginContext(1052, 46, true);
            WriteLiteral("                <tr>\r\n                    <td>");
            EndContext();
            BeginContext(1099, 17, false);
#line 28 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                   Write(item.CategoryName);

#line default
#line hidden
            EndContext();
            BeginContext(1116, 31, true);
            WriteLiteral("</td>\r\n                    <td>");
            EndContext();
            BeginContext(1148, 16, false);
#line 29 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                   Write(item.CompanyName);

#line default
#line hidden
            EndContext();
            BeginContext(1164, 31, true);
            WriteLiteral("</td>\r\n                    <td>");
            EndContext();
            BeginContext(1196, 34, false);
#line 30 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                   Write(item.StartDate.ToShortDateString());

#line default
#line hidden
            EndContext();
            BeginContext(1230, 42, true);
            WriteLiteral("</td>\r\n                    <td><a href=\"#\"");
            EndContext();
            BeginWriteAttribute("onclick", " onclick=\"", 1272, "\"", 1306, 3);
            WriteAttributeValue("", 1282, "editResistance(", 1282, 15, true);
#line 31 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
WriteAttributeValue("", 1297, item.Id, 1297, 8, false);

#line default
#line hidden
            WriteAttributeValue("", 1305, ")", 1305, 1, true);
            EndWriteAttribute();
            BeginContext(1307, 60, true);
            WriteLiteral("><i class=\"edit icon\"></i></a></td>\r\n                </tr>\r\n");
            EndContext();
#line 33 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
            }

#line default
#line hidden
            BeginContext(1382, 112, true);
            WriteLiteral("\r\n        </tbody>\r\n        <tfoot>\r\n            <tr>\r\n                <th colspan=\"3\">\r\n                    <p>");
            EndContext();
            BeginContext(1495, 14, false);
#line 39 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                  Write(Model.RowCount);

#line default
#line hidden
            EndContext();
            BeginContext(1509, 10, true);
            WriteLiteral(" kayıttan ");
            EndContext();
            BeginContext(1520, 10, false);
#line 39 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                                           Write(Model.From);

#line default
#line hidden
            EndContext();
            BeginContext(1530, 3, true);
            WriteLiteral(" - ");
            EndContext();
            BeginContext(1534, 8, false);
#line 39 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                                                         Write(Model.To);

#line default
#line hidden
            EndContext();
            BeginContext(1542, 232, true);
            WriteLiteral(" arasındakiler görüntüleniyor.</p>\r\n                </th>\r\n                <th>\r\n                    <div class=\"field\">\r\n                        <select class=\"ui compact small dropdown\" id=\"pagingDD\">\r\n                            ");
            EndContext();
            BeginContext(1774, 62, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "e4d26e8eb341403f9aea72d87bba8d0a", async() => {
                BeginContext(1810, 17, false);
#line 44 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                                                          Write(Model.CurrentPage);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 44 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                               WriteLiteral(Model.CurrentPage);

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
            BeginContext(1836, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 45 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                             for (int i = 1; i < @Model.PageCount + 1; i++)
                            {

#line default
#line hidden
            BeginContext(1946, 32, true);
            WriteLiteral("                                ");
            EndContext();
            BeginContext(1978, 30, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "b32bec0e7378499489947c2f398074b3", async() => {
                BeginContext(1998, 1, false);
#line 47 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                                              Write(i);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 47 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
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
            BeginContext(2008, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 48 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Resistance\_ResistanceList.cshtml"
                            }

#line default
#line hidden
            BeginContext(2041, 253, true);
            WriteLiteral("                        </select>\r\n                    </div>\r\n                </th>\r\n            </tr>\r\n        </tfoot>\r\n    </table>\r\n</div>\r\n\r\n<script>\r\n    $(document).ready(function () {\r\n        $(\'.ui.dropdown\').dropdown();\r\n    });\r\n</script>\r\n");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PagedResult<RS.COMMON.DTO.ResistanceIndexDto>> Html { get; private set; }
    }
}
#pragma warning restore 1591
