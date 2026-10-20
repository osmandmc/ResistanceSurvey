namespace RS.MVC.Models
{
    public class AppSettings
    {
        // Secrets come from configuration outside the repository: user-secrets locally, the deploy in production.
        public string AdminPass { get; set; }
        public string UserPass { get; set; }
    }
}