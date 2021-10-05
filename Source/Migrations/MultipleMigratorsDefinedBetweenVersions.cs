// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Version = Dolittle.Runtime.Versioning.Version;

namespace Dolittle.Runtime.Migrations
{
    /// <summary>
    /// Exception that gets thrown when there are multiple migrators defined between two versions.
    /// </summary>
    public class MultipleMigratorsDefinedBetweenVersions : Exception
    {
        /// <summary>
        /// Instantiates an instance of the <see cref="MultipleMigratorsDefinedBetweenVersions"/> class.
        /// </summary>
        /// <param name="from">The version to migrate from.</param>
        /// <param name="to">The version to migrate to.</param>
        public MultipleMigratorsDefinedBetweenVersions(Version from, Version to)
            : base($"There are no migrators that can migrate between version {from} and {to}")
        {
        }
    }
}
