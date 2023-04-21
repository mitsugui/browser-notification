using System.Text.Json;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;

public class PushNotificationService
{
    private readonly List<PushSubscriptionModel> _subscriptions;
    private readonly PushServiceClient _pushClient;
    private readonly ILogger<PushNotificationService> _logger;

    public IReadOnlyCollection<PushSubscriptionModel> Subscriptions => _subscriptions.AsReadOnly();

    public PushNotificationService(IConfiguration configuration, ILogger<PushNotificationService> logger)
    {
        _subscriptions = new List<PushSubscriptionModel>();
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
        _subscriptions.Add(subscription);
    }

    public async Task SendNotificationAsync(NotificationPayloadModel payload)
    {
        foreach (var subscription in _subscriptions)
        {
            var pushSubscription = new PushSubscription
            {
                Endpoint = subscription.Url
            };
            pushSubscription.SetKey(PushEncryptionKeyName.P256DH, subscription.P256dh);
            pushSubscription.SetKey(PushEncryptionKeyName.Auth, subscription.Auth);

            var pushMessage = new PushMessage(JsonSerializer.Serialize(payload))
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
