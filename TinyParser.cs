using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler_project
{
    public class TinyParser
    {
        private readonly List<Token> tokens;
        private int index = 0;
        private Token currentToken;

        public TinyParser(List<Token> tokens)
        {
            this.tokens = tokens;
            if (tokens.Count > 0)
                currentToken = tokens[0];
        }

        
        private void Match(string expected)
        {
            if (index < tokens.Count &&
                (currentToken.Value == expected || currentToken.Type == expected))
            {
                index++;
                currentToken = index < tokens.Count ? tokens[index] : null;
            }
            else
            {
                throw new Exception(
                    $"Expected '{expected}' but found '{currentToken?.Value}'"
                );
            }
        }

        private Token LookAhead(int k)
        {
            return (index + k < tokens.Count) ? tokens[index + k] : null;
        }

        private bool IsDatatype()
        {
            return currentToken?.Value == "int" ||
                   currentToken?.Value == "float" ||
                   currentToken?.Value == "string";
        }

        private void ParseDatatype()
        {
            if (IsDatatype())
                Match(currentToken.Value);
            else
                throw new Exception("Expected datatype");
        }

        
        public void ParseProgram()
        {
            while (index < tokens.Count)
            {
                if (IsDatatype())
                {
                    
                    if (LookAhead(1)?.Value == "main")
                    {
                        ParseMainFunction();
                    }
                    
                    else if (LookAhead(2)?.Value == "(")
                    {
                        ParseFunction();
                    }
                    
                    else
                    {
                        ParseDeclaration();
                    }
                }
                else
                {
                    throw new Exception(
                        $"Program must start with datatype or function. Found '{currentToken.Value}'"
                    );
                }
            }
        }

        
        private void ParseFunction()
        {
            ParseDatatype();
            Match("Identifier");
            Match("(");
            ParseParameters();
            Match(")");
            ParseFunctionBody();
        }

        private void ParseMainFunction()
        {
            ParseDatatype();

            if (currentToken.Value == "main")
                Match("main");
            else
                throw new Exception("Expected 'main'");

            Match("(");
            Match(")");
            ParseFunctionBody();
        }

        private void ParseParameters()
        {
            if (IsDatatype())
            {
                ParseDatatype();
                Match("Identifier");

                while (currentToken.Value == ",")
                {
                    Match(",");
                    ParseDatatype();
                    Match("Identifier");
                }
            }
        }

        private void ParseFunctionBody()
        {
            Match("{");
            ParseStatements();
            ParseReturn();
            Match("}");
        }

        
        private void ParseStatements()
        {
            while (index < tokens.Count &&
                   currentToken.Value != "}" &&
                   currentToken.Value != "end" &&
                   currentToken.Value != "until" &&
                   currentToken.Value != "return")   
            {
                ParseStatement();
            }
        }

        private void ParseStatement()
        {
            if (IsDatatype())
                ParseDeclaration();

            else if (currentToken.Type == "Identifier")
                ParseAssignment();

            else if (currentToken.Value == "read")
                ParseRead();

            else if (currentToken.Value == "write")
                ParseWrite();

            else if (currentToken.Value == "return")
                ParseReturn();

            else if (currentToken.Value == "if")
                ParseIf();

            else if (currentToken.Value == "repeat")
                ParseRepeat();

            else
                throw new Exception(
                    $"Invalid statement start: '{currentToken.Value}'"
                );
        }

        
        private void ParseDeclaration()
        {
            ParseDatatype();
            ParseDeclItem();

            while (currentToken.Value == ",")
            {
                Match(",");
                ParseDeclItem();
            }

            Match(";");
        }

        private void ParseDeclItem()
        {
            Match("Identifier");

            if (currentToken.Value == ":=")
            {
                Match(":=");
                ParseExpression();
            }
        }

        
        private void ParseAssignment()
        {
            Match("Identifier");
            Match(":=");
            ParseExpression();
            Match(";");
        }

        
        private void ParseRead()
        {
            Match("read");
            Match("Identifier");
            Match(";");
        }

        private void ParseWrite()
        {
            Match("write");

            if (currentToken.Value == "endl")
                Match("endl");
            else
                ParseExpression();

            Match(";");
        }

        
        private void ParseReturn()
        {
            Match("return");
            ParseExpression();
            Match(";");
        }

        
        private void ParseIf()
        {
            Match("if");
            ParseConditionStatement();
            Match("then");

            ParseStatements();

            while (currentToken.Value == "elseif")
            {
                Match("elseif");
                ParseConditionStatement();
                Match("then");
                ParseStatements();
            }

            if (currentToken.Value == "else")
            {
                Match("else");
                ParseStatements();
            }

            Match("end");
        }

        
        private void ParseRepeat()
        {
            Match("repeat");
            ParseStatements();
            Match("until");
            ParseConditionStatement();
        }

        
        private void ParseConditionStatement()
        {
            ParseCondition();

            while (currentToken.Value == "&&" || currentToken.Value == "||")
            {
                Match(currentToken.Value);
                ParseCondition();
            }
        }

        private void ParseCondition()
        {
            Match("Identifier");

            if (currentToken.Value == "<" ||
                currentToken.Value == ">" ||
                currentToken.Value == "=" ||
                currentToken.Value == "<>")
            {
                Match(currentToken.Value);
            }
            else
            {
                throw new Exception("Expected condition operator");
            }

            ParseTerm();
        }

        
        private void ParseExpression()
        {
            if (currentToken.Type == "String")
            {
                Match("String");
                return;
            }

            ParseEquation();
        }

        private void ParseEquation()
        {
            ParseTerm();

            while (currentToken.Value == "+" || currentToken.Value == "-")
            {
                Match(currentToken.Value);
                ParseTerm();
            }
        }

        private void ParseTerm()
        {
            ParseFactor();

            while (currentToken.Value == "*" || currentToken.Value == "/")
            {
                Match(currentToken.Value);
                ParseFactor();
            }
        }

        private void ParseFactor()
        {
            if (currentToken.Type == "Number")
            {
                Match("Number");
            }
            else if (currentToken.Type == "Identifier")
            {
                Match("Identifier");

               
                if (currentToken?.Value == "(")
                {
                    Match("(");
                    ParseArguments();
                    Match(")");
                }
            }
            else if (currentToken.Value == "(")
            {
                Match("(");
                ParseEquation();
                Match(")");
            }
            else
            {
                throw new Exception($"Unexpected token '{currentToken.Value}'");
            }
        }

        private void ParseArguments()
        {
            if (currentToken.Type == "Identifier")
            {
                Match("Identifier");

                while (currentToken.Value == ",")
                {
                    Match(",");
                    Match("Identifier");
                }
            }
        }
    }
}
