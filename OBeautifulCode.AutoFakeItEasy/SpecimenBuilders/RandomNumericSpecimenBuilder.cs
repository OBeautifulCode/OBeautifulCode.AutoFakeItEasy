﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomNumericSpecimenBuilder.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.AutoFakeItEasy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AutoFixture.Kernel;

    using OBeautifulCode.Math.Recipes;

    /// <summary>
    /// Creates random, unique numbers.
    /// </summary>
    public class RandomNumericSpecimenBuilder : ISpecimenBuilder
    {
        private readonly long inclusiveLowerLimit;
        private readonly long exclusiveUpperLimit;

        private HashSet<long> numbersUsed = new HashSet<long>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomNumericSpecimenBuilder" /> class.
        /// </summary>
        /// <param name="inclusiveLowerLimit">The lower limit.</param>
        /// <param name="exclusiveUpperLimit">The upper limit.</param>
        public RandomNumericSpecimenBuilder(
            long inclusiveLowerLimit,
            long exclusiveUpperLimit)
        {
            if (inclusiveLowerLimit >= exclusiveUpperLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(inclusiveLowerLimit), "inclusive lower limit is greater than or equal to exclusive upper limit");
            }

            this.inclusiveLowerLimit = inclusiveLowerLimit;
            this.exclusiveUpperLimit = exclusiveUpperLimit;
        }

        /// <summary>
        /// Creates a random number.
        /// </summary>
        /// <param name="request">The request that describes what to create.</param>
        /// <param name="context">A context that can be used to create other specimens.</param>
        /// <returns>
        /// The next random number in a sequence, if <paramref name="request"/> is a request
        /// for a numeric value; otherwise, a <see cref="NoSpecimen"/> instance.
        /// </returns>
        public object Create(
            object request,
            ISpecimenContext context)
        {
            var type = request as Type;
            if (type == null)
            {
                return new NoSpecimen();
            }

            var result = this.CreateRandom(type);

            return result;
        }

        private static long RandomLong(
            long inclusiveMin,
            long exclusiveMax)
        {
            var buffer = new byte[8];
            ThreadSafeRandom.NextBytes(buffer);
            var longRand = BitConverter.ToInt64(buffer, 0);

            var result = Math.Abs(longRand % (exclusiveMax - inclusiveMin)) + inclusiveMin;

            return result;
        }

        private object CreateRandom(
            Type request)
        {
            switch (Type.GetTypeCode(request))
            {
                case TypeCode.Byte:
                    return (byte)this.GetNextRandom();
                case TypeCode.Decimal:
                    return (decimal)this.GetNextRandom();
                case TypeCode.Double:
                    return (double)this.GetNextRandom();
                case TypeCode.Int16:
                    return (short)this.GetNextRandom();
                case TypeCode.Int32:
                    return (int)this.GetNextRandom();
                case TypeCode.Int64:
                    return this.GetNextRandom();
                case TypeCode.SByte:
                    return (sbyte)this.GetNextRandom();
                case TypeCode.Single:
                    return (float)this.GetNextRandom();
                case TypeCode.UInt16:
                    return (ushort)this.GetNextRandom();
                case TypeCode.UInt32:
                    return (uint)this.GetNextRandom();
                case TypeCode.UInt64:
                    return (ulong)this.GetNextRandom();
                default:
                    return new NoSpecimen();
            }
        }

        private long GetNextRandom()
        {
            long result;

            do
            {
                result = RandomLong(this.inclusiveLowerLimit, this.exclusiveUpperLimit);
            }
            while (this.numbersUsed.Contains(result));

            this.numbersUsed.Add(result);

            if (this.numbersUsed.LongCount() == (this.exclusiveUpperLimit - this.inclusiveLowerLimit))
            {
                this.numbersUsed = new HashSet<long>();
            }

            return result;
        }
    }
}