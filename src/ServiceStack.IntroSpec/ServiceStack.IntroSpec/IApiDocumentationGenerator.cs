// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.IntroSpec
{
    using System.Collections.Generic;
    using Host;
    using Models;
    using Settings;

    /// <summary>
    /// Basic signature used to generate documentation POCO
    /// </summary>
    public interface IApiDocumentationGenerator
    {
        /// <summary>
        /// Generate documentation object from supplied values
        /// </summary>
        /// <param name="operations">List of metadata operations</param>
        /// <param name="appHost">Running IAppHost</param>
        /// <param name="config">Extra configuration values</param>
        /// <returns></returns>
        ApiDocumentation GenerateDocumentation(IEnumerable<Operation> operations, IAppHost appHost, ApiSpecConfig config);
    }
}