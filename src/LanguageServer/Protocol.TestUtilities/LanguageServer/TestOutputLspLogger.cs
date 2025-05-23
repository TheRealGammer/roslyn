﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CodeAnalysis.LanguageServer;
using Microsoft.CommonLanguageServerProtocol.Framework;
using Xunit.Abstractions;

namespace Roslyn.Test.Utilities;

internal sealed class TestOutputLspLogger : AbstractLspLogger, ILspService
{
    private readonly ITestOutputHelper _testOutputHelper;
    public TestOutputLspLogger(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public override IDisposable? CreateContext(string context) => null;

    public override void LogDebug(string message, params object[] @params) => Log("Debug", message, @params);

    public override void LogError(string message, params object[] @params) => Log("Error", message, @params);

    public override void LogException(Exception exception, string? message = null, params object[] @params)
        => Log("Warning", $"{message}{Environment.NewLine}{exception}", @params);

    public override void LogInformation(string message, params object[] @params) => Log("Info", message, @params);

    public override void LogWarning(string message, params object[] @params) => Log("Warning", message, @params);

    private void Log(string level, string message, params object[] @params)
        => _testOutputHelper.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}][{level}]{message}", @params);
}
