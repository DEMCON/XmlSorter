using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Demcon.XmlSorter
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showHelp = false;
            try
            {
                // check the supplied arguments
                string filename;
                string[] ignoredNodes = { "" };
                string path;
                string[] extensions = {""};

                if (args.Count() == 1)
                {
                    filename = args[0];
                    SortFile(filename, ignoredNodes);
                }
                else if (args.Count() == 2)
                {
                    filename = args[0];
                    ignoredNodes = args[1].Split(';');
                    SortFile(filename, ignoredNodes);
                }
                else if (args.Count() == 3)
                {
                    path = args[0];
                    extensions = args[1].Split(';');
                    ignoredNodes = args[2].Split(';');
                    SortDir(path, extensions, ignoredNodes);
                }
                else
                {
                    showHelp = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                showHelp = true;
            }

            if (showHelp)
            {
                Console.WriteLine("XmlSorter functionality:");
                Console.WriteLine("   sort XML-nodes and XML-attributes alphabetically in XML-file");
                Console.WriteLine("   remove duplicate nodes and attributes");
                Console.WriteLine();
                Console.WriteLine("usage1: XmlSorter \"filename\"");
                Console.WriteLine("usage2: XmlSorter \"filename\" \"node;*\"");
                Console.WriteLine("usage3: XmlSorter \"dir\" \"ext;*\" \"node;*\"");
                Console.WriteLine("   \"filename\": only sort the given file");
                Console.WriteLine("   \"node;*\": XML node-names, separate by ';', these nodes are NOT sorted");
                Console.WriteLine("   \"dir\": sort all files in this directory and all sub-directories");
                Console.WriteLine("   \"ext;*\": file-extensions without '.', separated by ';', only sort files with given extensions");
                Console.WriteLine("   arguments can be included in double quotes \" \", but don't have to be");

                Environment.Exit(1);
            }
        }

        static private void SortDir(string path, string[] extensions, string[] ignoredNodes)
        {
            string[] files = Directory.GetFiles(path);
            string[] subDirs = Directory.GetDirectories(path);
            foreach (string file in files)
            {
                foreach (string extension in extensions)
                {
                    if (file.EndsWith("." + extension))
                    {
                        SortFile(file, ignoredNodes);
                    }
                }
            }

            foreach (string dir in subDirs)
            {
                SortDir(dir, extensions, ignoredNodes);
            }
        }

        static private void SortFile(string pathFilename, string[] ignoredNodes)
        {
            // read file
            string srcText = File.ReadAllText(pathFilename);
            XDocument srcXDoc = XDocument.Parse(srcText);

            // sort
            XElement sortedNode = SortNode(srcXDoc.Root, ignoredNodes);

            // write
            XDocument destXDoc = new XDocument(srcXDoc.Declaration, sortedNode);
            string destText;
            if (srcXDoc.Declaration != null)
            {
                destText = srcXDoc.Declaration.ToString() + "\r\n" + destXDoc.ToString();
            }
            else
            {
                destText = destXDoc.ToString();
            }
            
            File.WriteAllText(pathFilename, destText);
        }

        static private XElement SortNode(XElement node, string[] ignoredNodes)
        {
            // do not sort this node if it is in the ignored list
            string name = node.Name.LocalName;
            if (ignoredNodes.Contains(node.Name.LocalName))
            {
                return node;
            }

            // do not sort this node if it has an attribute xml:space="preserve"
            XAttribute attr = node.Attribute(XNamespace.Xml + "space");
            if (attr != null && attr.Value == "preserve")
            {
                return node;
            }

            // sort attributes (and delete double entries)
            if (node.HasAttributes)
            {
                List<XAttribute> sortedAttributes = node.Attributes().OrderBy(a => a.ToString()).ToList();
                node.RemoveAttributes();
                //sortedAttributes.ForEach(a => node.Add(a));
                XAttribute attrPrev = new XAttribute("EmptyAttr", "");
                foreach (XAttribute attrNew in sortedAttributes)
                {
                    if (attrNew.ToString() != attrPrev.ToString())
                    {
                        node.Add(attrNew);
                        attrPrev = attrNew;
                    }
                }
            }

            // sort the children (and delete double entries)
            if (node.HasElements)
            {
                // sort all child-node content
                foreach (XElement subNode in node.Elements())
                {
                    SortNode(subNode, ignoredNodes);
                }

                // sort the child-nodes themselves
                //List<XElement> sortedChildren = node.Elements().OrderBy(elem => elem.Attributes("Name").Any() ? elem.Attributes("Name").First().Value.ToString() : string.Empty).ToList();
                List<XElement> sortedChildren = node.Elements().OrderBy(elem => elem.ToString()).ToList();
                node.RemoveNodes();
                //sortedChildren.ForEach(c => node.Add(c));
                XElement nodePrev = new XElement("EmptyNode", "");
                foreach (XElement nodeNew in sortedChildren)
                {
                    if (nodeNew.ToString() != nodePrev.ToString())
                    {
                        node.Add(nodeNew);
                        nodePrev = nodeNew;
                    }
                }
            }

            return node;
        }

    }
}
