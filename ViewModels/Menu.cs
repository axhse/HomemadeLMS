namespace HomemadeLMS.ViewModels
{
    public enum LinkCategory
    {
        Danger,
        Highlighted,
        Important,
        Regular,
    }

    public class LinkGroup
    {
        public List<Link> Links = new();
    }

    public class Link
    {
        public string Href { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public LinkCategory LinkCategory { get; set; } = LinkCategory.Regular;
    }

    public class Menu
    {
        public List<LinkGroup> Groups = new();
    }
}