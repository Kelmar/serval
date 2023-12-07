using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Text;

using Serval.AST;
using Serval.Fault;

namespace Serval.CodeGen
{
    /// <summary>
    /// Test executor
    /// </summary>
    /// <remarks>
    /// This is here just to test the parser portion for now.
    /// 
    /// Later we'll replace this with a generator that will produce some intermediate code.
    /// </remarks>
    public class TestExec : IDisposable
    {
        private readonly Stack<int> m_stack = [];

        public TestExec()
        {
        }

        public void Dispose()
        {
        }

        public void Execute(Module module)
        {
            Debug.Assert(module != null, "Got NULL AST list!?");

            foreach (var expr in module.Expressions)
            {
                Execute(expr);
                //var value = Generate(expr);
            }
        }

        private void Execute(ExpressionNode expression)
        {
            switch (expression)
            {
            case AssignmentStatement assign:
                ExecuteAssignment(assign);
                break;

            case UnaryExpr unaryExpr:
                ExecuteUna(unaryExpr);
                break;

            case CastExpr castExpr:
                Execute(castExpr.Right); // Do nothing for now
                break;

            case BinaryExpr binExpr:
                ExecuteBin(binExpr);
                break;

            case VariableExpr varExpr:
                ExecuteVar(varExpr);
                break;

            case ConstExpr constExpr:
                ExecuteConst(constExpr);
                break;
            }
        }

        private void ExecuteAssignment(AssignmentStatement assign)
        {
            Execute(assign.Expression);

            assign.Identifier.Value = m_stack.Pop();

            Console.WriteLine("{0} => {1}", assign.Identifier.Name, assign.Identifier.Value);
        }

        private void ExecuteUna(UnaryExpr unaryExpr)
        {
            Execute(unaryExpr.Right);

            int r = m_stack.Pop();
            int res = unaryExpr.Operator switch
            {
                '+' => r,
                '-' => -r,
                '*' => r,
                '&' => r,
                '~' => ~r,
                '!' => r != 0 ? 0 : 1,
                _ => throw new Exception($"Unknown unary operator {unaryExpr.Operator}")
            };

            m_stack.Push(res);
        }

        private void ExecuteBin(BinaryExpr binExpr)
        {
            Execute(binExpr.Left);
            Execute(binExpr.Right);

            int l = m_stack.Pop();
            int r = m_stack.Pop();

            var res = binExpr.Operator switch
            {
                "+" => l + r,
                "-" => l - r,
                "*" => l * r,
                "/" => l / r,
                "%" => l % r,
                "<<" => l << r,
                ">>" => l >> r,
                ">" => l > r ? 1 : 0,
                "<" => l < r ? 1 : 0,
                ">=" => l >= r ? 1 : 0,
                "<=" => l <= r ? 1 : 0,
                "==" => l == r ? 1 : 0,
                "!=" => l != r ? 1 : 0,
                _ => throw new Exception($"Unknown operator {binExpr.Operator}"),
            };

            m_stack.Push(res);
        }

        private void ExecuteVar(VariableExpr varExpr)
        {
            int i = (int)varExpr.Symbol.Value;
            m_stack.Push(i);
        }

        private void ExecuteConst(ConstExpr constExpr)
        {
            if (constExpr.Type.Name != "int")
                throw new Exception($"Cannot read from {constExpr.Type} yet");

            int i = (int)constExpr.Token.Parsed;
            m_stack.Push(i);
        }
    }
}
