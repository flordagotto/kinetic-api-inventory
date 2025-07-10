namespace DTOs.RabbitDtos
{
    public class EventMessage
    {
        public Guid Id { get; set; }
        public ProductEventType EventType { get; set; }
        public string Description { get; set; } = default!;
        public DateTime EventDate { get; set; }
    }

    public enum ProductEventType
    {
        Created,
        Updated,
        Deleted
    }
}
