﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstrainDummyToExclude.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.AutoFakeItEasy.Test
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class
    public enum MostlyGoodStuffWithoutComparer
    {
        WorkingFromHome,

        RainyDays,

        Chocolate,

        Vacation,

        Meditation,

        FoodPoisoning,

        Bulldogs,
    }

    public enum MostlyGoodStuffWithComparer
    {
        WorkingFromHome,

        RainyDays,

        Chocolate,

        Vacation,

        Meditation,

        FoodPoisoning,

        Bulldogs,
    }

    public enum MostlyGoodStuffWithoutComparerReestablished
    {
        WorkingFromHome,

        RainyDays,

        Chocolate,

        Vacation,

        Meditation,

        FoodPoisoning,

        Bulldogs,
    }

    public enum MostlyGoodStuffWithComparerReestablished
    {
        WorkingFromHome,

        RainyDays,

        Chocolate,

        Vacation,

        Meditation,

        FoodPoisoning,

        Bulldogs,
    }

    public enum MostlyGoodStuffWithoutComparerIndirect
    {
        WorkingFromHome,

        RainyDays,

        Chocolate,

        Vacation,

        Meditation,

        FoodPoisoning,

        Bulldogs,
    }

    public enum MostlyGoodStuffWithComparerIndirect
    {
        WorkingFromHome,

        RainyDays,

        Chocolate,

        Vacation,

        Meditation,

        FoodPoisoning,

        Bulldogs,
    }

    public enum MostlyGoodStuffInGenericInterface
    {
        WorkingFromHome,

        RainyDays,

        Chocolate,

        Vacation,

        Meditation,

        FoodPoisoning,

        Bulldogs,
    }

    public enum MostlyGoodStuffInGenericInterfaceIndirect
    {
        WorkingFromHome,

        RainyDays,

        Chocolate,

        Vacation,

        Meditation,

        FoodPoisoning,

        Bulldogs,
    }

    public class ConstrainDummiesToExcludeIndirect
    {
        public string SomeProperty { get; set; }

        public MostlyGoodStuffWithoutComparerIndirect MostlyGoodStuffWithoutComparerIndirect { get; set; }

        public MostlyGoodStuffWithComparerIndirect MostlyGoodStuffWithComparerIndirect { get; set; }
    }

    public class SupportedButUnregisteredGenericInterfaceIndirect
    {
        public string SomeProperty { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "For testing purposes.")]
        public IEnumerable<MostlyGoodStuffInGenericInterfaceIndirect> Enumerable { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "For testing purposes.")]
        public ICollection<MostlyGoodStuffInGenericInterfaceIndirect> Collection { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "For testing purposes.")]
        public IList<MostlyGoodStuffInGenericInterfaceIndirect> List { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "For testing purposes.")]
        public IReadOnlyCollection<MostlyGoodStuffInGenericInterfaceIndirect> ReadOnlyCollection { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "For testing purposes.")]
        public IReadOnlyList<MostlyGoodStuffInGenericInterfaceIndirect> ReadOnlyList { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "For testing purposes.")]
        public IDictionary<string, MostlyGoodStuffInGenericInterfaceIndirect> Dictionary { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "For testing purposes.")]
        public IReadOnlyDictionary<string, MostlyGoodStuffInGenericInterfaceIndirect> ReadOnlyDictionary { get; set; }
    }

#pragma warning restore SA1402 // File may only contain a single class
#pragma warning restore SA1649 // File name must match first type name
}