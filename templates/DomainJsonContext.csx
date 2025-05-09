public string WriteCode()
{
    CodeBuilder.Clear();

    CodeBuilder.AppendLine("using System;");
    CodeBuilder.AppendLine("using System.Collections.Generic;");
    CodeBuilder.AppendLine("using System.Text.Json.Serialization;");
    CodeBuilder.AppendLine("using Arbiter.CommandQuery.Queries;");

    var names = EntityContext.Entities
        .SelectMany(e => e.Models.Select(m => m.ModelNamespace))
        .ToHashSet();

    foreach (var name in names)
        CodeBuilder.AppendLine($"using {name};");

    CodeBuilder.AppendLine();
    CodeBuilder.AppendLine("// ReSharper disable once CheckNamespace");
    CodeBuilder.AppendLine($"namespace {TemplateOptions.Namespace};");
    CodeBuilder.AppendLine();

    GenerateClass();

    CodeBuilder.AppendLine();

    return CodeBuilder.ToString();
}

private void GenerateClass()
{
    CodeBuilder.AppendLine($"[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]");
    CodeBuilder.AppendLine("#region Generated Attributes");

    foreach (var entity in EntityContext.Entities.OrderBy(e => e.ContextProperty))
    {
        var readModel = string.Empty;
        var createModel = string.Empty;
        var updateModel = string.Empty;

        foreach (var model in entity.Models)
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

        CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityIdentifierQuery<int, {readModel}>))]");
        CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityIdentifiersQuery<int, {readModel}>))]");
        CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityPagedQuery<{readModel}>))]");
        CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntitySelectQuery<{readModel}>))]");

        CodeBuilder.AppendLine($"[JsonSerializable(typeof({readModel}))]");
        CodeBuilder.AppendLine($"[JsonSerializable(typeof(IReadOnlyCollection<{readModel}>))]");
        CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityPagedResult<{readModel}>))]");

        if (!string.IsNullOrEmpty(createModel))
        {
            CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityCreateCommand<{createModel}, {readModel}>))]");

            CodeBuilder.AppendLine($"[JsonSerializable(typeof({createModel}))]");
        }

        if (!string.IsNullOrEmpty(updateModel))
        {
            CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityIdentifierQuery<int, {updateModel}>))]");
            CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityIdentifiersQuery<int, {updateModel}>))]");

            CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityUpdateCommand<int, {updateModel}, {readModel}>))]");
            CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityUpsertCommand<int, {updateModel}, {readModel}>))]");
            CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityPatchCommand<int, {readModel}>))]");
            CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityDeleteCommand<int, {readModel}>))]");

            CodeBuilder.AppendLine($"[JsonSerializable(typeof({updateModel}))]");
        }
    }

    // query types
    CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntityQuery))]");
    CodeBuilder.AppendLine($"[JsonSerializable(typeof(EntitySelect))]");

    CodeBuilder.AppendLine("#endregion");

    string className = System.IO.Path.GetFileNameWithoutExtension(TemplateOptions.FileName);

    CodeBuilder.AppendLine($"public partial class {className} : JsonSerializerContext");
    CodeBuilder.AppendLine("{");
    CodeBuilder.AppendLine("}");
}


// run script
WriteCode()
