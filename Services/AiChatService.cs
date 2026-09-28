using OpenAI.Responses;

namespace NexusTelecommunicationManagementSystem.Services;

public sealed class AiChatService
{
    private readonly IConfiguration _config;

    public AiChatService(IConfiguration config) => _config = config;

    public async Task<string> AskAsync(string message)
    {
        message = (message ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(message))
            return "Please enter a question and I’ll help you.";

        var key = _config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        // If an OpenAI key is configured, use the real AI assistant.
        // If it is not configured, use the built-in NEXUS support assistant instead of
        // showing a configuration error to customers. This keeps the customer chatbot
        // usable while allowing full AI responses once an API key is supplied.
        if (!string.IsNullOrWhiteSpace(key))
        {
            try
            {
                var model = _config["OpenAI:Model"] ?? "gpt-5.2";
                var client = new ResponsesClient(key);
                var instructions = """
                    You are the NEXUS Telecommunication Management System customer support assistant.
                    Help customers with plans, connection applications, order tracking, bills,
                    troubleshooting basics and support navigation. Do not invent account-specific
                    information. If a request requires account access, payment action, technician
                    dispatch, or a definitive network diagnosis, direct the customer to the appropriate
                    NEXUS support workflow. Be concise, friendly and practical.
                    """;

                var options = new CreateResponseOptions { Model = model };
                options.InputItems.Add(ResponseItem.CreateUserMessageItem(
                    instructions + "\nCustomer message: " + message));

                var response = await client.CreateResponseAsync(options);
                return response.Value.GetOutputText()
                    ?? "I could not generate a response right now. Please try again or contact NEXUS support.";
            }
            catch
            {
                // Do not expose API/provider errors to customers. Fall back to local support.
            }
        }

        return LocalSupportReply(message);
    }

    private static string LocalSupportReply(string message)
    {
        var m = message.ToLowerInvariant();

        if (ContainsAny(m, "hello", "hi", "hey", "salam", "assalam"))
            return "Hello! I’m the NEXUS customer support assistant. I can help with plans, new connections, order tracking, bills and basic troubleshooting. What do you need help with?";

        if (ContainsAny(m, "plan", "package", "internet package", "price", "pricing"))
            return "I can help you choose a NEXUS plan. Please open Plans to see the currently available packages and prices. If you tell me your preferred speed and budget, I can also explain what to look for.";

        if (ContainsAny(m, "connection", "new connection", "install", "installation"))
            return "For a new connection, open New Connection from the customer portal and submit your required details. NEXUS staff can then review the request and arrange the next steps.";

        if (ContainsAny(m, "track", "tracking", "order", "application status", "status"))
            return "You can check your application or order status from the Track section of the customer portal. If the status needs staff action, please contact NEXUS Support.";

        if (ContainsAny(m, "bill", "billing", "payment", "invoice", "due"))
            return "For billing questions, open Bills in your customer portal. You can review your available bills and payment status there. For payment corrections or account-specific issues, please contact NEXUS Support.";

        if (ContainsAny(m, "slow", "internet", "network", "wifi", "disconnect", "down", "not working", "troubleshoot"))
            return "For basic troubleshooting, restart your router/ONT, check its power and network indicators, and reconnect your device. If the service is still unavailable or repeatedly disconnects, use Support to report the issue so a technician can investigate.";

        if (ContainsAny(m, "support", "technician", "complaint", "ticket", "help"))
            return "Please use the Support page to submit a support request. Include your connection issue, when it started, and any router/ONT indicator information you can see. A NEXUS staff member can then handle the request.";

        if (ContainsAny(m, "profile", "account", "password", "login"))
            return "You can manage your customer profile from the Profile section. For password or login problems that cannot be resolved there, please use the Support page.";

        return "I can help with NEXUS plans, new connections, tracking, bills, basic internet troubleshooting and support. Please tell me what you need help with, for example: ‘How do I track my connection?’";
    }

    private static bool ContainsAny(string text, params string[] terms) =>
        terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
}
