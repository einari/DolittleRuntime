﻿// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Runtime.Events;
using Machine.Specifications;

namespace Dolittle.Events.Specs.for_EventSource
{
    [Subject(Subjects.creating_event_source)]
    public class when_creating_a_new_event_source : given.a_stateful_event_source
    {
        It should_have_a_generated_id = () => ((Guid)event_source.EventSourceId).ShouldNotEqual(Guid.Empty);
        It should_not_have_any_uncommitted_events = () => event_source.UncommittedEvents.ShouldBeEmpty();
        It should_have_a_version_of_zero = () => event_source.Version.ShouldEqual(EventSourceVersion.Initial);
    }
}