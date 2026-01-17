using DevQualX.Web.Components.Library.Atoms;
using DevQualX.Web.Components.Library.Molecules;

namespace DevQualX.Web.Configuration;

/// <summary>
/// Central configuration for application navigation structure.
/// </summary>
public static class NavigationConfig
{
    /// <summary>
    /// Gets the main navigation tree for the application.
    /// </summary>
    public static List<NavItem> GetNavItems()
    {
        return new List<NavItem>
        {
            new NavItem
            {
                Label = "Home",
                Href = "",
                Icon = HeroIcon.Home,
                MatchExact = true
            },
            new NavItem
            {
                Label = "Weather",
                Href = "weather",
                Icon = HeroIcon.Calendar
            },
            new NavItem
            {
                Label = "Development",
                Icon = HeroIcon.Cog6Tooth,
                Children = new List<NavItem>
                {
                    new NavItem
                    {
                        Label = "Component Showcase",
                        Href = "dev/components",
                        Icon = HeroIcon.Star,
                        DevOnly = true
                    }
                }
            }
        };
    }
}
