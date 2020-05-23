using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Frameworks.v3;

namespace Xunit
{
    /// <summary>
    /// This class be used to do discovery and execution of xUnit.net v3 tests
    /// using a reflection-based implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    public class Xunit3 : Xunit3Discoverer, IFrontController, ITestCaseBulkDeserializer
    {
        ITestCaseBulkDeserializer defaultTestCaseBulkDeserializer;
        readonly ITestFrameworkExecutor remoteExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2"/> class.
        /// </summary>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="diagnosticMessageSink">The message sink which received <see cref="IDiagnosticMessage"/> messages.</param>
        public Xunit3(ISourceInformationProvider sourceInformationProvider,
                      string assemblyFileName,
                      string configFileName = null,
                      IMessageSink diagnosticMessageSink = null)
            : base(sourceInformationProvider, assemblyFileName, configFileName, diagnosticMessageSink)
        {
            var an = Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFileName) }).GetName();
            var assemblyName = new AssemblyName { Name = an.Name, Version = an.Version };

            remoteExecutor = Framework.GetExecutor(assemblyName);
        }

        /// <inheritdoc/>
        public List<KeyValuePair<string, ITestCase>> BulkDeserialize(List<string> serializations)
        {
            if (defaultTestCaseBulkDeserializer == null)
                defaultTestCaseBulkDeserializer = new DefaultTestCaseBulkDeserializer(remoteExecutor);

            return defaultTestCaseBulkDeserializer.BulkDeserialize(serializations);
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
            => remoteExecutor.Deserialize(value);

        /// <inheritdoc/>
        public override sealed void Dispose()
        {
            remoteExecutor.SafeDispose();

            base.Dispose();
        }

        /// <summary>
        /// Starts the process of running all the xUnit.net v3 tests in the assembly.
        /// </summary>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="discoveryOptions">The options to be used during test discovery.</param>
        /// <param name="executionOptions">The options to be used during test execution.</param>
        public void RunAll(IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
            => remoteExecutor.RunAll(messageSink, discoveryOptions, executionOptions);

        /// <summary>
        /// Starts the process of running the selected xUnit.net v3 tests.
        /// </summary>
        /// <param name="testCases">The test cases to run; if null, all tests in the assembly are run.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="executionOptions">The options to be used during test execution.</param>
        public void RunTests(IEnumerable<ITestCase> testCases, IMessageSink messageSink, ITestFrameworkExecutionOptions executionOptions)
            => remoteExecutor.RunTests(testCases, messageSink, executionOptions);
    }
}
