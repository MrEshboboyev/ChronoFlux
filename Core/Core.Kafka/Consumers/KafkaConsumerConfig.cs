using Confluent.Kafka;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Core.Kafka.Consumers;

/// <summary>
/// Represents the configuration settings for Kafka consumers.
/// </summary>
public class KafkaConsumerConfig
{
    /// <summary>
    /// Gets or sets the <see cref="ConsumerConfig"/> for the Kafka consumer.
    /// </summary>
    public ConsumerConfig ConsumerConfig { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of topics to subscribe to.
    /// </summary>
    public string[] Topics { get; set; } = default!;

    /// <summary>
    /// Gets or sets a flag indicating whether deserialization errors should be ignored.
    /// </summary>
    public bool IgnoreDeserializationErrors { get; set; } = true;
}

/// <summary>
/// Provides extension methods to build <see cref="KafkaConsumerConfig"/> from configuration.
/// </summary>
public static class KafkaConsumerConfigExtensions
{
    private const string DefaultConfigKey = "KafkaConsumer";

    /// <summary>
    /// Binds and returns a <see cref="KafkaConsumerConfig"/> from the application configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The bound <see cref="KafkaConsumerConfig"/> instance.</returns>
    public static KafkaConsumerConfig GetKafkaConsumerConfig(this IConfiguration configuration)
    {
        // Manually bind to configuration from "kafka:Config" section.
        var config = configuration.GetSection("kafka").GetSection("Config").Get<KafkaConsumerConfig>();

        // Retrieve connection strings and required configuration.
        var connectionString = configuration.GetConnectionString("kafka");
        var kafkaProducerConfig = configuration.GetRequiredConfig<KafkaConsumerConfig>(DefaultConfigKey);

        if (connectionString == null)
            return kafkaProducerConfig;

        kafkaProducerConfig.ConsumerConfig.BootstrapServers = connectionString;
        kafkaProducerConfig.ConsumerConfig.AllowAutoCreateTopics = true; // TODO: Adjust as needed.
        kafkaProducerConfig.ConsumerConfig.SecurityProtocol = SecurityProtocol.Plaintext;
        kafkaProducerConfig.ConsumerConfig.SaslMechanism = SaslMechanism.Plain;

        return kafkaProducerConfig;
    }
}
