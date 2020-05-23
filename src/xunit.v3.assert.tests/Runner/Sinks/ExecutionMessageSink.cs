using System;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    public class ExecutionMessageSink : IMessageSink
    {
        readonly Func<bool> cancelThunk;
        volatile int errors;

        public ExecutionMessageSink(Func<bool> cancelThunk = null)
        {
            this.cancelThunk = cancelThunk ?? (() => false);
        }

        /// <summary>
        /// Gets the final execution summary, once the execution is finished.
        /// </summary>
        public ExecutionSummary ExecutionSummary { get; } = new ExecutionSummary();

        /// <summary>
        /// Gets an event which is signaled once execution is finished.
        /// </summary>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        void HandleTestAssemblyFinished(ITestAssemblyFinished testAssemblyFinishedMessage)
        {
            ExecutionSummary.Total = testAssemblyFinishedMessage.TestsRun;
            ExecutionSummary.Failed = testAssemblyFinishedMessage.TestsFailed;
            ExecutionSummary.Skipped = testAssemblyFinishedMessage.TestsSkipped;
            ExecutionSummary.Time = testAssemblyFinishedMessage.ExecutionTime;
            ExecutionSummary.Errors = errors;

            //completionCallback?.Invoke(Path.GetFileNameWithoutExtension(testAssemblyFinishedMessage.Message.TestAssembly.Assembly.AssemblyPath), ExecutionSummary);

            Finished.Set();
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            if (message is IErrorMessage ||
                message is ITestAssemblyCleanupFailure ||
                message is ITestCaseCleanupFailure ||
                message is ITestClassCleanupFailure ||
                message is ITestCleanupFailure ||
                message is ITestCollectionCleanupFailure ||
                message is ITestMethodCleanupFailure)
            {
                Interlocked.Increment(ref errors);
            }

            if (message is IFailureInformation failureInfo)
            {
                Console.WriteLine("Failure!");
                foreach (var exceptionType in failureInfo.ExceptionTypes)
                    Console.WriteLine(exceptionType);
                foreach (var stackTrace in failureInfo.StackTraces)
                    Console.WriteLine(stackTrace);
            }

            if (message is ITestAssemblyFinished testAssemblyFinishedMessage)
                HandleTestAssemblyFinished(testAssemblyFinishedMessage);

            return !cancelThunk();
        }
    }
}
