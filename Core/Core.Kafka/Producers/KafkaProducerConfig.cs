using Confluent.Kafka;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Core.Kafka.Producers;

/// <summary>
/// Represents the configuration settings for a Kafka producer.
/// </summary>
public class KafkaProducerConfig
{
    /// <summary>
    /// Gets or sets the <see cref="ProducerConfig"/> that configures the Kafka producer.
    /// </summary>
    public ProducerConfig ProducerConfig { get; set; } = default!;

    /// <summary>
    /// Gets or sets the Kafka topic to which messages should be published.
    /// </summary>
    public string Topic { get; set; } = default!;

    /// <summary>
    /// Gets or sets an optional producer timeout in milliseconds.
    /// </summary>
    public int? ProducerTimeoutInMs { get; set; }
}

/// <summary>
/// Provides extension methods to bind Kafka producer configuration from an <see cref="IConfiguration"/>.
/// </summary>
public static class KafkaProducerConfigExtensions
{
    private const string DefaultConfigKey = "KafkaProducer";

    /// <summary>
    /// Binds and returns a <see cref="KafkaProducerConfig"/> from the configuration source.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>A populated <see cref="KafkaProducerConfig"/> instance.</returns>
    public static KafkaProducerConfig GetKafkaProducerConfig(this IConfiguration configuration)
    {
        // Bind configuration from "kafka:ProducerConfig" section.
        var config = configuration.GetSection("kafka").GetSection("ProducerConfig").Get<KafkaProducerConfig>();

        // Retrieve connection string if available.
        var connectionString = configuration.GetConnectionString("kafka");
        var kafkaConfig = configuration.GetRequiredConfig<KafkaProducerConfig>(DefaultConfigKey);

        if (connectionString == null)
            return kafkaConfig;

        // Set bootstrap servers and security settings.
        kafkaConfig.ProducerConfig.BootstrapServers = connectionString;
        kafkaConfig.ProducerConfig.SecurityProtocol = SecurityProtocol.Plaintext;
        kafkaConfig.ProducerConfig.SaslMechanism = SaslMechanism.Plain;

        return kafkaConfig;
    }
}
