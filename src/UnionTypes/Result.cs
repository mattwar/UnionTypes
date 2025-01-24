using System;

namespace UnionTypes.Toolkit
{
    public partial struct Result<TValue, TError>
    {
        /// <summary>
        /// True when the result is undefined.
        /// </summary>
        public bool IsInvalid => this.Kind == 0;

        /// <summary>
        /// Map the success value to a new value or result
        /// </summary>
        public Result<TValue2, TError> Map<TValue2>(Func<TValue, Result<TValue2, TError>> whenSuccess) =>
            this.Kind switch
            {
                Case.Success => whenSuccess(this.Value),
                Case.Failure => Result<TValue2, TError>.Failure(this.Error),
                _ => default // still invalid
            };

        /// <summary>
        /// Map the success value to a new success value.
        /// </summary>
        public Result<TValue2, TError> Map<TValue2>(Func<TValue, TValue2> whenSuccess) =>
            this.Kind switch
            {
                Case.Success => Result<TValue2, TError>.Success(whenSuccess(this.Value)),
                Case.Failure => Result<TValue2, TError>.Failure(this.Error),
                _ => default // still invalid
            };
    }
}