#pragma checksum "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "9c21d002a97dfd3f4760cf333e03a8945e815568"
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"9c21d002a97dfd3f4760cf333e03a8945e815568", @"/Views/ProtestoType/_List.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ca0c1b4a5de6e1092ef9ea098172652b13d6abd5", @"/Views/_ViewImports.cshtml")]
    public class Views_ProtestoType__List : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PagedResult<RS.COMMON.Entities.LookupEntity.ProtestoType>>
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
            BeginContext(66, 391, true);
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
#line 22 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
         foreach (var item in Model.Results)
        {

#line default
#line hidden
            BeginContext(514, 125, true);
            WriteLiteral("        <tr>\r\n            <td>\r\n                <div class=\"ui fluid action input\">\r\n\r\n                    <input type=\"text\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 639, "\"", 657, 1);
#line 28 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
WriteAttributeValue("", 647, item.Name, 647, 10, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(658, 51, true);
            WriteLiteral(" disabled />\r\n                    <button data-id=\"");
            EndContext();
            BeginContext(710, 7, false);
#line 29 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(717, 190, true);
            WriteLiteral("\" class=\"ui icon button btnEdit\"><i class=\"edit icon\"></i></button>\r\n                </div>\r\n            </td>\r\n            <td width=\"10%\">\r\n                <input type=\"checkbox\" data-id=\"");
            EndContext();
            BeginContext(908, 7, false);
#line 33 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                           Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(915, 14, true);
            WriteLiteral("\" tabindex=\"0\"");
            EndContext();
            BeginWriteAttribute("checked", " checked=\"", 929, "\"", 953, 1);
#line 33 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
WriteAttributeValue("", 939, item.Approved, 939, 14, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(954, 100, true);
            WriteLiteral(" class=\"checkApproveProtestoType\">\r\n            </td>\r\n            <td width=\"10%\"><button data-id=\"");
            EndContext();
            BeginContext(1055, 7, false);
#line 35 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                        Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(1062, 75, true);
            WriteLiteral("\" class=\"ui basic red button btnDelete\" >Sil</button></td>\r\n        </tr>\r\n");
            EndContext();
#line 37 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
        }

#line default
#line hidden
            BeginContext(1148, 92, true);
            WriteLiteral("\r\n    </tbody>\r\n    <tfoot>\r\n        <tr>\r\n            <th colspan=\"2\">\r\n                <p>");
            EndContext();
            BeginContext(1241, 14, false);
#line 43 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
              Write(Model.RowCount);

#line default
#line hidden
            EndContext();
            BeginContext(1255, 10, true);
            WriteLiteral(" kayıttan ");
            EndContext();
            BeginContext(1266, 10, false);
#line 43 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                       Write(Model.From);

#line default
#line hidden
            EndContext();
            BeginContext(1276, 3, true);
            WriteLiteral(" - ");
            EndContext();
            BeginContext(1280, 8, false);
#line 43 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                                     Write(Model.To);

#line default
#line hidden
            EndContext();
            BeginContext(1288, 245, true);
            WriteLiteral(" arasındakiler görüntüleniyor.</p>\r\n            </th>\r\n            <th>\r\n                <div class=\"ui right floated pagination menu field\">\r\n                    <select class=\"ui compact small dropdown\" id=\"pagingDD\">\r\n                        ");
            EndContext();
            BeginContext(1533, 62, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "f7e0882c801049859175b488e8055f53", async() => {
                BeginContext(1569, 17, false);
#line 48 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                                      Write(Model.CurrentPage);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 48 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
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
            BeginContext(1595, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 49 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                         for (int i = 1; i < @Model.PageCount + 1; i++)
                        {

#line default
#line hidden
            BeginContext(1697, 28, true);
            WriteLiteral("                            ");
            EndContext();
            BeginContext(1725, 30, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("option", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "fbe81be736cf466ca02039102b242dd5", async() => {
                BeginContext(1745, 1, false);
#line 51 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                                          Write(i);

#line default
#line hidden
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.OptionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_OptionTagHelper);
            BeginWriteTagHelperAttribute();
#line 51 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
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
            BeginContext(1755, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 52 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\ProtestoType\_List.cshtml"
                        }

#line default
#line hidden
            BeginContext(1784, 113, true);
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PagedResult<RS.COMMON.Entities.LookupEntity.ProtestoType>> Html { get; private set; }
    }
}
#pragma warning restore 1591