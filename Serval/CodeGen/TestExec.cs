using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
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

            foreach (var statement in module.Statements)
            {
                ExecuteStatement(statement);
            }
        }

        private void ExecuteStatement(StatementNode statement)
        {
            switch (statement)
            {
            case AssignmentStatement assignStatement:
                ExecuteAssignment(assignStatement);
                break;

            case LabeledStatement labelStatement:
                ExecuteStatement(labelStatement.Next);
                break;

            case CompoundStatement compoundStatement:
                ExecuteCompoundStatement(compoundStatement);
                break;

            case WhileStatement whileStatement:
                ExecuteWhileStatement(whileStatement);
                break;

            case VariableDecl:
                break; // Nothing really to do right now.

            //case ExpressionStatement exprStatement:
            //    ExecuteExpression(exprStatement.Expression);
            //    break;

            default:
                throw new Exception($"Unknown statement type {statement.GetType().Name}");
            }
        }

        private void ExecuteWhileStatement(WhileStatement whileStatement)
        {
            while (true)
            {
                ExecuteExpression(whileStatement.Condition);
                int r = m_stack.Pop();

                if (r == 0)
                    break;

                ExecuteStatement(whileStatement.Body);
            }
        }

        private void ExecuteCompoundStatement(CompoundStatement compound)
        {
            foreach (var statement in compound.Statements)
                ExecuteStatement(statement);
        }

        private void ExecuteExpression(ExpressionNode expression)
        { 
            switch (expression)
            { 
            case UnaryExpr unaryExpr:
                ExecuteUna(unaryExpr);
                break;

            case CastExpr castExpr:
                ExecuteExpression(castExpr.Right); // Do nothing for now
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

            default:
                throw new Exception($"Unknown expression type {expression.GetType().Name}");
            }
        }

        private void ExecuteAssignment(AssignmentStatement assign)
        {
            ExecuteExpression(assign.Expression);

            assign.Target.Value = m_stack.Pop();

            Console.WriteLine("{0} => {1}", assign.Target.Name, assign.Target.Value);
        }

        private void ExecuteUna(UnaryExpr unaryExpr)
        {
            ExecuteExpression(unaryExpr.Right);

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
            ExecuteExpression(binExpr.Left);
            ExecuteExpression(binExpr.Right);

            int r = m_stack.Pop();
            int l = m_stack.Pop();

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
