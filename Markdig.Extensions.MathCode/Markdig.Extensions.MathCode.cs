using Markdig;
using Wolfram.NETLink;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Helpers;

namespace Markdig.Extensions.MathCode
{
    public class Netlink
    {
        public IKernelLink? Kernel { get; private set; }
        public int Valid { get; private set; }
        public int Init()
        {
            Kernel = MathLinkFactory.CreateKernelLink("-linkmode launch -linkname wolframkernel");
            Kernel.WaitAndDiscardAnswer();
            Valid = 1;
            return 0;
        }
        public int Finish()
        {
            if (Kernel != null && Valid == 1)
            {
                Kernel.Close();
                Valid = 0;
            }
            return 0;
        }
    }

    public class MathCode : FencedCodeBlock
    {
        public MathCode(BlockParser parser) : base(parser) { }
        private static int Input = 0;
        public static void Inc()
        {
            Input++;
        }
        public static int InID()
        {
            return Input;
        }
        public string GetScript()
        {
            if (Lines.Count > 0)
            {
                return string.Join(Environment.NewLine, Lines);
            }
            else
                return "";
        }
    }

    public class MathCodeRenderer : HtmlObjectRenderer<MathCode>
    {
        private readonly Netlink lnk;
        public bool OutputAttributesOnPre { get; set; }
        public HashSet<string> BlocksAsDiv { get; }
        public MathCodeRenderer(Netlink link)
        {
            lnk = link;
            BlocksAsDiv = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        protected override void Write(HtmlRenderer renderer, MathCode obj)
        {
            renderer.EnsureLine();

            bool showcode = true;
            string showtype = "";
            HtmlAttributes? attr = obj.TryGetAttributes();
            if (attr == null)
            {
                return;
            }

            if (attr.Classes == null)
            {
                return;
            }
            for (int i = 0; i < attr.Classes.Count; i++)
            {
                var cssClass = attr.Classes[i];
                if (cssClass.Trim() == "math0")
                {
                    showcode = false;
                    break;
                }
                else if (cssClass.Trim().StartsWith("math0/"))
                {
                    showcode = false;
                    showtype = cssClass.Trim().Substring(6);
                }
                else if (cssClass.Trim() == "math")
                {
                    showcode = true;
                }
                else if (cssClass.Trim().StartsWith("math/"))
                {
                    showcode = true;
                    showtype = cssClass.Trim().Substring(5);
                }
            }
            var fencedCodeBlock = obj as FencedCodeBlock;
            if (fencedCodeBlock?.Info != null && BlocksAsDiv.Contains(fencedCodeBlock.Info))
            {
                var infoPrefix = (obj.Parser as FencedCodeBlockParser)?.InfoPrefix ??
                                 FencedCodeBlockParser.DefaultInfoPrefix;

                // We are replacing the HTML attribute `language-mylang` by `mylang` only for a div block
                // NOTE that we are allocating a closure here
                if (renderer.EnableHtmlForBlock && showcode)
                {
                    //renderer.Write("<div")
                    //        .WriteAttributes(obj.TryGetAttributes(),
                    //            cls => cls.StartsWith(infoPrefix, StringComparison.Ordinal) ? cls.Substring(infoPrefix.Length) : cls)
                    //        .Write('>');
                    renderer.Write("<div class=\"mathematica\"");
                }
                if (showcode)
                    renderer.WriteLeafRawLines(obj, true, true, true);

                if (renderer.EnableHtmlForBlock && showcode)
                {
                    renderer.WriteLine("</div>");
                }

            }
            else
            {
                if (renderer.EnableHtmlForBlock && showcode)
                {
                    renderer.Write("<pre");

                    if (OutputAttributesOnPre)
                    {
                        //renderer.WriteAttributes(obj);
                        renderer.Write(" class=\"language-mathematica\"");
                    }

                    renderer.Write("><code");

                    if (!OutputAttributesOnPre)
                    {
                        renderer.Write(" class=\"language-mathematica\"");
                    }

                    renderer.Write('>');
                }

                if (showcode)
                {
                    renderer.WriteLeafRawLines(obj, true, true);
                }


                if (renderer.EnableHtmlForBlock && showcode)
                {
                    renderer.WriteLine("</code></pre>");
                }
            }

            renderer.EnsureLine();


            if (lnk.Valid == 0)
            {
                lnk.Init();
            }
            if (lnk.Kernel == null)
            {
                return;
            }
            string rst = lnk.Kernel.EvaluateToInputForm(obj.GetScript(), 0);
            MathCode.Inc();
            int id = MathCode.InID();
            //if (!Directory.Exists("math"))
            //{
            //    Directory.CreateDirectory("math");
            //}
            if (showtype.ToLower() == "null")
            {
                return;
            }
            if (showtype.ToLower() == "raw")
            {
                renderer.Write("<div>")
                        .Write(lnk.Kernel.EvaluateToOutputForm(obj.GetScript(), 0))
                        .Write("</div>");
            }
            else if (rst.StartsWith("Sound[") && showtype == "")
            {
                string b64 = lnk.Kernel.EvaluateToOutputForm("ExportString[ExportString[" + rst + ",\"wav\"],\"Base64\"]", 80);
                renderer.WriteLine("<div><audio controls><source src=")
                        .Write("\"data:audio/wav;base64,")
                        .Write(b64)
                        .Write("\"")
                        .Write("></audio controls></div>");
                //.Write("\"math/out" + id.ToString() + ".mout\" type=audio/wav")
                //lnk.Kernel.Evaluate("Export[\"math/out" + id.ToString() + ".mout\"," + rst + ",\"wav\"]");
                //lnk.Kernel.WaitAndDiscardAnswer();
            }
            else if (rst != "Null" && showtype == "")
            {
                string b64 = lnk.Kernel.EvaluateToOutputForm("ExportString[ExportString[" + rst + ",\"gif\"],\"Base64\"]", 80);
                renderer.WriteLine("<div><img src=")
                        .Write("\"data:image/gif;base64,")
                        .Write(b64)
                        .Write("\"")
                        .Write("/></div>");
                //.Write("\"math/out" + id.ToString() + ".mout\"")
                //lnk.Kernel.Evaluate("Export[\"math/out" + id.ToString() + ".mout\"," + rst + ",\"png\"]");
                //lnk.Kernel.WaitAndDiscardAnswer();
            }
            else if (rst != "Null" && showtype.ToLower() == "svg")
            {
                string b64 = lnk.Kernel.EvaluateToOutputForm("ExportString[" + rst + ", \"" + "svg" + "\"]", 0);

                renderer.Write("<div>\n");
                renderer.Write(b64);
                renderer.Write("</div>");
            }
            else if (rst != "Null" && showtype.ToLower() == "mathml")
            {
                string b64 = lnk.Kernel.EvaluateToOutputForm("ExportString[" + rst + ", \"" + "mathml" + "\"]", 0);

                renderer.Write("<div>\n");
                renderer.Write(b64);
                renderer.Write("</div>");
            }
            else if (rst != "Null")
            {

                string b64 = lnk.Kernel.EvaluateToOutputForm("ExportString[ExportString[" + rst + ", \"" + showtype + "\"],\"Base64\"]", 80);
                renderer.WriteLine("<div><img src=")
                        .Write("\"data:image/" + showtype + ";base64,")
                        .Write(b64)
                        .Write("\"")
                        .Write("/></div>");
            }
            renderer.EnsureLine();
        }
    }
    public class MathCodeParser : FencedBlockParserBase<MathCode>
    {
        public MathCodeParser()
        {
            OpeningCharacters = new[] { '`' };
            InfoPrefix = "";
            InfoParser = MathCodeInfoParser;
        }
        protected override MathCode CreateFencedBlock(BlockProcessor processor)
        {
            var block = new MathCode(this);
            return block;
        }

        public static bool MathCodeInfoParser(BlockProcessor state, ref StringSlice line, IFencedBlock fenced, char openingCharacter)
        {
            string infoString;
            string argString = null;

            var c = line.CurrentChar;
            // An info string cannot contain any backsticks
            int firstSpace = -1;
            for (int i = line.Start; i <= line.End; i++)
            {
                c = line.Text[i];
                if (c == '`')
                {
                    return false;
                }

                if (firstSpace < 0 && c.IsSpaceOrTab())
                {
                    firstSpace = i;
                }
            }

            if (firstSpace > 0)
            {
                infoString = line.Text.Substring(line.Start, firstSpace - line.Start).Trim();

                // Skip any spaces after info string
                firstSpace++;
                while (true)
                {
                    c = line[firstSpace];
                    if (c.IsSpaceOrTab())
                    {
                        firstSpace++;
                    }
                    else
                    {
                        break;
                    }
                }

                argString = line.Text.Substring(firstSpace, line.End - firstSpace + 1).Trim();
            }
            else
            {
                infoString = line.ToString().Trim();
            }
            if ((infoString != "math") && (infoString != "math0") && (!infoString.StartsWith("math/")) && (!infoString.StartsWith("math0/")))
                return false;

            fenced.Info = HtmlHelper.Unescape(infoString);
            fenced.Arguments = HtmlHelper.Unescape(argString);

            return true;
        }
    }

    public class MathCodeExtension : IMarkdownExtension
    {
        private readonly Netlink lnk;
        public MathCodeExtension(Netlink link)
        {
            lnk = link;
        }

        void IMarkdownExtension.Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<MathCodeParser>())
            {
                pipeline.BlockParsers.Insert(0, new MathCodeParser());
            }
        }

        void IMarkdownExtension.Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            HtmlRenderer htmlRenderer;
            ObjectRendererCollection renderers;

            htmlRenderer = renderer as HtmlRenderer;
            renderers = htmlRenderer?.ObjectRenderers;
            if (renderers != null && !renderers.Contains<MathCodeRenderer>())
            {
                renderers.Insert(0, new MathCodeRenderer(lnk));
            }
        }
    }

    public static class MathCodeExtensionFunctions
    {
        public static MarkdownPipelineBuilder UseMathCode(this MarkdownPipelineBuilder pipeline, Netlink lnk)
        {
            if (!pipeline.Extensions.Contains<MathCodeExtension>())
            {
                pipeline.Extensions.Add(new MathCodeExtension(lnk));
            }
            return pipeline;
        }
    }
}