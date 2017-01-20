using System;

namespace SolutionRenamer.Win.Logic
{
    public class RenamerRuleBuilder
    {
        private RenamerRule _root;
        private RenamerRule _nextRule;

        public RenamerRuleBuilder()
        {
            _root = _nextRule = RenamerRule.IsTrue;
        }

        public RenamerRule Build()
        {
            return _root;
        }

        public RenamerRuleBuilder SetNext(RenamerRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            _nextRule.Next = rule;
            _nextRule = rule;
            return this;
        }
    }
}