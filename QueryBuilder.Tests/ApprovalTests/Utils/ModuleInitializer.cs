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
            VerifierSettings.AddScrubber(x => x
                .Replace("SELECT", "\nSELECT")
                .Replace("INNER", "\nINNER")
                .Replace("FROM", "\nFROM")
                .Replace("WHERE", "\nWHERE")
                .Replace("ORDER BY ", "\nORDER BY ")
                .Replace("AND", "\nAND")
                .Replace("OR ", "\nOR ")
                .Replace("ROWS ", "\nROWS ")
                .Replace("UNION ", "\nUNION ")
                .Replace("VALUES ", "\nVALUES ")
                .Replace("), (", "), \n(")
                .Replace("AS tbl", "\nAS tbl")
            );
        }
    }
}
