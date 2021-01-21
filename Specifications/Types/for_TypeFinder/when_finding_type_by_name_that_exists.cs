﻿// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Machine.Specifications;

namespace Dolittle.Runtime.Types.for_TypeFinder
{
    [Subject(typeof(TypeFinder))]
    public class when_finding_type_by_name_that_exists : given.a_type_finder
    {
        static Type type_found;

        Because of = () => type_found = type_finder.FindTypeByFullName(typeof(Single).FullName);

        It should_not_return_null = () => type_found.ShouldNotBeNull();
        It should_return_the_correct_type = () => type_found.ShouldEqual(typeof(Single));
    }
}