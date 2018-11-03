using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Leibniz
{
    public static class Helpers
    {
        public static SyntaxTree SyntaxTreeFromFile(string filename)
        {
            var text = new StreamReader(filename)
                .ReadToEnd();
            var tree = SyntaxFactory.ParseCompilationUnit(text);
            return tree.SyntaxTree;
        }
    }
}
