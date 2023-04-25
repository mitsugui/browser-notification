using Newtonsoft.Json;

public record PushSubscriptionModel(
    [property:JsonProperty(PropertyName = "id")] string Id, 
    [property:JsonProperty(PropertyName = "endpoint")] string Endpoint, 
    [property:JsonProperty(PropertyName = "p256dh")] string P256dh, 
    [property:JsonProperty(PropertyName = "auth")] string Auth, 
    [property:JsonProperty(PropertyName = "subject")] string Subject);