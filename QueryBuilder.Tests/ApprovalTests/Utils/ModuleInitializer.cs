using System.Runtime.CompilerServices;
using DiffEngine;

namespace SqlKata.Tests.ApprovalTests.Utils
{
    public static class ModuleInitializer
    {

        [ModuleInitializer]
        public static void Initialize() =>
            VerifyDiffPlex.Initialize();


        [ModuleInitializer]
        public static void OtherInitialize()
        {
            DiffTools.AddToolBasedOn(DiffTool.AraxisMerge, "araxis");
            VerifierSettings.InitializePlugins();
            VerifierSettings.ScrubLinesContaining("DiffEngineTray");
            VerifierSettings.IgnoreStackTrace();
            //            VerifierSettings.AddScrubber(_ => _.Replace("String to verify", "new value"));
        }
    }
}
