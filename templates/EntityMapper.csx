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

    CodeBuilder.AppendLine("using Arbiter.CommandQuery.Definitions;");
    CodeBuilder.AppendLine("using Arbiter.CommandQuery.Mapping;");
    CodeBuilder.AppendLine();

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
        GenerateClass($"{readModel}To{createModel}Mapper", $"Models.{readModel}", $"Models.{createModel}", ["Id", "RowVersion"], ["Id"]);
        GenerateClass($"{readModel}To{updateModel}Mapper", $"Models.{readModel}", $"Models.{updateModel}", ["Id", "Created", "CreatedBy"]);
        GenerateClass($"{updateModel}To{createModel}Mapper", $"Models.{updateModel}", $"Models.{createModel}", ["RowVersion"], ["Id", "Created", "CreatedBy"]);
        GenerateClass($"{updateModel}To{readModel}Mapper", $"Models.{updateModel}", $"Models.{readModel}", null, ["Id", "Created", "CreatedBy"]);
    }

    if (string.IsNullOrEmpty(excludeEntity))
    {
        GenerateClass($"{entityClass}To{readModel}Mapper", $"Entities.{entityClass}", $"Models.{readModel}");
        GenerateClass($"{entityClass}To{updateModel}Mapper", $"Entities.{entityClass}", $"Models.{updateModel}", ["Id", "Created", "CreatedBy"]);
        GenerateClass($"{createModel}To{entityClass}Mapper", $"Models.{createModel}", $"Entities.{entityClass}", null, ["RowVersion"]);
        GenerateClass($"{updateModel}To{entityClass}Mapper", $"Models.{updateModel}", $"Entities.{entityClass}", null, ["Id", "Created", "CreatedBy"]);
    }

    return CodeBuilder.ToString();
}

private void GenerateClass(string className, string source, string destination, List<string>? sourceIgnore = null, List<string>? targetIgnore = null)
{
    CodeBuilder.AppendLine("[Mapper]");
    CodeBuilder.AppendLine($"[RegisterSingleton<IMapper<{source}, {destination}>>]");
    CodeBuilder.AppendLine($"internal sealed partial class {className}");
    CodeBuilder.AppendLine($"    : MapperBase<{source}, {destination}>");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    WriteMapper(source, destination, sourceIgnore, targetIgnore);

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}");
    CodeBuilder.AppendLine();
}

private void WriteMapper(string source, string destination, List<string>? sourceIgnore = null, List<string>? targetIgnore = null)
{
    if (sourceIgnore?.Count > 0)
        foreach(var property in sourceIgnore)
            CodeBuilder.AppendLine($"[MapperIgnoreSource(nameof({source}.{property}))]");

    if (targetIgnore?.Count > 0)
        foreach(var property in targetIgnore)
            CodeBuilder.AppendLine($"[MapperIgnoreTarget(nameof({destination}.{property}))]");

    CodeBuilder.AppendLine($"public override partial void Map(");
    CodeBuilder.AppendLine($"    {source} source,");
    CodeBuilder.AppendLine($"    {destination} destination);");
    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine($"public override partial IQueryable<{destination}> ProjectTo(");
    CodeBuilder.AppendLine($"    IQueryable<{source}> source);");
}

// run script
WriteCode()
