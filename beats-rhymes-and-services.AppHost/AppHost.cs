var builder = DistributedApplication.CreateBuilder(args);

var cms = builder.AddProject<Projects.CMS>("CMS");
var llmApi = builder.AddProject<Projects.LLMAPI>("LLMAPI");
var whisperApi = builder.AddProject<Projects.WhisperAPI>("WhisperAPI");
var mauiapp = builder.AddMauiProject("mauiapp", @"../App/App.csproj");

// Create a public dev tunnel for iOS and Android
// var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
//     .WithAnonymousAccess()
//     .WithReference(whisperApi);

// mauiapp.AddWindowsDevice()
//     .WithReference(llmApi)
//     .WithReference(cms)
//     .WithReference(whisperApi);

//Add Mac Catalyst device (uses localhost directly)
mauiapp.AddMacCatalystDevice()
    .WithReference(llmApi)
    .WithReference(cms)
    .WithReference(whisperApi);

// mauiapp.AddiOSSimulator()
//     .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
//     .WithReference(whisperApi, publicDevTunnel);

builder.Build().Run();
