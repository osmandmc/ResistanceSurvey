#pragma checksum "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "c6eb7f96e8b4925b07a6d2d909d7612b6d89852d"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Corporation__List), @"mvc.1.0.view", @"/Views/Corporation/_List.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Corporation/_List.cshtml", typeof(AspNetCore.Views_Corporation__List))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"c6eb7f96e8b4925b07a6d2d909d7612b6d89852d", @"/Views/Corporation/_List.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ca0c1b4a5de6e1092ef9ea098172652b13d6abd5", @"/Views/_ViewImports.cshtml")]
    public class Views_Corporation__List : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PagedResult<RS.COMMON.Entities.LookupEntity.Corporation, FilterModel>>
    {
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
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(78, 391, true);
            WriteLiteral(@"
<h4 class=""ui dividing header"">Kurumlar</h4>

<div class=""ui attached secondary segment"">
    <div class=""ui small filter form"">
        <div class=""fields"">

        </div>
    </div>
</div>

<table class=""ui celled striped table"">
    <thead>
        <tr>
            <th>Adı</th>
            <th>Onay</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
");
            EndContext();
#line 22 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
         foreach (var item in Model.Results)
        {

#line default
#line hidden
            BeginContext(526, 125, true);
            WriteLiteral("        <tr>\r\n            <td>\r\n                <div class=\"ui fluid action input\">\r\n\r\n                    <input type=\"text\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 651, "\"", 669, 1);
#line 28 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
WriteAttributeValue("", 659, item.Name, 659, 10, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(670, 51, true);
            WriteLiteral(" disabled />\r\n                    <button data-id=\"");
            EndContext();
            BeginContext(722, 7, false);
#line 29 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                                Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(729, 190, true);
            WriteLiteral("\" class=\"ui icon button btnEdit\"><i class=\"edit icon\"></i></button>\r\n                </div>\r\n            </td>\r\n            <td width=\"10%\">\r\n                <input type=\"checkbox\" data-id=\"");
            EndContext();
            BeginContext(920, 7, false);
#line 33 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                                           Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(927, 14, true);
            WriteLiteral("\" tabindex=\"0\"");
            EndContext();
            BeginWriteAttribute("checked", " checked=\"", 941, "\"", 965, 1);
#line 33 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
WriteAttributeValue("", 951, item.Approved, 951, 14, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(966, 99, true);
            WriteLiteral(" class=\"checkApproveCorporation\">\r\n            </td>\r\n            <td width=\"10%\"><button data-id=\"");
            EndContext();
            BeginContext(1066, 7, false);
#line 35 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                                        Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(1073, 75, true);
            WriteLiteral("\" class=\"ui basic red button btnDelete\" >Sil</button></td>\r\n        </tr>\r\n");
            EndContext();
#line 37 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
        }

#line default
#line hidden
            BeginContext(1159, 92, true);
            WriteLiteral("\r\n    </tbody>\r\n    <tfoot>\r\n        <tr>\r\n            <th colspan=\"2\">\r\n                <p>");
            EndContext();
            BeginContext(1252, 14, false);
#line 43 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
              Write(Model.RowCount);

#line default
#line hidden
            EndContext();
            BeginContext(1266, 10, true);
            WriteLiteral(" kayıttan ");
            EndContext();
            BeginContext(1277, 10, false);
#line 43 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                                       Write(Model.From);

#line default
#line hidden
            EndContext();
            BeginContext(1287, 3, true);
            WriteLiteral(" - ");
            EndContext();
            BeginContext(1291, 8, false);
#line 43 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                                                     Write(Model.To);

#line default
#line hidden
            EndContext();
            BeginContext(1299, 245, true);
            WriteLiteral(" arasındakiler görüntüleniyor.</p>\r\n            </th>\r\n            <th>\r\n                <div class=\"ui right floated pagination menu field\">\r\n                    <select class=\"ui compact small dropdown\" id=\"pagingDD\">\r\n                        ");
            EndContext();
            BeginContext(1544, 74, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "81506f3c8c5444a48f5d4b0f3427c5cd", async() => {
                BeginContext(1586, 23, false);
#line 48 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                                                            Write(Model.Filter.PageNumber);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 48 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
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
            BeginContext(1618, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 49 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                         for (int i = 1; i < @Model.PageCount + 1; i++)
                        {

#line default
#line hidden
            BeginContext(1720, 28, true);
            WriteLiteral("                            ");
            EndContext();
            BeginContext(1748, 30, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "06f064c8dcf045319d8461fb8cbb2e75", async() => {
                BeginContext(1768, 1, false);
#line 51 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                                          Write(i);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 51 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
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
            BeginContext(1778, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 52 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Corporation\_List.cshtml"
                        }

#line default
#line hidden
            BeginContext(1807, 113, true);
            WriteLiteral("                    </select>\r\n                </div>\r\n            </th>\r\n        </tr>\r\n    </tfoot>\r\n</table>\r\n");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PagedResult<RS.COMMON.Entities.LookupEntity.Corporation, FilterModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591
