using System;
using System.Activities;
using System.Activities.Statements;
using System.Activities.Validation;
using System.ServiceModel.Activities;

namespace Hosting.Activities
{
    public class Initiate : Activity
    {
        private sealed class ReadWorkflowInstanceId : CodeActivity<string>
        {
            protected override string Execute(CodeActivityContext context)
            {
                return context.WorkflowInstanceId.ToString("D");
            }
        }

        public Initiate()
        {
            this.Implementation = () =>
            {
                var workflowInstanceId = new Variable<string>("workflowInstanceId");

                var readWorkflowInstanceId = new ReadWorkflowInstanceId
                {
                    Result = new OutArgument<string>(workflowInstanceId),
                };

                var receive = new Receive
                {
                    CanCreateInstance = true,
                    OperationName = "Initiate",
                    ServiceContractName = this.ServiceContractName,
                    Content = new ReceiveParametersContent(),
                    SerializerOption = SerializerOption.DataContractSerializer,
                };

                var reply = new SendReply
                {
                    Request = receive,
                    Content = new SendParametersContent
                    {
                        Parameters =
                        {
                            { "instanceId", new InArgument<string>(workflowInstanceId) },
                        },
                    },
                };

                var performTransactedReceive = new TransactedReceiveScope
                {
                    Request = receive,
                    Body = reply
                };

                return new Sequence
                {
                    Variables = { workflowInstanceId, },
                    Activities =
                    {
                        readWorkflowInstanceId,
                        performTransactedReceive,
                    },
                };
            };
        }

        public string ServiceContractName { get; set; }

        protected override sealed Func<Activity> Implementation
        {
            get { return base.Implementation; }
            set { base.Implementation = value; }
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (string.IsNullOrWhiteSpace(this.ServiceContractName))
            {
                var serviceContractNameMandatoryError = new ValidationError(
                    "The service contract name must be specified.",
                    false,
                    "ServiceContractName");

                metadata.AddValidationError(serviceContractNameMandatoryError);
            }
        }
    }
}
