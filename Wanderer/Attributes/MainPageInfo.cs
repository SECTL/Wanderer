using System;
using FluentAvalonia.UI.Controls;
using Wanderer.Controls;

namespace Wanderer.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MainPageInfo : Attribute
{
    public bool IsSeparator { get; }
    
    public string Name { get; }
    public string Id { get; }
    public string IconGlyph { get; }
    
    public bool UseFullWidth { get; }
    public bool HidePageTitle { get; }

    public MainPageInfo(bool isSeparator)
    {
        if (isSeparator)
        {
            IsSeparator = true;
            Name = "分割线";
            Id = "separator";
            IconGlyph = "";
            UseFullWidth = false;
            HidePageTitle = false;
        }
        else
        {
            throw new ArgumentException("isSeparator 为 false!!!!!");
        }
    }
    
    public MainPageInfo(string name, string id, string iconGlyph = "\uE06F", bool useFullWidth = false, bool hidePageTitle = false)
    {
        IsSeparator = false;
        Name = name;
        Id = id;
        IconGlyph = iconGlyph;
        UseFullWidth = useFullWidth;
        HidePageTitle = hidePageTitle;
    }

    public NavigationViewItemBase ToNavigationViewItemBase()
    {
        if (IsSeparator)
        {
            return new NavigationViewItemSeparator();
        }

        return new NavigationViewItem
        {
            IconSource = new FluentIconSource(IconGlyph),
            Content = Name,
            Tag = this
        };
    }
}