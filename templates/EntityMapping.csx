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

    CodeBuilder.Clear();
    CodeBuilder.AppendLine("#pragma warning disable IDE0130 // Namespace does not match folder structure");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine("using Arbiter.CommandQuery.Definitions;");
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
        GenerateClass(readModel, createModel);
        GenerateClass(readModel, updateModel);
        GenerateClass(updateModel, createModel);
    }

    if (string.IsNullOrEmpty(excludeEntity))
    {
        GenerateClass(Entity, readModel);
        GenerateClass(Entity, updateModel);
        GenerateClass(createModel, Entity);
        GenerateClass(updateModel, Entity);
    }

    return CodeBuilder.ToString();
}

private void GenerateClass(object? source, object? destination)
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
    CodeBuilder.AppendLine($"internal sealed class {className} : Arbiter.CommandQuery.Mapping.MapperBase<{sourceName}, {destinationName}>");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    WriteMapper(sourceName, destinationName, sourceProperties, destinationProperties);

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}");
    CodeBuilder.AppendLine();
}

private void WriteMapper(
    string sourceType,
    string destinationType,
    IEnumerable<Property> sourceProperties,
    IEnumerable<Property> destinationProperties)
{
    if (sourceProperties == null || destinationProperties == null)
        return;

    var destinationNames = destinationProperties.Select(d => d.PropertyName);
    var commonProperties = sourceProperties.IntersectBy(destinationNames, p => p.PropertyName);

    WriteCopyMap(sourceType, destinationType, commonProperties);
    CodeBuilder.AppendLine();

    WriteQueryMap(sourceType, destinationType, commonProperties);
}

private void WriteQueryMap(string sourceType, string destinationType, IEnumerable<Property> properties)
{
    CodeBuilder.AppendLine($"public override IQueryable<{destinationType}> ProjectTo(IQueryable<{sourceType}> source)");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    CodeBuilder.AppendLine("ArgumentNullException.ThrowIfNull(source);");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine($"return source.Select(p =>");
    CodeBuilder.IncrementIndent();
    CodeBuilder.AppendLine($"new {destinationType}");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    CodeBuilder.AppendLine("#region Generated Query Properties");
    foreach (var property in properties)
    {
        CodeBuilder.AppendLine($"{property.PropertyName} = p.{property.PropertyName},");
    }
    CodeBuilder.AppendLine("#endregion");

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}"); // object initializer end

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine(");");

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}"); // function end
}

private void WriteCopyMap(string sourceType, string destinationType, IEnumerable<Property> properties)
{
    CodeBuilder.AppendLine($"public override void Map({sourceType} source, {destinationType} destination)");
    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    CodeBuilder.AppendLine("ArgumentNullException.ThrowIfNull(source);");
    CodeBuilder.AppendLine("ArgumentNullException.ThrowIfNull(destination);");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine("#region Generated Copied Properties");
    foreach (var property in properties)
    {
        CodeBuilder.AppendLine($"destination.{property.PropertyName} = source.{property.PropertyName};");
    }
    CodeBuilder.AppendLine("#endregion");

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}"); // function end
}

private static (string? ClassNamespace, string? ClassName, IEnumerable<Property>? Properties) GetNames(object value)
{
    return value switch
    {
        Model model => ("Models", model.ModelClass.ToSafeName(), model.Properties),
        Entity entity => ("Entities", entity.EntityClass.ToSafeName(), entity.Properties),
        _ => (null, null, null)
    };
}

// run script
WriteCode()
