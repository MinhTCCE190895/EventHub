using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using BLL.Interfaces;

namespace BLL.Services;

public class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private static readonly ConcurrentDictionary<string, string> TemplateCache = new();
    private readonly Assembly _assembly = typeof(EmailTemplateRenderer).Assembly;

    public async Task<string> RenderAsync(
        string templateName,
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken = default)
    {
        if (!TemplateCache.TryGetValue(templateName, out var template))
        {
            template = await LoadTemplateAsync(templateName, cancellationToken);
            TemplateCache.TryAdd(templateName, template);
        }

        var renderedTemplate = template;
        foreach (var (key, value) in values)
        {
            renderedTemplate = renderedTemplate.Replace(
                $"{{{{{key}}}}}",
                WebUtility.HtmlEncode(value),
                StringComparison.Ordinal);
        }

        return renderedTemplate;
    }

    private async Task<string> LoadTemplateAsync(
        string templateName,
        CancellationToken cancellationToken)
    {
        var resourceName = $"{_assembly.GetName().Name}.EmailTemplates.{templateName}.html";
        await using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Email template '{templateName}' was not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
