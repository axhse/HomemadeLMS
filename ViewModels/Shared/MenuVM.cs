namespace HomemadeLMS.ViewModels
{
    public enum LinkCategory
    {
        Regular = 0,
        Danger,
        Highlighted,
        Important,
    }

    public class LinkGroup
    {
        public List<LinkItem> Items = new();
    }

    public class LinkItem
    {
        public string Href { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public LinkCategory LinkCategory { get; init; } = LinkCategory.Regular;
    }

    public class MenuVM
    {
        public List<LinkGroup> LinkGroups = new();
    }
}