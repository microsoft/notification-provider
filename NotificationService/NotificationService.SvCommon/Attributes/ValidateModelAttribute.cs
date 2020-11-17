// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="ValidateModelAttribute"/> class validates the input request model.
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    public sealed class ValidateModelAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<ValidateModelAttribute> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateModelAttribute"/> class.
        /// </summary>
        /// <remarks>
        /// Note: The class cannot be instantiated without the service collection is able to inject logger.
        /// </remarks>
        /// <param name="logger">The instance of <see cref="ILogger{ValidateModelAttribute}"/>.</param>
        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger) => this.logger = logger;

        /// <summary>
        /// Executes the request model validation before every action method.
        /// </summary>
        /// <param name="context">The instance of <see cref="ActionExecutedContext"/>.</param>
        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            List<string> errors;
            ControllerActionDescriptor actionDescriptor = ((ControllerBase)context.Controller).ControllerContext.ActionDescriptor;
            if (!context.ModelState.IsValid)
            {
                errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                this.logger.LogError($"Invalid request data for action '{actionDescriptor?.ActionName} in {actionDescriptor?.ControllerName}Controller'\n" + string.Join("\n", errors));
                context.Result = new BadRequestObjectResult(errors);
            }
        }
    }
}
