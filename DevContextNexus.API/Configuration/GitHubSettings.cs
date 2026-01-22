namespace DevContextNexus.API.Configuration
{
    public class GitHubSettings
    {
        public string PersonalAccessToken { get; set; } = string.Empty;
        public string RepositoryOwner { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
    }
}
