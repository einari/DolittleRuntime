// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Rudimentary;

namespace Dolittle.Runtime.Projections.Store.Definition.Copies;

/// <summary>
/// Represents a field in a Projection read model.
/// </summary>
/// <param name="Value"></param>
public record ProjectionField(string Value) : ConceptAs<string>(Value);
