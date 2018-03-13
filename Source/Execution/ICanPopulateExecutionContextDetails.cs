﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
namespace Dolittle.Runtime.Execution
{
    /// <summary>
    /// Defines a visitor that takes part in populating all the details for the <see cref="IExecutionContext"/>.
    /// </summary>
    /// <remarks>
    /// Types inheriting from this interface will be automatically registered.
    /// An application can implement any number of these conventions.
    /// </remarks>
    public interface ICanPopulateExecutionContextDetails
    {
        /// <summary>
        /// Method that gets called when the <see cref="IExecutionContext"/> is being set up.
        /// </summary>
        /// <param name="executionContext"><see cref="IExecutionContext"/> that is populated.</param>
        /// <param name="details">Details for the <see cref="IExecutionContext"/> to populate.</param>
        void Populate(IExecutionContext executionContext, dynamic details);
    }
}
