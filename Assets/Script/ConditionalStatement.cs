using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLua;

namespace Script
{
    public class ConditionalStatement
    {
        protected LuaFunction conditionCheckFunction = null;
        protected int positionInExecList;

        // should returen true when finish executing
        public virtual bool Execute() { return false; }
        public virtual void Reset()
        {
            positionInExecList = 0;
        }

        //returns if value
        protected bool checkState()
        {
            if (conditionCheckFunction == null)
            {
                return true;
            }

            object[] result = conditionCheckFunction.Call();

            return (bool)result[0];
        }
    }
}
