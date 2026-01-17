using DevQualX.Web.Components.Library.Atoms;

namespace DevQualX.Web.Components.Library.Molecules;

/// <summary>
/// Represents a navigation item in the sidebar navigation tree.
/// </summary>
public class NavItem
{
    /// <summary>
    /// Display text for the navigation item.
    /// </summary>
    public required string Label { get; init; }
    
    /// <summary>
    /// URL path to navigate to. If null, this is a section header with children.
    /// </summary>
    public string? Href { get; init; }
    
    /// <summary>
    /// Optional HeroIcon to display to the left of the label.
    /// </summary>
    public HeroIcon? Icon { get; init; }
    
    /// <summary>
    /// Child navigation items for nested navigation.
    /// </summary>
    public List<NavItem>? Children { get; init; }
    
    /// <summary>
    /// Whether this item should match exactly (for root "/" path).
    /// </summary>
    public bool MatchExact { get; init; }
    
    /// <summary>
    /// Whether this item is only shown in development environment.
    /// </summary>
    public bool DevOnly { get; init; }
}
