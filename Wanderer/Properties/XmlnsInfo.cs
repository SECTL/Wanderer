using Avalonia.Metadata;

// Wanderer 自有 XAML 命名空间。用法：xmlns:wa="https://github.com/SECTL/Wanderer/schemas/xaml"
// 该定义只作用于 Wanderer 程序集（Global.props 中的 AssemblyInfo.cs 会被所有项目共享，
// 因此不能把 XmlnsDefinition 放在那里）。
[assembly: XmlnsPrefix("https://github.com/SECTL/Wanderer/schemas/xaml", "wa")]
[assembly: XmlnsDefinition("https://github.com/SECTL/Wanderer/schemas/xaml", "Wanderer")]
[assembly: XmlnsDefinition("https://github.com/SECTL/Wanderer/schemas/xaml", "Wanderer.Attributes")]
[assembly: XmlnsDefinition("https://github.com/SECTL/Wanderer/schemas/xaml", "Wanderer.Behaviors")]
[assembly: XmlnsDefinition("https://github.com/SECTL/Wanderer/schemas/xaml", "Wanderer.Controls")]
[assembly: XmlnsDefinition("https://github.com/SECTL/Wanderer/schemas/xaml", "Wanderer.Converters")]
[assembly: XmlnsDefinition("https://github.com/SECTL/Wanderer/schemas/xaml", "Wanderer.Enums")]
[assembly: XmlnsDefinition("https://github.com/SECTL/Wanderer/schemas/xaml", "Wanderer.MarkupExtensions")]
