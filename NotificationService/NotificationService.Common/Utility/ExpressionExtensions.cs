// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Utility
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// ExpressionExtensions.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// And Operation.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr1">First Expression.</param>
        /// <param name="expr2">Second Expression.</param>
        /// <returns>Combined Expression.</returns>
        public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null || expr2 == null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException($"Expression cannot be null");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            var secondBody = expr2.Body.Replace(expr2.Parameters[0], expr1.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, secondBody), expr1.Parameters);
        }

        private static Expression Replace(this Expression expression, Expression searchEx, Expression replaceEx)
        {
            return new ExpressionReplaceVisitor(searchEx, replaceEx).Visit(expression);
        }
    }
}