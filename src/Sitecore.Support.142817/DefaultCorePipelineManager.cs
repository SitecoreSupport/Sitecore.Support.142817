using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Abstractions;
using Sitecore.Pipelines;
using Sitecore.Diagnostics;
using System.Collections;

namespace Sitecore.Support.Pipelines
{
    public class DefaultCorePipelineManager : Sitecore.Pipelines.DefaultCorePipelineManager
    {
        public DefaultCorePipelineManager(BaseFactory factory) : base(factory)
        {
            this.factory = factory;
        }
        private readonly BaseFactory factory;
        private readonly Hashtable pipelines = new Hashtable();
        private System.Xml.XmlNode GetPipelineNode(string pipelineName, string pipelineGroup)
        {
            string str = string.Empty;
            if (pipelineGroup.Length > 0)
            {
                str = "group[@groupName='" + pipelineGroup + "']/pipelines/";
            }
            string xpath = "pipelines/" + str + pipelineName;
            return this.factory.GetConfigNode(xpath);
        }
        private CorePipeline CreatePipeline(string pipelineName, string pipelineGroup)
        {
            System.Xml.XmlNode pipelineNode = this.GetPipelineNode(pipelineName, pipelineGroup);
            if (pipelineNode != null)
            {
                CorePipeline pipeline = new CorePipeline(pipelineName);
                pipeline.Initialize(pipelineNode);
                return pipeline;
            }
            return null;
        }
        public new CorePipeline GetPipeline(string pipelineName, string pipelineGroup)
        {
            Assert.ArgumentNotNullOrEmpty(pipelineName, "pipelineName");
            Assert.ArgumentNotNull(pipelineGroup, "pipelineGroup");
            string str = pipelineGroup + @"\" + pipelineName;
            CorePipeline pipeline = (CorePipeline)this.pipelines[str];
            if (pipeline == null)
            {
                pipeline = this.CreatePipeline(pipelineName, pipelineGroup);
                if (pipeline == null)
                {
                    return pipeline;
                }
                lock (this.pipelines.SyncRoot)
                {
                    if (this.pipelines[str] == null)
                    {
                        this.pipelines[str] = pipeline;
                    }
                }
            }
            return pipeline;
        }
        public override void Run(string pipelineName, PipelineArgs args, string pipelineDomain, bool failIfNotExists)
        {
            Assert.ArgumentNotNullOrEmpty(pipelineName, "pipelineName");
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(pipelineDomain, "pipelineDomain");
            CorePipeline pipeline = this.GetPipeline(pipelineName, pipelineDomain);
            if ((pipeline != null) || failIfNotExists)
            {
                Assert.IsNotNull(pipeline, "Could not get pipeline: {0} (domain: {1})", new object[] { pipelineName, pipelineDomain });
                pipeline.Run(args);
            }
        }
    }
}
