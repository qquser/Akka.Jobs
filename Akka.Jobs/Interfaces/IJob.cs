namespace Akka.Jobs.Interfaces;

public interface IJob<in TIn, out TOut>
    where TIn : IJobInput
    where TOut : IJobResult 
{
    Task<bool> DoAsync(TIn input, CancellationToken token);
    
    /// <summary>
    /// //Describes the state of the fields of the IJob class that are changed by the DoAsync method.
    /// </summary>
    TOut GetCurrentState(string jobId);
}