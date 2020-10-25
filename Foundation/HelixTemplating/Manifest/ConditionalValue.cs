namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public class ConditionalValue
  {
    public string Value { get; set; }
    public string Condition { get; set; }
    public IEvaluateCondition ConditionEvaluator { get; set; }

    public bool Evaluate()
    {
      return ConditionEvaluator == null || ConditionEvaluator.Evaluate(Condition);
    }
  }
}