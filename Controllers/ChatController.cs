using Microsoft.AspNetCore.Mvc;
using NexusTelecommunicationManagementSystem.Services;

namespace NexusTelecommunicationManagementSystem.Controllers;
public class ChatController:Controller
{
    private readonly AiChatService _ai;
    public ChatController(AiChatService ai)=>_ai=ai;
    [HttpGet] public IActionResult Index()=>View();
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Ask(string message)
    {
        if(string.IsNullOrWhiteSpace(message)) return Json(new{ok=false,error="Please enter a message."});
        if(message.Length>2000) message=message[..2000];
        try{return Json(new{ok=true,response=await _ai.AskAsync(message)});}
        catch{return Json(new{ok=false,error="The AI service is temporarily unavailable. Please use Support."});}
    }
}