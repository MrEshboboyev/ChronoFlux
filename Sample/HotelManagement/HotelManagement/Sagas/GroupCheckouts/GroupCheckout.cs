using Core.Extensions;
using Core.Structures;

namespace HotelManagement.Sagas.GroupCheckouts;

public record GroupCheckoutInitiated(
    Guid GroupCheckoutId,
    Guid ClerkId,
    Guid[] GuestStayIds,
    DateTimeOffset InitiatedAt
);

public record GuestCheckoutsInitiated(
    Guid GroupCheckoutId,
    Guid[] InitiatedGuestStayIds,
    DateTimeOffset InitiatedAt
);

public record GuestCheckoutCompleted(
    Guid GroupCheckoutId,
    Guid GuestStayId,
    DateTimeOffset CompletedAt
);

public record GuestCheckoutFailed(
    Guid GroupCheckoutId,
    Guid GuestStayId,
    DateTimeOffset FailedAt
);

public record GroupCheckoutCompleted(
    Guid GroupCheckoutId,
    Guid[] CompletedCheckouts,
    DateTimeOffset CompletedAt
);

public record GroupCheckoutFailed(
    Guid GroupCheckoutId,
    Guid[] CompletedCheckouts,
    Guid[] FailedCheckouts,
    DateTimeOffset FailedAt
);

    public record GroupCheckout(
        Guid Id,
        Dictionary<Guid, CheckoutStatus> GuestStayCheckouts,
        CheckoutStatus Status = CheckoutStatus.Initiated
    )
    {
        public static GroupCheckoutInitiated Initiate(Guid groupCheckoutId, Guid clerkId, Guid[] guestStayIds, DateTimeOffset initiatedAt) =>
            new(groupCheckoutId, clerkId, guestStayIds, initiatedAt);

        public Maybe<GuestCheckoutsInitiated> RecordGuestCheckoutsInitiation(
            Guid[] initiatedGuestStayIds,
            DateTimeOffset now
        ) =>
            Maybe.If(
                Status == CheckoutStatus.Initiated,
                () => new GuestCheckoutsInitiated(Id, initiatedGuestStayIds, now)
            );

        public Maybe<object[]> RecordGuestCheckoutCompletion(
            Guid guestStayId,
            DateTimeOffset now
        ) =>
            Maybe.If(
                Status == CheckoutStatus.Initiated && GuestStayCheckouts[guestStayId] != CheckoutStatus.Completed,
                () =>
                {
                    var guestCheckoutCompleted = new GuestCheckoutCompleted(Id, guestStayId, now);

                    var guestStayCheckouts = GuestStayCheckouts.With(guestStayId, CheckoutStatus.Completed);

                    return AreAnyOngoingCheckouts(guestStayCheckouts)
                        ? [guestCheckoutCompleted]
                        : new[] { guestCheckoutCompleted, Finalize(guestStayCheckouts, now) };
                });

        public Maybe<object[]> RecordGuestCheckoutFailure(
            Guid guestStayId,
            DateTimeOffset now
        ) =>
            Maybe.If(
                Status == CheckoutStatus.Initiated && GuestStayCheckouts[guestStayId] != CheckoutStatus.Failed,
                () =>
                {
                    var guestCheckoutFailed = new GuestCheckoutFailed(Id, guestStayId, now);

                    var guestStayCheckouts = GuestStayCheckouts.With(guestStayId, CheckoutStatus.Failed);

                    return AreAnyOngoingCheckouts(guestStayCheckouts)
                        ? [guestCheckoutFailed]
                        : new[] { guestCheckoutFailed, Finalize(guestStayCheckouts, now) };
                });

        private object Finalize(Dictionary<Guid, CheckoutStatus> guestStayCheckouts, DateTimeOffset now) =>
            !AreAnyFailedCheckouts(guestStayCheckouts)
                ? new GroupCheckoutCompleted
                (
                    Id,
                    CheckoutsWith(guestStayCheckouts, CheckoutStatus.Completed),
                    now
                )
                : new GroupCheckoutFailed
                (
                    Id,
                    CheckoutsWith(guestStayCheckouts, CheckoutStatus.Completed),
                    CheckoutsWith(guestStayCheckouts, CheckoutStatus.Failed),
                    now
                );

        private static bool AreAnyOngoingCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
            guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Initiated or CheckoutStatus.Pending);

        private static bool AreAnyFailedCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
            guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Failed);

        private static Guid[] CheckoutsWith(Dictionary<Guid, CheckoutStatus> guestStayCheckouts, CheckoutStatus status) =>
            [.. guestStayCheckouts
                .Where(pair => pair.Value == status)
                .Select(pair => pair.Key)];

        public static GroupCheckout Create(GroupCheckoutInitiated @event) =>
            new(
                @event.GroupCheckoutId,
                @event.GuestStayIds.ToDictionary(id => id, _ => CheckoutStatus.Pending)
            );

        public GroupCheckout Apply(GuestCheckoutsInitiated @event)
        {
            var initiated = @event.InitiatedGuestStayIds.ToDictionary(
                id => id,
                _ => CheckoutStatus.Initiated
            );

            return this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => initiated.GetValueOrDefault(vs.Key, vs.Value)
                )
            };
        }

        public GroupCheckout Apply(GuestCheckoutCompleted @event) =>
            this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == @event.GuestStayId ? CheckoutStatus.Completed : vs.Value
                )
            };

        public GroupCheckout Apply(GuestCheckoutFailed @event) =>
            this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == @event.GuestStayId ? CheckoutStatus.Failed : vs.Value
                )
            };

        public GroupCheckout Apply(GroupCheckoutCompleted @event) =>
            this with { Status = CheckoutStatus.Completed };

        public GroupCheckout Apply(GroupCheckoutFailed @event) =>
            this with { Status = CheckoutStatus.Failed };
    }

    public enum CheckoutStatus
    {
        Pending,
        Initiated,
        Completed,
        Failed
    }
