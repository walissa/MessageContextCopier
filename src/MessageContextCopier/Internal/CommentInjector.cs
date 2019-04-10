using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.BizTalk.Streaming;
using System.IO;
using System.Xml;
namespace BizTalkComponents.PipelineComponents.MessageContextCopier.Internal
{
    class CommentInjector : XmlTranslatorStream
    {
        private readonly string _comment;

        public CommentInjector(Stream input, string comment, Encoding encoding)
            : base(new XmlTextReader(input), encoding)
        {            
            _comment=comment;
        }

        protected override void TranslateEndElement(bool full)
        {
            if (m_reader.Depth == 0)
                m_writer.WriteComment(_comment);
            base.TranslateEndElement(full);
        }

        protected override void TranslateXmlDeclaration(string target, string val)
        {
            m_writer.WriteProcessingInstruction(target, val);
        }
    }
}
