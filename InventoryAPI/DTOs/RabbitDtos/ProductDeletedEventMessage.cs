namespace DTOs.RabbitDtos
{
    public class ProductDeletedEventMessage : EventMessage
    {

        public ProductDeletedEventMessage() {
            EventType = ProductEventType.Deleted;
        }
    }
}
