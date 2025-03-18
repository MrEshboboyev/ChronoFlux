namespace Core.OpenTelemetry;

/// <summary>
/// Contains static constants for telemetry tags used in tracing and metrics.
/// Tags are organized into functional groups such as logic, commands, queries,
/// events, service metadata, and messaging.
/// </summary>
public static class TelemetryTags
{
    /// <summary>
    /// Contains tags related to logical entity tracking.
    /// </summary>
    public static class Logic
    {
        /// <summary>
        /// Base tag for an entity.
        /// </summary>
        public const string Entity = $"{ActivitySourceProvider.DefaultSourceName}.entity";

        /// <summary>
        /// Tag that identifies the type of an entity.
        /// </summary>
        public const string EntityType = $"{Entity}.type";

        /// <summary>
        /// Tag that represents the unique identifier of an entity.
        /// </summary>
        public const string EntityId = $"{Entity}.id";

        /// <summary>
        /// Tag that represents the version number of an entity.
        /// </summary>
        public const string EntityVersion = $"{Entity}.version";
    }

    /// <summary>
    /// Tag representing stream-related telemetry.
    /// </summary>
    public const string Stream = $"{ActivitySourceProvider.DefaultSourceName}.stream";

    /// <summary>
    /// Contains tags related to command processing.
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// Base tag for a command.
        /// </summary>
        public const string Command = $"{ActivitySourceProvider.DefaultSourceName}.command";

        /// <summary>
        /// Tag indicating the command type.
        /// </summary>
        public const string CommandType = $"{Command}.type";

        /// <summary>
        /// Meter tag for command-related metrics.
        /// </summary>
        public const string CommandsMeter = $"{ActivitySourceProvider.DefaultSourceName}.commands";

        /// <summary>
        /// Tag related to command handling operations.
        /// </summary>
        public const string CommandHandling = $"{CommandsMeter}.handling";

        /// <summary>
        /// Tag to record the number of active commands.
        /// </summary>
        public const string ActiveCommandsNumber = $"{CommandHandling}.active.number";

        /// <summary>
        /// Tag to record the total number of commands.
        /// </summary>
        public const string TotalCommandsNumber = $"{CommandHandling}.total";

        /// <summary>
        /// Tag representing the duration of command handling.
        /// </summary>
        public const string CommandHandlingDuration = $"{CommandHandling}.duration";
    }

    /// <summary>
    /// Contains tags related to query handling.
    /// </summary>
    public static class QueryHandling
    {
        /// <summary>
        /// Tag for queries.
        /// </summary>
        public const string Query = $"{ActivitySourceProvider.DefaultSourceName}.query";
    }

    /// <summary>
    /// Contains tags related to event handling.
    /// </summary>
    public static class EventHandling
    {
        /// <summary>
        /// Tag for events.
        /// </summary>
        public const string Event = $"{ActivitySourceProvider.DefaultSourceName}.event";
    }

    /// <summary>
    /// Contains service metadata tags.
    /// </summary>
    public static class Service
    {
        /// <summary>
        /// Tag representing the name of the service.
        /// </summary>
        public const string Name = "service.name";

        /// <summary>
        /// Tag representing the peer service name.
        /// </summary>
        public const string PeerName = "peer.service";
    }

    /// <summary>
    /// Tags and helper functions for messaging systems.
    /// </summary>
    public static class Messaging
    {
        /// <summary>
        /// Tag representing the messaging system in use.
        /// </summary>
        public const string System = "messaging.system";

        /// <summary>
        /// Contains operational tags for messaging.
        /// </summary>
        public static class Operation
        {
            /// <summary>
            /// The key for specifying the messaging operation.
            /// </summary>
            public const string Key = "messaging.operation";

            /// <summary>
            /// Operation for receiving messages.
            /// </summary>
            public const string Receive = "receive";

            /// <summary>
            /// Operation for sending messages.
            /// </summary>
            public const string Send = "send";

            /// <summary>
            /// Operation for processing messages.
            /// </summary>
            public const string Process = "process";
        }

        /// <summary>
        /// Tag specifying the destination channel for messaging.
        /// </summary>
        public const string Destination = "messaging.destination";

        /// <summary>
        /// Tag specifying the kind of destination (e.g., topic, queue).
        /// </summary>
        public const string DestinationKind = "messaging.destination_kind";

        /// <summary>
        /// Temporary destination tag.
        /// </summary>
        public const string TempDestination = "messaging.temp_destination";

        /// <summary>
        /// The messaging protocol tag.
        /// </summary>
        public const string Protocol = "messaging.protocol";

        /// <summary>
        /// The messaging protocol version tag.
        /// </summary>
        public const string ProtocolVersion = "messaging.protocol_version";

        /// <summary>
        /// The URL used for messaging.
        /// </summary>
        public const string Url = "messaging.url";

        /// <summary>
        /// Message id tag.
        /// </summary>
        public const string MessageId = "messaging.message_id";

        /// <summary>
        /// Conversation id tag.
        /// </summary>
        public const string ConversationId = "messaging.conversation_id";

        /// <summary>
        /// Tag for the size in bytes of the raw message payload.
        /// </summary>
        public const string MessagePayloadSizeBytes = "messaging.message_payload_size_bytes";

        /// <summary>
        /// Tag for the compressed size in bytes of the message payload.
        /// </summary>
        public const string MessagePayloadCompressedSizeBytes = "messaging.message_payload_compressed_size_bytes";

        /// <summary>
        /// Network peer name tag.
        /// </summary>
        public const string NetPeerName = "net.peer.name";

        /// <summary>
        /// Network socket family tag.
        /// </summary>
        public const string NetSocketFamily = "net.sock.family";

        /// <summary>
        /// Socket peer address tag.
        /// </summary>
        public const string NetSocketPeerAddress = "net.sock.peer.addr";

        /// <summary>
        /// Socket peer name tag.
        /// </summary>
        public const string NetSocketPeerName = "net.sock.peer.name";

        /// <summary>
        /// Socket peer port tag.
        /// </summary>
        public const string NetSocketPeerPort = "net.sock.peer.port";

        /// <summary>
        /// Tags specific to messaging consumers.
        /// </summary>
        public static class Consumers
        {
            /// <summary>
            /// Tag representing the consumer identifier.
            /// </summary>
            public const string ConsumerId = "consumer_id";
        }

        /// <summary>
        /// Contains Kafka-specific messaging tags and helper methods.
        /// </summary>
        public static class Kafka
        {
            /// <summary>
            /// The constant value representing the Kafka messaging system.
            /// </summary>
            public const string SystemValue = "kafka";

            /// <summary>
            /// Tag for Kafka topics.
            /// </summary>
            public const string DestinationTopic = "topic";

            /// <summary>
            /// Tag for the Kafka message key.
            /// </summary>
            public const string MessageKey = "messaging.kafka.message_key";

            /// <summary>
            /// Tag for the Kafka consumer group.
            /// </summary>
            public const string ConsumerGroup = "messaging.kafka.consumer_group";

            /// <summary>
            /// Tag for the Kafka client identifier.
            /// </summary>
            public const string ClientId = "messaging.kafka.client_id";

            /// <summary>
            /// Tag for identifying the Kafka partition.
            /// </summary>
            public const string Partition = "messaging.kafka.partition";

            /// <summary>
            /// Tag indicating a Kafka tombstone message.
            /// </summary>
            public const string Tombstone = "messaging.kafka.tombstone";

            /// <summary>
            /// Constructs a dictionary of common producer telemetry tags for Kafka.
            /// </summary>
            /// <param name="serviceName">The name of the service producing the message.</param>
            /// <param name="topicName">The Kafka topic name.</param>
            /// <param name="messageKey">The message key used for partitioning or identification.</param>
            /// <returns>A dictionary with producer tag mappings.</returns>
            public static Dictionary<string, object?> ProducerTags(
                string serviceName,
                string topicName,
                string messageKey
            ) =>
                new()
                {
                    { System, SystemValue },
                    { DestinationKind, DestinationTopic },
                    { Destination, topicName },
                    { Operation.Key, Operation.Send },
                    { Service.Name, serviceName },
                    { MessageKey, messageKey }
                };

            /// <summary>
            /// Constructs a dictionary of common consumer telemetry tags for Kafka.
            /// </summary>
            /// <param name="serviceName">The name of the service consuming the message.</param>
            /// <param name="topicName">The Kafka topic name.</param>
            /// <param name="messageKey">The message key used for identification.</param>
            /// <param name="partitionName">The partition name or index for the message.</param>
            /// <param name="consumerGroup">The consumer group to which this consumer belongs.</param>
            /// <returns>A dictionary with consumer tag mappings.</returns>
            public static Dictionary<string, object?> ConsumerTags(
                string serviceName,
                string topicName,
                string messageKey,
                string partitionName,
                string consumerGroup
            ) =>
                new()
                {
                    { System, SystemValue },
                    { DestinationKind, DestinationTopic },
                    { Destination, topicName },
                    { Operation.Key, Operation.Receive },
                    { Service.Name, serviceName },
                    { MessageKey, messageKey },
                    { Partition, partitionName },
                    { ConsumerGroup, consumerGroup }
                };
        }
    }
}
