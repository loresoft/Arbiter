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

    CodeBuilder.Clear();

    CodeBuilder.AppendLine("using System;");
    CodeBuilder.AppendLine("using System.Diagnostics.CodeAnalysis;");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine("using Injectio.Attributes;");
    CodeBuilder.AppendLine("using Riok.Mapperly.Abstractions;");
    CodeBuilder.AppendLine();

    CodeBuilder.AppendLine($"using Entities = {entityNamespace};");
    CodeBuilder.AppendLine($"using Models = {modelNamespace};");

    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine("#pragma warning disable IDE0130 // Namespace does not match folder structure");
    CodeBuilder.AppendLine($"namespace {TemplateOptions.Namespace};");
    CodeBuilder.AppendLine();

    GenerateClass(entityClass, readModel, createModel, updateModel);

    CodeBuilder.AppendLine();

    return CodeBuilder.ToString();
}

private void GenerateClass(string entityClass, string readModel, string createModel, string updateModel)
{
    var className = System.IO.Path.GetFileNameWithoutExtension(TemplateOptions.FileName);
    TemplateOptions.Parameters.TryGetValue("excludeEntity", out var excludeEntity);

    CodeBuilder.AppendLine("[Mapper]");
    CodeBuilder.AppendLine("[RegisterSingleton]");
    CodeBuilder.AppendLine($"public partial class {className}");
    CodeBuilder.IncrementIndent();

    CodeBuilder.AppendLine($": IMapper<Models.{readModel}, Models.{createModel}>");
    CodeBuilder.AppendLine($", IMapper<Models.{readModel}, Models.{updateModel}>");
    CodeBuilder.AppendLine($", IMapper<Models.{updateModel}, Models.{createModel}>");
    CodeBuilder.AppendLine($", IMapper<Models.{updateModel}, Models.{readModel}>");

    if (string.IsNullOrEmpty(excludeEntity))
    {
        CodeBuilder.AppendLine($", IMapper<Entities.{entityClass}, Models.{readModel}>");
        CodeBuilder.AppendLine($", IMapper<Entities.{entityClass}, Models.{updateModel}>");
        CodeBuilder.AppendLine($", IMapper<Models.{createModel}, Entities.{entityClass}>");
        CodeBuilder.AppendLine($", IMapper<Models.{updateModel}, Entities.{entityClass}>");
    }

    CodeBuilder.DecrementIndent();

    CodeBuilder.AppendLine("{");
    CodeBuilder.IncrementIndent();

    WriteMapper($"Models.{readModel}", $"Models.{createModel}");
    WriteMapper($"Models.{readModel}", $"Models.{updateModel}");
    WriteMapper($"Models.{updateModel}", $"Models.{createModel}");
    WriteMapper($"Models.{updateModel}", $"Models.{readModel}");

    if (string.IsNullOrEmpty(excludeEntity))
    {
        WriteMapper($"Entities.{entityClass}", $"Models.{readModel}");
        WriteMapper($"Entities.{entityClass}", $"Models.{updateModel}");
        WriteMapper($"Models.{createModel}", $"Entities.{entityClass}");
        WriteMapper($"Models.{updateModel}", $"Entities.{entityClass}");
    }

    CodeBuilder.DecrementIndent();
    CodeBuilder.AppendLine("}");
}

private void WriteMapper(string source, string destination)
{
    CodeBuilder.AppendLine($"#region Mapper {source} -> {destination}");
    CodeBuilder.AppendLine($"[return: NotNullIfNotNull(nameof(source))]");
    CodeBuilder.AppendLine($"public partial {destination}? Map({source}? source);");
    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine($"public partial void Map({source} source, {destination} destination);");
    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine($"public partial IQueryable<{destination}> ProjectTo(IQueryable<{source}> source);");
    CodeBuilder.AppendLine("#endregion");
    CodeBuilder.AppendLine();
}

// run script
WriteCode()
