using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Core.Scheduling;

/// <summary>
/// Provides extension methods for configuring Quartz scheduler with default settings,
/// including registration of passage-of-time jobs.
/// </summary>
public static class QuartzExtensions
{
    /// <summary>
    /// Adds default Quartz configuration to the service collection, including jobs for the passage of time.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <returns>The updated services collection with Quartz configured.</returns>
    public static IServiceCollection AddQuartzDefaults(this IServiceCollection services) =>
        services
            .AddQuartz(q =>
            {
                q.AddPassageOfTime(TimeUnit.Minute)
                 .AddPassageOfTime(TimeUnit.Hour)
                 .AddPassageOfTime(TimeUnit.Day);

                // Uncomment the following and provide configuration to enable persistent storage for Quartz:
                /*
                q.UsePersistentStore(x =>
                {
                    x.UseProperties = false;
                    x.PerformSchemaValidation = false;
                    x.UsePostgres(postgresConnectionString);
                });
                */
            })
            .AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    /// <summary>
    /// Registers a passage-of-time job and its trigger for a specified time unit.
    /// </summary>
    /// <param name="q">The Quartz configurator.</param>
    /// <param name="timeUnit">The time unit for which to schedule the job.</param>
    /// <returns>The updated Quartz configurator.</returns>
    public static IServiceCollectionQuartzConfigurator AddPassageOfTime(
        this IServiceCollectionQuartzConfigurator q,
        TimeUnit timeUnit)
    {
        // Create a unique job key for the PassageOfTimeJob based on the time unit.
        var jobKey = new JobKey($"PassageOfTimeJob_{timeUnit}");

        // Register the PassageOfTimeJob.
        q.AddJob<PassageOfTimeJob>(opts => opts.WithIdentity(jobKey));

        // Add a trigger that schedules the job at a fixed interval based on the time unit.
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{jobKey}-trigger")
            .UsingJobData("timeUnit", timeUnit.ToString())
            .WithSimpleSchedule(x => x.WithInterval(timeUnit.ToTimeSpan()))
        );

        return q;
    }
}
