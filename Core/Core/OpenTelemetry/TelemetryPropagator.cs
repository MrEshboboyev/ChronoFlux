namespace Core.OpenTelemetry;

/// <summary>
/// Provides helper methods for context propagation using OpenTelemetry.
/// This includes methods to inject, extract, and propagate tracing information
/// into carriers used for distributed tracing.
/// </summary>
public static class TelemetryPropagator
{
    // Begin with the default text map propagator provided by OpenTelemetry.
    private static TextMapPropagator propagator = Propagators.DefaultTextMapPropagator;

    /// <summary>
    /// Reconfigures the propagator to a composite propagator which uses both the
    /// TraceContext and Baggage propagators.
    /// </summary>
    public static void UseDefaultCompositeTextMapPropagator()
    {
        propagator = new CompositeTextMapPropagator(
        [
            new TraceContextPropagator(),
            new BaggagePropagator()
        ]);
    }

    /// <summary>
    /// Injects the propagation context into the specified carrier using the provided setter.
    /// </summary>
    /// <typeparam name="T">The type representing the carrier.</typeparam>
    /// <param name="context">The propagation context to inject.</param>
    /// <param name="carrier">The carrier that will have context values injected.</param>
    /// <param name="setter">Action delegate to set key-value pairs into the carrier.</param>
    public static void Inject<T>(
        this PropagationContext context,
        T carrier,
        Action<T, string, string> setter
    ) => propagator.Inject(context, carrier, setter);

    /// <summary>
    /// Extracts a propagation context from the specified carrier using the provided getter.
    /// </summary>
    /// <typeparam name="T">The type representing the carrier.</typeparam>
    /// <param name="carrier">The carrier from which to extract context values.</param>
    /// <param name="getter">Function to retrieve header values.</param>
    /// <returns>The extracted <see cref="PropagationContext"/>.</returns>
    public static PropagationContext Extract<T>(
        T carrier,
        Func<T, string, IEnumerable<string>> getter
    ) => propagator.Extract(default, carrier, getter);

    /// <summary>
    /// Extracts a propagation context using an existing context as a base.
    /// </summary>
    /// <typeparam name="T">The type representing the carrier.</typeparam>
    /// <param name="context">An existing propagation context.</param>
    /// <param name="carrier">The carrier from which to extract.</param>
    /// <param name="getter">Function to retrieve header values.</param>
    /// <returns>The updated <see cref="PropagationContext"/>.</returns>
    public static PropagationContext Extract<T>(
        PropagationContext context,
        T carrier,
        Func<T, string, IEnumerable<string>> getter
    ) => propagator.Extract(context, carrier, getter);

    /// <summary>
    /// Creates a propagation context from an activity's context and injects it
    /// into the given carrier.
    /// </summary>
    /// <typeparam name="T">The type representing the carrier.</typeparam>
    /// <param name="activity">The activity whose trace context will be propagated.</param>
    /// <param name="carrier">The carrier to inject the context into.</param>
    /// <param name="setter">An action delegate to set context key-value pairs in the carrier.</param>
    /// <returns>
    /// The <see cref="PropagationContext"/> that was injected into the carrier,
    /// or null if no valid activity context is found.
    /// </returns>
    public static PropagationContext? Propagate<T>(this Activity? activity, T carrier, Action<T, string, string> setter)
    {
        if (activity?.Context == null)
            return null;

        var propagationContext = new PropagationContext(activity.Context, Baggage.Current);
        propagationContext.Inject(carrier, setter);
        return propagationContext;
    }

    /// <summary>
    /// Returns a propagation context created from the given activity (or the current activity if none is provided).
    /// </summary>
    /// <param name="activity">
    /// An optional activity from which to derive the propagation context.
    /// If omitted, the ambient <c>Activity.Current</c> is used.
    /// </param>
    /// <returns>
    /// A new <see cref="PropagationContext"/> capturing the activity context and current baggage,
    /// or null if no valid context exists.
    /// </returns>
    public static PropagationContext? GetPropagationContext(Activity? activity = null)
    {
        var activityContext = (activity ?? Activity.Current)?.Context;
        if (!activityContext.HasValue)
            return null;

        return new PropagationContext(activityContext.Value, Baggage.Current);
    }
}
