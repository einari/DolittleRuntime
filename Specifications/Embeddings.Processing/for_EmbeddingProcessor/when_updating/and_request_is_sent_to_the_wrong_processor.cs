// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Dolittle.Runtime.ApplicationModel;
using Dolittle.Runtime.Execution;
using Dolittle.Runtime.Security;
using Machine.Specifications;
using Environment = Dolittle.Runtime.Execution.Environment;
using Version = Dolittle.Runtime.Versioning.Version;

namespace Dolittle.Runtime.Embeddings.Processing.for_EmbeddingProcessor.when_updating;

public class and_request_is_sent_to_the_wrong_processor : given.all_dependencies_and_a_desired_state
{
    static Task task;

    private Establish context = () =>
    {
        task = embedding_processor.Start(cancellation_token);
        execution_context_manager
            .SetupGet(_ => _.Current)
            .Returns(new ExecutionContext(
                MicroserviceId.NotSet,
                "6cc0728e-efc2-4786-8029-e1a83c95f964",
                Version.NotSet,
                Environment.Undetermined,
                CorrelationId.Empty,
                Claims.Empty,
                CultureInfo.InvariantCulture));
    };
    
    static Exception result;

    Because of = () => result = Catch.Exception(() => embedding_processor.Update(key, desired_state, cancellation_token).GetAwaiter().GetResult());
    
    It should_still_be_running = () => task.Status.ShouldEqual(TaskStatus.WaitingForActivation);
    It should_fail = () => result.ShouldBeOfExactType<EmbeddingRequestWorkScheduledForWrongTenant>();
}