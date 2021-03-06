﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.IntroSpec.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataAnnotations;
    using FakeItEasy;
    using Fixtures;
    using FluentAssertions;
    using Host;
    using IntroSpec;
    using IntroSpec.Models;
    using IntroSpec.Services;
    using IntroSpec.Settings;
    using NativeTypes;
    using Testing;
    using Xunit;

    [Collection("AppHost")]
    public class ApiSpecFeatureTests
    {
        private readonly ApiSpecFeature feature;
        private readonly ApiSpecConfig apiSpecConfig;
        private readonly IApiDocumentationGenerator generator;
        private readonly Func<KeyValuePair<Type, Operation>, bool> filter;
        private readonly AppHostFixture fixture;

        public ApiSpecFeatureTests(AppHostFixture fixture)
        {
            this.fixture = fixture;

            apiSpecConfig = new ApiSpecConfig
            {
                Contact = new ApiContact { Email = "ronald.macdonald@macdonalds.hq", Name = "ronnie mcd" },
                Description = "great api"
            };
            generator = A.Fake<IApiDocumentationGenerator>();

            filter = A.Fake<Func<KeyValuePair<Type, Operation>, bool>>();

            feature = new ApiSpecFeature(config => apiSpecConfig)
                .WithGenerator(generator)
                .WithOperationsFilter(filter);
        }

        [Fact]
        public void Ctor_Throws_IfConfigNull()
        {
            Action action = () => new ApiSpecFeature(config => null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_Throws_IfConfigInvalid()
        {
            Action action = () => new ApiSpecFeature(config => config);
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Register_Throws_IfNoMetadata()
        {
            Action action = () => feature.Register(A.Fake<IAppHost>());
            action.ShouldThrow<ArgumentException>()
                  .WithMessage("The Metadata Feature must be enabled to use the ApiSpec Feature");
        }

        [Fact]
        public void Register_RegistersService()
        {
            feature.Register(fixture.AppHost);
            fixture.AppHost.Container.TryResolve<ApiSpecService>().Should().NotBeNull();
        }

        [Fact]
        public void OperationsMapFilter_ExcludesTypesInIgnoreNamespaces()
        {
            var operationsMap = new Dictionary<Type, Operation>
            {
                { typeof(int), new Operation { RequestType = typeof(int) } },
                { typeof(TypesKotlin), new Operation { RequestType = typeof(TypesKotlin) } }
            };

            var filter = new ApiSpecFeature(config => apiSpecConfig).OperationsMapFilter;

            var result = operationsMap.Where(o => filter(o)).Select(o => o.Value).ToList();
            result.Count.Should().Be(1);
            result[0].RequestType.Should().Be<int>();
        }

        [Theory]
        [InlineData(typeof(ExcludeMetaData), 1)]
        [InlineData(typeof(ExcludeServiceDiscovery), 1)]
        [InlineData(typeof(ExcludeBoth), 1)]
        [InlineData(typeof(ExcludeRandom), 2)]
        public void OperationsMapFilter_ObeysExcludeAttribute(Type requestType, int expectedCount)
        {
            var operationsMap = new Dictionary<Type, Operation>
            {
                { typeof(int), new Operation { RequestType = typeof(int) } },
                { requestType, new Operation { RequestType = requestType } }
            };

            var filter = new ApiSpecFeature(config => apiSpecConfig).OperationsMapFilter;

            var result = operationsMap.Where(o => filter(o)).Select(o => o.Value).ToList();
            result.Count.Should().Be(expectedCount);
            result[0].RequestType.Should().Be<int>();
        }

        [Fact(Skip = "This is proving harder than expected. Will pick up again")]
        public void Register_CallsOperationsMapFilter()
        {
            var basicAppHost = new BasicAppHost(); // A.Fake<IAppHost>();
            basicAppHost.Plugins.Add(new MetadataFeature());

            /*A.CallTo(() => generator.GenerateDocumentation(A<IEnumerable<Operation>>.Ignored, basicAppHost))
                .Invokes((IEnumerable<Operation> ops, IAppHost apphost) => { ops.ToList(); });*/

            feature.Register(basicAppHost);

            A.CallTo(() => filter.Invoke(A<KeyValuePair<Type, Operation>>.Ignored)).MustHaveHappened();
        }
    }

    [Exclude(Feature.Metadata)] public class ExcludeMetaData { }

    [Exclude(Feature.ServiceDiscovery)] public class ExcludeServiceDiscovery { }

    [Exclude(Feature.ServiceDiscovery | Feature.Metadata)] public class ExcludeBoth { }

    [Exclude(Feature.Jsv)] public class ExcludeRandom { }
}
