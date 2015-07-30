﻿using System.Collections.Generic;
using UnityEngine;
using NLua;

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
        public static void RegisterCustomFunction(string custom_function_name, string func_definition)
        {
            if(!customScripts.ContainsKey(custom_function_name))
            {
                try
                {
                    LuaFunction lf = null;
                    environment.DoString(func_definition);
                    lf = environment.GetFunction(custom_function_name);
                    customScripts[custom_function_name] = lf;
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

        // Use it only for user-typed scripts
        public static LuaFunction RegisterUserIngameScript(string user_defined_script)
        {
            LuaFunction lf = null;
            try
            {
                string func_name = "UDS_" + createdUserScripts.ToString();
                string src = "function " + func_name  + "()\n" + user_defined_script + "\nend";
                environment.DoString(src);
                lf = environment.GetFunction(func_name);
            }
            catch (NLua.Exceptions.LuaException e)
            {
                throw e;
            }

            createdUserScripts++;

            return lf;
        }
    }
}