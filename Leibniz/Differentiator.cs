using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Leibniz
{
    public class Differentiator
    {
        public Differentiator(SemanticModel model)
        {
            Model = model;
        }

        public SemanticModel Model { get; }

        public SyntaxTree Transform(string methodName)
        {
            var methodNode = GetMethodSyntax(methodName);
            var parameter = methodNode.ParameterList.Parameters.Single();

            var returnStatement = methodNode.DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Single();
            var differentiated = DifferentiateExpression(
                returnStatement.Expression, parameter.Identifier);

            // remove outer parenthesis
            if (differentiated.Kind() == SyntaxKind.ParenthesizedExpression)
            {
                differentiated = (differentiated as ParenthesizedExpressionSyntax).Expression;
            }

            var newReturn = returnStatement.WithExpression(differentiated);
            return methodNode
                .ReplaceNode(returnStatement, newReturn)
                .WithIdentifier(Identifier("D_" + methodNode.Identifier.Value.ToString()))
                .NormalizeWhitespace()
                .SyntaxTree;
        }

        private MethodDeclarationSyntax GetMethodSyntax(string methodName)
        {
            var methodSyntax = Model.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.Value.ToString() == methodName)
                .Single();
            return methodSyntax;
        }

        ExpressionSyntax DifferentiateExpression(
            ExpressionSyntax expression, SyntaxToken x) // should x be an ISymbol?
        {
            var kind = expression.Kind();
            switch (kind)
            {
                case SyntaxKind.UnaryMinusExpression:
                    return DifferentiateExpression(
                        expression as PrefixUnaryExpressionSyntax, x);

                case SyntaxKind.MultiplyExpression:
                    return DifferntiateMultiplyExpression(
                        expression as BinaryExpressionSyntax, x);

                case SyntaxKind.AddExpression:
                    return DifferentiateAddExpression(
                        expression as BinaryExpressionSyntax, x);

                case SyntaxKind.SubtractExpression:
                    return DifferentiateSubtractExpression(
                        expression as BinaryExpressionSyntax, x);

                case SyntaxKind.DivideExpression:
                    return DifferentiateDivideExpression(
                        expression as BinaryExpressionSyntax, x);

                case SyntaxKind.IdentifierName:
                    var id = expression as IdentifierNameSyntax;
                    if ((string)id.Identifier.Value == (string)x.Value)
                    {
                        return 1.0.ToLiteral();
                    }
                    else
                    {
                        return 0.0.ToLiteral();
                    }

                case SyntaxKind.NumericLiteralExpression:
                    return 0.0.ToLiteral();

                case SyntaxKind.ParenthesizedExpression:
                    var paranthesized = expression as ParenthesizedExpressionSyntax;
                    var inner = DifferentiateExpression(paranthesized.Expression, x);
                    if (inner.Kind() == SyntaxKind.NumericLiteralExpression)
                        return inner;
                    else
                        return paranthesized.WithExpression(inner);

                case SyntaxKind.InvocationExpression:
                    var invocation = expression as InvocationExpressionSyntax;
                    var symbol = Model.GetSymbolInfo(invocation).Symbol;

                    if (invocation.ArgumentList.Arguments.Count() != 1)
                    {
                        throw new ArgumentException($"Too many arguments: {invocation.ToString()}");
                    }

                    var argument = invocation.ArgumentList.Arguments[0] as ArgumentSyntax;
                    var innerDervative = DifferentiateExpression(argument.Expression, x);

                    ExpressionSyntax outerDerivative;
                    switch (symbol.ToString())
                    {
                        case "System.Math.Sin(double)":
                            outerDerivative = invocation.WithExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Math"), IdentifierName("Cos")));
                            break;

                        case "System.Math.Cos(double)":
                            var sin = invocation.WithExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Math"), IdentifierName("Sin")));
                            outerDerivative = PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, sin);
                            break;

                        case "System.Math.Exp(double)":
                            outerDerivative = invocation;
                            break;

                        default:
                            throw new ArgumentException($"Unknown method: {invocation.ToString()}");
                    }

                    if (innerDervative.IsOne())
                    {
                        return outerDerivative;
                    }
                    else
                    {
                        return BinaryExpression(
                            SyntaxKind.MultiplyExpression, outerDerivative, innerDervative);
                    }

                default:
                    throw new Exception($"Unknown syntax kind: {kind}");
            }
        }

        private ExpressionSyntax DifferentiateDivideExpression(
            BinaryExpressionSyntax div, SyntaxToken x)
        {
            // (u/v)' = u'/v - u*v'/v^2
            var divleft = div.Left;
            var divright = div.Right;
            var ddivLeft = DifferentiateExpression(divleft, x);
            var ddivRight = DifferentiateExpression(divright, x);
            var divA = divright.IsOne() ? ddivLeft
                : ddivLeft.IsZero() ? ddivLeft
                : BinaryExpression(SyntaxKind.DivideExpression, ddivLeft, divright);
            var divB = divleft.IsZero() ? divleft
                : ddivRight.IsZero() ? ddivRight
                : BinaryExpression(SyntaxKind.DivideExpression,
                    BinaryExpression(SyntaxKind.MultiplyExpression, divleft, ddivRight),
                    ParenthesizedExpression(
                        BinaryExpression(SyntaxKind.MultiplyExpression, divright, divright)));
            return divB.IsZero() ? divA
                : BinaryExpression(SyntaxKind.SubtractExpression, divA, divB);
        }

        private ExpressionSyntax DifferentiateSubtractExpression(
            BinaryExpressionSyntax sub,   SyntaxToken x)
        {
            var subleft = sub.Left;
            var subright = sub.Right;
            var dSubLeft = DifferentiateExpression(subleft, x);
            var dSubRight = DifferentiateExpression(subright, x);
            return dSubRight.IsZero() ? dSubLeft
                : BinaryExpression(SyntaxKind.SubtractExpression, dSubLeft, dSubRight);
        }

        private ExpressionSyntax DifferentiateAddExpression(
            BinaryExpressionSyntax add, SyntaxToken x)
        {
            var sumleft = add.Left;
            var sumright = add.Right;
            var dSumLeft = DifferentiateExpression(sumleft, x);
            var dSumRight = DifferentiateExpression(sumright, x);
            if (dSumLeft.IsZero())
                return dSumRight;
            else
                return dSumRight.IsZero() ? dSumLeft
                    : BinaryExpression(SyntaxKind.AddExpression, dSumLeft, dSumRight);
        }

        private ExpressionSyntax DifferntiateMultiplyExpression(
            BinaryExpressionSyntax expression, SyntaxToken x)
        {
            var left = expression.Left;
            var right = expression.Right;
            var dLeft = DifferentiateExpression(left, x);
            var dRight = DifferentiateExpression(right, x);
            ExpressionSyntax exprA = null;
            ExpressionSyntax exprB = null;
            if (!dLeft.IsZero())
            {
                exprA = dLeft.IsOne() ? right
                : BinaryExpression(SyntaxKind.MultiplyExpression, dLeft, right);
            }
            if (!dRight.IsZero())
                exprB = dRight.IsOne() ? left
                    : BinaryExpression(SyntaxKind.MultiplyExpression, left, dRight);

            if (exprA != null && exprB != null)
            {
                return ParenthesizedExpression(BinaryExpression(SyntaxKind.AddExpression, exprA, exprB));
            }
            else if (exprA == null && exprB == null)
            {
                return 0.0.ToLiteral();
            }
            else
                return exprA ?? exprB;
        }

        private ExpressionSyntax DifferentiateExpression(
            PrefixUnaryExpressionSyntax expression, SyntaxToken x)
        {
            var innerDerivative = DifferentiateExpression(expression.Operand, x);

            switch(expression.Kind())
            {
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.UnaryMinusExpression:
                    return expression.WithOperand(innerDerivative);
                default:
                    throw new ArgumentException($"Unexpected syntax kind: {expression.Kind()}");
            }
        }
    }
}
