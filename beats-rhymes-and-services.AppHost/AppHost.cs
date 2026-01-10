var builder = DistributedApplication.CreateBuilder(args);

var cms = builder.AddProject<Projects.CMS>("CMS");
var llmApi = builder.AddProject<Projects.LLMAPI>("LLMAPI");
var whisperApi = builder.AddProject<Projects.WhisperAPI>("WhisperAPI");

builder.Build().Run();
