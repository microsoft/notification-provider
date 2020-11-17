// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Utility
{
    using System.Linq.Expressions;

    /// <summary>
    /// ReplaceVisitor.
    /// </summary>
    internal class ExpressionReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression fromExpression;
        private readonly Expression toExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionReplaceVisitor"/> class.
        /// </summary>
        /// <param name="fromExpression">From Expression.</param>
        /// <param name="toExpression">To Expression.</param>
        public ExpressionReplaceVisitor(Expression fromExpression, Expression toExpression)
        {
            this.fromExpression = fromExpression;
            this.toExpression = toExpression;
        }

        /// <summary>
        /// Visit.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <returns>Expression.</returns>
        public override Expression Visit(Expression node)
        {
            return node == this.fromExpression ? this.toExpression : base.Visit(node);
        }
    }
}
