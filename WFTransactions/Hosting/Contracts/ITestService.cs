using System.ServiceModel;

namespace Hosting.Contracts
{
    [ServiceContract(Namespace = "http://wftransactions.localtest.me")]
    public interface ITestService
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        [return: MessageParameter(Name = "instanceId")]
        string Initiate();
    }
}
