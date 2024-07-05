namespace Akka.Jobs.Interfaces;

/// <summary>
/// This IJob interface defines the interface for objects that represent background tasks.
/// These tasks are designed to run asynchronously in the background, without blocking the main application flow.
/// There is no need to create IJob instances manually.
/// This suggests that the library is responsible for creating and managing IJob,
/// providing a mechanism for registering and scheduling IJob tasks.
/// Management through IJobContext:
/// management of background tasks (starting, stopping, and querying their state) should be done exclusively through
/// the IJobContext interface.
/// </summary>
/// <typeparam name="TIn"> Input Parameter Data: This type parameter represents the type of data that will be
/// provided as input to the background task.</typeparam>
/// <typeparam name="TOut"> Current State Data: This type parameter represents the type of data that represents
/// the current state of the task.</typeparam>
public interface IJob<in TIn, out TOut>
    where TIn : IJobInput
    where TOut : IJobResult 
{
    /// <summary>
    /// Method describing the basic logic of the background task.
    /// </summary>
    /// <param name="input">Input data for the background task</param>
    /// <param name="token">The CancellationToken is managed by the library and is disabled when the Stop module is
    /// called from the IJobContext.</param>
    /// <returns>If IJobContext.DoJobAsync is waiting for the entire task to complete using the method,
    /// then the bool result of IJob.DoAsync will be added to JobDoneCommandResult</returns>
    Task<bool> DoAsync(TIn input, CancellationToken token);
    
    /// <summary>
    /// Describes the state of the fields of the IJob class that are changed by the DoAsync method.
    /// </summary>
    TOut GetCurrentState(string jobId);
}