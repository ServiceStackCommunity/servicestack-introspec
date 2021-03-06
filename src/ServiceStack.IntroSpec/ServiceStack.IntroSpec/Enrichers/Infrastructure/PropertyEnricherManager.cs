﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.IntroSpec.Enrichers.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Interfaces;
    using Models;

    /// <summary>
    /// Manages default logic for enriching Properties
    /// </summary>
    public class PropertyEnricherManager
    {
        private readonly IPropertyEnricher propertyEnricher;
        private readonly Action<IApiResourceType, Type> enrichResource;
        private static readonly Dictionary<Type, MemberInfo[]> PropertyDictionary = new Dictionary<Type, MemberInfo[]>();

        public PropertyEnricherManager(IPropertyEnricher propertyEnricher, Action<IApiResourceType, Type> enrichResource)
        {
            this.propertyEnricher = propertyEnricher;
            this.enrichResource = enrichResource;
        }

        public ApiPropertyDocumention[] EnrichParameters(ApiPropertyDocumention[] properties, Type dtoType)
        {
            if (propertyEnricher == null)
                return properties;

            // There might be a collection of Properties already - if so build up an easy lookup
            Dictionary<string, ApiPropertyDocumention> indexedParams;
            List<ApiPropertyDocumention> parameterDocuments = null;
            bool newList = false;

            MemberInfo[] allMembers = GetMemberInfo(dtoType);

            if (properties.IsNullOrEmpty())
            {
                //? Make this static to avoid needing to populate multiple times
                indexedParams = new Dictionary<string, ApiPropertyDocumention>();
                newList = true;
                parameterDocuments = new List<ApiPropertyDocumention>(allMembers.Length);
            }
            else
                indexedParams = properties.ToDictionary(k => k.Id, v => v);

            foreach (var mi in allMembers)
            {
                // Check if the property already exists. if so get it, If not create it 
                var property = indexedParams.SafeGet(mi.Name,
                    () => new ApiPropertyDocumention { Id = mi.Name, ClrType = mi.GetFieldPropertyType() });

                // Pass it to method to be populated.
                EnrichParameter(property, mi);

                if (newList)
                    parameterDocuments.Add(property);
            }

            // Do I need to return here?
            return newList ? parameterDocuments.ToArray() : properties;
        }

        private MemberInfo[] GetMemberInfo(Type dtoType)
        {
            // The same properties will be required multiple times (per enricher type) so build lookup
            return PropertyDictionary.SafeGetOrInsert(dtoType, () =>
                {
                    var allProperties = dtoType.GetSerializableProperties();
                    var allFields = dtoType.GetSerializableFields();
                    var allMembers = allProperties
                        .Select(p => p as MemberInfo)
                        .Union(allFields.Select(f => f as MemberInfo))
                        .Distinct()
                        .ToArray();
                    return allMembers;
                });
        }

        private void EnrichParameter(ApiPropertyDocumention property, MemberInfo mi)
        {
            if (property.Title == property.Id || string.IsNullOrEmpty(property.Title))
                property.Title = propertyEnricher.GetTitle(mi);

            property.Description = property.Description.GetIfNullOrEmpty(() => propertyEnricher.GetDescription(mi));
            property.Notes = property.Notes.GetIfNullOrEmpty(() => propertyEnricher.GetNotes(mi));
            property.ParamType = property.ParamType.GetIfNullOrEmpty(() => propertyEnricher.GetParamType(mi));
            property.Contraints = property.Contraints.GetIfNull(() => propertyEnricher.GetConstraints(mi));

            property.IsRequired = property.IsRequired.GetIfNoValue(() => propertyEnricher.GetIsRequired(mi));
            property.AllowMultiple = property.AllowMultiple.GetIfNoValue(() => propertyEnricher.GetAllowMultiple(mi));

            property.ExternalLinks = property.ExternalLinks.GetIfNullOrEmpty(() => propertyEnricher.GetExternalLinks(mi));

            EnrichEmbeddedResource(property, mi);
        }

        private void EnrichEmbeddedResource(ApiPropertyDocumention property, MemberInfo mi)
        {
            var fieldPropertyType = mi.GetFieldPropertyType();
            if (fieldPropertyType.IsSystemType())
                return;

            if (property.EmbeddedResource == null)
                property.EmbeddedResource = new ApiResourceType();

            enrichResource(property.EmbeddedResource, fieldPropertyType);
        }
    }
}
