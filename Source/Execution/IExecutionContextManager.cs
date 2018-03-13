﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
namespace Dolittle.Runtime.Execution
{
    /// <summary>
    /// Defines a manager for managing <see cref="IExecutionContext">ExecutionContexts</see>
    /// </summary>
    public interface IExecutionContextManager
    {
        /// <summary>
        /// Get the current <see cref="IExecutionContext"/>
        /// </summary>
        IExecutionContext Current { get; }
    }
}
