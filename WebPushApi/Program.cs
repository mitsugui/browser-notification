var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
builder.Services.AddScoped<PushNotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Lista inscritos
app.MapGet("/subscriptions", async (PushNotificationService servico) => 
    await servico.GetSubscriptionsAsync());

//Adiciona inscrito
app.MapPost("/subscribe", (PushSubscriptionModel model, PushNotificationService servico) => {
    servico.AddSubscription(model);
    return Results.Ok();
});

//Envia notificação
app.MapPost("/send", async (NotificationPayloadModel payload, PushNotificationService servico) => {
    var subscriptions = await servico.SendNotificationAsync(payload);
    return Results.Ok(subscriptions);
});

app.Run();
