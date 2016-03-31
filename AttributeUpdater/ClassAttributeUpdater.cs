using System.Linq;
using DebtAnalyzer;
using DebtAnalyzer.DebtAnnotation;
using DebtAnalyzer.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace AttributeUpdater
{
	class ClassAttributeUpdater : CSharpSyntaxRewriter
	{
		readonly int maxMethodLength;
		readonly int maxParameters;
		readonly Workspace workspace;

		public ClassAttributeUpdater(Workspace workspace, int maxParameters, int maxMethodLength) : base(false)
		{
			this.workspace = workspace;
			this.maxParameters = maxParameters;
			this.maxMethodLength = maxMethodLength;
		}

		public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
		{
			var leadingTrivia = node.GetLeadingTrivia();
			return base.VisitConstructorDeclaration(node).WithLeadingTrivia(leadingTrivia);
		}

		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			var leadingTrivia = node.GetLeadingTrivia();
			return base.VisitMethodDeclaration(node).WithLeadingTrivia(leadingTrivia);
		}

		public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
		{
			var result = (AttributeListSyntax) base.VisitAttributeList(node);
			return result.Attributes.Any() ? result : null;
		}

		public override SyntaxNode VisitAttribute(AttributeSyntax node)
		{
			if (node.Name.ToString() == nameof(DebtMethod))
			{
				var containingMethod = node.Ancestors().OfType<BaseMethodDeclarationSyntax>().First();
				if (containingMethod.ParameterList.Parameters.Count <= maxParameters && MethodLengthAnalyzer.GetMethodLength(containingMethod) < maxMethodLength)
				{
					return null;
				}
				return Formatter.Format(MethodDebtAnnotationProvider.GetAttribute(containingMethod), workspace);
			}
			return base.VisitAttribute(node);
		}
	}
}