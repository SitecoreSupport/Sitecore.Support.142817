using Microsoft.Extensions.DependencyInjection;
using Sitecore;
using Sitecore.Abstractions;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Diagnostics.Profiling;
using Sitecore.Pipelines;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sitecore.Support.Pipelines
{
    public class CorePipeline
    {
        private readonly string _name;
        private bool _performanceCritical;
        private CoreProcessor[] _processors = new CoreProcessor[0];
        private static readonly Lazy<BaseCorePipelineManager> PipelineManager = new Lazy<BaseCorePipelineManager>(() => ServiceProviderServiceExtensions.GetRequiredService<BaseCorePipelineManager>(ServiceLocator.ServiceProvider));

        public CorePipeline(string pipelineName)
        {
            Assert.ArgumentNotNullOrEmpty(pipelineName, "pipelineName");
            this._name = pipelineName;
        }

        private void AddProcessors(System.Xml.XmlNode pipelineNode)
        {
            System.Xml.XmlNodeList list = pipelineNode.SelectNodes("processor");
            CoreProcessor[] processorArray = new CoreProcessor[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                processorArray[i] = this.CreateProcessor(list[i]);
            }
            this._processors = processorArray;
        }

        private CoreProcessor CreateProcessor(System.Xml.XmlNode processorNode)
        {
            CoreProcessor processor = new CoreProcessor();
            processor.Initialize(processorNode);
            return processor;
        }

        public void Initialize(System.Xml.XmlNode configNode)
        {
            Assert.ArgumentNotNull(configNode, "configNode");
            Assert.IsTrue(configNode.LocalName == this._name, "Invalid configuration node. Pipeline name: " + this._name + ". Config node name: " + configNode.LocalName);
            this._performanceCritical = MainUtil.GetBool(XmlUtil.GetAttribute("performanceCritical", configNode), false);
            this.AddProcessors(configNode);
        }

        public void Run(PipelineArgs args)
        {
            //Sitecore.Support.8888
            //args.Initialize();
            Assert.ArgumentNotNull(args, "args");
            Assembly asm = Assembly.Load("Sitecore.Kernel");
            Type type = asm.GetType("Sitecore.Pipelines.PipelineArgs");
            MethodInfo Initialize = type.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
            Initialize.Invoke(args, null);

            object[] parameters = new object[] { args };
            if (this._performanceCritical)
            {
                for (int i = 0; i < this._processors.Length; i++)
                {
                    CoreProcessor processor = this._processors[i];
                    if (!args.Aborted || processor.RunIfAborted)
                    {
                        processor.Invoke(parameters);
                    }
                }
            }
            else
            {
                using (IPipelineProfilingScope scope = ProfilerApi.StartPipelineScope(this._name, this._processors.Length))
                {
                    for (int j = 0; j < this._processors.Length; j++)
                    {
                        CoreProcessor processor2 = this._processors[j];
                        scope.RegisterProcessor(j, processor2.Name, processor2.MethodName);
                        if (!args.Aborted || processor2.RunIfAborted)
                        {
                            using (scope.StartProcessorScope(j))
                            {
                                processor2.Invoke(parameters);
                            }
                            JobsCount.PipelinesProcessorsExecuted.Increment(1L);
                        }
                    }
                }
                JobsCount.PipelinesPipelinesExecuted.Increment(1L);
                if (args.Aborted)
                {
                    JobsCount.PipelinesPipelinesAborted.Increment(1L);
                }
            }
        }

        public static void Run(string pipelineName, PipelineArgs args)
        {
            PipelineManager.Value.Run(pipelineName, args);
        }

        public static void Run(string pipelineName, PipelineArgs args, bool failIfNotExists)
        {
            PipelineManager.Value.Run(pipelineName, args, failIfNotExists);
        }

        public static void Run(string pipelineName, PipelineArgs args, string pipelineDomain)
        {
            PipelineManager.Value.Run(pipelineName, args, pipelineDomain);
        }

        public static void Run(string pipelineName, PipelineArgs args, string pipelineDomain, bool failIfNotExists)
        {
            Assert.ArgumentNotNullOrEmpty(pipelineName, "pipelineName");
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(pipelineDomain, "pipelineDomain");
            PipelineManager.Value.Run(pipelineName, args, pipelineDomain, failIfNotExists);
        }

        public bool IsEmpty =>
            (this._processors.Length == 0);

        public string Name =>
            this._name;
    }
}