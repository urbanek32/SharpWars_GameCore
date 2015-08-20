using System.Collections.Generic;
using UnityEngine; //debug
using NLua;

namespace Script
{
    public class WhileStatement : ConditionalStatement
    {
        private bool stateChecked;
        protected List<ConditionalStatement> executionBlock;

        public WhileStatement(LuaFunction checkstate_function)
        {
            conditionCheckFunction = checkstate_function;
            executionBlock = null;
            stateChecked = false;
        }

        public override bool Execute()
        {
            if (!stateChecked)
            {
                stateChecked = true;

                if (!checkState())
                {
                    return true;
                }
            }

            if (executionBlock == null || executionBlock.Count == 0)
            {
                Reset();
                return false;
            }

            while (positionInExecList != executionBlock.Count && executionBlock[positionInExecList].Execute())
            {
                positionInExecList++;
            }

            if (positionInExecList == executionBlock.Count)
            {
                Reset();
            }

            return false;
        }

        public void SetExecutionBlock(List<ConditionalStatement> exec_block)
        {
            executionBlock = exec_block;
            positionInExecList = 0;
        }

        public void AddCSToExecutionBlock(ConditionalStatement cs_block)
        {
            if (executionBlock == null)
            {
                executionBlock = new List<ConditionalStatement>();
            }
            executionBlock.Add(cs_block);
        }

        //back to state before execution
        public override void Reset()
        {
            base.Reset();
            stateChecked = false;

            if (executionBlock == null)
                return;

            foreach (ConditionalStatement cs in executionBlock)
            {
                cs.Reset();
            }
        }
    }
}
