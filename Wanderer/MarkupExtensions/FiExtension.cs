using System;
using Wanderer.Icons;

namespace Wanderer.MarkupExtensions;

/// <summary>
///     XAML 标记扩展，用法：<c>{wa:Fi AccessTimeFilled}</c>。
///     图标名由 <c>roslyn/IconsMappingGenerator</c> 依据
///     <c>Assets/FluentSystemIcons-Resizable.json</c> 生成。
/// </summary>
public class FiExtension
{
    /// <inheritdoc cref="FiExtension" />
    public FiExtension()
    {
    }

    /// <inheritdoc cref="FiExtension" />
    public FiExtension(FluentIconKind icon)
    {
        Icon = icon;
    }

    /// <summary>
    ///     Fluent Icon 种类
    /// </summary>
    public FluentIconKind Icon { get; set; }

    /// <summary>
    ///     提供值
    /// </summary>
    /// <param name="serviceProvider">Avalonia 服务提供器</param>
    /// <returns>Fluent Icon 字符串值</returns>
    public string ProvideValue(IServiceProvider serviceProvider)
    {
        return char.ConvertFromUtf32((int)Icon);
    }
}