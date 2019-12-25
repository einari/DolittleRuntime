// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Machine.Specifications;

namespace Dolittle.Runtime.Events.for_EventSourceVersion.when_incrementing_the_commit
{
    [Subject(typeof(EventSourceVersion), "NextCommit")]
    public class on_the_initial_version
    {
        static EventSourceVersion result;

        Because of = () => result = EventSourceVersion.Initial.NextCommit();

        It should_be_the_second_commit = () => result.Commit.ShouldEqual(2ul);
    }
}