// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     BaseArchitectureTest.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Notes.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Notes.Tests.Architecture;

/// <summary>
/// Base class for architecture tests providing common assemblies.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class BaseArchitectureTest
{
	/// <summary>
	/// Gets the Notes.Web assembly using the marker interface.
	/// </summary>
	protected static Assembly WebAssembly => typeof(global::Web.IAppMarker).Assembly;

	/// <summary>
	/// Gets the Shared assembly.
	/// </summary>
	protected static Assembly SharedAssembly => typeof(Shared.Entities.Note).Assembly;

	/// <summary>
	/// Gets the Notes.ServiceDefaults assembly.
	/// </summary>
	protected static Assembly ServiceDefaultsAssembly => typeof(Microsoft.Extensions.Hosting.Extensions).Assembly;

	/// <summary>
	/// Gets all application assemblies to test.
	/// </summary>
	protected static Assembly[] ApplicationAssemblies =>
	[
		WebAssembly,
		SharedAssembly,
		ServiceDefaultsAssembly
	];
}
