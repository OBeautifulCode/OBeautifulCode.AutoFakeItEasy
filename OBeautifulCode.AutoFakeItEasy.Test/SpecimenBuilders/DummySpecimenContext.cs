﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummySpecimenContext.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.AutoFakeItEasy.Test
{
    using System;

    using AutoFixture.Kernel;

    internal class DummySpecimenContext : ISpecimenContext
    {
        public DummySpecimenContext()
        {
            this.OnResolve = r => null;
        }

        internal Func<object, object> OnResolve { get; set; }

        public object Resolve(object request)
        {
            return this.OnResolve(request);
        }
    }
}