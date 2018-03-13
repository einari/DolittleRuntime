﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
namespace Dolittle.Runtime.Tenancy
{
    /// <summary>
    /// Defines a system that can populate <see cref="ITenant"/>
    /// </summary>
    public interface ITenantPopulator
    {
        /// <summary>
        /// Populate a tenant 
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="details"></param>
        void Populate(ITenant tenant, dynamic details);
    }
}
