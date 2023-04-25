using System.Text.Json;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;

public class PushNotificationService
{
    private readonly PushServiceClient _pushClient;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly IPushSubscriptionRepository _subscriptionRepository;

    public PushNotificationService(IConfiguration configuration, IPushSubscriptionRepository subscriptionRepository, ILogger<PushNotificationService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _pushClient = new PushServiceClient();
        _logger = logger;

        // Configurar chaves VAPID
        var publicKey = configuration["VapidKeys:PublicKey"];
        var privateKey = configuration["VapidKeys:PrivateKey"];
        _pushClient.DefaultAuthentication = new VapidAuthentication(publicKey, privateKey)
        {
            Subject = "mailto:julio.kawai@gmail.com" // Preencha com um e-mail válido
        };
    }

    public void AddSubscription(PushSubscriptionModel subscription)
    {
        var item = _subscriptionRepository.GetSubscriptionAsync(subscription.Endpoint);
        if(item != null) _subscriptionRepository.RemoveSubscriptionAsync(subscription.Id, subscription.Endpoint);
        _subscriptionRepository.AddSubscriptionAsync(subscription);
    }

    public async Task SendNotificationAsync(NotificationPayloadModel payload)
    {
        var subscriptions = await _subscriptionRepository.GetSubscriptionsBySubjectAsync(payload.Subject);
        foreach (var subscription in subscriptions)
        {
            if (!string.Equals(subscription.Subject, payload.Subject,
                StringComparison.OrdinalIgnoreCase)) continue;

            var pushSubscription = new PushSubscription
            {
                Endpoint = subscription.Endpoint
            };
            pushSubscription.SetKey(PushEncryptionKeyName.P256DH, subscription.P256dh);
            pushSubscription.SetKey(PushEncryptionKeyName.Auth, subscription.Auth);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var pushMessage = new PushMessage(JsonSerializer.Serialize(payload, options))
            {
                Urgency = PushMessageUrgency.Normal
            };

            try
            {
                await _pushClient.RequestPushMessageDeliveryAsync(pushSubscription, pushMessage);
            }
            catch (Exception ex)
            {
                // Tratar exceções conforme necessário
                _logger.LogError(ex, "Erro ao enviar notificação");
            }
        }
    }

    public async Task<IEnumerable<PushSubscriptionModel>> GetSubscriptionsAsync()
    {
        return await _subscriptionRepository.GetSubscriptionsAsync();
    }
}
