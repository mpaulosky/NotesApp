using System.Diagnostics.CodeAnalysis;

namespace Notes.Web.Tests.Unit.Abstractions;

[ExcludeFromCodeCoverage]
public class ResultTests
{
	[Fact]
	public void Ok_ShouldCreateSuccessfulResult()
	{
		// Act
		var result = Result.Ok();

		// Assert
		result.Success.Should().BeTrue();
		result.Failure.Should().BeFalse();
		result.Error.Should().BeNull();
	}

	[Fact]
	public void Fail_ShouldCreateFailedResult()
	{
		// Arrange
		const string errorMessage = "Something went wrong";

		// Act
		var result = Result.Fail(errorMessage);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be(errorMessage);
	}

	[Fact]
	public void OkGeneric_ShouldCreateSuccessfulResultWithValue()
	{
		// Arrange
		const string value = "Test Value";

		// Act
		var result = Result.Ok(value);

		// Assert
		result.Success.Should().BeTrue();
		result.Failure.Should().BeFalse();
		result.Error.Should().BeNull();
		result.Value.Should().Be(value);
	}

	[Fact]
	public void FailGeneric_ShouldCreateFailedResultWithoutValue()
	{
		// Arrange
		const string errorMessage = "Operation failed";

		// Act
		var result = Result.Fail<string>(errorMessage);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be(errorMessage);
		result.Value.Should().BeNull();
	}

	[Fact]
	public void FromValue_WithNonNullValue_ShouldCreateSuccessfulResult()
	{
		// Arrange
		const int value = 42;

		// Act
		var result = Result.FromValue(value);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be(value);
	}

	[Fact]
	public void FromValue_WithNullValue_ShouldCreateFailedResult()
	{
		// Arrange
		string? value = null;

		// Act
		var result = Result.FromValue(value);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Provided value is null.");
		result.Value.Should().BeNull();
	}

	[Fact]
	public void ImplicitOperator_ResultToValue_ShouldReturnValue()
	{
		// Arrange
		const string expectedValue = "Test";
		var result = Result.Ok(expectedValue);

		// Act
		string? actualValue = result;

		// Assert
		actualValue.Should().Be(expectedValue);
	}

	[Fact]
	public void ImplicitOperator_ValueToResult_ShouldCreateSuccessfulResult()
	{
		// Arrange
		const int value = 100;

		// Act
		Result<int> result = value;

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be(value);
	}

	[Fact]
	public void ImplicitOperator_ResultToValue_WithNull_ShouldThrowArgumentNullException()
	{
		// Arrange
		Result<string>? result = null;

		// Act
		Action act = () =>
		{
			System.Convert.ToString(result!);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Result_Failure_ShouldBeOppositeOfSuccess()
	{
		// Arrange
		var successResult = Result.Ok();
		var failureResult = Result.Fail("Error");

		// Assert
		successResult.Success.Should().BeTrue();
		successResult.Failure.Should().BeFalse();
		failureResult.Success.Should().BeFalse();
		failureResult.Failure.Should().BeTrue();
	}
}
