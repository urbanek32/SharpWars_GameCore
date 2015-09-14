using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using NLua;
using STL;
using Script;

namespace RTS
{
    public static class ScriptManager
    {
        private static string initialScriptCode = "import 'System'\nimport 'UnityEngine'\nimport 'Assembly-CSharp'\n";
        //no initialised on run
        private static bool isInited = false;

        private static Lua environment;

        private static System.UInt64 createdUserScripts = 0;

        private static Dictionary<string, LuaFunction> globals = new Dictionary<string, LuaFunction>();
        private static Dictionary<string, LuaFunction> customScripts = new Dictionary<string, LuaFunction>();
        private static List<Pair<string, LuaFunction>> blockingFunctions = new List<Pair<string, LuaFunction>>();

        public static void Init()
        {
            if (!isInited)
            {
                environment = new Lua();
                environment.LoadCLRPackage();

                try
                {
                    environment.DoString(initialScriptCode);
                    isInited = true;
                }
                catch (NLua.Exceptions.LuaException e)
                {
                    Debug.Log("[ScriptManager-Init-Ex] " + e.ToString());
                }
            }
        }

        //this method avoid memleak caused by NLua [] operator
        public static void SetGlobal(string global_variable, object val)
        {
            if (!globals.ContainsKey(global_variable))
            {
                string setter_method = "function GlobalVariableSetter" + global_variable + "(val)\n" + global_variable + " = val\nend";
                LuaFunction setter_func = null;
                try
                {
                    environment.DoString(setter_method);
                    setter_func = environment.GetFunction("GlobalVariableSetter" + global_variable);
                    globals[global_variable] = setter_func;
                }
                catch (NLua.Exceptions.LuaException e)
                {
                    Debug.Log("[ScriptManager-Ex-G]" + e.ToString());
                }
            }

            LuaFunction lf = globals[global_variable];
            object[] args = new object[1];
            args[0] = val;
            lf.Call(args);
        }

        //for example if we want to define "PanzerVor(xxx)"
        //1) Function name
        //2) Function definition
        //3) Function name that checks if action has ended [Function must be registered before calling this method!]
        public static void RegisterCustomFunction(string custom_function_name, string func_definition, string state_checker_name)
        {
            if (!customScripts.ContainsKey(custom_function_name) && customScripts.ContainsKey(state_checker_name))
            {
                try
                {
                    LuaFunction lf = null;
                    environment.DoString(func_definition);
                    lf = environment.GetFunction(custom_function_name);
                    customScripts[custom_function_name] = lf;

                    blockingFunctions.Add(new Pair<string, LuaFunction>(custom_function_name, customScripts[state_checker_name]));
                }
                catch (NLua.Exceptions.LuaException e)
                {
                    throw e;
                }
            }
            else
            {
                if (customScripts.ContainsKey(custom_function_name))
                    throw new System.InvalidOperationException("[LUA-EX] Function already defined!");
                else
                    throw new System.InvalidOperationException("[LUA-EX] Checker \"" + state_checker_name + "\" function not exist!");
            }
        }

        //Registering non-blocking custom functions
        public static void RegisterNBCustomFuntion(string function_name, string function_definition)
        {
            if (!customScripts.ContainsKey(function_name))
            {
                try
                {
                    LuaFunction lf = null;
                    environment.DoString(function_definition);
                    lf = environment.GetFunction(function_name);
                    customScripts[function_name] = lf;
                }
                catch (NLua.Exceptions.LuaException e)
                {
                    throw e;
                }
            }
            else
            {
                throw new System.InvalidOperationException("[LUA-EX] Function already defined!");
            }
        }

        //Get custom function
        public static LuaFunction GetCustomFunction(string function_name)
        {
            return environment.GetFunction(function_name);
        }

        private static LuaFunction GenerateUserFunction(string source)
        {
            string user_func_name = "UDS_" + createdUserScripts.ToString();
            string src = "function " + user_func_name + "()\n" + source + "\nend";

            try
            {
                environment.DoString(src);
                createdUserScripts++;
            }
            catch (NLua.Exceptions.LuaException e)
            {
                throw e;
            }

            return environment.GetFunction(user_func_name);
        }

        // Use it only for user-typed scripts
        public static List<ConditionalStatement> RegisterUserIngameScript(string user_defined_script)
        {
            List<ConditionalStatement> ret_val = new List<ConditionalStatement>();
            List<Pair<LuaFunction, LuaFunction>> ncs_block = new List<Pair<LuaFunction, LuaFunction>>();
            Stack<ConditionalStatement> block_depth = new Stack<ConditionalStatement>();

            Pair<string, LuaFunction> blocking_func_pair = null;
            string func_buffer = "";
            string[] loc = user_defined_script.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

            Regex re_if = new Regex("(\\s+if\\s|^if\\s)(?<ConditionExp>[a-zA-Z0-9\\s=+\\/*-<>~]*)\\sthen($|\\s)");
            Regex re_elseif = new Regex("(\\s+elseif\\s|^elseif\\s)(?<ConditionExp>[a-zA-Z0-9\\s=+\\/*-<>~]*)\\sthen($|\\s)");
            Regex re_else = new Regex("(\\s+|^)else($|\\s)");
            Regex re_end = new Regex("(\\s+|^)end($|\\s)");
            Regex re_while = new Regex("(\\s+while\\s|^while\\s)(?<ConditionExp>[a-zA-Z0-9\\s=+\\/*-<>~]*)\\sdo($|\\s)");

            foreach (string line in loc)
            {
                //if match one of the following: [if elseif else end while] then do:
                if (re_if.IsMatch(line) || re_elseif.IsMatch(line) || re_else.IsMatch(line) || re_end.IsMatch(line) || re_while.IsMatch(line))
                {
                    LuaFunction lf = (func_buffer.Length > 3) ? GenerateUserFunction(func_buffer) : null;
                    if (lf != null)
                    {
                        ncs_block.Add(new Pair<LuaFunction, LuaFunction>(lf, null));
                    }

                    if (ncs_block.Count > 0)
                    {
                        if (block_depth.Count > 0 && block_depth.Peek() is IfElseStatement)
                        {
                            IfElseStatement ies = (IfElseStatement)block_depth.Peek();
                            ies.AddCSToExecutionBlock(new NonconditionalStatement(ncs_block));
                        }
                        else if (block_depth.Count > 0 && block_depth.Peek() is WhileStatement)
                        {
                            WhileStatement ws = (WhileStatement)block_depth.Peek();
                            ws.AddCSToExecutionBlock(new NonconditionalStatement(ncs_block));
                        }
                        else if (block_depth.Count == 0)
                        {
                            ret_val.Add(new NonconditionalStatement(ncs_block));
                        }
                        else
                        {
                            Debug.LogError("[LUA:RUIS] Lel script.");
                        }
                        ncs_block = new List<Pair<LuaFunction, LuaFunction>>();
                    }
                    func_buffer = "";
                }


                //check for IF statement
                if (re_if.IsMatch(line))
                {
                    //IF found!

                    //Start a statement
                    LuaFunction cclf = GenerateUserFunction("if " + re_if.Match(line).Groups["ConditionExp"].Value + "then return true\n else return false\n end");
                    IfElseStatement new_statement = new IfElseStatement(cclf);
                    if (block_depth.Count == 0)
                    {
                        ret_val.Add(new_statement);
                    }
                    else
                    {
                        if (block_depth.Peek() is IfElseStatement)
                        {
                            IfElseStatement ies = (IfElseStatement)block_depth.Peek();
                            ies.AddCSToExecutionBlock(new_statement);
                        }
                        else if (block_depth.Peek() is WhileStatement)
                        {
                            WhileStatement ws = (WhileStatement)block_depth.Peek();
                            ws.AddCSToExecutionBlock(new_statement);
                        }
                    }
                    block_depth.Push(new_statement);
                }
                //check for ELSEIF statement
                else if (re_elseif.IsMatch(line))
                {
                    //ELSEIF found!
                    LuaFunction cclf = GenerateUserFunction("if " + re_elseif.Match(line).Groups["ConditionExp"].Value + "then return true\n else return false\n end");
                    IfElseStatement new_statement = new IfElseStatement(cclf);

                    if (block_depth.Peek() is IfElseStatement)
                    {
                        IfElseStatement ies = (IfElseStatement)block_depth.Pop();
                        ies.SetNextConditionalStatement(new_statement);
                        block_depth.Push(new_statement);
                    }
                }
                //check for ELSE statement
                else if (re_else.IsMatch(line))
                {
                    //ELSE found!
                    IfElseStatement new_statement = new IfElseStatement(null);

                    if (block_depth.Peek() is IfElseStatement)
                    {
                        IfElseStatement ies = (IfElseStatement)block_depth.Pop();
                        ies.SetNextConditionalStatement(new_statement);
                        block_depth.Push(new_statement);
                    }
                }
                //check for while
                else if (re_while.IsMatch(line))
                {
                    //WHILE found!
                    LuaFunction cclf = GenerateUserFunction("if " + re_while.Match(line).Groups["ConditionExp"].Value + "then return true\n else return false\n end");
                    WhileStatement new_statement = new WhileStatement(cclf);

                    if (block_depth.Count == 0)
                    {
                        ret_val.Add(new_statement);
                    }
                    else
                    {
                        if (block_depth.Peek() is IfElseStatement)
                        {
                            IfElseStatement ies = (IfElseStatement)block_depth.Peek();
                            ies.AddCSToExecutionBlock(new_statement);
                        }
                        else if (block_depth.Peek() is WhileStatement)
                        {
                            WhileStatement ws = (WhileStatement)block_depth.Peek();
                            ws.AddCSToExecutionBlock(new_statement);
                        }
                    }
                    block_depth.Push(new_statement);
                }
                //check for END ✞
                else if (re_end.IsMatch(line))
                {
                    //E.N.D.
                    block_depth.Pop();
                }
                else
                {
                    bool found_blocking_func = false;
                    foreach (Pair<string, LuaFunction> f in blockingFunctions)
                    {
                        //regexp byłyby wydajniejszy?
                        string[] variants = { f.First + " ", f.First + "\t", f.First + "(" };
                        foreach (string v in variants)
                        {
                            if (line.Contains(v))
                            {
                                found_blocking_func = true;
                                blocking_func_pair = f;
                                break;
                            }
                        }

                        if (found_blocking_func)
                            break;

                    }

                    func_buffer += line + "\n";

                    if (found_blocking_func)
                    {
                        try
                        {
                            ncs_block.Add(new Pair<LuaFunction, LuaFunction>(GenerateUserFunction(func_buffer), blocking_func_pair.Second));

                            func_buffer = "";
                        }
                        catch (NLua.Exceptions.LuaException e)
                        {
                            throw e;
                        }
                    }
                }
            }

            if (func_buffer.Length > 2)
            {
                try
                {
                    ncs_block.Add(new Pair<LuaFunction, LuaFunction>(GenerateUserFunction(func_buffer), null));
                }
                catch (NLua.Exceptions.LuaException e)
                {
                    throw e;
                }
            }

            if (ncs_block.Count > 0)
                ret_val.Add(new NonconditionalStatement(ncs_block));

            return ret_val;
        }
    }
}