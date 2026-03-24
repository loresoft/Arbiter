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

    TemplateOptions.Parameters.TryGetValue("excludeDomain", out var excludeDomain);
    TemplateOptions.Parameters.TryGetValue("excludeEntity", out var excludeEntity);

    var hasReadModel = !string.IsNullOrEmpty(readModel);
    var hasCreateModel = !string.IsNullOrEmpty(createModel);
    var hasUpdateModel = !string.IsNullOrEmpty(updateModel);
    var includeEntity = string.IsNullOrEmpty(excludeEntity);
    var includeDomain = string.IsNullOrEmpty(excludeDomain);

    // skip if no models are defined
    if (!hasReadModel && !hasCreateModel && !hasUpdateModel)
        return string.Empty;

    // skip readonly for domain mapping
    if (includeDomain && !includeEntity && !hasCreateModel && !hasUpdateModel)
        return string.Empty;

    CodeBuilder.Clear();
    CodeBuilder.AppendLine("#pragma warning disable IDE0130 // Namespace does not match folder structure");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine("using Arbiter.Mapping;");
    CodeBuilder.AppendLine();

    if (includeEntity)
    {
        CodeBuilder.AppendLine($"using Entities = {entityNamespace};");
    }
    CodeBuilder.AppendLine($"using Models = {modelNamespace};");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine($"namespace {TemplateOptions.Namespace};");
    CodeBuilder.AppendLine();

    if (includeDomain)
    {
        if (hasReadModel && hasCreateModel)
            GenerateClass(readModel, createModel, "Models", "Models");

        if (hasReadModel && hasUpdateModel)
            GenerateClass(readModel, updateModel, "Models", "Models");

        if (hasUpdateModel && hasCreateModel)
            GenerateClass(updateModel, createModel, "Models", "Models");
    }

    if (includeEntity)
    {
        if (hasReadModel)
            GenerateClass(entityClass, readModel, "Entities", "Models");

        if (hasUpdateModel)
            GenerateClass(entityClass, updateModel, "Entities", "Models");

        if (hasCreateModel)
            GenerateClass(createModel, entityClass, "Models", "Entities");

        if (hasUpdateModel)
            GenerateClass(updateModel, entityClass, "Models", "Entities");
    }

    return CodeBuilder.ToString();
}

private void GenerateClass(string sourceClass, string destinationClass, string sourcePrefix, string destinationPrefix)
{
    var className = $"{sourceClass}To{destinationClass}Mapper";
    var sourceType = $"{sourcePrefix}.{sourceClass}";
    var destinationType = $"{destinationPrefix}.{destinationClass}";

    CodeBuilder.AppendLine("[GenerateMapper]");
    CodeBuilder.AppendLine("[RegisterSingleton]");
    CodeBuilder.AppendLine($"internal sealed partial class {className}");
    CodeBuilder.AppendLine($"    : MapperProfile<{sourceType}, {destinationType}>");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    CodeBuilder.AppendLine($"protected override void ConfigureMapping(MappingBuilder<{sourceType}, {destinationType}> mapping)");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();
    CodeBuilder.AppendLine("// custom mapping here");
    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}");

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}");
    CodeBuilder.AppendLine();
}

// run script
WriteCode()
