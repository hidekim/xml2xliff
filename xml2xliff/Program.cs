namespace xml2xliff
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Localization.Xliff.OM.Core;
    using Localization.Xliff.OM.Extensibility;
    using Localization.Xliff.OM.Serialization;
    using IO = System.IO;

    using HtmlAgilityPack;

    public class SourceTranslationPair
    {
        public string Source { get; set; }
        public string Translation { get; set; }
    };
    
    public class Xml2Xlf
    {
        const string customPrefix = "srcxml";
        const string customNamespace = "urn:custom:extension:1.0";

        public static void Main(string[] args)
        {
            if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
            {
                IWantToExit("Need 3 arguments");
            }

            var fileList = new List<string>();
            string srcDir = string.Empty;
            string trnDir = string.Empty;
            string translationLanguage = string.Empty;
            string outputDir = string.Empty;

            ParseParams(args, ref fileList, ref srcDir, ref trnDir, ref translationLanguage, ref outputDir);

            foreach (var file in fileList)
            {
                var sourceXml      = IO.Path.Combine(srcDir,file);
                var translationXml = IO.Path.Combine(trnDir, file);
                var outputXliff    = IO.Path.Combine(outputDir, $@"{file}.xlf");
                GenerateXliff(sourceXml, translationXml, translationLanguage, outputXliff);
            }

        }

        static void ParseParams(string[] args, ref List<string> fileList, ref string srcDir, ref string trnDir, ref string translationLanguage, ref string outputDir)
        {
            var srcXmlPath = IO.Path.GetFullPath((args[0]));
            var translationXmlPath = IO.Path.GetFullPath(args[1]);
            translationLanguage = args[2];
            var outputPath = IO.Path.GetFullPath(args[3]);

            if (!IO.File.Exists(srcXmlPath))
            {
                if (!IO.Directory.Exists(srcXmlPath))
                {
                    IWantToExit("File/Directory srcXmlPath does not exist.");
                }
                srcDir = srcXmlPath;
                if (!srcDir.EndsWith(@"\"))
                {
                    srcDir += @"\";
                }

                var xfiles = IO.Directory.GetFiles(srcDir, "*.xml", IO.SearchOption.AllDirectories);
                var hfiles = IO.Directory.GetFiles(srcDir, "*.html", IO.SearchOption.AllDirectories);

                var allfiles = new List<string>(xfiles);
                allfiles.AddRange(new List<string>(hfiles));

                var dirLength = srcDir.Length;
                foreach (var file in allfiles)
                {
                    var relPath = file.Substring(dirLength);

                    if (!string.IsNullOrWhiteSpace(relPath))
                    {
                        fileList.Add(relPath);
                    }
                }
            }
            else
            {
                var srcFileName = IO.Path.GetFileName(srcXmlPath);
                if (string.IsNullOrWhiteSpace(srcFileName))
                {
                    IWantToExit($"srcFileName is null or empty: file name: {srcXmlPath}");
                }

                fileList.Add(srcFileName);
                srcDir = IO.Path.GetDirectoryName(srcXmlPath);
                if (string.IsNullOrWhiteSpace(srcDir))
                {
                    IWantToExit($"srcDir is null or empty: file name: {srcXmlPath}");
                }
            }

            if (!IO.Directory.Exists(translationXmlPath))
            {
                IWantToExit($"Translation Directory {translationXmlPath} does not exist.");
            }

            trnDir = translationXmlPath;

            if (!IO.Directory.Exists(outputPath))
            {
                IWantToExit($"Output Directory: {outputPath} does not exist.");
            }

            outputDir = outputPath;
        }

        public static void GenerateXliff(string srcFile, string translationFile, string translationLanguage, string outputXliff)
        {
            var translationDictionary = new Dictionary<string, SourceTranslationPair>();
            var ext = IO.Path.GetExtension(srcFile).ToLower();

            if (ext == ".xml")
            {
                ParseSourceXmlInto(translationDictionary, srcFile);
                ParseTranslationXmlInto(translationDictionary, translationFile);
            }
            else if (ext == ".html")
            {
                ParseSourceHtmlInto(translationDictionary, srcFile);
                ParseTranslationHtmlInto(translationDictionary, translationFile);
            }
            else
            {
                IWantToExit($"File extension {ext} is not supported.");
            }

            var filenameInXliff = IO.Path.GetFileName(srcFile);

            try
            {

                XliffDocument document = new XliffDocument("en-US");
                document.TargetLanguage = translationLanguage;
                document.Files.Add(new File(filenameInXliff));

                int i = 1;
                foreach (var xpath in translationDictionary.Keys)
                {
                    Unit unit = new Unit($"u{i}");
                    IExtensible extensible = unit;

                    var x = new SrcXPathExtension();
                    if (extensible.SupportsAttributeExtensions)
                    {
                        x.AddAttribute(new SrcXPathAttribute(customPrefix, customNamespace, "originxpath", xpath )); // "/ManagementPackFragment[1]/LanguagePacks[1]/LanguagePack[1]/DisplayStrings[1]/DisplayString2[1]/Name[1]"));
                        extensible.Extensions.Add(x);
                    }

                    SourceTranslationPair stPair = translationDictionary[xpath];
                    bool error = false;
                    Segment segment = new Segment("s1")
                    {
                        Source = new Source(),
                        Target = new Target()
                    };
                    {
                        if (string.IsNullOrEmpty(stPair.Source)) // Item1 == source
                        {
                            IWantToWarn($@"Source entry is null or empty: XPath {xpath}");
                            error = true;
                        }
                        if (string.IsNullOrEmpty(stPair.Translation)) // Item1 == translation
                        {
                            IWantToWarn($@"Translation entry is null or empty: XPath {xpath}: source: {stPair.Source}");
                            error = true;
                        }

                        if (stPair.Source == stPair.Translation)
                        {
                            error = true; // not exactly a error but just want to exclude src == translation cases
                        }

                        if (!error)
                        {
                            segment.Source.Text.Add(new PlainText(stPair.Source));
                            segment.Target.Text.Add(new PlainText(stPair.Translation));
                        }
                    }

                    if (!error)
                    {
                        unit.Resources.Add(segment);
                        document.Files[0].Containers.Add(unit);
                    }
                    ++i;
                }

                WriteDocument(document, outputXliff);
            }
            finally
            {
                // IO.File.Delete(path);
            }
        }

        // using TransDic = System.Collections.Generic.Dictionary<string, Pair<string, string>>;

        public static void ParseSourceXmlInto(Dictionary<string, SourceTranslationPair> translationDictionary, string srcXml)
        {
            ParseXmlInto(translationDictionary, srcXml, true);
        }

        public static void ParseTranslationXmlInto(Dictionary<string, SourceTranslationPair> translationDictionary, string trnXml)
        {
            ParseXmlInto(translationDictionary, trnXml, false);
        }

        public static void ParseXmlInto(Dictionary<string, SourceTranslationPair> translationDictionary, string xmlFile, bool isSrcFile)
        {
            var doc = new XmlDocument();
            doc.Load(xmlFile);

            var dic = translationDictionary;

            doc.IterateThroughAllNodes(
                    delegate (NodeVisitorXml nodeVisitor)
                    {
                        var node = nodeVisitor.Item1;
                        var xpath = nodeVisitor.Item2;

                        if (node.NodeType == XmlNodeType.Text)
                        {
                            var text = node.InnerText;
                            if (!string.IsNullOrWhiteSpace(text))
                            {

                                if (isSrcFile)
                                {
                                    AddToDictionarySource(dic, xpath, text);
                                }
                                else
                                {
                                    AddToDictionaryTarget(dic, xpath, text);
                                }
                            }
                        }
                    }
                );
        }

        public static void ParseSourceHtmlInto(Dictionary<string, SourceTranslationPair> translationDictionary, string srcHtml)
        {
            ParseHtmlInto(translationDictionary, srcHtml, true);
        }

        public static void ParseTranslationHtmlInto(Dictionary<string, SourceTranslationPair> translationDictionary, string trnHtml)
        {
            ParseHtmlInto(translationDictionary, trnHtml, false);
        }

        public static void ParseHtmlInto(Dictionary<string, SourceTranslationPair> translationDictionary, string htmlFile, bool src)
        {
            var doc = new HtmlDocument();
            // http://stackoverflow.com/questions/12787449/html-agility-pack-removing-unwanted-tags-without-removing-content
            // remove unwanted tags
            //doc.LoadHtml(html);
            ;
            doc.Load(htmlFile);

            var dic = translationDictionary;

            doc.IterateThroughAllNodes(
                    delegate (NodeVisitorHtml nodeVisitor)
                    {
                        HtmlNode node = nodeVisitor.Item1;
                        var xpath = nodeVisitor.Item2;

                        if (!string.IsNullOrEmpty(xpath))
                        {
                            if (node.NodeType == HtmlNodeType.Text)
                            {
                                var text = node.InnerText;

                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    if (src)
                                    {
                                        AddToDictionarySource(dic, xpath, text);
                                    }
                                    else
                                    {
                                        AddToDictionaryTarget(dic, xpath, text);
                                    }
                                }
                            }
                        }
                    }
                );
        }

        public static void AddToDictionarySource(Dictionary<string, SourceTranslationPair> dic, string xpath, string source)
        {
            if (dic.ContainsKey(xpath))
            {
                IWantToExit($@"Duplicate source entry: XPath: {xpath}: Existing Text: {dic[xpath].Source}: New Text: {source}");
            }
            dic.Add(xpath, new SourceTranslationPair());
            dic[xpath].Source = source;
        }

        public static void AddToDictionaryTarget(Dictionary<string, SourceTranslationPair> dic, string xpath, string translation)
        {
            if (!dic.ContainsKey(xpath))
            {
                IWantToWarn($@"No source entry: XPath: Translation: {translation}");
                // just a warning
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dic[xpath].Source))
                {
                    IWantToWarn($@"No source string: XPath {xpath}: Translation: {translation}");
                }
                else
                {
                    dic[xpath].Translation = translation;
                }
            }
        }

        public static void IWantToShow(string message)
        {
            Console.WriteLine($@"[INFO] {message}");
        }

        public static void IWantToWarn(string message)
        {
            Console.WriteLine($@"[WARNING] {message}");
        }

        public static void IWantToExit(string additionalMessage)
        {
            if (!string.IsNullOrWhiteSpace(additionalMessage))
            {
                Console.WriteLine($"[ERROR] {additionalMessage}");
            }

            Console.WriteLine("Usage: xml2xliff.exe src_XMLHTML(dir|file) translation_XMLHTML(dir) outputDir");
            throw new Exception();
        }

        /// <summary>
        /// Demonstrates how to disable validation when writing an XLIFF document.
        /// </summary>
        /// <param name="document">The document to write.</param>
        /// <param name="file">The path to the document to write.</param>
        public static void WriteDocument(XliffDocument document, string file, bool disableValidation = false)
        {
            using (IO.FileStream stream = new IO.FileStream(file, IO.FileMode.Create, IO.FileAccess.Write))
            {
                XliffWriter writer;
                XliffWriterSettings settings = new XliffWriterSettings();

                settings.Indent = true;
                settings.IndentChars = "\t";

                if (disableValidation)
                {
                    settings.Validators.Clear();
                }

                writer = new XliffWriter(settings);
                writer.Serialize(stream, document);
            }
        }

        /// <summary>
        /// Demonstrates how to read an XLIFF document from a file.
        /// </summary>
        /// <param name="file">The path to the document to read.</param>
        public static void ReadDocument(string file)
        {
            using (IO.FileStream stream = new IO.FileStream(file, IO.FileMode.Open, IO.FileAccess.Read))
            {
                XliffDocument document;
                XliffReader reader;

                reader = new XliffReader();
                document = reader.Deserialize(stream);
            }
        }
    }
}
