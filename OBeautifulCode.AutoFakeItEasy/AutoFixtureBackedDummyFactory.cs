﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoFixtureBackedDummyFactory.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.AutoFakeItEasy
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using AutoFixture;
    using AutoFixture.Kernel;

    using FakeItEasy;

    using OBeautifulCode.DateTime.Recipes;
    using OBeautifulCode.Math.Recipes;
    using OBeautifulCode.Reflection.Recipes;

    /// <summary>
    /// A dummy factory backed by AutoFixture.
    /// </summary>
    public class AutoFixtureBackedDummyFactory : IDummyFactory
    {
        private static readonly Fixture FixtureWithCreators = new Fixture();

        private static readonly Fixture FixtureWithoutCreators = new Fixture();

        private static readonly object FixtureLock = new object();

        private static readonly ConcurrentDictionary<Type, object> CachedRegisteredTypes = new ConcurrentDictionary<Type, object>();

        private static readonly MethodInfo AutoFixtureCreateMethod =
            typeof(SpecimenFactory)
            .GetMethods()
            .Single(_ => (_.Name == nameof(SpecimenFactory.Create)) && (_.GetParameters().Length == 1) && (_.GetParameters().Single().ParameterType == typeof(ISpecimenBuilder)));

        private static readonly Type[] SupportedUnregisteredInterfaces = { typeof(IEnumerable<>), typeof(IList<>), typeof(ICollection<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>), typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>) };

        private static readonly ConcurrentDictionary<Type, MethodInfo> CachedTypeToAutoFixtureCreateMethodMap = new ConcurrentDictionary<Type, MethodInfo>();

        private static readonly ConcurrentDictionary<Assembly, IReadOnlyCollection<Type>> CachedAssemblyToTypesMap = new ConcurrentDictionary<Assembly, IReadOnlyCollection<Type>>();

        /// <summary>
        /// Initializes static members of the <see cref="AutoFixtureBackedDummyFactory"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "These cannot be in-lined.")]
        static AutoFixtureBackedDummyFactory()
        {
            ConfigureRecursionBehavior(FixtureWithCreators);
            ConfigureRecursionBehavior(FixtureWithoutCreators);

            AddCustomizations(FixtureWithCreators);
            AddCustomizations(FixtureWithoutCreators);

            RegisterCustomTypes(FixtureWithCreators);

            RegisterSystemTypes(FixtureWithCreators);
        }

        /// <inheritdoc />
        public Priority Priority => FakeItEasy.Priority.Default;

        /// <summary>
        /// Loads this factory in the app domain, which makes it
        /// visible to FakeItEasy's extension point scanner.
        /// </summary>
        public static void LoadInAppDomain()
        {
        }

        /// <summary>
        /// Registers a function for creating a dummy of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the dummy to create.</typeparam>
        /// <param name="dummyCreatorFunc">The function to call to create the dummy.</param>
        /// <remarks>
        /// If this method is called multiple times for the same type,
        /// the most recently added creator will be used.
        /// </remarks>
        public static void AddDummyCreator<T>(
            Func<T> dummyCreatorFunc)
        {
            AddDummyCreator(FixtureWithCreators, dummyCreatorFunc);
        }

        /// <summary>
        /// Constrain dummies of the specified type to always be in a specified set of dummies.
        /// </summary>
        /// <typeparam name="T">The type of the dummy to create.</typeparam>
        /// <param name="possibleDummies">The dummies that can be returned.</param>
        public static void ConstrainDummyToBeOneOf<T>(
            params T[] possibleDummies)
        {
            ConstrainDummyToBeOneOf<T>(possibleDummies, null);
        }

        /// <summary>
        /// Constrain dummies of the specified type to always be in a specified set of dummies.
        /// </summary>
        /// <typeparam name="T">The type of the dummy to create.</typeparam>
        /// <param name="possibleDummies">The dummies that can be returned.</param>
        /// <param name="comparer">An equality comparer to use when comparing constructed dummies against the dummies that can be returned.</param>
        public static void ConstrainDummyToBeOneOf<T>(
            IEnumerable<T> possibleDummies,
            IEqualityComparer<T> comparer = null)
        {
            T CreatorFunc()
            {
                // ReSharper disable PossibleMultipleEnumeration
                T result = comparer == null
                    ? ((T)CreateType(FixtureWithoutCreators, typeof(T))).ThatIsIn(possibleDummies)
                    : ((T)CreateType(FixtureWithoutCreators, typeof(T))).ThatIsIn(possibleDummies, comparer);
                return result;

                // ReSharper restore PossibleMultipleEnumeration
            }

            AddDummyCreator(CreatorFunc);
        }

        /// <summary>
        /// Constrain dummies of the specified type to never be in a specified set of dummies.
        /// </summary>
        /// <typeparam name="T">The type of the dummy to create.</typeparam>
        /// <param name="possibleDummies">The dummies that cannot be returned.</param>
        public static void ConstrainDummyToExclude<T>(
            params T[] possibleDummies)
        {
            ConstrainDummyToExclude<T>(possibleDummies, null);
        }

        /// <summary>
        /// Constrain dummies of the specified type to never be in a specified set of dummies.
        /// </summary>
        /// <typeparam name="T">The type of the dummy to create.</typeparam>
        /// <param name="possibleDummies">The dummies that cannot be returned.</param>
        /// <param name="comparer">An equality comparer to use when comparing constructed dummies against the dummies to exclude.</param>
        public static void ConstrainDummyToExclude<T>(
            IEnumerable<T> possibleDummies,
            IEqualityComparer<T> comparer = null)
        {
            T CreatorFunc()
            {
                // ReSharper disable PossibleMultipleEnumeration
                T result = comparer == null
                    ? ((T)CreateType(FixtureWithoutCreators, typeof(T))).ThatIsNotIn(possibleDummies)
                    : ((T)CreateType(FixtureWithoutCreators, typeof(T))).ThatIsNotIn(possibleDummies, comparer);
                return result;

                // ReSharper restore PossibleMultipleEnumeration
            }

            AddDummyCreator(CreatorFunc);
        }

        /// <summary>
        /// Instructs the dummy factory to use a random, concrete subclass
        /// of the specified type when making dummies of that type.
        /// </summary>
        /// <typeparam name="T">The reference type.</typeparam>
        /// <param name="typesToExclude">The subclass types to exclude; include all other concrete subclasses.</param>
        /// <param name="typesToInclude">The subclass types include; only choose between these concrete subclasses.</param>
        /// <exception cref="ArgumentException">Only one of <paramref name="typesToExclude"/> and <paramref name="typesToInclude"/> can be specified.  Both are not null.</exception>
        /// <exception cref="ArgumentException"><paramref name="typesToExclude"/> is not null, but empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="typesToInclude"/> is not null, but empty.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In this case we just need the type, not a parameter of that type.")]
        public static void UseRandomConcreteSubclassForDummy<T>(
            IReadOnlyCollection<Type> typesToExclude = null,
            IReadOnlyCollection<Type> typesToInclude = null)
        {
            if ((typesToExclude != null) && (typesToInclude != null))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Only one of {0} and {1} can be specified.  Both are not null.", nameof(typesToExclude), nameof(typesToInclude)));
            }

            // ReSharper disable once PossibleMultipleEnumeration
            if ((typesToExclude != null) && !typesToExclude.Any())
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is not null, but empty.", nameof(typesToExclude)));
            }

            // ReSharper disable once PossibleMultipleEnumeration
            if ((typesToInclude != null) && !typesToInclude.Any())
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} is not null, but empty.", nameof(typesToInclude)));
            }

            var type = typeof(T);

            CacheLoadedAssemblyTypes();
            CacheAssemblyTypes(type.Assembly);
            CacheAssemblyTypes(typesToInclude);
            CacheAssemblyTypes(typesToExclude);

            var concreteSubclasses = CachedAssemblyToTypesMap
                .SelectMany(_ => _.Value)
                .Where(_ => _.IsSubclassOf(type))
                .Where(_ => !_.IsAbstract)
                .Where(CanCreateType)
                .Where(_ =>
                {
                    if (typesToExclude != null)
                    {
                        // ReSharper disable once PossibleMultipleEnumeration
                        return !typesToExclude.Contains(_);
                    }

                    if (typesToInclude != null)
                    {
                        // ReSharper disable once PossibleMultipleEnumeration
                        return typesToInclude.Contains(_);
                    }

                    return true;
                })
                .ToList();

            if (concreteSubclasses.Count == 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "There are no concrete subclasses of {0} (after inclusions and exclusions are applied)", type.Name));
            }

            T RandomSubclassDummyCreator()
            {
                // get random subclass
                var randomIndex = ThreadSafeRandom.Next(0, concreteSubclasses.Count);
                var randomSubclass = concreteSubclasses[randomIndex];

                // create the subclass
                object result = AD.ummy(randomSubclass);
                return (T)result;
            }

            AddDummyCreator(RandomSubclassDummyCreator);
        }

        /// <summary>
        /// Instructs the dummy factory to use a random implementation
        /// of the specified interface when making dummies of that interface type.
        /// </summary>
        /// <param name="includeOtherInterfaces">
        /// Determines whether to include interfaces that implement <typeparamref name="T"/>
        /// when selecting a random implementation of <typeparamref name="T"/>.
        /// Default is false; interfaces that implement <typeparamref name="T"/> will be excluded.
        /// </param>
        /// <typeparam name="T">The interface type.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In this case we just need the type, not a parameter of that type.")]
        public static void UseRandomInterfaceImplementationForDummy<T>(
            bool includeOtherInterfaces = false)
        {
            var type = typeof(T);

            CacheLoadedAssemblyTypes();
            CacheAssemblyTypes(type.Assembly);

            var interfaceImplementations = CachedAssemblyToTypesMap
                .SelectMany(_ => _.Value)
                .Where(t => type != t)
                .Where(t => type.IsAssignableFrom(t))
                .Where(t => includeOtherInterfaces || !t.IsInterface)
                .ToList();

            if (interfaceImplementations.Count == 0)
            {
                throw new ArgumentException("There are no implementations of the interface " + type.Name);
            }

            T RandomInterfaceImplementationDummyGenerator()
            {
                // get random implementation
                var randomIndex = ThreadSafeRandom.Next(0, interfaceImplementations.Count);
                var randomImplementation = interfaceImplementations[randomIndex];

                // call the FakeItEasy A.Dummy method to create that implementation
                object result = AD.ummy(randomImplementation);
                return (T)result;
            }

            AddDummyCreator(RandomInterfaceImplementationDummyGenerator);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "factory caller will ensure this is not null")]
        public bool CanCreate(
            Type type)
        {
            return CanCreateType(type);
        }

        /// <inheritdoc />
        public object Create(
            Type type)
        {
            var result = CreateType(FixtureWithCreators, type);

            return result;
        }

        private static void CacheLoadedAssemblyTypes()
        {
            try
            {
                var assemblies = AssemblyLoader.GetLoadedAssemblies();

                foreach (var assembly in assemblies)
                {
                    CacheAssemblyTypes(assembly);
                }
            }
            catch (Exception)
            {
            }
        }

        private static void CacheAssemblyTypes(
            IReadOnlyCollection<Type> types)
        {
            if (types != null)
            {
                foreach (var type in types)
                {
                    if (type != null)
                    {
                        CacheAssemblyTypes(type.Assembly);
                    }
                }
            }
        }

        private static void CacheAssemblyTypes(
            Assembly assembly)
        {
            if (!CachedAssemblyToTypesMap.ContainsKey(assembly))
            {
                try
                {
                    var types = new[] { assembly }.GetTypesFromAssemblies();

                    CachedAssemblyToTypesMap.TryAdd(assembly, types);
                }
                catch (Exception)
                {
                }
            }
        }

        private static void ConfigureRecursionBehavior(
            Fixture fixture)
        {
            // It's not AutoFixture's job to detect recursion.  Infinite recursion will cause the process to blow-up;
            // it's typically a behavior that's easy to observe/detect.  By allowing recursion we enable a swath
            // of legitimate scenarios (e.g. a tree).
            var throwingRecursionBehaviors = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList();
            foreach (var throwingRecursionBehavior in throwingRecursionBehaviors)
            {
                fixture.Behaviors.Remove(throwingRecursionBehavior);
            }
        }

        private static void AddCustomizations(
            Fixture fixture)
        {
            // fix some of AutoFixture's poor defaults - see README.md

            // this will generate numbers in the range [-32768,32768]
            fixture.Customizations.Insert(0, new RandomNumericSpecimenBuilder(short.MinValue, short.MaxValue + 2));
            fixture.Customizations.Insert(0, new RandomBoolSpecimenBuilder());
            fixture.Customizations.Insert(0, new RandomEnumSpecimenBuilder());
            fixture.Customizations.Insert(0, new ConcurrentCollectionSpecimenBuilder());
        }

        private static void RegisterCustomTypes(
            Fixture fixture)
        {
            AddDummyCreator(fixture, () => new PositiveInteger(Math.Abs(A.Dummy<int>().ThatIsNot(0))));
            AddDummyCreator(fixture, () => new NegativeInteger(-1 * Math.Abs(A.Dummy<int>().ThatIsNot(0))));
            AddDummyCreator(fixture, () => new ZeroOrPositiveInteger(Math.Abs(A.Dummy<int>())));
            AddDummyCreator(fixture, () => new ZeroOrNegativeInteger(-1 * Math.Abs(A.Dummy<int>())));

            AddDummyCreator(fixture, () => new PositiveDouble(Math.Abs(A.Dummy<double>().ThatIsNot(0))));
            AddDummyCreator(fixture, () => new NegativeDouble(-1d * Math.Abs(A.Dummy<double>().ThatIsNot(0))));
            AddDummyCreator(fixture, () => new ZeroOrPositiveDouble(Math.Abs(A.Dummy<double>())));
            AddDummyCreator(fixture, () => new ZeroOrNegativeDouble(-1 * Math.Abs(A.Dummy<double>())));

            AddDummyCreator(fixture, PercentChangeAsDouble.CreateConstrainedValue);
            AddDummyCreator(fixture, PercentChangeAsDecimal.CreateConstrainedValue);

            AddDummyCreator(fixture, () => new UtcDateTime(A.Dummy<DateTime>().ToUtc()));
        }

        private static void RegisterSystemTypes(
            Fixture fixture)
        {
            AddDummyCreator(fixture, () => new Version(A.Dummy<ZeroOrPositiveInteger>(), A.Dummy<ZeroOrPositiveInteger>(), A.Dummy<ZeroOrPositiveInteger>()));
        }

        private static void AddDummyCreator<T>(
            Fixture fixture,
            Func<T> dummyCreatorFunc)
        {
            lock (FixtureLock)
            {
                fixture.Register(dummyCreatorFunc);
            }

            var type = typeof(T);
            CachedRegisteredTypes.TryAdd(type, new object());
        }

        private static bool CanCreateType(
            Type type)
        {
            if (CachedRegisteredTypes.ContainsKey(type))
            {
                return true;
            }

            if (type.IsInterface)
            {
                return CanCreateUnregisteredInterface(type);
            }

            if (type.IsAbstract)
            {
                return false;
            }

            return true;
        }

        private static bool CanCreateUnregisteredInterface(
            Type type)
        {
            if (type.IsGenericType)
            {
                var genericInterfaceType = type.GetGenericTypeDefinition();

                if (SupportedUnregisteredInterfaces.Contains(genericInterfaceType))
                {
                    return true;
                }
            }

            return false;
        }

        private static object CreateType(
            Fixture fixture,
            Type type)
        {
            var autoFixtureGenericCreateMethod = CachedTypeToAutoFixtureCreateMethodMap.GetOrAdd(type, t => AutoFixtureCreateMethod.MakeGenericMethod(type));

            // Lock because Fixture is not thread-safe: https://github.com/AutoFixture/AutoFixture/issues/211
            lock (FixtureLock)
            {
                var result = autoFixtureGenericCreateMethod.Invoke(null, new object[] { fixture });

                return result;
            }
        }
    }
}
