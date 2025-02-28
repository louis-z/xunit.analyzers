﻿using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Xunit.Analyzers.CodeActions;
using Xunit.Analyzers.FixProviders;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public sealed class MemberDataShouldReferenceValidMember_ReturnTypeFixer : MemberFixBase
	{
		const string title = "Change Member Return Type";

		public MemberDataShouldReferenceValidMember_ReturnTypeFixer()
			: base(new[] { Descriptors.X1019_MemberDataMustReferenceMemberOfValidType.Id })
		{ }

		public override async Task RegisterCodeFixesAsync(
			CodeFixContext context,
			ISymbol member)
		{
			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			var type = TypeSymbolFactory.IEnumerableOfObjectArray(semanticModel.Compilation);

			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedSolution: ct => context.Document.Project.Solution.ChangeMemberType(member, type, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}
	}
}
