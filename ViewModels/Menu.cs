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
        public List<LinkItem> Items = new();
    }

    public class LinkItem
    {
        public string Href { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public LinkCategory LinkCategory { get; set; } = LinkCategory.Regular;
    }

    public class Menu
    {
        public List<LinkGroup> LinkGroups = new();
    }
}