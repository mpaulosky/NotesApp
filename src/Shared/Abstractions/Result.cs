// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Result.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Abstractions;

public class Result
{

	protected Result(bool success, string? errorMessage = null)
	{
		Success = success;
		Error = errorMessage;
	}

	public bool Success { get; }

	public bool Failure => !Success;

	public string? Error { get; }

	public static Result Ok()
	{
		return new Result(true);
	}

	public static Result Fail(string errorMessage)
	{
		return new Result(false, errorMessage);
	}

	public static Result<T> Ok<T>(T value)
	{
		return new Result<T>(value, true);
	}

	public static Result<T> Fail<T>(string errorMessage)
	{
		return new Result<T>(default, false, errorMessage);
	}

	public static Result<T> FromValue<T>(T? value)
	{
		return value is not null ? Ok(value) : Result<T>.Fail("Provided value is null.");
	}

}

public sealed class Result<T> : Result
{

	internal Result(T? value, bool success, string? errorMessage = null)
			: base(success, errorMessage)
	{
		Value = value;
	}

	public T? Value { get; }

	private static Result<T> Ok(T value)
	{
		return new Result<T>(value, true);
	}

	public static new Result<T> Fail(string errorMessage)
	{
		return new Result<T>(default, false, errorMessage);
	}

	public static implicit operator T? (Result<T> result)
	{
		ArgumentNullException.ThrowIfNull(result);

		return result.Value;
	}

	public static implicit operator Result<T>(T value)
	{
		return Ok(value);
	}

}