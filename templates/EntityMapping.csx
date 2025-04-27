public string WriteCode()
{
    if (Entity.Models.Count == 0)
        return string.Empty;

    var entityClass = Entity.EntityClass.ToSafeName();

    var entityNamespace = Entity.EntityNamespace;
    var modelNamespace = Entity.Models.Select(m => m.ModelNamespace).FirstOrDefault();

    var readModel = string.Empty;
    var createModel = string.Empty;
    var updateModel = string.Empty;

    foreach (var model in Entity.Models)
    {
        switch (model.ModelType)
        {
            case ModelType.Read:
                readModel = model.ModelClass.ToSafeName();
                break;
            case ModelType.Create:
                createModel = model.ModelClass.ToSafeName();
                break;
            case ModelType.Update:
                updateModel = model.ModelClass.ToSafeName();
                break;
        }
    }

    if (string.IsNullOrEmpty(updateModel))
        return string.Empty;

    TemplateOptions.Parameters.TryGetValue("excludeDomain", out var excludeDomain);
    TemplateOptions.Parameters.TryGetValue("excludeEntity", out var excludeEntity);

    CodeBuilder.Clear();
    CodeBuilder.AppendLine("#pragma warning disable IDE0130 // Namespace does not match folder structure");
    CodeBuilder.AppendLine("#pragma warning disable RMG012 // Source member was not found for target member");
    CodeBuilder.AppendLine("#pragma warning disable RMG020 // Source member is not mapped to any target member");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine("using System;");
    CodeBuilder.AppendLine("using System.Diagnostics.CodeAnalysis;");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine("using Arbiter.CommandQuery.Definitions;");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine("using Injectio.Attributes;");
    CodeBuilder.AppendLine("using Riok.Mapperly.Abstractions;");
    CodeBuilder.AppendLine();

    if (string.IsNullOrEmpty(excludeEntity))
    {
        CodeBuilder.AppendLine($"using Entities = {entityNamespace};");
    }
    CodeBuilder.AppendLine($"using Models = {modelNamespace};");

    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine($"namespace {TemplateOptions.Namespace};");
    CodeBuilder.AppendLine();

    if (string.IsNullOrEmpty(excludeDomain))
    {
        GenerateClass($"{readModel}To{createModel}Mapper", $"Models.{readModel}", $"Models.{createModel}");
        GenerateClass($"{readModel}To{updateModel}Mapper", $"Models.{readModel}", $"Models.{updateModel}");
        GenerateClass($"{updateModel}To{createModel}Mapper", $"Models.{updateModel}", $"Models.{createModel}");
        GenerateClass($"{updateModel}To{readModel}Mapper", $"Models.{updateModel}", $"Models.{readModel}");
    }

    if (string.IsNullOrEmpty(excludeEntity))
    {
        GenerateClass($"{entityClass}To{readModel}Mapper", $"Entities.{entityClass}", $"Models.{readModel}");
        GenerateClass($"{entityClass}To{updateModel}Mapper", $"Entities.{entityClass}", $"Models.{updateModel}");
        GenerateClass($"{createModel}To{entityClass}Mapper", $"Models.{createModel}", $"Entities.{entityClass}");
        GenerateClass($"{updateModel}To{entityClass}Mapper", $"Models.{updateModel}", $"Entities.{entityClass}");
    }

    return CodeBuilder.ToString();
}

private void GenerateClass(string className, string source, string destination)
{
    CodeBuilder.AppendLine("[Mapper]");
    CodeBuilder.AppendLine($"[RegisterSingleton<IMapper<{source}, {destination}>>]");
    CodeBuilder.AppendLine($"internal sealed partial class {className} : IMapper<{source}, {destination}>");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    WriteMapper(source, destination);

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}");
    CodeBuilder.AppendLine();
}

private void WriteMapper(string source, string destination)
{
    CodeBuilder.AppendLine($"[return: NotNullIfNotNull(nameof(source))]");
    CodeBuilder.AppendLine($"public partial {destination}? Map({source}? source);");
    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine($"public partial void Map({source} source, {destination} destination);");
    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine($"public partial IQueryable<{destination}> ProjectTo(IQueryable<{source}> source);");
}

// run script
WriteCode()
