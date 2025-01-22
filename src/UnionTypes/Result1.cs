namespace UnionTypes
{
    public partial struct Result<TValue>
    {
        public static implicit operator Result<TValue, string>(Result<TValue> result)
        {
            return result.Kind switch
            {
                Result<TValue>.Case.Success => Result<TValue, string>.Success(result.SuccessValue),
                Result<TValue>.Case.Failure => Result<TValue, string>.Failure(result.FailureMessage),
                _ => throw new System.InvalidOperationException("Invalid case"),
            };
        }
    }
}