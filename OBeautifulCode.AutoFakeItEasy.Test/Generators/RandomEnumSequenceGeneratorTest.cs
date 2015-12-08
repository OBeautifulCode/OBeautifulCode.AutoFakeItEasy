﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomEnumSequenceGeneratorTest.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace OBeautifulCode.AutoFakeItEasy.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FakeItEasy;

    using FluentAssertions;

    using Ploeh.AutoFixture.Kernel;

    using Xunit;

    /// <summary>
    /// Tests the <see cref="RandomEnumSequenceGenerator"/> class.
    /// </summary>
    public static class RandomEnumSequenceGeneratorTest
    {
        [Fact]
        public static void Constructor___When_object_is_constructed__Then_resulting_object_is_assignable_to_ISpecimenBuilder()
        {
            // Arrange, Act
            var systemUnderTest = new RandomEnumSequenceGenerator();

            // Assert
            systemUnderTest.Should().BeAssignableTo<ISpecimenBuilder>();
        }

        [Fact]
        public static void Create___When_called_with_null_request___Then_method_returns_object_equal_to_NoSpecimen()
        {
            // Arrange
            var dummyContainer = new DummySpecimenContext();
            var systemUnderTest = new RandomEnumSequenceGenerator();
            var expectedResult = new NoSpecimen();

            // Act
            var actualResult = systemUnderTest.Create(null, dummyContainer);

            // Assert
            actualResult.Should().Be(expectedResult);
        }

        [Fact]
        public static void Create___When_called_with_null_container___Then_method_does_not_throw()
        {
            // Arrange
            var systemUnderTest = new RandomEnumSequenceGenerator();
            var dummyRequest = new object();

            // Act
            var ex = Record.Exception(() => systemUnderTest.Create(dummyRequest, null));

            // Assert
            ex.Should().BeNull();
        }

        [Fact]
        public static void Create___When_request_is_not_an_enum___Then_method_returns_object_equal_to_NoSpecimen()
        {
            // Arrange
            var dummyContainer = new DummySpecimenContext();
            var systemUnderTest = new RandomEnumSequenceGenerator();
            var expectedResult = new NoSpecimen();
            var request = new object();

            // Act
            var actualResult = systemUnderTest.Create(request, dummyContainer);

            // Assert
            actualResult.Should().Be(expectedResult);
        }

        [Fact]
        public static void Create___When_request_is_for_an_enum___Then_method_returns_result_of_same_enum_type_as_request()
        {
            // Arrange
            var dummyContainer = new DummySpecimenContext();
            var systemUnderTest = new RandomEnumSequenceGenerator();
            var request = typeof(Number);

            // Act
            var actualResult = systemUnderTest.Create(request, dummyContainer);

            // Assert
            actualResult.Should().BeOfType<Number>();
        }

        [Fact]
        public static void Create___When_multiple_requests_are_made_for_enum___Then_method_returns_random_enum_values()
        {
            // Arrange
            var dummyContainer = new DummySpecimenContext();
            var systemUnderTest = new RandomEnumSequenceGenerator();
            var request = typeof(Number);
            var enumValuesCount = Enum.GetValues(typeof(Number)).Length;

            // Act
            var actualResult = Enumerable.Range(1, enumValuesCount).Select(_ => systemUnderTest.Create(request, dummyContainer)).Cast<Number>().ToList();

            // Assert
            actualResult.Should().NotBeAscendingInOrder();
        }
    }
}

// ReSharper restore InconsistentNaming
// ReSharper restore CheckNamespace