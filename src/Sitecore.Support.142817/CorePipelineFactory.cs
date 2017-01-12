using Sitecore.Exceptions;
using Sitecore.Pipelines;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Support.Pipelines
{
    public static class CorePipelineFactory
    {
        internal static ProcessorObject GetProcessorObject(System.Xml.XmlNode processorNode)
        {
            //Sitecore.Support.8888
            //string attribute = XmlUtil.GetAttribute("ref", processorNode);
            //if (attribute.Length > 0)
            //{
            //    return GetObjectFromName(attribute, processorNode);
            //}
            //ProcessorObject objectFromType = GetObjectFromType(processorNode);
            //if (objectFromType == null)
            //{
            //    throw new InvalidValueException("Processor node contains neither type or ref attribute");
            //}
            //return objectFromType;

            Assembly asm = Assembly.Load("Sitecore.Kernel");
            Type type = asm.GetType("Sitecore.Pipelines.CorePipelineFactory");
            MethodInfo GetObjectFromName = type.GetMethod("GetObjectFromName", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo GetObjectFromType = type.GetMethod("GetObjectFromType", BindingFlags.NonPublic | BindingFlags.Static);

            string attribute = XmlUtil.GetAttribute("type", processorNode);
            if (attribute.Length > 0)
            {
                return (ProcessorObject)GetObjectFromType.Invoke(null, new object[] { processorNode });
            }
            string objectName = XmlUtil.GetAttribute("ref", processorNode);
            if (objectName.Length <= 0)
            {
                throw new InvalidValueException("Processor node contains neither type or ref attribute");
            }
            return (ProcessorObject)GetObjectFromName.Invoke(null, new object[] { objectName, processorNode });
        }
    }
}