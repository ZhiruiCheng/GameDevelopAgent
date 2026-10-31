using Anthropic;
using Anthropic.Models.Messages;

// 1. 自己读环境变量，显式交给 SDK。没设就立刻报你自己写的错，不用等 401
AnthropicClient client = new()
{
    ApiKey  = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
              ?? throw new Exception("没设 DEEPSEEK_API_KEY"),
    BaseUrl = "https://api.deepseek.com/anthropic",
};

// 2. 请求参数，就是笔记第 2 题那几个字段。没 System、没 Tools，所以是"最小"
var resp = await client.Messages.Create(new MessageCreateParams
{
    Model = "deepseek-flash",      // 用哪个模型，字符串
    MaxTokens = 1024,              // 输出上限
    Messages = [ new UserMessageParam("用一句话说帧同步传什么") ],   // 一条 role=user 的消息
});

// 3. 返回的 content 是块数组，不是一个字符串，因为以后会混着 tool_use 和 thinking 块
foreach (var block in resp.Content)
{
    if (block.TryPickThinking(out var th)) Console.WriteLine($"[thinking] {th.Thinking[..Math.Min(80, th.Thinking.Length)]}...");
    else if (block.TryPickText(out var text)) Console.WriteLine($"[text] {text.Text}");
    else Console.WriteLine($"[other] {block}");
}

Console.WriteLine($"stop_reason={resp.StopReason} in={resp.Usage.InputTokens} out={resp.Usage.OutputTokens}");
