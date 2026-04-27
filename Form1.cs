using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace compiler_project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {  


        }

        private void button1_Click(object sender, EventArgs e)
        {
            string input = textBox1.Text + "";
            string keywords = @"\b(if|then|elseif|string|int|float|return|else|endl|until|repeat|read|write)\b"; 
            string identifiers = @"\b[a-zA-Z][a-zA-Z0-9]*\b"; 
            string numbers = @"\b\d+(\.\d+)?\b"; 
            string strings = "\"[^\"]*\""; 
            string comments = @"/\*.*?\*/"; 
            string arethmatic_operators = @"[+\-*/]"; 
            string assignment_operators = @":=";
            string relational_operators = @"(<=|>=|<>|<|>|=)";
            string boolean_operators = @"(\|\||&&)";
            string symbols = @"[;(),{}]";
            string term = $@"({numbers}|{identifiers})";
            string masterPattern = $"{comments}|{strings}|{assignment_operators}|{relational_operators}|{boolean_operators}|{arethmatic_operators}|{symbols}|{identifiers}|{numbers}|{keywords}";


            DataTable dt = new DataTable();
            dt.Columns.Add("Lexeme");
            dt.Columns.Add("Token Type");
            

            MatchCollection matches = Regex.Matches(input, masterPattern);
            List<Token> allTokens = new List<Token>();
            foreach (Match m in matches)
            {
                string lex = m.Value;
                string type = "";


                if (Regex.IsMatch(lex, comments))
                    type = "Comment_Statement";
                else if (Regex.IsMatch(lex, strings))
                    type = "String";
                
                else if (Regex.IsMatch(lex, keywords)) { 
                    if (lex == "int" || lex == "float" || lex == "string")
                        type = "Datatype"; 
                    else
                        type = "Reserved_Keyword";
                }
                   
               
                else if (Regex.IsMatch(lex, numbers))
                    type = "Number";
                else if (Regex.IsMatch(lex, identifiers))
                    type = "Identifier";
                else if (Regex.IsMatch(lex, assignment_operators))
                    type = ":=";

                else if (Regex.IsMatch(lex, relational_operators))
                    type = "RelOp";

                else if (Regex.IsMatch(lex, boolean_operators))
                    type = "BoolOp";
                else if (Regex.IsMatch(lex, arethmatic_operators))
                {
                    switch (lex)
                    {
                        case "+": type = "Plus_Op"; break;
                        case "-": type = "Minus_Op"; break;
                        case "*": type = "Multiply_Op"; break;
                        case "/": type = "Divide_Op"; break;
                        default: type = "Operator"; break;
                    }
                }
                else if (Regex.IsMatch(lex, symbols))
                {
                    switch (lex)
                    {
                        case ";": type = "Semicolon"; break;
                        case "(": type = "Left_Paren"; break;
                        case ")": type = "Right_Paren"; break;
                        case ",": type = "Comma"; break;
                        default: type = "Symbol"; break;
                    }
                }
                else
                    type = "Unknown";

                dt.Rows.Add(lex, type);
                allTokens.Add(new Token { Value = lex, Type = type });
            }
            dataGridView1.DataSource = dt;
            try
            {
                if (allTokens.Count == 0) return;
                TinyParser parser = new TinyParser(allTokens);
                parser.ParseProgram();
                MessageBox.Show(" Success: Your TinyLanguage code is correct!", " Success" ,
MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Syntax Error: " +ex.Message, " Error",
MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

