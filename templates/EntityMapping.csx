public string WriteCode()
{
    if (Entity.Models.Count == 0)
        return string.Empty;

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
    CodeBuilder.AppendLine();

    if (!string.IsNullOrEmpty(modelNamespace))
    {
        CodeBuilder.AppendLine($"using {modelNamespace};");
        CodeBuilder.AppendLine();
    }

    CodeBuilder.AppendLine($"namespace {TemplateOptions.Namespace};");
    CodeBuilder.AppendLine();

    GenerateClass(readModel, createModel, updateModel);

    CodeBuilder.AppendLine();

    return CodeBuilder.ToString();
}

private void GenerateClass(string readModel, string createModel, string updateModel)
{
    var className = System.IO.Path.GetFileNameWithoutExtension(TemplateOptions.FileName);
    CodeBuilder.AppendLine($"public class {className} : AutoMapper.Profile");

    CodeBuilder.AppendLine("{");

    using (CodeBuilder.Indent())
    {
        CodeBuilder.AppendLine($"public {className}()");
        CodeBuilder.AppendLine("{");
        using (CodeBuilder.Indent())
        {
            CodeBuilder.AppendLine($"CreateMap<{readModel}, {createModel}>()");
            CodeBuilder.AppendLine($"    .ForMember(d => d.Id, opt => opt.Ignore());");
            CodeBuilder.AppendLine();
            CodeBuilder.AppendLine($"CreateMap<{updateModel}, {createModel}>()");
            CodeBuilder.AppendLine($"    .ForMember(d => d.Id, opt => opt.Ignore());");
            CodeBuilder.AppendLine();
            CodeBuilder.AppendLine($"CreateMap<{readModel}, {updateModel}>();");
            CodeBuilder.AppendLine();
            CodeBuilder.AppendLine($"CreateMap<{updateModel}, {readModel}>();");
        }
        CodeBuilder.AppendLine("}");
        CodeBuilder.AppendLine();
    }

    CodeBuilder.AppendLine("}");
}

// run script
WriteCode()
