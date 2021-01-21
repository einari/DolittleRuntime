// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dolittle.Runtime.Booting
{
    /// <summary>
    /// Defines the system for performing operations before a specific <see cref="BootStage"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="IRepresentSettingsForBootStage"/> type.</typeparam>
    public interface ICanRunBeforeBootStage<T> : ICanPerformPartOfBootStage<T>
        where T : IRepresentSettingsForBootStage
    {
    }
}
