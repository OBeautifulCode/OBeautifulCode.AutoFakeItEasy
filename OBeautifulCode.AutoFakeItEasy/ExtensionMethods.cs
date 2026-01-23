// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.AutoFakeItEasy
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using FakeItEasy;
    using OBeautifulCode.Equality.Recipes;
    using OBeautifulCode.Math.Recipes;
    using OBeautifulCode.Type.Recipes;
    using static System.FormattableString;

    /// <summary>
    /// Some extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        private const int MaxAttempts = 1000;

        /// <summary>
        /// Returns a reference dummy if it meets a specified condition or
        /// creates new dummies of the same type until the condition is met.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="condition">A function to test the reference dummy against a condition.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that meets the condition, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy
        /// because the condition fails on the reference dummy, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to meet the condition.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>Returns the reference dummy if it meets the specified condition.  Otherwise, returns a new dummy that meets the condition.</returns>
        public static T ThatIs<T>(
            this T referenceDummy,
            Func<T, bool> condition,
            int maxAttempts = MaxAttempts)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var referenceDummyType = referenceDummy?.GetType();

            string someDummiesCallName = null;

            if ((referenceDummyType != null) && referenceDummyType.IsGenericType)
            {
                var genericTypeDefinition = referenceDummyType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(SomeDummiesList<>))
                {
                    someDummiesCallName = nameof(Some.Dummies);
                }
                else if (genericTypeDefinition == typeof(SomeReadOnlyDummiesList<>))
                {
                    someDummiesCallName = nameof(Some.ReadOnlyDummies);
                }
            }

            var attempts = 1;

            while (!condition(referenceDummy))
            {
                if (attempts == maxAttempts)
                {
                    throw new InvalidOperationException(Invariant($"Unable to create a dummy '{referenceDummyType?.ToStringReadable()}' that satisfies the specified condition."));
                }

                if (someDummiesCallName != null)
                {
                    var someDummiesMethod = typeof(Some).GetMethods().Single(_ => _.Name == someDummiesCallName);
                    var typeOfElementsInList = referenceDummyType.GetGenericArguments().Single();
                    var someDummiesGenericMethod = someDummiesMethod.MakeGenericMethod(typeOfElementsInList);

                    // ReSharper disable once PossibleNullReferenceException
                    var numberOfElements = referenceDummyType.GetProperty(nameof(ISomeDummies.NumberOfElementsSpecifiedInCallToSomeDummies)).GetValue(referenceDummy);

                    // ReSharper disable once PossibleNullReferenceException
                    var createWith = referenceDummyType.GetProperty(nameof(ISomeDummies.CreateWithSpecifiedInCallToSomeDummies)).GetValue(referenceDummy);
                    referenceDummy = (T)someDummiesGenericMethod.Invoke(null, new[] { numberOfElements, createWith });
                }
                else
                {
                    referenceDummy = A.Dummy<T>();
                }

                attempts++;
            }

            return referenceDummy;
        }

        /// <summary>
        /// Returns a reference dummy if it meets a specified condition or
        /// creates new dummies of the same type until the condition is met.
        /// This method does the exact same thing as <see cref="ThatIs{T}(T, Func{T, bool}, int)"/>.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="condition">A function to test the reference dummy against a condition.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that meets the condition, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy
        /// because the condition fails on the reference dummy, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to meet the condition.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>Returns the reference dummy if it meets the specified condition.  Otherwise, returns a new dummy that meets the condition.</returns>
        public static T Whose<T>(
            this T referenceDummy,
            Func<T, bool> condition,
            int maxAttempts = MaxAttempts)
            => ThatIs(referenceDummy, condition, maxAttempts);

        /// <summary>
        /// Returns a reference dummy if it is not equal to a comparison dummy or
        /// creates new dummies of the same type until a dummy is created that does not equal the comparison dummy.
        /// Uses <see cref="EqualityExtensions.IsEqualTo{T}(T, T, IEqualityComparer{T})"/>.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="comparisonDummy">A dummy to compare against.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is not equal to the comparison dummy, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the comparision dummy is equal to the reference dummy, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is not equal to the comparison dummy.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is not equal to the comparison dummy.
        /// Otherwise, returns a new dummy that that is not equal to the comparison dummy.
        /// </returns>
        public static T ThatIsNot<T>(
            this T referenceDummy,
            T comparisonDummy,
            int maxAttempts = MaxAttempts)
        {
            bool Condition(T t) => !t.IsEqualTo(comparisonDummy);

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference dummy if it is not in a set of comparison dummies or
        /// creates new dummies of the same type until a dummy is created that is not in the set of comparison dummies.
        /// Uses <see cref="EqualityComparerHelper.GetEqualityComparerToUse{T}(IEqualityComparer{T})"/>.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="comparisonDummies">The set of comparison dummies.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is not in the set of comparison dummy, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the comparision dummies contain the reference dummy, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is not in the set of comparison dummies.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is not in the set of comparison dummies.
        /// Otherwise, returns a new dummy that that is not in the set of comparison dummies.
        /// </returns>
        public static T ThatIsNotIn<T>(
            this T referenceDummy,
            IEnumerable<T> comparisonDummies,
            int maxAttempts = MaxAttempts)
        {
            comparisonDummies = comparisonDummies?.ToList() ?? throw new ArgumentNullException(nameof(comparisonDummies));

            bool Condition(T t) => !comparisonDummies.Contains(t, EqualityComparerHelper.GetEqualityComparerToUse<T>());

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference dummy if it is not in a set of comparison dummies or
        /// creates new dummies of the same type until a dummy is created that is not in the set of comparison dummies.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="comparisonDummies">The set of comparison dummies.</param>
        /// <param name="comparer">An equality comparer to use when comparing the reference dummy against the comparison dummies.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is not in the set of comparison dummy, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the comparision dummies contain the reference dummy, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is not in the set of comparison dummies.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is not in the set of comparison dummies.
        /// Otherwise, returns a new dummy that that is not in the set of comparison dummies.
        /// </returns>
        public static T ThatIsNotIn<T>(
            this T referenceDummy,
            IEnumerable<T> comparisonDummies,
            IEqualityComparer<T> comparer,
            int maxAttempts = MaxAttempts)
        {
            comparisonDummies = comparisonDummies?.ToList() ?? throw new ArgumentNullException(nameof(comparisonDummies));

            bool Condition(T t) => !comparisonDummies.Contains(t, comparer);

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference dummy if it is in a set of comparison dummies or
        /// creates new dummies of the same type until a dummy is created that is in the set of comparison dummies.
        /// Uses <see cref="EqualityComparerHelper.GetEqualityComparerToUse{T}(IEqualityComparer{T})"/>.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="comparisonDummies">The set of comparison dummies.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is in the set of comparison dummy, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the comparision dummies contain the reference dummy, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is in the set of comparison dummies.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is in the set of comparison dummies.
        /// Otherwise, returns a new dummy that that is in the set of comparison dummies.
        /// </returns>
        public static T ThatIsIn<T>(
            this T referenceDummy,
            IEnumerable<T> comparisonDummies,
            int maxAttempts = MaxAttempts)
        {
            comparisonDummies = comparisonDummies?.ToList() ?? throw new ArgumentNullException(nameof(comparisonDummies));

            bool Condition(T t) => comparisonDummies.Contains(t, EqualityComparerHelper.GetEqualityComparerToUse<T>());

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference dummy if it is in a set of comparison dummies or
        /// creates new dummies of the same type until a dummy is created that is in the set of comparison dummies.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="comparisonDummies">The set of comparison dummies.</param>
        /// <param name="comparer">An equality comparer to use when comparing the reference dummy against the comparison dummies.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is in the set of comparison dummy, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the comparision dummies contain the reference dummy, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is in the set of comparison dummies.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is in the set of comparison dummies.
        /// Otherwise, returns a new dummy that that is in the set of comparison dummies.
        /// </returns>
        public static T ThatIsIn<T>(
            this T referenceDummy,
            IEnumerable<T> comparisonDummies,
            IEqualityComparer<T> comparer,
            int maxAttempts = MaxAttempts)
        {
            comparisonDummies = comparisonDummies?.ToList() ?? throw new ArgumentNullException(nameof(comparisonDummies));

            bool Condition(T t) => comparisonDummies.Contains(t, comparer);

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference dummy if it is contained within a specified range (inclusive of endpoints)
        /// or creates new dummies of the same type until a dummy is created that is within the specified range.
        /// Uses the comparison dummy's implementation of <see cref="IComparable.CompareTo(object)"/>.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="rangeStartInclusive">The start of the range.  The range is inclusive of this value.</param>
        /// <param name="rangeEndInclusive">The end of the range.  The range is inclusive of this value.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is in specified range, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the reference dummy is not within the specified range, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is in the specified range.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is in the specified range.
        /// Otherwise, returns a new dummy that is in the specified range.
        /// </returns>
        public static T ThatIsInRange<T>(
            this T referenceDummy,
            T rangeStartInclusive,
            T rangeEndInclusive,
            int maxAttempts = MaxAttempts)
            where T : IComparable<T>
        {
            if ((rangeStartInclusive != null) && (rangeEndInclusive != null))
            {
                if (rangeStartInclusive.CompareTo(rangeEndInclusive) > 0)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is > {1}", nameof(rangeStartInclusive), nameof(rangeEndInclusive)));
                }
            }

            if ((rangeStartInclusive != null) && (rangeEndInclusive == null))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is > {1}", nameof(rangeStartInclusive), nameof(rangeEndInclusive)));
            }

            bool Condition(T dummy) => dummy == null ? rangeStartInclusive == null : (dummy.CompareTo(rangeStartInclusive) >= 0) && (dummy.CompareTo(rangeEndInclusive) <= 0);

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference dummy if it is contained within a specified range (inclusive of endpoints)
        /// or creates new dummies of the same type until a dummy is created that is within the specified range.
        /// Uses the comparison dummy's implementation of <see cref="IComparable.CompareTo(object)"/>.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="rangeStartInclusive">The start of the range.  The range is inclusive of this value.</param>
        /// <param name="rangeEndInclusive">The end of the range.  The range is inclusive of this value.</param>
        /// <param name="comparer">The comparer to use when comparing dummies against the range.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is in specified range, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the reference dummy is not within the specified range, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is in the specified range.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is in the specified range.
        /// Otherwise, returns a new dummy that is in the specified range.
        /// </returns>
        public static T ThatIsInRange<T>(
            this T referenceDummy,
            T rangeStartInclusive,
            T rangeEndInclusive,
            IComparer<T> comparer,
            int maxAttempts = MaxAttempts)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            // not throwing ArgumentNullException if start or end of range is null
            // according to the documentation of IComparer<T>, null is comparable
            // https://msdn.microsoft.com/en-us/library/xh5ks3b3(v=vs.110).aspx
            if (comparer.Compare(rangeStartInclusive, rangeEndInclusive) > 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is > {1}", nameof(rangeStartInclusive), nameof(rangeEndInclusive)));
            }

            bool Condition(T dummy) => (comparer.Compare(dummy, rangeStartInclusive) >= 0) && (comparer.Compare(dummy, rangeEndInclusive) <= 0);

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference integer dummy if it is contained within a specified range (inclusive of endpoints)
        /// or creates a new integer that is within the specified range.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="rangeStartInclusive">The start of the range.  The range is inclusive of this value.</param>
        /// <param name="rangeEndInclusive">The end of the range.  The range is inclusive of this value.</param>
        /// <returns>
        /// Returns the reference dummy if is in the specified range.
        /// Otherwise, returns a new dummy that is in the specified range.
        /// </returns>
        public static int ThatIsInRange(
            this int referenceDummy,
            int rangeStartInclusive,
            int rangeEndInclusive)
        {
            if (rangeStartInclusive > rangeEndInclusive)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is > {1}", nameof(rangeStartInclusive), nameof(rangeEndInclusive)));
            }

            int result;

            if ((referenceDummy >= rangeStartInclusive) && (referenceDummy <= rangeEndInclusive))
            {
                result = referenceDummy;
            }
            else
            {
                if (rangeEndInclusive < int.MaxValue)
                {
                    result = ThreadSafeRandom.Next(rangeStartInclusive, rangeEndInclusive + 1);
                }
                else if (rangeStartInclusive > int.MinValue)
                {
                    result = ThreadSafeRandom.Next(rangeStartInclusive - 1, rangeEndInclusive) + 1;
                }
                else
                {
                    throw new InvalidOperationException("Logically impossible to get here");
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a reference dummy if it is not contained within a specified range (inclusive of endpoints)
        /// or creates new dummies of the same type until a dummy is created that is not within the specified range.
        /// Uses the comparison dummy's implementation of <see cref="IComparable.CompareTo(object)"/>.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="rangeStartInclusive">The start of the range.  The range is inclusive of this value.</param>
        /// <param name="rangeEndInclusive">The end of the range.  The range is inclusive of this value.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is not in specified range, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the reference dummy is within the specified range, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is not in the specified range.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is not in the specified range.
        /// Otherwise, returns a new dummy that is not in the specified range.
        /// </returns>
        public static T ThatIsNotInRange<T>(
            this T referenceDummy,
            T rangeStartInclusive,
            T rangeEndInclusive,
            int maxAttempts = MaxAttempts)
            where T : IComparable<T>
        {
            if ((rangeStartInclusive != null) && (rangeEndInclusive != null))
            {
                if (rangeStartInclusive.CompareTo(rangeEndInclusive) > 0)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is > {1}", nameof(rangeStartInclusive), nameof(rangeEndInclusive)));
                }
            }

            if ((rangeStartInclusive != null) && (rangeEndInclusive == null))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is > {1}", nameof(rangeStartInclusive), nameof(rangeEndInclusive)));
            }

            bool Condition(T dummy) => dummy == null ? rangeStartInclusive != null : (dummy.CompareTo(rangeStartInclusive) < 0) || (dummy.CompareTo(rangeEndInclusive) > 0);

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference dummy if it is not contained within a specified range (inclusive of endpoints)
        /// or creates new dummies of the same type until a dummy is created that is not within the specified range.
        /// Uses the comparison dummy's implementation of <see cref="IComparable.CompareTo(object)"/>.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="rangeStartInclusive">The start of the range.  The range is inclusive of this value.</param>
        /// <param name="rangeEndInclusive">The end of the range.  The range is inclusive of this value.</param>
        /// <param name="comparer">The comparer to use when comparing dummies against the range.</param>
        /// <param name="maxAttempts">
        /// The maximum number of times to attempt to create a dummy that is not in specified range, before throwing.
        /// The reference dummy is itself considered the first attempt.  If this method creates a new dummy because
        /// the reference dummy is within the specified range, that is considered the second attempt.
        /// If max attempts is zero or negative then the method tries an infinite number of times to create a dummy
        /// that is not in the specified range.
        /// </param>
        /// <typeparam name="T">The type of dummy.</typeparam>
        /// <returns>
        /// Returns the reference dummy if is not in the specified range.
        /// Otherwise, returns a new dummy that is not in the specified range.
        /// </returns>
        public static T ThatIsNotInRange<T>(
            this T referenceDummy,
            T rangeStartInclusive,
            T rangeEndInclusive,
            IComparer<T> comparer,
            int maxAttempts = MaxAttempts)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            // not throwing ArgumentNullException if start or end of range is null
            // according to the documentation of IComparer<T>, null is comparable
            // https://msdn.microsoft.com/en-us/library/xh5ks3b3(v=vs.110).aspx
            if (comparer.Compare(rangeStartInclusive, rangeEndInclusive) > 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is > {1}", nameof(rangeStartInclusive), nameof(rangeEndInclusive)));
            }

            bool Condition(T dummy) => (comparer.Compare(dummy, rangeStartInclusive) < 0) || (comparer.Compare(dummy, rangeEndInclusive) > 0);

            var result = ThatIs(referenceDummy, Condition, maxAttempts);

            return result;
        }

        /// <summary>
        /// Returns a reference integer dummy if it is not contained within a specified range (inclusive of endpoints)
        /// or creates a new integer that is not within the specified range.
        /// </summary>
        /// <param name="referenceDummy">The reference dummy.</param>
        /// <param name="rangeStartInclusive">The start of the range.  The range is inclusive of this value.</param>
        /// <param name="rangeEndInclusive">The end of the range.  The range is inclusive of this value.</param>
        /// <returns>
        /// Returns the reference dummy if is not in the specified range.
        /// Otherwise, returns a new dummy that is not in the specified range.
        /// </returns>
        public static int ThatIsNotInRange(
            this int referenceDummy,
            int rangeStartInclusive,
            int rangeEndInclusive)
        {
            if (rangeStartInclusive > rangeEndInclusive)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is > {1}", nameof(rangeStartInclusive), nameof(rangeEndInclusive)));
            }

            if ((rangeStartInclusive == int.MinValue) && (rangeEndInclusive == int.MaxValue))
            {
                throw new ArgumentException("All integers are within the specified range.");
            }

            int result;

            if ((referenceDummy < rangeStartInclusive) || (referenceDummy > rangeEndInclusive))
            {
                result = referenceDummy;
            }
            else
            {
                if (rangeStartInclusive == int.MinValue)
                {
                    result = ThreadSafeRandom.Next(rangeEndInclusive, int.MaxValue) + 1;
                }
                else if (rangeEndInclusive == int.MaxValue)
                {
                    result = ThreadSafeRandom.Next(int.MinValue, rangeStartInclusive);
                }
                else
                {
                    // random choose between left and right-hand side of number line
                    if (ThreadSafeRandom.Next(0, 2) == 0)
                    {
                        result = ThreadSafeRandom.Next(rangeEndInclusive, int.MaxValue) + 1;
                    }
                    else
                    {
                        result = ThreadSafeRandom.Next(int.MinValue, rangeStartInclusive);
                    }
                }
            }

            return result;
        }
    }
}
