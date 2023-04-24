namespace HomemadeLMS.ViewModels
{
	public class MenuBuilder
    {
		public static LinkGroup GetDefaultNavbarMenu()
		{
            var linkGroup = new LinkGroup();
            linkGroup.Items.Add(new LinkItem { Href = "/account", Label = "Аккаунт" });
            linkGroup.Items.Add(new LinkItem { Href = "/course", Label = "Курсы" });
            linkGroup.Items.Add(new LinkItem { Href = "/extension", Label = "Дополнения" });
            return linkGroup;
        }
    }
}