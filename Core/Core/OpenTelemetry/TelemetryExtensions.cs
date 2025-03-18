namespace Core.OpenTelemetry;

/// <summary>
/// Provides extension methods to set up OpenTelemetry tracing and metrics within the application.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry (both tracing and metrics) to the IServiceCollection using default options.
    /// </summary>
    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        string serviceName
    ) => AddOpenTelemetry(services, serviceName, OpenTelemetryOptions.Default);

    /// <summary>
    /// Configures the host application builder to register OpenTelemetry services and logging.
    /// </summary>
    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder,
        string serviceName,
        OpenTelemetryOptions? options = null
    )
    {
        // Enhance logging with OpenTelemetry support.
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        // Register OpenTelemetry services using the provided service name and options.
        builder.Services.AddOpenTelemetry(serviceName, options ?? OpenTelemetryOptions.Default);

        // Optionally register additional exporters (e.g., OTLP) based on configuration.
        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// Adds OpenTelemetry to the service collection with tracing and metrics instrumentation.
    /// </summary>
    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        OpenTelemetryOptions options
    )
    {
        // Ensure the default Activity ID format is set to W3C.
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        services.AddOpenTelemetry()
            // Configure metrics instrumentation.
            .WithMetrics(metrics =>
                options.ConfigureMeterProvider(
                    metrics.AddAspNetCoreInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddRuntimeInstrumentation()
                // To add service metadata, uncomment and customize the following:
                // .SetResourceBuilder(
                //     ResourceBuilder.CreateDefault()
                //         .AddService(serviceName)
                //         .AddTelemetrySdk()
                // )
                )
            )
            // Configure tracing instrumentation.
            .WithTracing(tracing =>
            {
                options.ConfigureTracerProvider(
                    tracing.AddAspNetCoreInstrumentation()
                           // Uncomment to add gRPC client instrumentation if required.
                           //.AddGrpcClientInstrumentation()
                           .AddHttpClientInstrumentation()
                );

                // Add a console exporter unless disabled by the options.
                if (!options.ShouldDisableConsoleExporter)
                    tracing.AddConsoleExporter();
            });

        return services;
    }

    /// <summary>
    /// Private helper that registers additional OpenTelemetry exporters (e.g., OTLP) based on configuration.
    /// </summary>
    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        // Use the OTLP exporter if an endpoint is specified in the configuration.
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following to enable Prometheus or Azure Monitor exporters.
        /*
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddPrometheusExporter());

        if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            builder.Services.AddOpenTelemetry()
                .UseAzureMonitor();
        }
        */

        return builder;
    }
}

/// <summary>
/// Represents configuration options for OpenTelemetry, allowing customization of tracing and metrics.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Gets a default set of OpenTelemetryOptions.
    /// </summary>
    public static OpenTelemetryOptions Default => new OpenTelemetryOptions();

    /// <summary>
    /// Delegate to customize the TracerProviderBuilder. Defaults to an identity function.
    /// </summary>
    public Func<TracerProviderBuilder, TracerProviderBuilder> ConfigureTracerProvider { get; private set; } = p => p;

    /// <summary>
    /// Delegate to customize the MeterProviderBuilder. Defaults to an identity function.
    /// </summary>
    public Func<MeterProviderBuilder, MeterProviderBuilder> ConfigureMeterProvider { get; private set; } = p => p;

    /// <summary>
    /// Indicates whether the console exporter should be disabled.
    /// </summary>
    public bool ShouldDisableConsoleExporter { get; private set; }

    // Private constructor enforces the use of default or fluent builder methods.
    private OpenTelemetryOptions() { }

    /// <summary>
    /// Fluent builder method for configuring OpenTelemetryOptions.
    /// </summary>
    public static OpenTelemetryOptions Build(Func<OpenTelemetryOptions, OpenTelemetryOptions> build) =>
        build(Default);

    /// <summary>
    /// Configures tracing via a custom delegate.
    /// </summary>
    public OpenTelemetryOptions WithTracing(Func<TracerProviderBuilder, TracerProviderBuilder> configure)
    {
        this.ConfigureTracerProvider = configure;
        return this;
    }

    /// <summary>
    /// Configures metrics via a custom delegate.
    /// </summary>
    public OpenTelemetryOptions WithMetrics(Func<MeterProviderBuilder, MeterProviderBuilder> configure)
    {
        this.ConfigureMeterProvider = configure;
        return this;
    }

    /// <summary>
    /// Allows disabling of the console exporter.
    /// </summary>
    public OpenTelemetryOptions DisableConsoleExporter(bool shouldDisable)
    {
        ShouldDisableConsoleExporter = shouldDisable;
        return this;
    }
}
