using System;
using BizTalkComponents.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;
using Microsoft.BizTalk.Component;
using System.Net.Mail;
using BizTalkComponents.PipelineComponents.MessageContextCopier;
using System.Text;
using System.IO;
namespace BizTalkComponents.PipelineComponents.MessageContextCopier.Tests.UnitTests
{
    [TestClass]
    public class EFactNamespaceCustomizerTester
    {
        [TestMethod]
        public void InjectMessageContextAsComment()
        {
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var component = new MessageContextCopier { 
                Enabled=true,
                ShowPropertyInfoAsNodes=true,
                SelectedProperties = "http://schemas.microsoft.com/BizTalk/2003/file-properties#ReceivedFileName;http://schemas.microsoft.com/BizTalk/2003/system-properties#SchemaStrongName;"
            };
            pipeline.AddComponent(component, PipelineStage.Decode);                        
            var message = MessageHelper.CreateFromString(@"<TestMessage><node1><node2>value</node2></node1></TestMessage>");
            message.Context.Promote(new ContextProperty(FileProperties.ReceivedFileName),"FileName");
            message.Context.Write(new ContextProperty(SystemProperties.SchemaStrongName), "testschema");
            message.Context.Promote(new ContextProperty(SystemProperties.IsRequestResponse), "true");
            var output = pipeline.Execute(message);
            var retStr= MessageHelper.ReadString(message);
            Assert.IsFalse(retStr.Contains(@"<messagecontex>"));
        }

    }
}
