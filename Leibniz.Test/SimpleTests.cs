using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Leibniz;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using KellermanSoftware.CompareNetObjects;

namespace Leibniz.Test
{
    [TestClass]
    public class SimpleTests
    {
        private const string TestFileName = @"..\..\..\..\Leibniz.Test.Input\SimpleTests.cs";
        private const string OutputFileName = @"..\..\..\..\Leibniz.Test.Output\SimpleTests.cs";

        [DataTestMethod]
        [DataRow("Const")]
        [DataRow("Add")]
        [DataRow("Subtract")]
        [DataRow("Multiply")]
        [DataRow("Divide")]
        public void TestSimpleExpressions(string functionName)
            => DoTestSimpleExpressions(functionName);

        [TestMethod]
        [DataRow("CallMath")]
        [DataRow("CallExp")]
        [DataRow("NestedCalls")]
        public void CallMath(string functionName)
            => DoTestSimpleExpressions(functionName);

        [TestMethod]
        [DataRow("Const")]
        public void TestTransformToElementary(string functionName)
            => DoTestTransformToElementaryOperations(functionName);

        public void DoTestSimpleExpressions(string functionName)
        {
            var model = GetSematicModel();

            var diffOperator = new Differentiator(model);
            var transformed = diffOperator.Transform(functionName);

            var outputTree = Helpers.SyntaxTreeFromFile(OutputFileName);
            var expectedTree = outputTree.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.Value.ToString() == ("D_" + functionName))
                .Select(m => SyntaxFactory.SyntaxTree(m.NormalizeWhitespace()))
                .Single();

            var comparer = new CompareLogic();
            var results = comparer.Compare(expectedTree, transformed);

            IsTrue(results.AreEqual, results.DifferencesString);
        }

        private static SemanticModel GetSematicModel()
        {
            var inputTree = Helpers.SyntaxTreeFromFile(TestFileName);
            var Mscorlib = MetadataReference.CreateFromFile(Constants.MsCorLibDll);
            var comp = CSharpCompilation.Create("test", new[] { inputTree })
                .AddReferences(Mscorlib);
            var model = comp.GetSemanticModel(inputTree);
            return model;
        }

        public void DoTestTransformToElementaryOperations(string functionName)
        {
            var model = GetSematicModel();

            var diffOperator = new Differentiator(model);
            var transformed = diffOperator.TransformToElementaryOperations(functionName);

            var outputTree = Helpers.SyntaxTreeFromFile(OutputFileName);
            var expectedTree = outputTree.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.Value.ToString() == ("T_" + functionName))
                .Select(m => m.NormalizeWhitespace())
                .Single();

            AreEqual(expectedTree.ToFullString(), transformed.ToFullString());
        }
    }
}
