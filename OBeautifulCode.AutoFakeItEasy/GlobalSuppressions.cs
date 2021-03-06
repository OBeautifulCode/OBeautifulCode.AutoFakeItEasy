// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Code Analysis results, point to "Suppress Message", and click
// "In Suppression File".
// You do not need to add suppressions to this file manually.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope = "member", Target = "OBeautifulCode.AutoFakeItEasy.ConstrainedValue`1.#op_Implicit(OBeautifulCode.AutoFakeItEasy.ConstrainedValue`1<!0>):!0", Justification = "See https://stackoverflow.com/questions/36840841/how-to-fix-ca2225-operatoroverloadshavenamedalternates-when-using-generic-clas")]