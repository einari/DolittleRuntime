// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Projections.Store.Definition;

namespace Dolittle.Runtime.Events.Processing.Projections;

/// <summary>
/// Defines a validator for <see cref="OccurredFormat"/>.
/// </summary>
public interface IValidateOccurredFormat
{
    /// <summary>
    /// Checks whether the given <see cref="OccurredFormat"/> is valid.
    /// </summary>
    /// <param name="format">The <see cref="OccurredFormat"/>to check</param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    bool IsValid(OccurredFormat format, out string errorMessage);
}
