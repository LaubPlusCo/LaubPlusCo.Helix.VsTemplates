using System;

namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public class BooleanConditionEvaluator : IEvaluateCondition
  {
    public bool Evaluate(string condition)
    {
      return string.IsNullOrEmpty(condition)
             || condition.IndexOf("$", StringComparison.OrdinalIgnoreCase) >= 0
             || EvaluateExpression(condition);
    }

    protected virtual bool EvaluateExpression(string condition)
    {
      return condition.StartsWith("!")
        ? !EvaluateBool(condition.Replace("!", ""))
        : EvaluateBool(condition);
    }

    protected virtual bool EvaluateBool(string condition)
    {
      return bool.TryParse(condition, out var b) && b;
    }
  }
}