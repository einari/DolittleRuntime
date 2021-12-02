// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dolittle.Runtime.Rudimentary.given;

public record InheritingFromLongConcept(long value) : LongConcept(value)
{
    public static implicit operator InheritingFromLongConcept(long value) => new(value);
}