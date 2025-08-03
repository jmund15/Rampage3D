using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JmoAI.UtilityAI
{
    public enum ConsiderationOperator
    {
        Add, //suggestion: 2 considerations only 
        Subtract, //suggestion: 2 considerations only
        Multiply, //suggestion: 2 considerations only
        Divide, //suggestion: 2 considerations only
        Average,
        Max,
        Min,
        Random // gets random consid from list
    }
    [GlobalClass, Tool]
    public partial class CompositeConsideration : UtilityConsideration
    {
        [Export]
        protected Godot.Collections.Array<UtilityConsideration> Considerations = new Godot.Collections.Array<UtilityConsideration>();
        [Export]
        protected ConsiderationOperator Operator = ConsiderationOperator.Average;

        protected override float CalculateBaseScore(IBlackboard context)
        {
            if (Considerations.Count == 0) { return 0f; }

            if (Operator == ConsiderationOperator.Random)
            {
                var randConsid = Considerations[Global.Rnd.Next(0, Considerations.Count)]; // get random consid
                return randConsid.Evaluate(context);
            }

            float compositeResult = Considerations[0].Evaluate(context);

            foreach (var consideration in Considerations.Skip(1)) // already grabbed first so skip it
            {
                float result = consideration.Evaluate(context);

                switch (Operator)
                {
                    case ConsiderationOperator.Add:
                        compositeResult += result; // add (SUGGESTED TWO CONSIDERATIONS TOTAL)
                        break;
                    case ConsiderationOperator.Subtract:
                        compositeResult -= result; // subtract from first (SUGGESTED TWO CONSIDERATIONS TOTAL)
                        break;
                    case ConsiderationOperator.Multiply:
                        compositeResult *= result; // multiply (SUGGESTED TWO CONSIDERATIONS TOTAL)
                        break;
                    case ConsiderationOperator.Divide:
                        compositeResult /= result; // divide (SUGGESTED TWO CONSIDERATIONS TOTAL)
                        break;
                    case ConsiderationOperator.Average:
                        compositeResult += result; // will average at end
                        break;
                    case ConsiderationOperator.Max:
                        compositeResult = Mathf.Max(compositeResult, result);
                        break;
                    case ConsiderationOperator.Min:
                        compositeResult = Mathf.Min(compositeResult, result);
                        break;
                    default:
                        //error here?
                        return 0f;
                }
            }

            if (Operator == ConsiderationOperator.Average)
            {
                compositeResult /= Considerations.Count; // get average
            }

            return Mathf.Clamp(compositeResult, 0f, 1f);
        }

        
    }
}
