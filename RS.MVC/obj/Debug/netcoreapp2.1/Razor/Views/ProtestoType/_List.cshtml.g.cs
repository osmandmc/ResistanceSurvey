#pragma checksum "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "fe8ad5ad14efb3189cc61aa5fa1ac19131b3244c"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_ProtestoType__List), @"mvc.1.0.view", @"/Views/ProtestoType/_List.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/ProtestoType/_List.cshtml", typeof(AspNetCore.Views_ProtestoType__List))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"fe8ad5ad14efb3189cc61aa5fa1ac19131b3244c", @"/Views/ProtestoType/_List.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ca0c1b4a5de6e1092ef9ea098172652b13d6abd5", @"/Views/_ViewImports.cshtml")]
    public class Views_ProtestoType__List : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PagedResult<RS.COMMON.Entities.LookupEntity.ProtestoType, ProtestoTypeFilterModel>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("ui fluid"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("name", "filterName", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
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
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(91, 410, true);
            WriteLiteral(@"
<h4 class=""ui dividing header"">Eylem Türleri</h4>

<div class=""ui attached secondary segment"">
    <div class=""ui small filter form"">
         <div class=""title"">
            <i class=""icon dropdown""></i>
            Filtreler
        </div>
        <div class=""content ui form"">
            <div class=""field"">
                <label class=""transition visible"">Eylem Türü</label>
                ");
            EndContext();
            BeginContext(501, 73, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "fe8ad5ad14efb3189cc61aa5fa1ac19131b3244c4716", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper);
#line 14 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
__Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper.For = ModelExpressionProvider.CreateModelExpression(ViewData, __model => Model.Filter.Name);

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("asp-for", __Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper.For, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
            __Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper.Name = (string)__tagHelperAttribute_1.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_1);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(574, 519, true);
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
            <th>Adı</th>
            <th>Üretime Yönelik</th>
            <th>Onay</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
");
            EndContext();
#line 35 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
         foreach (var item in Model.Results)
        {

#line default
#line hidden
            BeginContext(1150, 123, true);
            WriteLiteral("        <tr>\r\n            <td>\r\n                <div class=\"ui fluid action input\">\r\n                    <input type=\"text\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 1273, "\"", 1291, 1);
#line 40 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
WriteAttributeValue("", 1281, item.Name, 1281, 10, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1292, 51, true);
            WriteLiteral(" disabled />\r\n                    <button data-id=\"");
            EndContext();
            BeginContext(1344, 7, false);
#line 41 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(1351, 190, true);
            WriteLiteral("\" class=\"ui icon button btnEdit\"><i class=\"edit icon\"></i></button>\r\n                </div>\r\n            </td>\r\n            <td width=\"10%\">\r\n                <input type=\"checkbox\" data-id=\"");
            EndContext();
            BeginContext(1542, 7, false);
#line 45 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                           Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(1549, 14, true);
            WriteLiteral("\" tabindex=\"0\"");
            EndContext();
            BeginWriteAttribute("checked", " checked=\"", 1563, "\"", 1596, 1);
#line 45 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
WriteAttributeValue("", 1573, item.AgainstProduction, 1573, 23, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1597, 129, true);
            WriteLiteral(" class=\"setAgainstProduction\">\r\n            </td>\r\n            <td width=\"10%\">\r\n                <input type=\"checkbox\" data-id=\"");
            EndContext();
            BeginContext(1727, 7, false);
#line 48 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                           Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(1734, 14, true);
            WriteLiteral("\" tabindex=\"0\"");
            EndContext();
            BeginWriteAttribute("checked", " checked=\"", 1748, "\"", 1772, 1);
#line 48 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
WriteAttributeValue("", 1758, item.Approved, 1758, 14, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1773, 100, true);
            WriteLiteral(" class=\"checkApproveProtestoType\">\r\n            </td>\r\n            <td width=\"10%\"><button data-id=\"");
            EndContext();
            BeginContext(1874, 7, false);
#line 50 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                        Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(1881, 74, true);
            WriteLiteral("\" class=\"ui basic red button btnDelete\">Sil</button></td>\r\n        </tr>\r\n");
            EndContext();
#line 52 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
        }

#line default
#line hidden
            BeginContext(1966, 92, true);
            WriteLiteral("\r\n    </tbody>\r\n    <tfoot>\r\n        <tr>\r\n            <th colspan=\"2\">\r\n                <p>");
            EndContext();
            BeginContext(2059, 14, false);
#line 58 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
              Write(Model.RowCount);

#line default
#line hidden
            EndContext();
            BeginContext(2073, 10, true);
            WriteLiteral(" kayıttan ");
            EndContext();
            BeginContext(2084, 10, false);
#line 58 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                       Write(Model.From);

#line default
#line hidden
            EndContext();
            BeginContext(2094, 3, true);
            WriteLiteral(" - ");
            EndContext();
            BeginContext(2098, 8, false);
#line 58 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                                     Write(Model.To);

#line default
#line hidden
            EndContext();
            BeginContext(2106, 245, true);
            WriteLiteral(" arasındakiler görüntüleniyor.</p>\r\n            </th>\r\n            <th>\r\n                <div class=\"ui right floated pagination menu field\">\r\n                    <select class=\"ui compact small dropdown\" id=\"pagingDD\">\r\n                        ");
            EndContext();
            BeginContext(2351, 74, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "fe8ad5ad14efb3189cc61aa5fa1ac19131b3244c12475", async() => {
                BeginContext(2393, 23, false);
#line 63 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                                            Write(Model.Filter.PageNumber);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 63 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
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
            BeginContext(2425, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 64 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                         for (int i = 1; i < @Model.PageCount + 1; i++)
                        {

#line default
#line hidden
            BeginContext(2527, 28, true);
            WriteLiteral("                            ");
            EndContext();
            BeginContext(2555, 30, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "fe8ad5ad14efb3189cc61aa5fa1ac19131b3244c14833", async() => {
                BeginContext(2575, 1, false);
#line 66 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                          Write(i);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 66 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
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
            BeginContext(2585, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 67 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                        }

#line default
#line hidden
            BeginContext(2614, 113, true);
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PagedResult<RS.COMMON.Entities.LookupEntity.ProtestoType, ProtestoTypeFilterModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591
