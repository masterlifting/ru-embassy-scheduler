﻿using System.Globalization;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using Telegram.ApAzureBot.Core.Abstractions.Services;

namespace TelegramBot.Azure;

public class Functions(ITelegramClient client)
{
    private readonly ITelegramClient _client = client;

    private const string SetReceiverFunction = "set";
    private const string ReceiveFunction = "receive";
    private const string ListenFunction = "listen";

    [Function(SetReceiverFunction)]
    public Task SetReceiver([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, CancellationToken cToken)
    {
        var url = request.Url.ToString().Replace(SetReceiverFunction, ReceiveFunction, true, CultureInfo.InvariantCulture);

        return _client.SetWebhook(url, cToken);
    }

    [Function(ReceiveFunction)]
    public async Task Receive([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request, CancellationToken cToken)
    {
        var data = await request.ReadAsStringAsync();

        ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

        await _client.ReceiveMessage(data, cToken);
    }

    [Function(ListenFunction)]
    public Task Listen([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData _, CancellationToken cToken) =>
        _client.ListenMessages(cToken);
}