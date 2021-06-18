// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.CustomValidations
{
    using System.ComponentModel.DataAnnotations;
    using NotificationService.Common;
    using NotificationService.Common.Utility;

    /// <summary>
    /// Class to validate emails.
    /// </summary>
    public sealed class EmailIdListValidationAttribute : ValidationAttribute
    {
        public string PropertyName { get; set; }

        public bool Nullable { get; set; } = true;

        /// <inheritdoc/>
        protected override ValidationResult IsValid(object emailIdsString, ValidationContext validationContext)
        {
            if (emailIdsString == null )
            {
                return this.Nullable ? ValidationResult.Success : new ValidationResult($"{this.PropertyName} can't be null/empty ");
            }

            if (emailIdsString.ToString().HasWhitespaces())
            {
                return new ValidationResult($"Whitespaces are not allowed in property ['{this.PropertyName}'].");
            }

            string[] emailIds = emailIdsString.ToString().Split(ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries);

            bool isValid = true;
            string invalidEmailAddresses = null;
            foreach (var emailId in emailIds)
            {
                if (!emailId.IsValidEmail())
                {
                    isValid = false;
                    invalidEmailAddresses = invalidEmailAddresses == null ? emailId : string.Concat(invalidEmailAddresses, ",", emailId);
                }
            }

            return isValid ? ValidationResult.Success : new ValidationResult($"EmailIds [{invalidEmailAddresses}] are invalid for property ['{this.PropertyName}'].");
        }
    }
}
