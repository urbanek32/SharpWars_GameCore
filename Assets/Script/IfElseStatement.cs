using System;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace Script
{
    public class IfElseStatement : ConditionalStatement
    {
        private IfElseStatement nextStatement;
        private IfElseStatement executingStatement;
        private bool stateChecked = false;

        protected List<ConditionalStatement> executionBlock;

        public IfElseStatement(LuaFunction checkstate_function)
        {
            conditionCheckFunction = checkstate_function;
            executionBlock = null;
            nextStatement = null;
            executingStatement = null;
        }

        public override bool Execute()
        {
            if (!stateChecked)
            {
                stateChecked = true;

                if (checkState())
                {
                    executingStatement = this;
                }
                else
                {
                    IfElseStatement ies = nextStatement;
                    while( ies != null && executingStatement == null )
                    {
                        if (!ies.checkState())
                        {
                            ies = ies.nextStatement;
                        }
                        else
                        {
                            executingStatement = ies;
                        }
                    }
                }
            }

            if (executingStatement != null)
            {
                if (executingStatement.executionBlock == null)
                    return true;

                while (positionInExecList != executingStatement.executionBlock.Count && executingStatement.executionBlock[positionInExecList].Execute())
                {
                    positionInExecList++;
                }
                if (positionInExecList == executingStatement.executionBlock.Count)
                {
                    //we did it(whole queue)!
                    return true;
                }
            }
            else
            {
                //There is nothing to do
                return true;
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

        public bool isStateChecked()
        {
            return stateChecked;
        }

        //returns if value
        private bool checkState()
        {
            if (conditionCheckFunction == null)
            {
                return true;
            }

            object[] result = conditionCheckFunction.Call();

            return (bool)result[0];
        }

        public void SetNextConditionalStatement(IfElseStatement cs)
        {
            nextStatement = cs;
        }
    }
}
