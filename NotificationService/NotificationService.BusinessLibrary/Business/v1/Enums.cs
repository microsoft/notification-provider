// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace NotificationService.BusinessLibrary.Business
{
    /// <summary>
    /// Speciefis Day of the week.
    /// </summary>
    /// <remarks>
    /// Remarks:
    ///     For the standard days of the week (Sunday, Monday...) the DayOfTheWeek enum value
    ///     is the same as the System.DayOfWeek enum type. These values can be safely cast
    ///     between the two enum types. The special days of the week (Day, Weekday and WeekendDay)
    ///     are used for monthly and yearly recurrences and cannot be cast to System.DayOfWeek
    ///     values.
    /// </remarks>
    public enum DayOfTheWeek
    {
        /// <summary>
        /// The sunday
        /// </summary>
        Sunday = 0,

        /// <summary>
        /// The monday
        /// </summary>
        Monday = 1,

        /// <summary>
        /// The tuesday
        /// </summary>
        Tuesday = 2,

        /// <summary>
        /// The wednesday
        /// </summary>
        Wednesday = 3,

        /// <summary>
        /// The thursday
        /// </summary>
        Thursday = 4,

        /// <summary>
        /// The friday
        /// </summary>
        Friday = 5,

        /// <summary>
        /// The saturday
        /// </summary>
        Saturday = 6,

        /// <summary>
        /// The day
        /// </summary>
        Day = 7,

        /// <summary>
        /// The weekday
        /// </summary>
        Weekday = 8,

        /// <summary>
        /// The weekend day
        /// </summary>
        WeekendDay = 9,
    }
}
