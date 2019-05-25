using System;

namespace RS.MVC.Models
{
    public class Response
    {
        public bool Success { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
}