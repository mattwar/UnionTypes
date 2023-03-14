using System.Runtime.InteropServices;
using UnionTypes;

namespace TagUnionWithTypeParameter;
#nullable enable

[Union]
public partial struct Result<T>
{
    public static partial Result<T> Success(T value);
    public static partial Result<T> Failure(string reason);
}

public partial struct Result<T> : IEquatable<Result<T>>
{
    private enum Tag
    {
        Success = 1,
        Failure = 2,
    }

    private readonly Tag _tag;
    private readonly RefData _refData;
    private readonly T _field0;

    [StructLayout(LayoutKind.Explicit)]
    private struct RefData
    {
        [FieldOffset(0)]
        public Failure_Data Failure;

        public struct Failure_Data
        {
            public System.String reason;
        }
    }

    private Result(Tag tag, RefData refData, T field0)
    {
        _tag = tag;
        _refData = refData;
        this._field0 = field0;
    }

    public static partial Result<T> Success(T value)
    {
        return new Result<T>(
            Tag.Success
            , new RefData { }
            , field0: value
            );
    }

    public static partial Result<T> Failure(System.String reason)
    {
        return new Result<T>(
            Tag.Failure
            , new RefData
            {
                Failure = new RefData.Failure_Data
                {
                    reason = reason
                }
            }
            , field0: default!
            );
    }

    public bool IsSuccess => _tag == Tag.Success;
    public bool IsFailure => _tag == Tag.Failure;

    public bool TryGetSuccess(out T value)
    {
        if (IsSuccess)
        {
            value = _field0;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }

    public bool TryGetFailure(out System.String reason)
    {
        if (IsFailure)
        {
            reason = _refData.Failure.reason;
            return true;
        }
        else
        {
            reason = default!;
            return false;
        }
    }

    public T GetSuccess() =>
        TryGetSuccess(out var value) ? value : throw new InvalidCastException();

    public System.String GetFailure() =>
        TryGetFailure(out var reason) ? reason : throw new InvalidCastException();

    public T GetSuccessOrDefault() =>
        TryGetSuccess(out var value) ? value : default!;

    public System.String GetFailureOrDefault() =>
        TryGetFailure(out var reason) ? reason : default!;

    public bool Equals(Result<T> other)
    {
        if (_tag != other._tag) return false;

        switch (_tag)
        {
            case Tag.Success:
                {
                    var value = GetSuccess();
                    var otherValue = other.GetSuccess();
                    return value.Equals(otherValue);
                }
            case Tag.Failure:
                {
                    var value = GetFailure();
                    var otherValue = other.GetFailure();
                    return (value != null && otherValue != null)
                        || (value != null && otherValue != null && value.Equals(otherValue));
                }
            default:
                return true;
        }
    }

    public override bool Equals(object? other)
    {
        return other is Result<T> union && Equals(union);
    }

    public override int GetHashCode()
    {
        switch (_tag)
        {
            case Tag.Success:
                return GetSuccess().GetHashCode();
            case Tag.Failure:
                return GetFailure()?.GetHashCode() ?? 0;
            default:
                return 0;
        }
    }

    public static bool operator ==(Result<T> left, Result<T> right) =>
        left.Equals(right);

    public static bool operator !=(Result<T> left, Result<T> right) =>
        !left.Equals(right);

    public override string ToString()
    {
        switch (_tag)
        {
            case Tag.Success:
                return $"Success({GetSuccess()})";
            case Tag.Failure:
                return $"Failure({GetFailure()})";
            default:
                return "";
        }
    }
}
