namespace Core.Structures;

/// <summary>
/// Represents a container for a value that might or might not be present.
/// </summary>
public class Maybe<TSomething>
{
    private readonly TSomething? value;

    // Indicates if a value is present.
    public bool IsPresent { get; }

    // Private constructor to enforce controlled creation.
    private Maybe(TSomething value, bool isPresent)
    {
        this.value = value;
        this.IsPresent = isPresent;
    }

    // Represents an empty Maybe.
    public static readonly Maybe<TSomething> Empty = new(default!, false);

    // Creates a Maybe containing a value if it's non-null, otherwise returns Empty.
    public static Maybe<TSomething> Of(TSomething value) =>
        value != null ? new Maybe<TSomething>(value, true) : Empty;

    // Creates a Maybe if the condition is true, otherwise returns Empty.
    public static Maybe<TSomething> If(bool check, Func<TSomething> getValue) =>
        check ? new Maybe<TSomething>(getValue(), true) : Empty;

    // Returns the value if present, otherwise throws an exception.
    public TSomething GetOrThrow() =>
        IsPresent ? value! : throw new ArgumentNullException(nameof(value));

    // Returns the value if present, otherwise returns a default value.
    public TSomething GetOrDefault(TSomething defaultValue = default!) =>
        IsPresent ? value ?? defaultValue : defaultValue;

    // Executes an action if the value is present.
    public void IfExists(Action<TSomething> perform)
    {
        if (IsPresent)
        {
            perform(value!);
        }
    }
}

public static class Maybe
{
    public static Maybe<TSomething> Of<TSomething>(TSomething value) =>
        Maybe<TSomething>.Of(value);

    public static Maybe<TSomething> If<TSomething>(bool check, Func<TSomething> getValue) =>
        Maybe<TSomething>.If(check, getValue);

}