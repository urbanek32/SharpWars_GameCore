using System;
using System.Collections.Generic;

using NLua;
using STL;

namespace Script
{
    public class NonconditionalStatement : ConditionalStatement
    {
        protected List<Pair<LuaFunction, LuaFunction>> blockToExecute = null;

        public NonconditionalStatement(List<Pair<LuaFunction, LuaFunction>> execution_block)
        {
            blockToExecute = execution_block;
            positionInExecList = 0;
        }

        public override bool Execute()
        {
            if (blockToExecute == null)
                return true;

            //execution
            while(positionInExecList < blockToExecute.Count)
            {
                Pair<LuaFunction, LuaFunction> task = blockToExecute[positionInExecList];
                task.First.Call();

                if (task.Second == null)
                {
                    positionInExecList++;
                }
                else
                {
                    bool result = (bool)task.Second.Call()[0];
                    if (result)
                    {
                        positionInExecList++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return positionInExecList == blockToExecute.Count;
        }
    }
}
