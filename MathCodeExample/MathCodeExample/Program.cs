using Markdig;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.MathCode;

namespace MathCodeExample
{
    class Program
    {
        public static int Main(string[] args)
        {
            string appName = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (args.Length == 0)
            {
                Console.WriteLine("mathdown filename.md");
                return 1;
            }
            MarkdownPipeline pipline;
            string html;
            string markdown;
            if (!args[0].EndsWith(".md"))
            {
                Console.WriteLine("input should be a \".md\" file");
                return 1;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("file " + args[0] + "does not exist.");
                return 1;
            }
            var fs = new FileStream(args[0], FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            markdown = sr.ReadToEnd();

            Netlink nlnk = new Netlink();
            nlnk.Init();
            pipline = new MarkdownPipelineBuilder()
                            .UseMathCode(nlnk)
                            .UseMathematics()
                            .Build();
            html = Markdown.ToHtml(markdown, pipline);
            sr.Close();
            fs.Close();
            nlnk.Finish();
            string outname = args[0].Substring(0, args[0].Length - 3) + ".html";
            var outf = new FileStream(outname, FileMode.Create);
            outf.Close();

            string prehtml = @"<!DOCTYPE html>
<html>
<head>
    </head>
    <body>
";
            string posthtml = @"
<script src='https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-MML-AM_CHTML' async></script>
</body>
</html>";
            File.WriteAllText(outname, prehtml + html + posthtml);
            return 0;
        }
    }
}
