﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
namespace Dolittle.Runtime.Tenancy
{
    /// <summary>
    /// Defines a tenant in the system
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// Gets the <see cref="TenantId">identifier</see> of the <see cref="ITenant"/>
        /// </summary>
        TenantId TenantId { get; }

        /// <summary>
        /// Gets the details for the tenant
        /// </summary>
        dynamic Details { get; }
    }
}