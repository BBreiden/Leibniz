using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leibniz
{
    public static class LiteralExtensions
    {
        public static LiteralExpressionSyntax ToLiteral (this double value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(value));
        }

        public static IdentifierNameSyntax ToLiteral (this string name)
        {
            return SyntaxFactory.IdentifierName(name);
        }
    }
}
