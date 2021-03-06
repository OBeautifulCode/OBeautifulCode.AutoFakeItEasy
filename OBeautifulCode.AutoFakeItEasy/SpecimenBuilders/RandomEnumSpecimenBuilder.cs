﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomEnumSpecimenBuilder.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.AutoFakeItEasy
{
    using System;
    using System.Linq;

    using AutoFixture.Kernel;

    using OBeautifulCode.Enum.Recipes;
    using OBeautifulCode.Math.Recipes;

    /// <summary>
    /// Generates random enum values.
    /// </summary>
    /// <remarks>
    /// Adapted from <a href="https://github.com/AutoFixture/AutoFixture/blob/master/Src/AutoFixture/EnumGenerator.cs"/>.
    /// </remarks>
    public class RandomEnumSpecimenBuilder : ISpecimenBuilder
    {
        /// <summary>
        /// Creates a new, random enum value based on a request.
        /// </summary>
        /// <param name="request">The request that describes what to create.</param>
        /// <param name="context">A context that can be used to create other specimens. Not used.</param>
        /// <returns>
        /// An enum value if appropriate; otherwise a <see cref="NoSpecimen"/> instance.
        /// </returns>
        public object Create(
            object request,
            ISpecimenContext context)
        {
            // can I handle this request?
            var t = request as Type;
            if ((t == null) || !t.IsEnum)
            {
                return new NoSpecimen();
            }

            var enumValues = t.GetAllPossibleEnumValues().ToList();

            var randomIndex = ThreadSafeRandom.Next(0, enumValues.Count);

            var result = enumValues[randomIndex];

            return result;
        }
    }
}