using System.Text.Json;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;

public class PushNotificationService
{
    private readonly Dictionary<string, PushSubscriptionModel> _subscriptions;
    private readonly PushServiceClient _pushClient;
    private readonly ILogger<PushNotificationService> _logger;

    public IReadOnlyCollection<PushSubscriptionModel> Subscriptions => _subscriptions.Values;

    public PushNotificationService(IConfiguration configuration, ILogger<PushNotificationService> logger)
    {
        _subscriptions = new Dictionary<string, PushSubscriptionModel>();
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
        if (_subscriptions.ContainsKey(subscription.Endpoint))
            _subscriptions.Remove(subscription.Endpoint);

        _subscriptions.Add(subscription.Endpoint, subscription);
    }

    public async Task SendNotificationAsync(NotificationPayloadModel payload)
    {
        foreach (var subscription in _subscriptions.Values)
        {
            if (string.Equals(subscription.Subject, payload.Subject,
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
}
