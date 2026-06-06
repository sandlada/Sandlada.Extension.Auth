using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.Tests.Commons;

public sealed class ResultTests {
    [Fact]
    public void Success_NoValue_ReturnsSuccessWithNoneError() {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(DomainError.General.None, result.Error);
    }

    [Fact]
    public void Failure_NoValue_ReturnsFailureWithProvidedError() {
        var result = Result.Failure(DomainError.User.NotFound);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainError.User.NotFound, result.Error);
    }

    [Fact]
    public void Success_WithValue_ReturnsSuccessAndValue() {
        var result = Result.Success("ok");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(DomainError.General.None, result.Error);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public void Failure_WithValue_AccessingValueThrowsInvalidOperationException() {
        var result = Result.Failure<string>(DomainError.Auth.InvalidCredentials);

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => {
            _ = result.Value;
        });
    }

    [Fact]
    public void ConstructorProbe_SuccessWithNonNoneError_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => {
            _ = new ResultProbe(isSuccess: true, error: DomainError.User.NotFound);
        });
    }

    [Fact]
    public void ConstructorProbe_FailureWithNoneError_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => {
            _ = new ResultProbe(isSuccess: false, error: DomainError.General.None);
        });
    }

    [Fact]
    public void GenericConstructorProbe_SuccessWithNonNoneError_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => {
            _ = new GenericResultProbe<string>("value", isSuccess: true, error: DomainError.User.NotFound);
        });
    }

    [Fact]
    public void GenericConstructorProbe_FailureWithNoneError_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => {
            _ = new GenericResultProbe<string>(null, isSuccess: false, error: DomainError.General.None);
        });
    }

    private sealed class ResultProbe : Result {
        public ResultProbe(bool isSuccess, DomainError error) : base(isSuccess, error) {
        }
    }

    private sealed class GenericResultProbe<TValue> : Result<TValue> {
        public GenericResultProbe(TValue? value, bool isSuccess, DomainError error) : base(value, isSuccess, error) {
        }
    }
}
