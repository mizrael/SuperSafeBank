namespace SuperSafeBank.Transport.Kafka
{
    public record EventsProducerConfig
    {
        public EventsProducerConfig(string kafkaConnectionString, string topicBaseName)
        {
            if (string.IsNullOrWhiteSpace(kafkaConnectionString))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(kafkaConnectionString));
            if (string.IsNullOrWhiteSpace(topicBaseName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicBaseName));

            KafkaConnectionString = kafkaConnectionString;
            TopicBaseName = topicBaseName;
        }

        public string KafkaConnectionString { get; }
        public string TopicBaseName { get; }
    }
}