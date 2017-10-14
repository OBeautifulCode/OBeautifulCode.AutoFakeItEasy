﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositiveDoubleTest.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace OBeautifulCode.AutoFakeItEasy.Test
{
    using System;

    using FluentAssertions;

    using OBeautifulCode.Math.Recipes;

    using Xunit;

    /// <summary>
    /// Tests the <see cref="PositiveDouble"/> class.
    /// </summary>
    public static class PositiveDoubleTest
    {
        [Fact]
        public static void Constructor___Should_throw_ArgumentOutOfRangeException___When_the_value_parameter_is_zero_or_negative()
        {
            // Arrange, Act
            var ex1 = Record.Exception(() => new PositiveDouble(0));
            var ex2 = Record.Exception(() => new PositiveDouble(-.0001));
            var ex3 = Record.Exception(() => new PositiveDouble(double.MinValue));
            var ex4 = Record.Exception(() => new PositiveDouble(-1d * Math.Abs(ThreadSafeRandom.NextDouble())));

            // Assert
            ex1.Should().BeOfType<ArgumentOutOfRangeException>();
            ex2.Should().BeOfType<ArgumentOutOfRangeException>();
            ex3.Should().BeOfType<ArgumentOutOfRangeException>();
            ex4.Should().BeOfType<ArgumentOutOfRangeException>();
        }

        [Fact]
        public static void Constructor___Should_not_throw___When_the_value_parameter_is_positive()
        {
            // Arrange, Act
            var ex1 = Record.Exception(() => new PositiveDouble(.00001));
            var ex2 = Record.Exception(() => new PositiveDouble(double.MaxValue));
            var ex3 = Record.Exception(() => new PositiveDouble(Math.Abs(ThreadSafeRandom.NextDouble())));

            // Assert
            ex1.Should().BeNull();
            ex2.Should().BeNull();
            ex3.Should().BeNull();
        }

        [Fact]
        public static void Value___Should_return_the_same_value_passed_to_constructor___When_getting()
        {
            // Arrange
            var expectedDouble = Math.Abs(ThreadSafeRandom.NextDouble());
            var systemUnderTest = new PositiveDouble(expectedDouble);

            // Act
            var actualInt = systemUnderTest.Value;

            // Assert
            actualInt.Should().Be(expectedDouble);
        }

        [Fact]
        public static void Cast___Should_return_the_same_value_passed_to_constructor___When_casting_to_double()
        {
            // Arrange
            var expectedInt = Math.Abs(ThreadSafeRandom.NextDouble());
            var systemUnderTest = new PositiveDouble(expectedInt);

            // Act
            var actualInt = (double)systemUnderTest;

            // Assert
            actualInt.Should().Be(expectedInt);
        }

        [Fact]
        public static void Implicit_conversion___Should_return_same_double_value_passed_to_constructor___When_converting_to_object_of_type_double()
        {
            // Arrange
            var expectedInt = Math.Abs(ThreadSafeRandom.NextDouble());
            var systemUnderTest = new PositiveDouble(expectedInt);

            // Act
            double actualInt = systemUnderTest;

            // Assert
            actualInt.Should().Be(expectedInt);
        }
    }
}

// ReSharper restore InconsistentNaming
// ReSharper restore CheckNamespace