﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Core.WebApi.Swagger;

public class MetadataOperationFilter: IOperationFilter
{
    private static readonly string[] StateChangeMethods =
    [
        HttpMethod.Post.Method, HttpMethod.Put.Method, HttpMethod.Delete.Method
    ];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        if (context.ApiDescription.HttpMethod != null && StateChangeMethods.Contains(context.ApiDescription.HttpMethod))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match",
                In = ParameterLocation.Header,
                Description = "If-Match",
                Required = context.ApiDescription.HttpMethod != HttpMethod.Post.Method
            });
        }
    }
}
