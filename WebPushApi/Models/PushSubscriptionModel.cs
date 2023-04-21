public record SubscriptionKeys(string P256dh, string Auth);

public record PushSubscriptionModel(string Endpoint, SubscriptionKeys Keys);