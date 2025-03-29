using Microsoft.AspNetCore.Mvc.Rendering;

namespace NettruyenRemake.Helpers
{
    public static class BreadcrumbHelper
    {
        public static List<BreadcrumbItem> GetBreadcrumbs(ViewContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Home", "Index", "Home")
        };

            
            var displayNames = new Dictionary<string, string>
            {

                ["Chapter"] = "Chapter",
                ["History"] = "Reading History",
                ["Profile"] = "User Profile",
                ["Category"] = "Categories",
                ["ManageComics"] = "Manage Comics",

                // Action mappings (controller-specific)
                ["Profile.Edit"] = "Edit Profile", 
                ["Category.Edit"] = "Edit Category"
            };

            if (!string.IsNullOrEmpty(controller) && controller != "Home")
            {
                breadcrumbs.Add(new BreadcrumbItem(
                    displayNames.TryGetValue(controller, out var cName) ? cName : controller,
                    "Index",
                    controller
                ));
            }

            if (!string.IsNullOrEmpty(action) && action != "Index")
            {
                breadcrumbs.Add(new BreadcrumbItem(
                    displayNames.TryGetValue(action, out var aName) ? aName : action,
                    action,
                    controller
                ));
            }

            return breadcrumbs;
        }
    }

    public class BreadcrumbItem
    {
        public string Title { get; }
        public string Action { get; }
        public string Controller { get; }

        public BreadcrumbItem(string title, string action, string controller)
        {
            Title = title;
            Action = action;
            Controller = controller;
        }
    }   
}
