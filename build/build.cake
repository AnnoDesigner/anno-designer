#addin nuget:?package=Cake.FileHelpers&version=3.2.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "DEBUG");

//from cake source:
/// <summary>
/// MSBuild tool version: <c>Visual Studio 2017</c>
/// </summary>
//VS2017 = 6,
/// <summary>
/// MSBuild tool version: <c>Visual Studio 2019</c>
/// </summary>
//VS2019 = 7

var msbuildVersion = Argument<int>("msbuildVersion", 7);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var rootAbsoluteDir = MakeAbsolute(Directory("./")).FullPath;
var logDirectory = MakeAbsolute(Directory($"./logs/{configuration}")).FullPath;
var outDirectory = MakeAbsolute(Directory($"./out/{configuration}")).FullPath;

var solutionFiles = new List<string>
{
    "./../AnnoDesigner.sln",
    "./../ColorPresetsDesigner.sln",
    "./../FandomParser/FandomParser.sln",
    "./../FandomTemplateExporter/FandomTemplateExporter.sln"
};

var versionNumber = System.IO.File.ReadAllText("./../version.txt");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");
    Information("");
    Information($"{nameof(target)}: {target}");
    Information($"{nameof(configuration)}: {configuration}");
    Information("");
    Information($"{nameof(logDirectory)}: {logDirectory}");
    Information($"{nameof(outDirectory)}: {outDirectory}");
    Information("");
    Information($"version: {versionNumber}");

    EnsureDirectoryExists(logDirectory);
    CleanDirectory(logDirectory);

    EnsureDirectoryExists(outDirectory);
    CleanDirectory(outDirectory);
});

Teardown(ctx =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

var cleanTask = Task("Clean")
.Description("Cleans all directories that are used during the build process.")
.Does(() =>
{
    CleanDirectories($"**/bin/{configuration}");
    CleanDirectories($"**/obj/{configuration}");
    CleanDirectories($"**/bin/*/{configuration}");
    CleanDirectories($"**/obj/*/{configuration}");
});

var restoreNuGetTask = Task("Restore-NuGet-Packages")
.Description("Restores all the NuGet packages that are used by the specified solution.")
.IsDependentOn(cleanTask)
.Does(() =>
{
    foreach (var curSolutionFile in solutionFiles)
    {
        var curSolutionFileName = System.IO.Path.GetFileName(curSolutionFile);
        Information($"{DateTime.Now:hh:mm:ss.ff} restoring NuGet packages for {curSolutionFileName}");
        NuGetRestore(curSolutionFile);
    }
});

var updateAssemblyInfoTask = Task("Update-Assembly-Info")
.IsDependentOn(restoreNuGetTask)
.Does(() =>
{
    ReplaceRegexInFiles("./../AnnoDesigner/Properties/AssemblyInfo.cs",
                        "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))",
                        $"{versionNumber}.0.0");
    ReplaceRegexInFiles("./../AnnoDesigner/Properties/AssemblyInfo.cs",
                        "(?<=AssemblyFileVersion\\(\")(.+?)(?=\"\\))",
                        $"{versionNumber}.0.0");

    ReplaceRegexInFiles("./../PresetParser/Properties/AssemblyInfo.cs",
                        "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))",
                        $"{versionNumber}.0.0");
    ReplaceRegexInFiles("./../PresetParser/Properties/AssemblyInfo.cs",
                        "(?<=AssemblyFileVersion\\(\")(.+?)(?=\"\\))",
                        $"{versionNumber}.0.0");
});

var buildTask = Task("Build")
.Description("Builds all the different parts of the project.")
//.IsDependentOn(cleanTask)
.IsDependentOn(updateAssemblyInfoTask)
.Does(() =>
{
    foreach (var curSolutionFile in solutionFiles)
    {
        var curSolutionFileName = System.IO.Path.GetFileName(curSolutionFile);
        var msBuildSettings = new MSBuildSettings()
        {
            Configuration = configuration,
            PlatformTarget = PlatformTarget.MSIL,
            ToolVersion = (Cake.Common.Tools.MSBuild.MSBuildToolVersion)msbuildVersion,
            MaxCpuCount = 0,//use all available
            NoConsoleLogger = true,
            BinaryLogger = new MSBuildBinaryLogSettings
            {
                Enabled = true,
                FileName = $"{logDirectory}/{curSolutionFileName}.binlog"
            },
            DetailedSummary = true,
            Verbosity = Verbosity.Minimal,
            NoLogo = true
        };

        //msBuildSettings.FileLoggers.Add(new MSBuildFileLogger
        //{
        //    LogFile = $"{logDirectory}/{curSolutionFileName}.log"
        //});

        msBuildSettings = msBuildSettings.WithTarget("Clean");
        msBuildSettings = msBuildSettings.WithTarget("Restore");
        msBuildSettings = msBuildSettings.WithTarget("Build");

        Information($"{DateTime.Now:hh:mm:ss.ff} compiling {curSolutionFileName}");

        MSBuild(curSolutionFile, msBuildSettings);
        Information("");
    }
});

var copyFilesTask = Task("Copy-Files")
.IsDependentOn(buildTask)
.Does(() =>
{
    var outputDirectoryIcons=$"{outDirectory}/icons";
    EnsureDirectoryExists(outputDirectoryIcons);
    CleanDirectory(outputDirectoryIcons);

    Information($"{DateTime.Now:hh:mm:ss.ff} copy icons to \"{outputDirectoryIcons}\"");
    CopyDirectory($"./../AnnoDesigner/bin/{configuration}/icons", $"{outputDirectoryIcons}");

    Information($"{DateTime.Now:hh:mm:ss.ff} copy application to \"{outDirectory}\"");
    CopyFileToDirectory($"./../AnnoDesigner/bin/{configuration}/AnnoDesigner.exe", $"{outDirectory}");
    CopyFileToDirectory($"./../AnnoDesigner/bin/{configuration}/AnnoDesigner.Core.dll", $"{outDirectory}");
    CopyFileToDirectory($"./../AnnoDesigner/bin/{configuration}/colors.json", $"{outDirectory}");
    CopyFileToDirectory($"./../AnnoDesigner/bin/{configuration}/icons.json", $"{outDirectory}");
    CopyFileToDirectory($"./../AnnoDesigner/bin/{configuration}/presets.json", $"{outDirectory}");
    CopyFileToDirectory($"./../AnnoDesigner/bin/{configuration}/Octokit.dll", $"{outDirectory}");
    CopyFileToDirectory($"./../AnnoDesigner/bin/{configuration}/Xceed.Wpf.Toolkit.dll", $"{outDirectory}");

    Information("");
});

var zipTask = Task("Compress-Output")
.IsDependentOn(copyFilesTask)
.Does(() =>
{
    if(configuration.Equals("DEBUG", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var outputFilePath = $"{outDirectory}/../Anno.Designer.v{versionNumber}.zip";

    Information($"{DateTime.Now:hh:mm:ss.ff} creating zip file: \"{outputFilePath}\"");
    Zip($"{outDirectory}", outputFilePath);
    //use 7zip: https://github.com/cake-build/cake/issues/1283#issuecomment-254921290

    Information("");
});

Task("Default")
.Description("This is the default task which will be ran if no specific target is passed in.")
//.IsDependentOn(copyFilesTask)
//.IsDependentOn(buildTask)
.IsDependentOn(zipTask)
.Does(() => {});

RunTarget(target);