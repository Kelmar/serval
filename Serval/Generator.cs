using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Serval.Fault;
using Serval.Parsing.AST;

using Llvm.NET;
using Llvm.NET.Interop;
using Llvm.NET.Instructions;
using Llvm.NET.Values;
using Llvm.NET.Types;

//using static Llvm.NET.Interop.Library;

namespace Serval
{
    public class Generator : IDisposable
    {
        private readonly IDictionary<string, DeclarationExpr> m_symbolTable = new Dictionary<string, DeclarationExpr>();

        private readonly IDisposable m_llvm;

        private readonly Context m_context;
        private readonly BitcodeModule m_module;
        private readonly InstructionBuilder m_builder;

        public Generator()
        {
            m_llvm = Library.InitializeLLVM();
            Library.RegisterNative(TargetRegistrations.AsmPrinter);

            //m_llvm.RegisterTarget(CodeGenTarget.Native);

            m_context = new Context();
            m_module = m_context.CreateBitcodeModule();
            m_builder = new InstructionBuilder(m_context);
        }

        public void Dispose()
        {
            m_module.Dispose();
            m_context.Dispose();
            m_llvm.Dispose();
        }

        public void Generate(List<Expression> astList)
        {
            Debug.Assert(astList != null, "Got NULL in AST!?");

            foreach (var ast in astList)
            {
                var value = Generate(ast);
            }
        }

        public Value Generate(Expression ast)
        {
            switch (ast)
            {
            case ConstExpr ce: return GenerateConstExpr(ce);
            case VariableExpr ve: return GenerateVariableExpr(ve);
            case AssignmentStatement @as: return GenerateAssignment(@as);
            case BinaryExpr be: return GenerateBinaryExpr(be);
            case UniaryExpr ue: return GenerateUniaryExpr(ue);

            case DeclarationExpr de: return GenerateDeclExpr(de);

            default:
                string name = ast.GetType().Name;
                throw new NotImplementedException($"{name} not implemented.");
            }
        }

        private Value GenerateConstExpr(ConstExpr ast)
        {
            switch (ast.Token.Type)
            {
            case Lexing.TokenType.IntConst:
                return m_context.CreateConstant((int)ast.Token.Parsed);

            case Lexing.TokenType.FloatConst:
                return m_context.CreateConstant((float)ast.Token.Parsed);

            case Lexing.TokenType.StringConst:
                return m_context.CreateConstantString((string)ast.Token.Parsed, nullTerminate: true);

            default:
                throw new NotImplementedException($"Unknown constant type {ast.Token.Type}");
            }
        }

        private Value GenerateVariableExpr(VariableExpr ast)
        {
            return default;
        }

        private Value GenerateAssignment(AssignmentStatement ast)
        {
            var body = Generate(ast.Body);

            if (!m_symbolTable.TryGetValue(ast.Identifier.Literal, out var symbol))
                throw new CompilerBugException($"Undeclared variable {ast.Identifier.Literal} on line {ast.Identifier.LineNumber}");

            return null;

            //return m_builder.MemSet(symbol, body, default, false);
        }

        private Value GenerateBinaryExpr(BinaryExpr ast)
        {
            var left = Generate(ast.Left);
            var right = Generate(ast.Right);

            switch (ast.Operator)
            {
            case "+" : return m_builder.Add(left, right);
            case "-" : return m_builder.Sub(left, right);
            case "*" : return m_builder.Mul(left, right);
            case "/" : return m_builder.SDiv(left, right);
            case "%" : return m_builder.SRem(left, right);
            case "<<": return m_builder.ArithmeticShiftRight(left, right);
            case ">>": return m_builder.ShiftLeft(left, right);
            case "<" : return m_builder.Compare(IntPredicate.SignedLess, left, right);
            case ">" : return m_builder.Compare(IntPredicate.SignedGreater, left, right);
            case "<=": return m_builder.Compare(IntPredicate.SignedLessOrEqual, left, right);
            case ">=": return m_builder.Compare(IntPredicate.SignedGreaterOrEqual, left, right);
            case "==": return m_builder.Compare(IntPredicate.Equal, left, right);
            case "!=": return m_builder.Compare(IntPredicate.NotEqual, left, right);

            default:
                throw new NotImplementedException($"Unknown binary operator {ast.Operator}");
            }
        }

        private Value GenerateUniaryExpr(UniaryExpr ast)
        {
            var right = Generate(ast.Right);

            switch (ast.Operator)
            {
            case '+': // Uniary +, does nothing
                return right;

            case '-': // Uniary -, negate the value.
                return m_builder.Neg(right);

            //case '*': // Pointer dereference
            //    return m_builder.;

            //case '&': // Get address
            //    return m_builder.;

            //case '~': // Bitwise not
            //    return m_builder.;

            case '!': // Logical not
                return m_builder.Not(right);

            default:
                throw new NotImplementedException($"Unknown uniary operator {ast.Operator}");
            }
        }

        private Value GenerateDeclExpr(DeclarationExpr ast)
        {
            //m_symbolTable.Add(ast.Identifier.Literal, ast);
            //m_module.

            return default;
        }
    }
}
