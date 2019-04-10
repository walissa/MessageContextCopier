using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BizTalkComponents.Utils;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;
using System.ComponentModel;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using Microsoft.XLANGs.BaseTypes;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.BizTalk.Streaming;
using System.Text.RegularExpressions;
using BizTalkComponents.PipelineComponents.MessageContextCopier.Internal;

namespace BizTalkComponents.PipelineComponents.MessageContextCopier
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    [System.Runtime.InteropServices.Guid("9d0e4103-4cce-4536-83fa-4a5040674ad6")]
    public partial class MessageContextCopier : IBaseComponent, IComponent, IComponentUI, IPersistPropertyBag
    {
        [RequiredRuntime]
        [DisplayName("Show Property Info As Nodes")]
        [Description("Show property info (name, namespace, promoted, and value) as nodes or as attributes")]
        public bool ShowPropertyInfoAsNodes { get; set; }

        [DisplayName("Selected Properties")]
        [Description("http://namespace1#prop1;http://namespace2/prop2")]
        [RegularExpression(@"^([^;#]+#[^;#]+;?)*$", ErrorMessage = "A property path should be formatted as namespace#property.")]
        public string SelectedProperties { get; set; }

        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            if (!Enabled)
            {
                return pInMsg;
            }
            string errorMessage;
            if (!Validate(out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }
            var contentReader = new ContentReader();

            var data = pInMsg.BodyPart.GetOriginalDataStream();
            const int bufferSize = 0x280;
            const int thresholdSize = 0x100000;

            if (!data.CanSeek || !data.CanRead)
            {

                data = new ReadOnlySeekableStream(data, new VirtualStream(bufferSize, thresholdSize), bufferSize);
                pContext.ResourceTracker.AddResource(data);
            }
            if (contentReader.IsXmlContent(data))
            {
                var encoding = contentReader.Encoding(data);
                string nodeFormat = ShowPropertyInfoAsNodes ?
                    "<property><name>{0}</name><namespace>{1}</namespace><promoted>{2}</promoted><value>{3}</value></property>"
                    : "<property name=\"{0}\" namespace=\"{1}\" promoted=\"{2}\">{3}</property>";
                string msgctx = "", propName, propNS;
                object retval;
                if (string.IsNullOrEmpty(SelectedProperties))
                    for (int i = 0; i < pInMsg.Context.CountProperties; i++)
                    {
                        retval = pInMsg.Context.ReadAt(i, out propName, out propNS);
                        msgctx += string.Format(nodeFormat, propName, propNS, pInMsg.Context.IsPromoted(propName, propNS), retval);
                    }
                else
                {
                    var selectedProps = SelectedProperties.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < selectedProps.Length; i++)
                    {
                        var property = new ContextProperty(selectedProps[i]);
                        retval = pInMsg.Context.Read(property);
                        if (retval != null)
                            msgctx += string.Format(nodeFormat, property.PropertyName, property.PropertyNamespace, pInMsg.Context.IsPromoted(property), retval);
                    }
                }
                msgctx = "<messagecontext>" + msgctx + "</messagecontext>";

                data = new CommentInjector(data, msgctx, encoding);
                data = new ReadOnlySeekableStream(data, new VirtualStream(bufferSize, thresholdSize), bufferSize);
                pContext.ResourceTracker.AddResource(data);
                pInMsg.BodyPart.Data = data;
            }
            else
            {
                data.Seek(0, SeekOrigin.Begin);
                pInMsg.BodyPart.Data = data;
            }
            return pInMsg;
        }
    }
}