#pragma checksum "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "bce852f74f8980a251a8dfdec528528bd6d9bb75"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_News__List), @"mvc.1.0.view", @"/Views/News/_List.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/News/_List.cshtml", typeof(AspNetCore.Views_News__List))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"bce852f74f8980a251a8dfdec528528bd6d9bb75", @"/Views/News/_List.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ca0c1b4a5de6e1092ef9ea098172652b13d6abd5", @"/Views/_ViewImports.cshtml")]
    public class Views_News__List : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<NewsListModel>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(35, 306, true);
            WriteLiteral(@"
<table class=""ui small compact celled table"">
            <thead>
                <tr>
                    <td>Haber</td>
                    <td>Tarih</td>
                    <td>Direniş Haberi</td>
                    <td></td>
                </tr>
            </thead>
            <tbody>
");
            EndContext();
#line 13 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                 foreach(var item in Model)
                {

#line default
#line hidden
            BeginContext(405, 34, true);
            WriteLiteral("                    <tr data-id =\"");
            EndContext();
            BeginContext(440, 7, false);
#line 15 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                             Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(447, 34, true);
            WriteLiteral("\">\r\n                        <td><a");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 481, "\"", 498, 1);
#line 16 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
WriteAttributeValue("", 488, item.Link, 488, 10, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(499, 10, true);
            WriteLiteral(" data-id=\"");
            EndContext();
            BeginContext(510, 7, false);
#line 16 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                                                     Write(item.Id);

#line default
#line hidden
            EndContext();
            BeginContext(517, 18, true);
            WriteLiteral("\" target=\"_blank\">");
            EndContext();
            BeginContext(536, 10, false);
#line 16 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                                                                               Write(item.Title);

#line default
#line hidden
            EndContext();
            BeginContext(546, 39, true);
            WriteLiteral("</a></td>\r\n                        <td>");
            EndContext();
            BeginContext(586, 9, false);
#line 17 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                       Write(item.Date);

#line default
#line hidden
            EndContext();
            BeginContext(595, 35, true);
            WriteLiteral("</td>\r\n                        <td>");
            EndContext();
#line 18 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                             if(item.Status==RS.COMMON.Constants.Enums.Status.Linked)
                            {

#line default
#line hidden
            BeginContext(720, 70, true);
            WriteLiteral("                                <i class=\"green thumbs up icon\"></i>\r\n");
            EndContext();
#line 21 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                            }
                            else if(item.Status == RS.COMMON.Constants.Enums.Status.Passive)
                            {

#line default
#line hidden
            BeginContext(946, 70, true);
            WriteLiteral("                                <i class=\"red thumbs down icon\"></i>\r\n");
            EndContext();
#line 25 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                            }
                            else{

#line default
#line hidden
            BeginContext(1082, 58, true);
            WriteLiteral("                                <i class=\"eye icon\"></i>\r\n");
            EndContext();
#line 28 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                            }

#line default
#line hidden
            BeginContext(1171, 507, true);
            WriteLiteral(@"                        </td>   
                        <td>
                            <div class=""ui buttons"">
                                <button class=""ui mini icon button green approve""><i class=""icon check""></i></button>
                                <div class=""or""></div>
                                <button class=""ui mini icon button red cancel""><i class=""icon cancel""></i></button>
                            </div>
                        </td>   
                    </tr>
");
            EndContext();
#line 38 "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\News\_List.cshtml"
                } 

#line default
#line hidden
            BeginContext(1698, 42, true);
            WriteLiteral("                </tbody>\r\n        </table>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IEnumerable<NewsListModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591
