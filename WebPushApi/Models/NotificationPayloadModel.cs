using Newtonsoft.Json;

public record NotificationPayloadModel(
    [property:JsonProperty(PropertyName = "title")] string Title, 
    [property:JsonProperty(PropertyName = "body")] string Body, 
    [property:JsonProperty(PropertyName = "subject")] string Subject);