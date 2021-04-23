// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.SvCommon.Attributes
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NotificationService.SvCommon.Attributes;
    using NUnit.Framework;

    /// <summary>
    /// Tests for the ValidateModelAttribute.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ValidateModelAttributeTests
    {
        private ILogger<ValidateModelAttribute> logger;

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.logger = Mock.Of<ILogger<ValidateModelAttribute>>();
        }

        /// <summary>
        /// Tests for OnActionExecuting method for invalid inputs.
        /// </summary>
        [Test]
        public void OnActionExecutingInvalidInput()
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");

            var actionContext = new ActionContext(
                Mock.Of<HttpContext>(),
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(),
                modelState);

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                Mock.Of<Controller>());

            ValidateModelAttribute validateModelAttribute = new ValidateModelAttribute(this.logger);
            validateModelAttribute.OnActionExecuting(actionExecutingContext);

            Assert.IsInstanceOf<BadRequestObjectResult>(actionExecutingContext.Result);
        }

        /// <summary>
        /// Tests for OnActionExecuting method for valid inputs.
        /// </summary>
        [Test]
        public void OnActionExecutingValidInput()
        {
            var modelState = new ModelStateDictionary();

            var actionContext = new ActionContext(
                Mock.Of<HttpContext>(),
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(),
                modelState);

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                Mock.Of<Controller>());

            ValidateModelAttribute validateModelAttribute = new ValidateModelAttribute(this.logger);
            validateModelAttribute.OnActionExecuting(actionExecutingContext);

            Assert.IsNull(actionExecutingContext.Result);
        }
    }
}
