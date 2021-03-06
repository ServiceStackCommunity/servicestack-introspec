﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.IntroSpec.Tests.Enrichers
{
    using System;
    using FluentAssertions;
    using Host;
    using Html;
    using IntroSpec.Enrichers;
    using IntroSpec.Models;
    using IntroSpec.Settings;
    using IntroSpec.TypeSpec;
    using Xunit;

    public class AbstractClassEnricherTests
    {
        private AbstractClassEnricher GetEnricher() => new AbstractClassEnricher();
        private Type dtoType = typeof(MyDto);
        private Type nonDtoType = typeof(MyNonDto);
        private Type notesDtoType = typeof(MyNotesDto);

        [Fact]
        public void GetContentTypes_TypeSpec_ReturnsNull()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = nonDtoType };
                GetEnricher().GetContentTypes(operation, "GET").Should().BeNull();
            }
        }

        [Fact]
        public void GetContentTypes_ReturnsGlobal_IfNoVerbSpecific()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = dtoType };
                var result = GetEnricher().GetContentTypes(operation, "DELETE");
                result.Length.Should().Be(1);
                result.Should().Contain("application/json");
            }
        }

        [Fact]
        public void GetContentTypes_ReturnsGlobalAndVerbSpecific()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = dtoType };
                var result = GetEnricher().GetContentTypes(operation, "GET");
                result.Length.Should().Be(2);
                result.Should().Contain("application/json").And.Contain("text/html");
            }
        }

        [Fact]
        public void GetStatusCodes_TypeSpec_ReturnsNull()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = nonDtoType };
                GetEnricher().GetStatusCodes(operation, "GET").Should().BeNull();
            }
        }

        [Fact]
        public void GetStatusCodes_ReturnsGlobal_IfNoVerbSpecific()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = dtoType };
                var result = GetEnricher().GetStatusCodes(operation, "DELETE");
                result.Length.Should().Be(1);
                result.Should().Contain((StatusCode)200);
            }
        }

        [Fact]
        public void GetStatusCodes_ReturnsGlobalAndVerbSpecific()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = dtoType };
                var result = GetEnricher().GetStatusCodes(operation, "PUT");
                result.Length.Should().Be(2);
                result.Should().Contain((StatusCode)200).And.Contain((StatusCode)201);
            }
        }

        [Fact]
        public void GetNotes_ForVerb_ReturnsNull_NoNotes()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = nonDtoType };
                GetEnricher().GetNotes(operation, "GET").Should().BeNull();
            }
        }

        [Fact]
        public void GetNotes_ForVerb_ReturnsGlobal_IfNoVerbSpecific()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = dtoType };
                GetEnricher().GetNotes(operation, "GET").Should().Be("GlobalNotes");
            }
        }

        [Fact]
        public void GetNotes_ForVerb_ReturnsVerb_IfVerbSpecific()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = notesDtoType };
                GetEnricher().GetNotes(operation, "GET").Should().Be("Verb notes");
            }
        }

        [Fact]
        public void GetNotes_ForVerb_ReturnsGlobal_IfVerbSpecific_ForDifferentVerb()
        {
            using (DocumenterSettings.With(assemblies: new[] { typeof(AbstractClassEnricherTests).Assembly }))
            {
                var operation = new Operation { RequestType = notesDtoType };
                GetEnricher().GetNotes(operation, "PUT").Should().Be("GlobalNotes");
            }
        }
    }

    public class MyDto
    {
        public string Foo { get; set; }
        public int Bar { get; set; }
    }

    public class MyNotesDto { }
    public class MyNonDto { }

    public class MyDtoSpec : AbstractRequestSpec<MyDto>
    {
        public MyDtoSpec()
        {
            AddContentTypes("application/json");
            AddContentTypes(HttpVerbs.Get, "text/html");

            AddStatusCodes((StatusCode)200);
            AddStatusCodes(HttpVerbs.Put, (StatusCode)201);

            AddRouteNotes("GlobalNotes");
        }
    }

    public class NotesSpec : AbstractRequestSpec<MyNotesDto>
    {
        public NotesSpec()
        {
            AddRouteNotes("GlobalNotes");
            AddRouteNotes(HttpVerbs.Get, "Verb notes");
        }
    }

    public class MyNonDtoSpec : AbstractTypeSpec<MyNonDto> { }
}
