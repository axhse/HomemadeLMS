namespace HomemadeLMS.ViewModels
{
	public static class MenuBuilder
    {
        public static Link AccountLink => new() { Href = "/account", Label = "Аккаунт" };
        public static Link CourseLink => new() { Href = "/courses", Label = "Курсы" };
        public static Link ExtensionLink => new() { Href = "/extension", Label = "Дополнения" };

        public static LinkGroup DefaultNavbarMenu
		{
            get
            {
                var linkGroup = new LinkGroup();
                linkGroup.Links.Add(AccountLink);
                linkGroup.Links.Add(CourseLink);
                linkGroup.Links.Add(ExtensionLink);
                return linkGroup;
            }
        }
    }
}