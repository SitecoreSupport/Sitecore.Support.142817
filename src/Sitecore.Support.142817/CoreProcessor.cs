using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Pipelines;
using Sitecore.Reflection;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Support.Pipelines
{
    public class CoreProcessor
    {
        private System.Xml.XmlNode _configNode;
        private readonly object _lock = new object();
        private PipelineMethod _method;
        private string _methodName = "Process";
        private string _name = ("_processor_" + MainUtil.GetNextSequence());
        private bool _runIfAborted;

        private PipelineMethod GetMethod(object[] parameters)
        {
            if (this._method == null)
            {
                lock (this._lock)
                {
                    if (this._method != null)
                    {
                        return this._method;
                    }
                    ProcessorObject processorObject = CorePipelineFactory.GetProcessorObject(this._configNode);
                    MethodInfo methodInfo = this.GetMethodInfo(processorObject, parameters);
                    PipelineMethod method = new PipelineMethod(processorObject.Object, methodInfo);
                    if (processorObject.Scope == ObjectScope.Invocation)
                    {
                        return method;
                    }
                    this._method = method;
                }
            }
            return this._method;
        }

        private MethodInfo GetMethodInfo(ProcessorObject obj, object[] parameters)
        {
            MethodInfo info = ReflectionUtil.GetMethod(obj.Object, this._methodName, parameters);
            if (info == null)
            {
                throw new InvalidOperationException("Could not find method: " + this._methodName + ". Pipeline: " + XmlUtil.GetPath(this._configNode));
            }
            return info;
        }

        public void Initialize(System.Xml.XmlNode configNode)
        {
            Assert.ArgumentNotNull(configNode, "configNode");
            string attribute = XmlUtil.GetAttribute("name", configNode);
            if (string.IsNullOrEmpty(attribute))
            {
                string[] values = new string[] { XmlUtil.GetAttribute("type", configNode) ?? XmlUtil.GetAttribute("ref", configNode) };
                attribute = StringUtil.GetString(values);
            }
            this._name = attribute;
            this._methodName = StringUtil.GetString(new string[] { XmlUtil.GetAttribute("method", configNode), this._methodName });
            this._runIfAborted = MainUtil.GetBool(XmlUtil.GetAttribute("runIfAborted", configNode), false);
            this._configNode = configNode;
        }
        public void Invoke(object[] parameters)
        {
            Assert.ArgumentNotNull(parameters, "parameters");
            this.GetMethod(parameters).Invoke(parameters);
        }

        public string MethodName =>
            this._methodName;

        public string Name =>
            this._name;

        public bool RunIfAborted =>
            this._runIfAborted;
    }
}
