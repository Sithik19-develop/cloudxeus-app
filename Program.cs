using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

// Read settings from environment variables (this is how the container gets config)
var endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"]!;
var apiKey = builder.Configuration["AZURE_OPENAI_KEY"]!;
var deployment = builder.Configuration["AZURE_OPENAI_DEPLOYMENT"] ?? "gpt-4o-mini";

var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
builder.Services.AddSingleton(client.GetChatClient(deployment));

var app = builder.Build();

app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head><title>AI Chat API</title></head>
    <body style="font-family:Segoe UI;margin:40px">
      <h1>AI Chat API - v2</h1>
      <p>Running in a container. Built by an ACR Task from GitHub.</p>
      <p>POST <code>/chat</code> with <code>{ "prompt": "..." }</code></p>
    </body>
    </html>
    """, "text/html"));

app.MapPost("/chat", async (ChatRequest req, ChatClient chat) =>
{
    ChatCompletion result = await chat.CompleteChatAsync(
        new SystemChatMessage("You are a helpful assistant."),
        new UserChatMessage(req.Prompt));

    return Results.Ok(new { reply = result.Content[0].Text });
});

app.Run();

record ChatRequest(string Prompt);