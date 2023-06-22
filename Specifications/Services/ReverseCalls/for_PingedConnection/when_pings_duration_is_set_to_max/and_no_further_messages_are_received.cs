// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Services.ReverseCalls.for_PingedConnection.given;
using Machine.Specifications;

namespace Dolittle.Runtime.Services.ReverseCalls.for_PingedConnection.when_pings_duration_is_set_to_max;

public class and_no_further_messages_are_received : given.all_dependencies
{
    Establish context = () =>
    {
        scenario = Scenario.New(_ =>
        {
            _.Receive.Message(first_message_with_0_second_pings).AtTime(6);
        });
    };

    Because of = () => scenario.Simulate(
        request_id,
        server_call_context,
        message_converter.Object,
        metrics,
        logger_factory);

    It should_not_schedule_a_ping_callback = () => scenario.ScheduledCallbacks.ShouldBeEmpty();
    It should_not_have_refreshed_the_token = () => scenario.RefreshedTokenTimes.ShouldBeEmpty();
}