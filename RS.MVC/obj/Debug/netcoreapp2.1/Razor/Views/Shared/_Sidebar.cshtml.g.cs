#pragma checksum "C:\Users\odemirci\source\repos\ResistanceSurvey\RS.MVC\Views\Shared\_Sidebar.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "8c945922edf1ced36b85dc605f9d074d03df808c"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Shared__Sidebar), @"mvc.1.0.view", @"/Views/Shared/_Sidebar.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Shared/_Sidebar.cshtml", typeof(AspNetCore.Views_Shared__Sidebar))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"8c945922edf1ced36b85dc605f9d074d03df808c", @"/Views/Shared/_Sidebar.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ca0c1b4a5de6e1092ef9ea098172652b13d6abd5", @"/Views/_ViewImports.cshtml")]
    public class Views_Shared__Sidebar : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(0, 622, true);
            WriteLiteral(@"<div class=""ui vertical inverted menu"">
    <div class=""item"">
    <a class=""ui logo icon image"" href=""/"">
        <img src=""/images/logo.png"">
    </a>
    <a href=""/""><b>EÇT</b></a>
    </div>
    
    <div class=""item"">
        <div class=""header"">Introduction</div>
        <div class=""menu"">
            <a href=""/Resistance"" class=""active item"">
                Direnişler
            </a>
            <a href=""/Resistance/Create"" class=""item"">
                Direniş Ekle
            </a>
            <a class=""item"">
                Haberler
            </a>
        </div>
    </div>
</div>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
