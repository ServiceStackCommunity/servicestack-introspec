﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Documentation.Tests.Services
{
    using System;
    using Documentation.DTO;
    using Documentation.Models;
    using Documentation.Services;
    using FluentAssertions;
    using Xunit;

    public class ApiDocumentationFilterTests
    {
        [Fact]
        public void GetApiDocumentation_Throws_IfRequestNull()
        {
            Action action = () => ApiDocumentationFilter.GetApiDocumentation(null, new ApiDocumentation());
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void GetApiDocumentation_Throws_IfDocumentationNull()
        {
            Action action = () => ApiDocumentationFilter.GetApiDocumentation(new Filterable(), null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void GetApiDocumentation_ReturnsWholeObject_IfNoFilterCriteria()
        {
            var documentation = new ApiDocumentation { Title = "Test Documentation" };
            var result = ApiDocumentationFilter.GetApiDocumentation(new Filterable(), documentation);

            result.Should().Be(documentation);
        }

        [Fact]
        public void GetApiDocumentation_FiltersDtoName()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Title = "DTO1" },
                new ApiResourceDocumentation { Title = "DTO2" }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable { DtoName = new[] { "DTO1" } };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);

            result.Resources.Length.Should().Be(1);
            result.Resources[0].Title.Should().Be("DTO1");
        }

        [Fact]
        public void GetApiDocumentation_FiltersMultipleDtoName()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Title = "DTO1" },
                new ApiResourceDocumentation { Title = "DTO2" },
                new ApiResourceDocumentation { Title = "DTO3" }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable { DtoName = new[] { "DTO1", "DTO3" } };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);

            result.Resources.Length.Should().Be(2);
            result.Resources[0].Title.Should().Be("DTO1");
            result.Resources[1].Title.Should().Be("DTO3");
        }

        [Fact]
        public void GetApiDocumentation_HandlesUnknownDtoName()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Title = "DTO1" },
                new ApiResourceDocumentation { Title = "DTO2" }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable { DtoName = new[] { "sunkilmoon" } };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);

            result.Resources.Should().BeNullOrEmpty();
        }

        [Fact]
        public void GetApiDocumentation_FiltersTags()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Tags = new[] { "Tag1" } },
                new ApiResourceDocumentation { Tags = new[] { "Tag2" } }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable { Tags = new[] { "Tag1" } };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);

            result.Resources.Length.Should().Be(1);
            result.Resources[0].Tags[0].Should().Be("Tag1");
        }

        [Fact]
        public void GetApiDocumentation_FiltersMultipleTags()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Tags = new[] { "Tag1" } },
                new ApiResourceDocumentation { Tags = new[] { "Tag2" } },
                new ApiResourceDocumentation { Tags = new[] { "Tag1", "Tag3" } }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable { Tags = new[] { "Tag1", "Tag2" } };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);

            result.Resources.Length.Should().Be(3);
        }

        [Fact]
        public void GetApiDocumentation_HandlesUnknownTags()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Tags = new[] { "Tag1" } },
                new ApiResourceDocumentation { Tags = new[] { "Tag2" } }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable { Tags = new[] { "sunkilmoon" } };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);

            result.Resources.Should().BeNullOrEmpty();
        }

        [Fact]
        public void GetApiDocumentation_FiltersCategory()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Category = "Category1" },
                new ApiResourceDocumentation { Category = "Category2" }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable { Category = "Category1" };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);

            result.Resources.Length.Should().Be(1);
            result.Resources[0].Category.Should().Be("Category1");
        }

        [Fact]
        public void GetApiDocumentation_HandlesUnknownCategory()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Category = "Category1" },
                new ApiResourceDocumentation { Category = "Category2" }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable { Category = "unknown" };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);

            result.Resources.Should().BeNullOrEmpty();
        }

        [Fact]
        public void GetApiDocumentation_MultipleFieldFilter()
        {
            var resources = new ApiResourceDocumentation[]
            {
                new ApiResourceDocumentation { Category = "Category1", Tags = new[] { "Tag1" }, Title = "DTO1" },
                new ApiResourceDocumentation { Category = "Category1", Tags = new[] { "Tag2" }, Title = "DTO2" },
                new ApiResourceDocumentation { Category = "Category2", Tags = new[] { "Tag2", "Tag3" }, Title = "DTO3" }
            };

            var documentation = new ApiDocumentation { Title = "Test Documentation", Resources = resources };
            var filter = new Filterable
            {
                Category = "Category2",
                Tags = new[] { "Tag2" },
                DtoName = new[] { "DTO3", "DTO2" }
            };

            var result = ApiDocumentationFilter.GetApiDocumentation(filter, documentation);
            result.Resources.Length.Should().Be(1);
            result.Resources[0].Title.Should().Be("DTO3");
        }
    }

    public class Filterable : IFilterableSpecRequest
    {
        public string[] DtoName { get; set; }
        public string Category { get; set; }
        public string[] Tags { get; set; }
    }
}