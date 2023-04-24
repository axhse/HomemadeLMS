namespace HomemadeLMS.ViewModels
{
	public static class MenuBuilder
    {
        public static LinkItem AccountLink => new() { Href = "/account", Label = "Аккаунт" };
        public static LinkItem CourseLink => new() { Href = "/courses", Label = "Курсы" };
        public static LinkItem ExtensionLink => new() { Href = "/extension", Label = "Дополнения" };

        public static LinkGroup DefaultNavbarMenu
		{
            get
            {
                var linkGroup = new LinkGroup();
                linkGroup.Items.Add(AccountLink);
                linkGroup.Items.Add(CourseLink);
                linkGroup.Items.Add(ExtensionLink);
                return linkGroup;
            }
        }
    }
}