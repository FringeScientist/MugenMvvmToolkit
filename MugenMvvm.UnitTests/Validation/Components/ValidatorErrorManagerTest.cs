﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ValidatorErrorManagerTest : UnitTestBase
    {
        private const string TwoErrorSource = "t2";
        private const string SingleErrorSource = "t1";
        private const string NoErrorSource = "t0";

        private const string Member1 = "1";
        private const string Member2 = "2";
        private readonly Validator _validator;
        private readonly Dictionary<object, List<ValidationErrorInfo>> _sourceErrors;
        private readonly List<ValidationErrorInfo> _allErrors;

        private readonly ValidationErrorInfo _member1Error;
        private readonly ValidationErrorInfo _member2Error;

        public ValidatorErrorManagerTest()
        {
            _validator = new Validator();
            _validator.AddComponent(new ValidatorErrorManager());
            _member1Error = new ValidationErrorInfo(new object(), Member1, Member1);
            _member2Error = new ValidationErrorInfo(new object(), Member2, Member2);
            _sourceErrors = new Dictionary<object, List<ValidationErrorInfo>>
            {
                [TwoErrorSource] = new() {_member1Error, _member2Error},
                [SingleErrorSource] = new() {_member1Error},
                [NoErrorSource] = new()
            };
            _allErrors = new List<ValidationErrorInfo>();
            foreach (var sourceError in _sourceErrors)
                _allErrors.AddRange(sourceError.Value);
        }

        [Fact]
        public void HasErrorsShouldBeValid()
        {
            AddDefaultErrors();
            _validator.HasErrors().ShouldBeTrue();
            _validator.HasErrors(default, TwoErrorSource).ShouldBeTrue();
            _validator.HasErrors(default, SingleErrorSource).ShouldBeTrue();
            _validator.HasErrors(default, NoErrorSource).ShouldBeFalse();

            _validator.HasErrors(new[] {Member1, Member2}).ShouldBeTrue();
            _validator.HasErrors(new[] {Member1, Member2}, TwoErrorSource).ShouldBeTrue();
            _validator.HasErrors(new[] {Member1, Member2}, SingleErrorSource).ShouldBeTrue();
            _validator.HasErrors(new[] {Member1, Member2}, NoErrorSource).ShouldBeFalse();

            _validator.HasErrors(Member2).ShouldBeTrue();
            _validator.HasErrors(Member2, TwoErrorSource).ShouldBeTrue();
            _validator.HasErrors(Member2, SingleErrorSource).ShouldBeFalse();
            _validator.HasErrors(Member2, NoErrorSource).ShouldBeFalse();

            _validator.HasErrors(Member1).ShouldBeTrue();
            _validator.HasErrors(Member1, TwoErrorSource).ShouldBeTrue();
            _validator.HasErrors(Member1, SingleErrorSource).ShouldBeTrue();
            _validator.HasErrors(Member1, NoErrorSource).ShouldBeFalse();
        }

        [Fact]
        public void GetErrorsRawShouldBeValid()
        {
            AddDefaultErrors();
            var errors = new ItemOrListEditor<object>();

            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors.Select(info => info.Error));

            errors.Clear();
            _validator.GetErrors(default, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource].Select(info => info.Error));

            errors.Clear();
            _validator.GetErrors(default, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors.AsList().ShouldContain(_sourceErrors[SingleErrorSource].Select(info => info.Error));

            errors.Clear();
            _validator.GetErrors(default, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            _validator.GetErrors(new[] {Member1, Member2}, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors.Select(info => info.Error));

            errors.Clear();
            _validator.GetErrors(new[] {Member1, Member2}, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource].Select(info => info.Error));

            errors.Clear();
            _validator.GetErrors(new[] {Member1, Member2}, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors.AsList().ShouldContain(_sourceErrors[SingleErrorSource].Select(info => info.Error));

            errors.Clear();
            _validator.GetErrors(new[] {Member1, Member2}, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            _validator.GetErrors(Member2, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error.Error);

            errors.Clear();
            _validator.GetErrors(Member2, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error.Error);

            errors.Clear();
            _validator.GetErrors(Member2, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            _validator.GetErrors(Member2, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            _validator.GetErrors(Member1, ref errors);
            errors.Count.ShouldEqual(2);
            errors[0].ShouldEqual(_member1Error.Error);
            errors[1].ShouldEqual(_member1Error.Error);

            errors.Clear();
            _validator.GetErrors(Member1, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member1Error.Error);

            errors.Clear();
            _validator.GetErrors(Member1, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member1Error.Error);

            errors.Clear();
            _validator.GetErrors(Member1, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void GetErrorsShouldBeValid()
        {
            AddDefaultErrors();
            var errors = new ItemOrListEditor<ValidationErrorInfo>();

            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors);

            errors.Clear();
            _validator.GetErrors(default, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            errors.Clear();
            _validator.GetErrors(default, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors.AsList().ShouldContain(_sourceErrors[SingleErrorSource]);

            errors.Clear();
            _validator.GetErrors(default, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            _validator.GetErrors(new[] {Member1, Member2}, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors);

            errors.Clear();
            _validator.GetErrors(new[] {Member1, Member2}, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            errors.Clear();
            _validator.GetErrors(new[] {Member1, Member2}, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors.AsList().ShouldContain(_sourceErrors[SingleErrorSource]);

            errors.Clear();
            _validator.GetErrors(new[] {Member1, Member2}, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            _validator.GetErrors(Member2, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error);

            errors.Clear();
            _validator.GetErrors(Member2, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error);

            errors.Clear();
            _validator.GetErrors(Member2, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            _validator.GetErrors(Member2, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            _validator.GetErrors(Member1, ref errors);
            errors.Count.ShouldEqual(2);
            errors[0].ShouldEqual(_member1Error);
            errors[1].ShouldEqual(_member1Error);

            errors.Clear();
            _validator.GetErrors(Member1, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member1Error);

            errors.Clear();
            _validator.GetErrors(Member1, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member1Error);

            errors.Clear();
            _validator.GetErrors(Member1, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void SetErrorsShouldNotifyListeners()
        {
            string[] members = {Member1, Member2};
            int invokeCount = 0;
            _validator.AddComponent(new TestValidatorErrorsChangedListener
            {
                OnErrorsChanged = (validator, list, m) =>
                {
                    ++invokeCount;
                    validator.ShouldEqual(_validator);
                    list.AsList().ShouldEqual(members);
                    m.ShouldEqual(DefaultMetadata);
                }
            });

            invokeCount = 0;
            _validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member2Error, _member2Error}, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            _validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member2Error, _member2Error, new ValidationErrorInfo(_member1Error.Target, _member1Error.Member, null)}, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            members = new[] {Member2};
            _validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member2Error}, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            _validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member2Error}, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            _validator.SetErrors(TwoErrorSource, _member1Error, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            _validator.SetErrors(TwoErrorSource, _member2Error, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            members = new[] {Member1};
            _validator.SetErrors(SingleErrorSource, _member1Error, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            _validator.SetErrors(SingleErrorSource, default, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            members = new[] {Member1};
            _validator.SetErrors(SingleErrorSource, new ValidationErrorInfo(_member1Error.Target, _member1Error.Member, null), DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void SetErrorsShouldBeValid()
        {
            var errors = new ItemOrListEditor<ValidationErrorInfo>();
            _validator.SetErrors(TwoErrorSource, _sourceErrors[TwoErrorSource], DefaultMetadata);

            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            errors.Clear();
            _validator.SetErrors(TwoErrorSource, _member1Error, DefaultMetadata);
            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            errors.Clear();
            _validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member1Error}, DefaultMetadata);
            _validator.GetErrors(Member1, ref errors);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_member1Error, _member1Error);

            errors.Clear();
            _validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member1Error}, DefaultMetadata);
            _validator.SetErrors(TwoErrorSource, new ValidationErrorInfo(_member1Error.Target, _member1Error.Member, null), DefaultMetadata);
            _validator.GetErrors(Member1, ref errors);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void ClearErrorsShouldNotifyListeners()
        {
            string[] members = {Member1, Member2};
            int invokeCount = 0;
            bool ignore = true;
            _validator.AddComponent(new TestValidatorErrorsChangedListener
            {
                OnErrorsChanged = (validator, list, m) =>
                {
                    if (ignore)
                        return;
                    ++invokeCount;
                    validator.ShouldEqual(_validator);
                    list.AsList().ShouldEqual(members);
                    m.ShouldEqual(DefaultMetadata);
                }
            });

            ignore = true;
            AddDefaultErrors();
            ignore = false;

            _validator.ClearErrors(default, null, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            ignore = true;
            AddDefaultErrors();
            invokeCount = 0;
            ignore = false;
            members = new[] {Member1};
            _validator.ClearErrors(Member1, null, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            ignore = true;
            AddDefaultErrors();
            invokeCount = 0;
            ignore = false;
            _validator.ClearErrors(Member1, NoErrorSource, DefaultMetadata);
            invokeCount.ShouldEqual(0);
        }

        [Fact]
        public void ClearShouldBeValid()
        {
            var errors = new ItemOrListEditor<ValidationErrorInfo>();

            AddDefaultErrors();
            _validator.ClearErrors();
            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(0);

            AddDefaultErrors();
            errors.Clear();
            _validator.ClearErrors(new[] {Member1, Member2});
            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(0);

            AddDefaultErrors();
            errors.Clear();
            _validator.ClearErrors(Member1);
            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error);

            AddDefaultErrors();
            errors.Clear();
            _validator.ClearErrors(Member1, SingleErrorSource);
            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            AddDefaultErrors();
            errors.Clear();
            _validator.ClearErrors(Member1, NoErrorSource);
            _validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors);
        }

        private void AddDefaultErrors()
        {
            _validator.ClearErrors();
            foreach (var sourceError in _sourceErrors)
                _validator.SetErrors(sourceError.Key, sourceError.Value);
        }
    }
}