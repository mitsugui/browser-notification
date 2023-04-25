public interface IPushSubscriptionRepository
{
    Task AddSubscriptionAsync(PushSubscriptionModel subscription);

    Task<PushSubscriptionModel?> GetSubscriptionAsync(string endpoint);

    Task<IEnumerable<PushSubscriptionModel>> GetSubscriptionsBySubjectAsync(string subject);

    Task<IEnumerable<PushSubscriptionModel>> GetSubscriptionsAsync();

    Task RemoveSubscriptionAsync(string id, string endpoint);
}