using System.Collections.Generic;
using UnityEngine;
using NLua;
using STL;

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
                } catch(NLua.Exceptions.LuaException e)
                {
                    Debug.Log("[ScriptManager-Init-Ex] " + e.ToString());
                }
            }
        }

        //this method avoid memleak caused by NLua method by using [] operator
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
            if(!customScripts.ContainsKey(custom_function_name) && customScripts.ContainsKey(state_checker_name))
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
                    throw new System.InvalidOperationException("[LUA-EX] Checker \"" + state_checker_name +"\" function not exist!");
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
        public static List<Pair<LuaFunction, LuaFunction>> RegisterUserIngameScript(string user_defined_script)
        {
            List<Pair<LuaFunction, LuaFunction>> ret_val = new List<Pair<LuaFunction,LuaFunction>>();

            Pair<string, LuaFunction> blocking_func_pair = null;
            string func_buffer = "";
            string[] loc = user_defined_script.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

            foreach (string line in loc)
            {
                bool found_blocking_func = false;
                foreach (Pair<string, LuaFunction> f in blockingFunctions)
                {
                    //regexp byłyby wydajniejszy?
                    string[] variants = {f.First + " ", f.First + "\t", f.First + "("};
                    foreach(string v in variants)
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
                        ret_val.Add(new Pair<LuaFunction, LuaFunction>(GenerateUserFunction(func_buffer), blocking_func_pair.Second));
                        
                        func_buffer = "";
                    }
                    catch (NLua.Exceptions.LuaException e)
                    {
                        throw e;
                    }
                }
            }

            if (func_buffer.Length > 2)
            {
                try
                {
                    ret_val.Add(new Pair<LuaFunction, LuaFunction>(GenerateUserFunction(func_buffer), null));
                }
                catch (NLua.Exceptions.LuaException e)
                {
                    throw e;
                }
            }

            return ret_val;
        }
    }
}
