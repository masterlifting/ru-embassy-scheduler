﻿using System.Text;
using System.Text.Json;

using KdmidScheduler.Abstractions.Interfaces;

using Microsoft.Extensions.Configuration;

namespace KdmidScheduler.Infrastructure.Services;

public sealed class KdmidCaptchaService : IKdmidCaptcha
{
    private readonly string _apiKey;
    private readonly IHttpClientFactory _httpClientFactory;

    public KdmidCaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;

        var apiKey = configuration["AntiCaptchaKey"];
        ArgumentNullException.ThrowIfNull(apiKey, "AntiCaptchaKey is not set");
        _apiKey = apiKey;
    }

    public async Task<string> SolveIntegerCaptcha(byte[] image, CancellationToken cToken)
    {
        var captcha = Convert.ToBase64String(image);
        var content = new StringContent($"{{\"clientKey\": \"{_apiKey}\", \"task\": {{\"type\": \"ImageToTextTask\", \"body\": \"{captcha}\", \"phrase\": false, \"case\": false, \"numeric\": true, \"math\": 0, \"minLength\": 1, \"maxLength\": 1}}}}", Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient(Constants.AntiCaptcha);

        var response = await httpClient.PostAsync(httpClient.BaseAddress + "createTask", content, cToken);
        var responseContent = await response.Content.ReadAsStringAsync(cToken);

        var taskId = JsonSerializer.Deserialize<Dictionary<string, int>>(responseContent)!["taskId"];

        var status = "processing";

        content = new StringContent($"{{\"clientKey\": \"{_apiKey}\", \"taskId\": \"{taskId}\"}}", Encoding.UTF8, "application/json");

        while (status == "processing")
        {
            await Task.Delay(500, cToken);

            response = await httpClient.PostAsync(httpClient.BaseAddress + "getTaskResult", content, cToken);
            responseContent = await response.Content.ReadAsStringAsync(cToken);

            var taskResult = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

            status = taskResult!["status"].ToString();

            if (status == "ready")
            {
                var resultContent = taskResult!["solution"]?.ToString();

                if (string.IsNullOrWhiteSpace(resultContent))
                {
                    throw new InvalidOperationException("Captcha solving failed.");
                }

                var resultObject = 
                    JsonSerializer.Deserialize<Dictionary<string, object?>>(resultContent) 
                    ?? throw new InvalidOperationException("Captcha solving failed.");
                
                var result = resultObject["text"]?.ToString();

                return string.IsNullOrWhiteSpace(result)
                    ? throw new InvalidOperationException(result)
                    : result;
            }
        }

        throw new InvalidOperationException("Captcha solving failed.");
    }
}