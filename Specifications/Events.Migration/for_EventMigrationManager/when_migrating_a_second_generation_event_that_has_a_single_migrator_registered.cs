﻿// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Events;
using Dolittle.Runtime.Events.Migration.Specs.Fakes.v2;
using Dolittle.Runtime.Events.Migration.Specs.for_EventMigrationService.given;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Migration.Specs.for_EventMigrationService
{
    public class when_migrating_a_second_generation_event_that_has_a_single_migrator_registered : an_event_with_a_migrator
    {
        static IEvent result;

        Because of = () => result = event_migrator_manager.Migrate(source_event);

        It should_migrate_the_event_to_the_second_generation_type = () => result.ShouldBeOfExactType(typeof(SimpleEvent));

        It should_migrate_the_correct_values = () =>
                                                   {
                                                       var v2 = result as SimpleEvent;
                                                       v2.SecondGenerationProperty.ShouldEqual(SimpleEvent.DEFAULT_VALUE_FOR_SECOND_GENERATION_PROPERTY);
                                                   };
    }
}