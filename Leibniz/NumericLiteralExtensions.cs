using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Leibniz
{
    public static class NumericLiteralExtensions
    {
        public static bool IsZero (this ExpressionSyntax expr)
        {
            if (expr.Kind() != SyntaxKind.NumericLiteralExpression)
                return false;

            var token = (expr as LiteralExpressionSyntax).Token;
            switch (token.Value)
            {
                case double d:
                    return d == 0.0;
                case long l:
                    return l == 0;
                case int i:
                    return i == 0;
                default:
                    return false;
            }
        }

        public static bool IsOne (this ExpressionSyntax expr)
        {
            if (expr.Kind() != SyntaxKind.NumericLiteralExpression)
                return false;

            var token = (expr as LiteralExpressionSyntax).Token;
            switch (token.Value)
            {
                case double d:
                    return d == 1.0;
                case long l:
                    return l == 1;
                case int i:
                    return i == 1;
                default:
                    return false;
            }
        }
    }
}
