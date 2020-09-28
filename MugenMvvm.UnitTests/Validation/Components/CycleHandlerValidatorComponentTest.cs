﻿using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class CycleHandlerValidatorComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ValidateAsyncShouldHandleCycles()
        {
            var expectedMember = "test";
            var invokeCount = 0;
            var validator = new Validator();
            var tcs = new TaskCompletionSource<ValidationResult>();
            validator.AddComponent(new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    ++invokeCount;
                    validator.ValidateAsync(expectedMember).IsCompleted.ShouldBeTrue();
                    return new ValueTask<ValidationResult>(tcs.Task);
                }
            });
            validator.AddComponent(new CycleHandlerValidatorComponent());

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None);
            task!.IsCompleted.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ValidateAsyncShouldCancelPreviousValidation()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var validator = new Validator();
            validator.AddComponent(new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    token.Register(() => tcs.SetCanceled());
                    return new ValueTask<ValidationResult>(tcs.Task);
                }
            });
            validator.AddComponent(new CycleHandlerValidatorComponent());

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None)!;
            task.IsCompleted.ShouldBeFalse();

            validator.ValidateAsync(expectedMember, CancellationToken.None);
            task.WaitEx();
            task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void HasErrorsShouldReturnTrueAsync()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var validator = new Validator();
            validator.AddComponent(new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, _) => new ValueTask<ValidationResult>(tcs.Task)
            });
            validator.AddComponent(new CycleHandlerValidatorComponent());

            validator.HasErrors().ShouldBeFalse();
            var task = validator.ValidateAsync(expectedMember, CancellationToken.None);
            validator.HasErrors().ShouldBeTrue();
            validator.HasErrors(expectedMember).ShouldBeTrue();
            tcs.SetResult(default);
            task.WaitEx();

            validator.HasErrors().ShouldBeFalse();
            validator.HasErrors(expectedMember).ShouldBeFalse();
        }

        #endregion
    }
}