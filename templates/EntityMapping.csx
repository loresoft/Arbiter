public string WriteCode()
{
    if (Entity.Models.Count == 0)
        return string.Empty;

    var entityClass = Entity.EntityClass.ToSafeName();

    var entityNamespace = Entity.EntityNamespace;
    var modelNamespace = Entity.Models.Select(m => m.ModelNamespace).FirstOrDefault();

    Model? readModel = null;
    Model? createModel = null;
    Model? updateModel = null;

    foreach (var model in Entity.Models)
    {
        switch (model.ModelType)
        {
            case ModelType.Read:
                readModel = model;
                break;
            case ModelType.Create:
                createModel = model;
                break;
            case ModelType.Update:
                updateModel = model;
                break;
        }
    }

    if (updateModel == null)
        return string.Empty;

    TemplateOptions.Parameters.TryGetValue("excludeDomain", out var excludeDomain);
    TemplateOptions.Parameters.TryGetValue("excludeEntity", out var excludeEntity);

    TemplateOptions.Parameters.TryGetValue("readMapping", out var readMapping);
    var readProperties = readMapping?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    TemplateOptions.Parameters.TryGetValue("createMapping", out var createMapping);
    var createProperties = createMapping?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    TemplateOptions.Parameters.TryGetValue("updateMapping", out var updateMapping);
    var updateProperties = updateMapping?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    CodeBuilder.Clear();
    CodeBuilder.AppendLine("#pragma warning disable IDE0130 // Namespace does not match folder structure");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine("using System.Linq.Expressions;");
    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine("using Arbiter.CommandQuery.Definitions;");
    CodeBuilder.AppendLine("using Arbiter.CommandQuery.Mapping;");
    CodeBuilder.AppendLine();


    if (string.IsNullOrEmpty(excludeEntity))
    {
        CodeBuilder.AppendLine($"using E = {entityNamespace};");
    }
    CodeBuilder.AppendLine($"using M = {modelNamespace};");

    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine($"namespace {TemplateOptions.Namespace};");
    CodeBuilder.AppendLine();

    if (string.IsNullOrEmpty(excludeDomain))
    {
        GenerateClass(readModel, createModel, readProperties.Intersect(createProperties).ToArray());
        GenerateClass(readModel, updateModel, readProperties.Intersect(updateProperties).ToArray());
        GenerateClass(updateModel, createModel, updateProperties.Intersect(createProperties).ToArray());
    }

    if (string.IsNullOrEmpty(excludeEntity))
    {
        GenerateClass(Entity, readModel, readProperties);
        GenerateClass(Entity, updateModel, readProperties.Intersect(updateProperties).ToArray());
        GenerateClass(createModel, Entity, createProperties.Intersect(readProperties).ToArray());
        GenerateClass(updateModel, Entity, updateProperties.Intersect(readProperties).ToArray());
    }

    return CodeBuilder.ToString();
}

private void GenerateClass(object? source, object? destination, string[] manualProperties)
{
    if (source == null || destination == null)
        return;

    var (sourceNamespace, sourceClass, sourceProperties) = GetNames(source);
    var (destinationNamespace, destinationClass, destinationProperties) = GetNames(destination);

    if (sourceNamespace == null
        || destinationNamespace == null
        || sourceClass == null
        || destinationClass == null
        || sourceProperties == null
        || destinationProperties == null)
    {
        return;
    }

    var className = $"{sourceClass}To{destinationClass}Mapper";

    var sourceName = $"{sourceNamespace}.{sourceClass}";
    var destinationName = $"{destinationNamespace}.{destinationClass}";

    CodeBuilder.AppendLine($"[RegisterSingleton<IMapper<{sourceName}, {destinationName}>>]");
    CodeBuilder.AppendLine($"internal sealed class {className}");
    CodeBuilder.AppendLine($"    : MapperBase<{sourceName}, {destinationName}>");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    WriteMapper(sourceName, destinationName, sourceProperties, destinationProperties, manualProperties);

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}");
    CodeBuilder.AppendLine();
}

private void WriteMapper(
    string sourceType,
    string destinationType,
    IEnumerable<Property> sourceProperties,
    IEnumerable<Property> destinationProperties,
    string[] manualProperties)
{
    if (sourceProperties == null || destinationProperties == null)
        return;

    var destinationNames = destinationProperties.Select(d => d.PropertyName);
    var commonProperties = sourceProperties.IntersectBy(destinationNames, p => p.PropertyName);

    WriteExpressionMap(sourceType, destinationType, commonProperties, manualProperties);
}

private void WriteExpressionMap(string sourceType, string destinationType, IEnumerable<Property> properties, string[] manualProperties)
{
    CodeBuilder.AppendLine($"protected override Expression<Func<{sourceType}, {destinationType}>> CreateMapping()");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    CodeBuilder.AppendLine($"return source => new {destinationType}");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    CodeBuilder.AppendLine("#region Generated Mappings");
    foreach (var property in properties)
    {
        CodeBuilder.AppendLine($"{property.PropertyName} = source.{property.PropertyName},");
    }
    CodeBuilder.AppendLine("#endregion");


    if (manualProperties.Length > 0 && properties.Any())
    {
        CodeBuilder.AppendLine();
        CodeBuilder.AppendLine("// Manual Mappings");
    }

    foreach (var property in manualProperties)
    {
        CodeBuilder.AppendLine($"{property} = source.{property},");
    }

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("};"); // object initializer end

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}"); // function end
}

private static (string? ClassNamespace, string? ClassName, IEnumerable<Property>? Properties) GetNames(object value)
{
    return value switch
    {
        Model model => ("M", model.ModelClass.ToSafeName(), model.Properties),
        Entity entity => ("E", entity.EntityClass.ToSafeName(), entity.Properties),
        _ => (null, null, null)
    };
}

// run script
WriteCode()
