var builder = DistributedApplication.CreateBuilder(args);

var cms = builder.AddProject<Projects.CMS>("CMS");
var llmApi = builder.AddProject<Projects.LLMAPI>("LLMAPI");
var whisperApi = builder.AddProject<Projects.WhisperAPI>("WhisperAPI");
var mauiapp = builder.AddMauiProject("mauiapp", @"../App/App.csproj");

mauiapp.AddWindowsDevice()
    .WithReference(llmApi)
    .WithReference(cms)
    .WithReference(whisperApi);

// Add Mac Catalyst device (uses localhost directly)
mauiapp.AddMacCatalystDevice()
    .WithReference(llmApi)
    .WithReference(cms)
    .WithReference(whisperApi);

builder.Build().Run();
