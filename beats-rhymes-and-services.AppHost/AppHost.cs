var builder = DistributedApplication.CreateBuilder(args);

var cms = builder.AddProject<Projects.CMS>("CMS");

builder.Build().Run();
