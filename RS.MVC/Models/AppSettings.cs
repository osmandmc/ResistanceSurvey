namespace RS.MVC.Models
{
    public class AppSettings
    {
        // Secrets come from configuration outside the repository: user-secrets locally, the deploy in production.

        /// <summary>Only for the very first sign-in ("admin"), while no account exists (F4a); ignored after that.</summary>
        public string AdminPass { get; set; }
    }
}