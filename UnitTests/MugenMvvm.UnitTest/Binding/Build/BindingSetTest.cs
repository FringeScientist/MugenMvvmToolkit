﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Build
{
    public class BindingSetTest : UnitTestBase
    {
        #region Fields

        private static readonly BindingExpressionRequest ConverterRequest = new BindingExpressionRequest("", null, null);

        #endregion

        #region Methods

        [Fact]
        public void BindShouldBuildBinding1()
        {
            var target = this;
            object? source = null;
            var binding = new TestBinding();
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(DefaultMetadata);
                    return binding;
                }
            };
            var bindingManager = new BindingManager();
            var invokeCount = 0;
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, type, arg3) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(bindingManager);
                    ((BindingBuilderRequest)o).ToBindingExpressionRequest().ShouldEqual(ConverterRequest);
                    type.ShouldEqual(typeof(BindingBuilderRequest));
                    arg3.ShouldEqual(DefaultMetadata);
                    return testBuilder;
                }
            });

            var bindingSet = new BindingSet<object>(source, bindingManager);
            bindingSet.Bind(target, builderTarget => ConverterRequest, DefaultMetadata);
            bindingSet.BuildIncludeBindings(DefaultMetadata).Item.ShouldEqual(binding);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void BindShouldBuildBinding2()
        {
            var target = this;
            var source = "";
            var binding = new TestBinding();
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(DefaultMetadata);
                    return binding;
                }
            };
            var bindingManager = new BindingManager();
            var invokeCount = 0;
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, type, arg3) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(bindingManager);
                    ((BindingBuilderRequest)o).ToBindingExpressionRequest().ShouldEqual(ConverterRequest);
                    type.ShouldEqual(typeof(BindingBuilderRequest));
                    arg3.ShouldEqual(DefaultMetadata);
                    return testBuilder;
                }
            });

            var bindingSet = new BindingSet<object>(bindingManager);
            bindingSet.Bind(target, source, builderTarget => ConverterRequest, DefaultMetadata);
            bindingSet.BuildIncludeBindings(DefaultMetadata).Item.ShouldEqual(binding);
            invokeCount.ShouldEqual(1);
        }


        [Fact]
        public void BindShouldBuildBinding3()
        {
            var request = "Test";
            var target = this;
            var source = "";
            var binding = new TestBinding();
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(DefaultMetadata);
                    return binding;
                }
            };
            var bindingManager = new BindingManager();
            var invokeCount = 0;
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, type, arg3) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(bindingManager);
                    o.ShouldEqual(request);
                    type.ShouldEqual(typeof(string));
                    arg3.ShouldEqual(DefaultMetadata);
                    return testBuilder;
                }
            });

            var bindingSet = new BindingSet<object>(bindingManager);
            bindingSet.Bind(target, request, source, DefaultMetadata);
            bindingSet.BuildIncludeBindings(DefaultMetadata).Item.ShouldEqual(binding);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void BindShouldBuildBinding4()
        {
            var request = "Test";
            var target = this;
            var source = "";
            var binding = new TestBinding();
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(DefaultMetadata);
                    return binding;
                }
            };
            var bindingManager = new BindingManager();
            var invokeCount = 0;
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, type, arg3) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(bindingManager);
                    o.ShouldEqual(request);
                    type.ShouldEqual(typeof(string));
                    arg3.ShouldEqual(DefaultMetadata);
                    return testBuilder;
                }
            });

            var bindingSet = new BindingSet<object>(source, bindingManager);
            bindingSet.Bind(target, request, source: null, DefaultMetadata);
            bindingSet.BuildIncludeBindings(DefaultMetadata).Item.ShouldEqual(binding);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        [InlineData(1, 10)]
        [InlineData(1, 1)]
        public void BuildIncludeBindingsShouldHandleListOfTargets(int count, int bindingCount)
        {
            var invokeBuilderCount = 0;
            var list = new List<(object target, object source, TestBindingBuilder builder, TestBinding binding, string request)>();
            for (var i = 0; i < count; i++)
            {
                var target = new object();
                var source = new object();
                var binding = new TestBinding();
                var testBuilder = new TestBindingBuilder
                {
                    Build = (o, o1, arg3) =>
                    {
                        ++invokeBuilderCount;
                        o.ShouldEqual(target);
                        o1.ShouldEqual(source);
                        arg3.ShouldEqual(DefaultMetadata);
                        return binding;
                    }
                };

                list.Add((target, source, testBuilder, binding, i.ToString()));
            }

            var sortCount = 0;
            var bindingManager = new BindingManager();
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, type, arg3) =>
                {
                    m.ShouldEqual(bindingManager);
                    if (o is IReadOnlyList<IBindingBuilder> builders)
                    {
                        ++sortCount;
                        return ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>>.FromRawValue(builders);
                    }

                    return list.Single(tuple => tuple.request.Equals(o)).builder;
                }
            });

            var bindingSet = new BindingSet<object>(bindingManager);
            for (var i = 0; i < bindingCount; i++)
            {
                foreach (var valueTuple in list)
                    bindingSet.Bind(valueTuple.target, valueTuple.request, valueTuple.source, DefaultMetadata);
            }

            var bindings = bindingSet.BuildIncludeBindings(DefaultMetadata);
            invokeBuilderCount.ShouldEqual(count * bindingCount);
            sortCount.ShouldEqual(bindingCount > 1 ? count : 0);
            var groupBy = bindings.AsList().GroupBy(binding => binding);
            groupBy.Count().ShouldEqual(count);
            foreach (var group in groupBy)
                group.Count().ShouldEqual(bindingCount);

            bindingSet.BuildIncludeBindings(DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        [InlineData(1, 10)]
        [InlineData(1, 1)]
        public void DisposeShouldHandleListOfTargets(int count, int bindingCount)
        {
            var invokeBuilderCount = 0;
            var list = new List<(object target, object source, TestBindingBuilder builder, TestBinding binding, string request)>();
            for (var i = 0; i < count; i++)
            {
                var target = new object();
                var source = new object();
                var binding = new TestBinding();
                var testBuilder = new TestBindingBuilder
                {
                    Build = (o, o1, arg3) =>
                    {
                        ++invokeBuilderCount;
                        o.ShouldEqual(target);
                        o1.ShouldEqual(source);
                        arg3.ShouldBeNull();
                        return binding;
                    }
                };

                list.Add((target, source, testBuilder, binding, i.ToString()));
            }

            var sortCount = 0;
            var bindingManager = new BindingManager();
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, type, arg3) =>
                {
                    m.ShouldEqual(bindingManager);
                    if (o is IReadOnlyList<IBindingBuilder> builders)
                    {
                        ++sortCount;
                        return ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>>.FromRawValue(builders);
                    }

                    return list.Single(tuple => tuple.request.Equals(o)).builder;
                }
            });

            var bindingSet = new BindingSet<object>(bindingManager);
            for (var i = 0; i < bindingCount; i++)
            {
                foreach (var valueTuple in list)
                    bindingSet.Bind(valueTuple.target, valueTuple.request, valueTuple.source, DefaultMetadata);
            }

            bindingSet.Dispose();
            invokeBuilderCount.ShouldEqual(count * bindingCount);
            sortCount.ShouldEqual(bindingCount > 1 ? count : 0);
            bindingSet.Dispose();
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        [InlineData(1, 10)]
        [InlineData(1, 1)]
        public void BuildShouldHandleListOfTargets(int count, int bindingCount)
        {
            var invokeBuilderCount = 0;
            var list = new List<(object target, object source, TestBindingBuilder builder, TestBinding binding, string request)>();
            for (var i = 0; i < count; i++)
            {
                var target = new object();
                var source = new object();
                var binding = new TestBinding();
                var testBuilder = new TestBindingBuilder
                {
                    Build = (o, o1, arg3) =>
                    {
                        ++invokeBuilderCount;
                        o.ShouldEqual(target);
                        o1.ShouldEqual(source);
                        arg3.ShouldEqual(DefaultMetadata);
                        return binding;
                    }
                };

                list.Add((target, source, testBuilder, binding, i.ToString()));
            }

            var sortCount = 0;
            var bindingManager = new BindingManager();
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (m, o, type, arg3) =>
                {
                    m.ShouldEqual(bindingManager);
                    if (o is IReadOnlyList<IBindingBuilder> builders)
                    {
                        ++sortCount;
                        return ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>>.FromRawValue(builders);
                    }

                    return list.Single(tuple => tuple.request.Equals(o)).builder;
                }
            });

            var bindingSet = new BindingSet<object>(bindingManager);
            for (var i = 0; i < bindingCount; i++)
            {
                foreach (var valueTuple in list)
                    bindingSet.Bind(valueTuple.target, valueTuple.request, valueTuple.source, DefaultMetadata);
            }

            bindingSet.Build(DefaultMetadata);
            invokeBuilderCount.ShouldEqual(count * bindingCount);
            sortCount.ShouldEqual(bindingCount > 1 ? count : 0);
            bindingSet.Dispose();
        }

        #endregion
    }
}