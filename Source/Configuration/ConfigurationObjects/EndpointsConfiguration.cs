// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Dolittle.Runtime.Configuration.ConfigurationObjects;

/// <summary>
/// Represents the configuration for hosts by <see cref="EndpointVisibility"/>.
/// </summary>
public class EndpointsConfiguration :
    Dictionary<EndpointVisibility, EndpointConfiguration>
    // IConfigurationObject
{
}
