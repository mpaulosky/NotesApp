// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Result.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

using Shared.Entities;

namespace Shared.Abstractions;

/// <summary>
///   Represents the result of an operation that can succeed or fail.
/// </summary>
public class Result
{
	/// <summary>
	///   Initializes a new instance of the <see cref="Result"/> class.
	/// </summary>
	/// <param name="success">Indicates whether the operation was successful.</param>
	/// <param name="errorMessage">Optional error message when the operation failed.</param>
	protected Result(bool success, string? errorMessage = null)
	{
		Success = success;
		Error = errorMessage;
	}

	/// <summary>
	///   Gets a value indicating whether the operation was successful.
	/// </summary>
	public bool Success { get; }

	/// <summary>
	///   Gets a value indicating whether the operation failed.
	/// </summary>
	public bool Failure => !Success;

	/// <summary>
	///   Gets the error message if the operation failed.
	/// </summary>
	public string? Error { get; }

	/// <summary>
	///   Creates a successful result.
	/// </summary>
	/// <returns>A successful result.</returns>
	public static Result Ok()
	{
		return new Result(true);
	}

	/// <summary>
	///   Creates a failed result with an error message.
	/// </summary>
	/// <param name="errorMessage">The error message.</param>
	/// <returns>A failed result.</returns>
	public static Result Fail(string errorMessage)
	{
		return new Result(false, errorMessage);
	}

	/// <summary>
	///   Creates a successful result with a value.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="value">The value.</param>
	/// <returns>A successful result containing the value.</returns>
	public static Result<T> Ok<T>(T value)
	{
		return new Result<T>(value, true);
	}

	/// <summary>
	///   Creates a failed result for a typed result.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="errorMessage">The error message.</param>
	/// <returns>A failed result.</returns>
	public static Result<T> Fail<T>(string errorMessage)
	{
		return new Result<T>(default, false, errorMessage);
	}

	/// <summary>
	///   Creates a result from a nullable value.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="value">The value.</param>
	/// <returns>A successful result if value is not null, otherwise a failed result.</returns>
	public static Result<T> FromValue<T>(T? value)
	{
		return value is not null ? Ok(value) : Result<T>.Fail("Provided value is null.");
	}
}

/// <summary>
///   Represents the result of an operation that can succeed or fail and returns a value.
/// </summary>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
public sealed class Result<T> : Result
{
	/// <summary>
	///   Initializes a new instance of the <see cref="Result{T}"/> class.
	/// </summary>
	/// <param name="value">The value if successful.</param>
	/// <param name="success">Indicates whether the operation was successful.</param>
	/// <param name="errorMessage">Optional error message when the operation failed.</param>
	internal Result(T? value, bool success, string? errorMessage = null)
			: base(success, errorMessage)
	{
		Value = value;
	}

	/// <summary>
	///   Gets the value if the operation was successful.
	/// </summary>
	public T? Value { get; }

	/// <summary>
	///   Creates a successful result with a value.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>A successful result containing the value.</returns>
	private static Result<T> Ok(T value)
	{
		return new Result<T>(value, true);
	}

	/// <summary>
	///   Creates a failed result with an error message.
	/// </summary>
	/// <param name="errorMessage">The error message.</param>
	/// <returns>A failed result.</returns>
	public static new Result<T> Fail(string errorMessage)
	{
		return new Result<T>(default, false, errorMessage);
	}

	/// <summary>
	///   Implicitly converts a Result to its value.
	/// </summary>
	/// <param name="result">The result to convert.</param>
	public static implicit operator T?(Result<T> result)
	{
		ArgumentNullException.ThrowIfNull(result);

		return result.Value;
	}

	/// <summary>
	///   Implicitly converts a value to a successful Result.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	public static implicit operator Result<T>(T value)
	{
		return Ok(value);
	}
}