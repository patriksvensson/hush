var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

////////////////////////////////////////////////////////////////
// Tasks

Task("Build")
    .Does(context => 
{
    DotNetCoreBuild("./src/Hush.sln", new DotNetCoreBuildSettings {
        Configuration = configuration,
        NoIncremental = context.HasArgument("rebuild"),
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Package")
    .IsDependentOn("Build")
    .Does(context => 
{
    context.CleanDirectory("./.artifacts");

    context.DotNetCorePublish($"./src/Hush.sln", new DotNetCorePublishSettings {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = "./.artifacts",
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("PublishSingleFile", "true")
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Default")
    .IsDependentOn("Package");

////////////////////////////////////////////////////////////////
// Execution

RunTarget(target)