﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	public abstract class AssertUsageAnalyzerBase : DiagnosticAnalyzer
	{
		readonly HashSet<string> targetMethods;

		protected AssertUsageAnalyzerBase(
			DiagnosticDescriptor descriptor,
			IEnumerable<string> methods)
				: this(new[] { descriptor }, methods)
		{ }

		protected AssertUsageAnalyzerBase(
			IEnumerable<DiagnosticDescriptor> descriptors,
			IEnumerable<string> methods)
		{
			SupportedDiagnostics = ImmutableArray.CreateRange(descriptors);
			targetMethods = new HashSet<string>(methods, StringComparer.Ordinal);
		}

		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public sealed override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(context =>
			{
				var assertType = context.Compilation.GetTypeByMetadataName(Constants.Types.XunitAssert);
				if (assertType is null)
					return;

				context.RegisterOperationAction(context =>
				{
					if (context.Operation is IInvocationOperation invocationOperation)
					{
						var methodSymbol = invocationOperation.TargetMethod;
						if (methodSymbol.MethodKind != MethodKind.Ordinary || !Equals(methodSymbol.ContainingType, assertType) || !targetMethods.Contains(methodSymbol.Name))
							return;

						Analyze(context, invocationOperation, methodSymbol);
					}
				}, OperationKind.Invocation);
			});
		}

		protected abstract void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method);
	}
}
