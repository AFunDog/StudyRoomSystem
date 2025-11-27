using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.Server.OpenApi;

[Obsolete]
internal class ModelDescSchemeTransformer : IOpenApiSchemaTransformer
{
    // public Task TransformAsync(
    //     OpenApiDocument document,
    //     OpenApiDocumentTransformerContext context,
    //     CancellationToken cancellationToken)
    // {
    //     
    //     foreach (var (_,schema) in document.Components.Schemas)
    //     {
    //         
    //     }
    //     // Log.Logger.Trace().Debug("{@Schemas}",document.Components);
    //
    //     return Task.CompletedTask;
    // }

    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.ParameterDescription?.Type is {} type && type.GetCustomAttribute<DescriptionAttribute>() is {} description)
        {
            // Log.Logger.Trace().Debug("{Type}",context.ParameterDescription.Type);
            schema.Title ??= description.Description;
        }
        // if(schema.Xml is not null)
        //     Log.Logger.Trace().Debug("{Type}",schema.Xml);
        // if (schema.Type is not null)
        //     Log.Logger.Trace().Debug("{Title} {Type}",context.JsonTypeInfo.Type, schema.Type);
        
    }
}