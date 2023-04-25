using Microsoft.Azure.Cosmos;

public class PushSubscriptionRepository : IPushSubscriptionRepository
{
    
    private readonly Dictionary<string, PushSubscriptionModel> _subscriptions;

    public PushSubscriptionRepository()
    {
        _subscriptions = new Dictionary<string, PushSubscriptionModel>();
    }

    public async Task AddSubscriptionAsync(PushSubscriptionModel subscription)
    {
        using CosmosClient client = new(
            accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!,
            authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_KEY")!
        );

        var container = client.GetDatabase("BancoDados").GetContainer("Inscricoes");

        await container.CreateItemAsync<PushSubscriptionModel>(subscription, new PartitionKey(subscription.Endpoint));
    }

    public async Task<PushSubscriptionModel?> GetSubscriptionAsync(string endpoint)
    {
        using CosmosClient client = new(
            accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!,
            authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_KEY")!
        );

        var container = client.GetDatabase("BancoDados").GetContainer("Inscricoes");
        var query = new QueryDefinition("SELECT * FROM i WHERE i.endpoint = @endpoint")
            .WithParameter("@endpoint", endpoint);

        using var feed = container.GetItemQueryIterator<PushSubscriptionModel>(query);
        if (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            return response.FirstOrDefault();
        }
        return null;
    }

    public async Task<IEnumerable<PushSubscriptionModel>> GetSubscriptionsBySubjectAsync(string subject)
    {
        using CosmosClient client = new(
            accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!,
            authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_KEY")!
        );

        var container = client.GetDatabase("BancoDados").GetContainer("Inscricoes");
        var query = new QueryDefinition("SELECT * FROM i WHERE i.subject like '@subject%'")
            .WithParameter("@subject", subject);

        using var feed = container.GetItemQueryIterator<PushSubscriptionModel>(query);

        var itens = new List<PushSubscriptionModel>();
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();

            itens.AddRange(response);
        }
        return itens;
    }

    public async Task<IEnumerable<PushSubscriptionModel>> GetSubscriptionsAsync()
    {
        using CosmosClient client = new(
            accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!,
            authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_KEY")!
        );

        var container = client.GetDatabase("BancoDados").GetContainer("Inscricoes");
        var query = new QueryDefinition("SELECT * FROM i");

        using var feed = container.GetItemQueryIterator<PushSubscriptionModel>(query);

        var itens = new List<PushSubscriptionModel>();
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();

            itens.AddRange(response);
        }
        return itens;
    }

    public async Task RemoveSubscriptionAsync(string id, string endpoint)
    {
        using CosmosClient client = new(
            accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!,
            authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_KEY")!
        );

        var container = client.GetDatabase("BancoDados").GetContainer("Inscricoes");

        await container.DeleteItemAsync<PushSubscriptionModel>(id, new PartitionKey(endpoint));
    }
}
