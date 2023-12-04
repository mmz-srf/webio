namespace WebIO.Application;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Model.Display;

public class ScriptHelper
{
    private readonly ILogger<ScriptHelper> _log;

    public ScriptHelper(ILogger<ScriptHelper> log)
    {
        _log = log;
    }

    public void InitializeColumnScripts(IEnumerable<ColumnDefinition> columns)
    {
        foreach (var columnDefinition in columns)
        {
            if (columnDefinition.Script == null ||
                columnDefinition.Script.Count == 0)
            {
                continue;
            }

            var scriptCode = string.Join(Environment.NewLine, columnDefinition.Script);
            if (string.IsNullOrWhiteSpace(scriptCode))
            {
                continue;
            }

            _log.LogInformation("Compiling script for column {Column}", columnDefinition.DisplayName);

            var script = CSharpScript.Create<string>(scriptCode,
                globalsType: typeof(ValueGetter),
                options: ScriptOptions.Default
                    .WithImports("System")
                    .WithImports("System.Collections")
                    .WithImports("System.Collections.Generic"));
            var diagnostics = script.Compile();

            foreach (var message in diagnostics)
            {
                var logLevel = GetLogLevel(message);
                _log.Log(logLevel, message.GetMessage());
            }

            if (diagnostics.Any(m => m.Severity == DiagnosticSeverity.Error))
            {
                _log.LogError("Errors in script for column {Column}", columnDefinition.DisplayName);
            }
            else
            {
                var runner = script.CreateDelegate();
                columnDefinition.ScriptDelegate = valueGetter => runner(valueGetter).Result;
            }
        }
    }

    private static LogLevel GetLogLevel(Diagnostic message)
        => message.Severity switch
        {
            DiagnosticSeverity.Info => LogLevel.Information,
            DiagnosticSeverity.Warning => LogLevel.Warning,
            DiagnosticSeverity.Error => LogLevel.Error,
            _ => LogLevel.Debug,
        };
}