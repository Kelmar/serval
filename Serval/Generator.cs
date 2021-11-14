using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using LLVMSharp;

using Serval.Fault;
using Serval.Parsing.AST;

namespace Serval
{
    public class Generator : IDisposable
    {
        private readonly IRBuilder m_builder;

        public Generator()
        {
            m_builder = new IRBuilder();
            
        }

        public void Dispose()
        {
            m_builder.Dispose();
        }

        public LLVMValueRef Generate(Expression ast)
        {
            Debug.Assert(ast != null);

            switch (ast)
            {
            case AssignmentStatement @as: return GenerateAssignment(@as);
            case BinaryExpr be: return GenerateBinaryExpr(be);
            case UniaryExpr ue: return GenerateUniaryExpr(ue);
            case DeclarationExpr de: return GenerateDeclExpr(de);
            }

            return default;
        }

        private LLVMValueRef GenerateAssignment(AssignmentStatement ast)
        {
            return default;
        }

        private LLVMValueRef GenerateBinaryExpr(BinaryExpr ast)
        {
            var left = Generate(ast.Left);
            var right = Generate(ast.Right);

            switch (ast.Operator)
            {
            case "+": return m_builder.CreateAdd(left, right, String.Empty);
            case "-": return m_builder.CreateSub(left, right, String.Empty);
            case "*": return m_builder.CreateMul(left, right, String.Empty);
            case "/": return m_builder.CreateSDiv(left, right, String.Empty);
            case "%": return m_builder.CreateSRem(left, right, String.Empty);
            case "<<": return m_builder.CreateShl(left, right, String.Empty);
            case ">>": return m_builder.CreateLShr(left, right, String.Empty);
            case "<": return m_builder.CreateICmp(LLVMIntPredicate.LLVMIntSLT, left, right, String.Empty);
            case ">": return m_builder.CreateICmp(LLVMIntPredicate.LLVMIntSGT, left, right, String.Empty);
            case "<=": return m_builder.CreateICmp(LLVMIntPredicate.LLVMIntSLE, left, right, String.Empty);
            case ">=": return m_builder.CreateICmp(LLVMIntPredicate.LLVMIntSGE, left, right, String.Empty);
            case "==": return m_builder.CreateICmp(LLVMIntPredicate.LLVMIntEQ, left, right, String.Empty);
            case "!=": return m_builder.CreateICmp(LLVMIntPredicate.LLVMIntNE, left, right, String.Empty);

            default:
                throw new CompilerBugException($"Unknown binary operator {ast.Operator}");
            }

            return default;
        }

        private LLVMValueRef GenerateUniaryExpr(UniaryExpr ast)
        {
            var right = Generate(ast.Right);

            switch (ast.Operator)
            {
            case '+':
                // Does nothing
                return right;

            case '-':
                return m_builder.CreateNeg(right, String.Empty);

            case '*':
                // Pointer dereference
                return default;

            case '&':
                // Get address
                return default;

            case '~':
                // Bitwise not
                return m_builder.CreateNot(right, String.Empty);

            case '!':
                // Logical not
                return m_builder.CreateNot(right, String.Empty);

            default:
                throw new CompilerBugException($"Unknown uniary operator {ast.Operator}");
            }
        }

        private LLVMValueRef GenerateDeclExpr(DeclarationExpr ast)
        {
            return default;
        }
    }
}
