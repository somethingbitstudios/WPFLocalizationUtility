using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using DevExpress.Xpo.DB;

namespace AutoLocalizationInator
{
    //by David Gráf
    public class Localization
    {
        public int autoAnswer = 0;
        public List<string> FilesToSkip = new List<string>();
        public string TXTPath = "./Skip.txt";
        public string Resource = "./Resource.resx";
        public string Namespace = "path needed for properties here";

        public void Start()
        {
            Console.Write("Remember: use 'autoyes' instead of 'yes' to speed up testing localization");
            ShowHelp();
            LoadFileToSkip();
            Boot();
        }
        public void Boot()
        {



            autoAnswer = 0;
            Console.Write("YOU: ");
            switch (Console.ReadLine().ToLower())
            {
                case ("rc"):
                    ShowHelp();
                    Console.WriteLine("C : Initiating resource change from:" + Resource+" to new file.");
                    Console.WriteLine("Input new resource file:");ChangeResource(Console.ReadLine());
                    break;
                case ("l"):
                    ShowHelp(); Console.WriteLine("L : Initiating single file localization");
                    Console.WriteLine(LocalizeFile(Console.ReadLine()));
                    break;
                case ("ld"):
                    ShowHelp(); Console.WriteLine("L : Initiating single folder localization");
                    LocalizeFolder(Console.ReadLine());
                    break;
                case ("ldr"):
                    ShowHelp(); Console.WriteLine("L : Initiating recursive folder localization");
                    LocalizeFolderRecur(Console.ReadLine());
                    break;
                case ("c"):
                    ShowHelp(); Console.WriteLine("C : Initiating single file check");
                    Console.WriteLine(PrintList(CheckFile(Console.ReadLine())));
                    break;
                case ("cd"):
                    ShowHelp(); Console.WriteLine("CD : Initiating single folder check"); Console.WriteLine(PrintList(CheckFolder(Console.ReadLine())));
                    break;
                case ("cdr"):
                    ShowHelp(); Console.WriteLine("CDR : Initiating recursive folder check"); Console.WriteLine(PrintList(CheckFolderRecur(Console.ReadLine())));
                    break;
                case ("s"):
                    ShowHelp(); Console.WriteLine("S : Initiating single file skip"); Console.WriteLine(SkipFile(Console.ReadLine()));
                    break;
                case ("sd"):
                    ShowHelp(); Console.WriteLine("SD : Initiating folder skip"); Console.WriteLine(SkipFolder(Console.ReadLine()));
                    break;
                case ("sdr"):
                    ShowHelp(); Console.WriteLine("SDR : Initiating recursive folder skip"); Console.WriteLine(SkipFolderRecur(Console.ReadLine()));
                    break;
                case ("u"):
                    ShowHelp(); Console.WriteLine("U : Initiating single file un-skip"); Console.WriteLine(UnSkipFile(Console.ReadLine()));
                    break;
                case ("ud"):
                    ShowHelp(); Console.WriteLine("UD : Initiating folder un-skip"); Console.WriteLine(UnSkipFolder(Console.ReadLine()));
                    break;
                case ("udr"):
                    ShowHelp(); Console.WriteLine("UDR : Initiating recursive folder un-skip"); Console.WriteLine(UnSkipFolderRecur(Console.ReadLine()));
                    break;
                case ("save"):
                    ShowHelp(); Console.WriteLine("SAVE : Saving skip data to file: "+TXTPath); SaveSkipToFile();
                    break;
                case ("load"):
                    ShowHelp(); Console.WriteLine("LOAD : Loading skip data from file: " + TXTPath);
                    LoadFileToSkip();
                    break;
                case ("clear"):
                    ShowHelp(); Console.WriteLine("CLEAR : Clearing skip data from memory");
                    FilesToSkip = new List<string>();
                    break;
                case ("delete"):
                    ShowHelp(); Console.WriteLine("DELETE : Deleting skip data from file: " + TXTPath);
                    ClearSkipFile();
                    break;
                case ("change"):
                    ShowHelp(); Console.WriteLine("CHANGE : Changing skip data file from: " + TXTPath+" to:");
                    SkipFileChange(Console.ReadLine());
                    break;
                case ("check"):
                    ShowHelp(); Console.WriteLine("CHECK : File:" + TXTPath + ", Contents:");
                    PrintFile(TXTPath);
                    break;
                case ("show"):
                    ShowHelp(); Console.WriteLine("SHOW : Loaded in memory: \n" +PrintList(FilesToSkip));
                   
                    break;
                case ("quit"):
                    ShowHelp(); Console.WriteLine("You cannot rest now. There are enemies nearby!");
                    Environment.Exit(69);
                    break;
            }
            
            Boot();

        }

        public void ChangeResource(string path)
        {
            if (path.Length > 3)
            {
                Resource = path;
            }
        }
        public void ShowHelp()
        {
            Console.Clear();
            Console.SetCursorPosition(0,8);
        
            string line = "";
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                line += "-";
            }
            Console.WriteLine(line);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("AUTO-LOCALIZATOR-INATOR");
            Console.WriteLine(" C- Check amount of localizable data in file, CD-Check directory, CDR-Check directory recursively,\n L-Localize file, D- Localize directory, R- Localize directory recursively,\n S - Skip file, SD- Skip directory SDR- Skip Recursively,\n U - Un-Skip file, UD- Un-Skip directory UDR- Un-Skip Recursively,\n SAVE - Saves skip data to file, LOAD- Loads skip data from file CLEAR - Clears skip data in RAM, keeps skip data on file, DELETE-Deletes all skip data,\n CHANGE-Changes file skip data is saved to, CHECK-Check data and name of skip file, QUIT - Quit");
            Console.SetCursorPosition(0, 10);
            

        }

        public List<LocText> NameFixer(List<LocText> list)
        {
            if (list == null)
            {
                return null;
            }
            List<string> names = new List<string>();
            List<string> designer = Resource.Split(".").ToList();
            designer.RemoveAt(designer.Count - 1);
            designer.Add("Designer.cs");
            string designerpath = string.Join(".", designer).ToString();
            string text = File.ReadAllText(designerpath);

            foreach (LocText loc in list)
            {
                while (names.Contains(loc.Name) || text.Contains(loc.Name))
                {
                    var match = Regex.Match(loc.Name, "[0-9]*$");
                    if (match.Value != ""&&match.Value != String.Empty)
                    {
                        
                        int num = Convert.ToInt32(match.Value);
                        loc.Name = loc.Name.Replace(match.Value.ToString(), String.Empty) + (num+1).ToString();
                    }
                    else
                    {
                        loc.Name += "1";
                    }
                }

                if (loc.Name != "_NOT_NEEDED")
                {
                    names.Add(loc.Name);
                }
                

                

            }
            return list;
        }
        public void PrintFile(string path)
        {
            //FilesToSkip = new List<string>();
            StreamReader file;
            try
            {
                file = new StreamReader(TXTPath);
            }
            catch
            {
                File.Create(TXTPath);
                file = new StreamReader(TXTPath);
            }

            int lineCount = File.ReadLines(TXTPath).Count();
            for (int i = 0; i < lineCount; i++)
            {
               Console.WriteLine(file.ReadLine());
            }
            file.Close();
        }
        public void SkipFileChange(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                path = "./Skip.txt";
            }
            TXTPath = path;

        }
        public void ClearSkipFile()
        {
            File.WriteAllText(TXTPath, String.Empty);
            
        }
        public void LoadFileToSkip()
        {
            FilesToSkip = new List<string>();
            StreamReader file;
            try
            {
                file = new StreamReader(TXTPath);
            }
            catch
            {
                File.Create(TXTPath);
                file = new StreamReader(TXTPath);
            }
            
            int lineCount = File.ReadLines(TXTPath).Count();
            for (int i = 0; i < lineCount; i++)
            {
                FilesToSkip.Add(file.ReadLine());
            }
            file.Close();
        }
        public void SaveSkipToFile()
        {
            
            StreamWriter file;
            file = new StreamWriter(TXTPath);
            int lineCount = FilesToSkip.Count;
            for (int i = 0; i < lineCount; i++)
            {
                file.WriteLine(FilesToSkip[i]);
            }
            file.Close();
        }
        public string PrintList<T>(List<T> text)
        {
          
          

            if (typeof(T).ToString() == "AutoLocalizationInator.LocText")
            {


                if (text == null)
                {
                    return "Cannot be printed";
                }

                try
                {
                    string a = "Found " + text.Count + " localizable strings:\n";
                    foreach (var VARIABLE in text)
                    {
                        a += VARIABLE.ToString() + "\n";
                    }

                    return a;
                }
                catch
                {
                    return "Unable to print invalid data!";
                }
            }
            else if (typeof(T).ToString() == "System.String")
            {
                string a = "";
                foreach (var VARIABLE in text)
                {
                    a += VARIABLE.ToString() + "\n";
                }

                return a;
            }
            else
            {
                return "unsupported type";
            }
        }
        public List<LocText> CheckFolderRecur(string path)
        {
            if (path == null)
            {
                return null;
            }
            if (File.Exists(path))
            {
                string[] PathColl = path.Split("\\");
                PathColl = PathColl.Where((val, idx) => idx != (PathColl.Length - 1)).ToArray();
                path = String.Join("\\", PathColl);
            }

           
                string[] filePaths = Directory.GetFiles(@path, "*.cs", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(@path, "*.xaml", SearchOption.AllDirectories)).ToArray();
                Console.WriteLine("Checking " + filePaths.Length + " files... Please wait");
                List<LocText> loc = new List<LocText>();
                for (int i = 0; i < filePaths.Length; i++)
                {
                    List<LocText> lic = CheckFile(filePaths[i]);
                    if (lic != null)
                    {
                        loc = loc.Concat(lic).ToList();
                    }
                }

                return loc;
           
        }
        /*
        public bool IsFile(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            return false;
        }
        */
        public List<LocText> CheckFolder(string path)
        {
            if (path == null)
            {
                return null;
            }

            if (File.Exists(path))
            {
                string[] PathColl = path.Split("\\");
                PathColl = PathColl.Where((val, idx) => idx != (PathColl.Length - 1)).ToArray();
                path = String.Join("\\", PathColl);
            }
         
           
                string[] filePaths = Directory.GetFiles(@path, "*.cs").Concat(Directory.GetFiles(@path, "*.xaml"))
                    .ToArray();
                Console.WriteLine("Checking " + filePaths.Length + " files... Please wait");
                List<LocText> loc = new List<LocText>();
                for (int i = 0; i < filePaths.Length; i++)
                {

                    List<LocText> lic = CheckFile(filePaths[i]);
                    if (lic != null)
                    {
                        loc = loc.Concat(lic).ToList();
                    }

                }

                return loc;
           
        }
        public void LocalizeFolder(string path)
        {
            if (path == null)
            {
                return;
            }

            if (File.Exists(path))
            {
                string[] PathColl = path.Split("\\");
                PathColl = PathColl.Where((val, idx) => idx != (PathColl.Length - 1)).ToArray();
                path = String.Join("\\", PathColl);
            }


            string[] filePaths = Directory.GetFiles(@path, "*.cs").Concat(Directory.GetFiles(@path, "*.xaml"))
                .ToArray();
            Console.WriteLine("Checking " + filePaths.Length + " files... Please wait");
          
            for (int i = 0; i < filePaths.Length; i++)
            {

                LocalizeFile(filePaths[i]);

            }

            return;

        }
        public void LocalizeFolderRecur(string path)
        {
            if (path == null)
            {
                return;
            }
            if (File.Exists(path))
            {
                string[] PathColl = path.Split("\\");
                PathColl = PathColl.Where((val, idx) => idx != (PathColl.Length - 1)).ToArray();
                path = String.Join("\\", PathColl);
            }
            


            string[] filePaths = Directory.GetFiles(@path, "*.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(@path, "*.xaml", SearchOption.AllDirectories)).ToArray();
            Console.WriteLine("Checking " + filePaths.Length + " files... Please wait");
            
            for (int i = 0; i < filePaths.Length; i++)
            {
                LocalizeFile(filePaths[i]);
            }

            return;

        }
        public bool SkipFolderRecur(string path)
        {
            path.Trim('"');
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }
            string[] PathColl = path.Split("\\");
            if (PathColl[^1].Contains("."))
            {
                PathColl = PathColl.Where((val, idx) => idx != (PathColl.Length - 1)).ToArray();
            }

            if (PathColl.Length == 1)
            {
                path = PathColl[0];
            }
            else
            {
                path = String.Join("\\", PathColl);
            }
            path = String.Join("\\", PathColl);
            string[] filePaths = Directory.GetFiles(@path, "*.cs", SearchOption.AllDirectories).Concat(Directory.GetFiles(@path, "*.xaml", SearchOption.AllDirectories)).ToArray();
            Console.WriteLine("Affecting " + filePaths.Length + " files... Please wait");
            for (int i = 0; i < filePaths.Length; i++)
            {

                SkipFile(filePaths[i]);

            }
            return true;

        }
        public bool UnSkipFolderRecur(string path)
        {
            if (path == null)
            {
                return false;
            }
            string[] PathColl = path.Split("\\");
            if (PathColl[^1].Contains("."))
            {
                PathColl = PathColl.Where((val, idx) => idx != (PathColl.Length - 1)).ToArray();
            }

            path = String.Join("\\", PathColl);
         
            string[] filePaths = Directory.GetFiles(@path, "*.cs", SearchOption.AllDirectories).Concat(Directory.GetFiles(@path, "*.xaml", SearchOption.AllDirectories)).ToArray();
            Console.WriteLine("Affecting " + filePaths.Length + " files... Please wait");
            for (int i = 0; i < filePaths.Length; i++)
            {

                UnSkipFile(filePaths[i]);

            }
            return true;

        }
        public bool SkipFolder(string path)
        {
            string[] PathColl = path.Split("\\");
            if (PathColl[^1].Contains("."))
            {
                PathColl = PathColl.Where((val, idx) => idx != (PathColl.Length - 1)).ToArray();
            }

            path = String.Join("\\", PathColl);
            string[] filePaths = Directory.GetFiles(@path, "*.cs").Concat(Directory.GetFiles(@path, "*.xaml")).ToArray();
            Console.WriteLine("Affecting " + filePaths.Length + " files... Please wait");
            for (int i = 0; i < filePaths.Length; i++)
            {
               
               SkipFile(filePaths[i]);

            }
            return true;

        }
        public bool UnSkipFolder(string path)
        {
            string[] PathColl = path.Split("\\");
            if (PathColl[^1].Contains("."))
            {
                PathColl = PathColl.Where((val, idx) => idx != (PathColl.Length - 1)).ToArray();
            }

            path = String.Join("\\", PathColl);
            string[] filePaths = Directory.GetFiles(@path, "*.cs").Concat(Directory.GetFiles(@path, "*.xaml")).ToArray();
            Console.WriteLine("Affecting " + filePaths.Length + " files... Please wait");
            for (int i = 0; i < filePaths.Length; i++)
            {

                UnSkipFile(filePaths[i]);

            }
            return true;

        }
        public bool SkipFile(string path)
        {
            string toskip = path.Split("\\")[^1];
            if (FilesToSkip.Contains(toskip)||path == "")
            {
                return false;
            }
            else
            {
                FilesToSkip.Add(toskip);
                return true;
            }
        }
        public bool UnSkipFile(string path)
        {
            string toskip = path.Split("\\")[^1];
            if (FilesToSkip.Contains(toskip))
            {
                FilesToSkip.Remove(toskip);
                return true;
            }
            else
            {
               
                return false;
            }
        }

        public List<LocText> CheckFiledsgs(string path)
        {
            List<LocText> entries = new List<LocText>();
            /*
            int lineCount = File.ReadLines(path).Count();
            StreamReader file = new StreamReader(path);
            file.BaseStream.Seek(0, SeekOrigin.Begin);
            */
            int check = 2;
            string[] lines = File.ReadAllLines(path);

            
            for (int i = 0; i < lines.Length; i++)
            {
                if (check == 1)
                {
                    check = 0;
                }

                string line = lines[i];



                line = Regex.Replace(line, @"//.*", string.Empty);
                if (line.Contains("*/"))
                {
                    line = Regex.Replace(line, @"(.*?)*/", string.Empty);
                    check = 2;
                }
                else if (line.Contains("/*"))
                {
                    line = Regex.Replace(line, @"/*.*", string.Empty);
                    check -= 1;
                    if (check < 0)
                    {
                        check = 0;
                    }
                }


                if (check > 0)
                {

                    //var matches = Regex.Matches(line, "\"[0-9\\p{L}\\s\\.,\\-:;\\?\\!\\{\\}\\<\\>\\[\\]]+\"");
                    var matches = Regex.Matches(line, "\".*\"");
                    if (!matches.Any())
                        continue;

                    if (Regex.Match(line, @"^\[.*\]").Success && !Regex.Match(line, @"   ^"".*[.*].*""  ").Success)
                        continue;

                    foreach (var match in matches)
                    {
                        var matchText = (match as Match).Value;

                        LocText loc = new LocText(i + 1, path, matchText);
                        entries.Add(loc);
                    }
                }
            }
        
            return entries;
        }
        public List<LocText> CheckFile(string path)
        {
            
            if (FilesToSkip.Contains(path.Split("\\")[^1]))
            {
                return null;
            }
            
            string fileType = Path.GetExtension(path);
            if (fileType != ".cs" && fileType != ".xaml")
            {
                return null;
            }



            List<LocText> entries = new List<LocText>();
           
            string[] lines = File.ReadAllLines(path);
           

          
            
            
            //checking part
           

            /*
            int chars = 0;
            
            bool check = false;
            bool special = false;
            */
            if (fileType == ".cs")
            {
                int check = 2;

                for (int i = 0; i < lines.Length; i++)
                {
                    /*
                    check = false;
                    special = false;
                    */
                    if (check == 1)
                    {
                        check = 0;
                    }
                    string line = lines[i];



                    line = Regex.Replace(line, @"//.*", string.Empty);
                    if (line.Contains("*/"))
                    {
                        line = Regex.Replace(line, @"(.*?)*/", string.Empty);
                        check = 2;
                    }
                    else if (line.Contains("/*"))
                    {
                        line = Regex.Replace(line, @"/*.*", string.Empty);
                        check -= 1;
                        if (check < 0)
                        {
                            check = 0;
                        }
                    }


                    if (check > 0)
                    {

                         //var matches = Regex.Matches(line, "\"[0-9\\p{L}\\s\\.,\\-:;\\?\\!\\{\\}\\<\\>\\[\\]]+\"");
                        var matches = Regex.Matches(line, "\"[^\"]*\"");
                        if (!matches.Any())
                            continue;
                    
                        if (Regex.Match(line, @"^\[.*\]").Success && !Regex.Match(line, @"   ^"".*[.*].*""  ").Success)
                            continue;
                       
                        foreach (var match in matches)
                        {
                            var matchText = (match as Match).Value;
                            
                            LocText loc = new LocText(i + 1, path, matchText);
                            entries.Add(loc);
                        }
                    }
                }
               
            }
            else if (fileType == ".xaml")
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string text = lines[i];
                    var matches = Regex.Matches(text, "Content=\"[^\"]*\"").Concat(Regex.Matches(text, "Title=\"[^\"]*\"")
                        .Concat(Regex.Matches(text, "Explanation=\"[^\"]*\"").Concat(Regex.Matches(text, "Tooltip=\"[^\"]*\"")
                            .Concat(
                                Regex.Matches(text, "Hint=\"[^\"]*\"")
                                    .Concat(Regex.Matches(text, "Description=\"[^\"]*\"").Concat(Regex.Matches(text, ">.+</")))))));
                    //var matches = Regex.Matches(text, "Content=\".*\"");
                    if (matches.Any())
                    {
                        foreach (var match in matches)
                        {
                            LocText loc = new LocText(i+1, path, match.Value);
                            entries.Add(loc);
                        }
                    }
                }
             
            }
            else
            {
            
                return null;
            }
           

            NameFixer(entries);
            return entries;
        }

        
        public string LocalizeFile(string path)
        {

            
            //add properties to file
            if (!File.Exists(path) )
            {
                return "NOT A FILE";
            }

            string link;
            if (Path.GetExtension(path) == ".cs")
            {
                link = "using "+Namespace+".Properties;";
            }
            else
            {
                link = " xmlns:CC=\"clr-namespace:"+Namespace+".Properties\"";
            }
           
            StreamReader reader = new StreamReader(path);
            string firstline = reader.ReadLine();
            if (firstline == null)
            {
                return "File is empty";
            }
            string rest = reader.ReadToEnd();
            reader.Close();
          
            StreamWriter writer = new StreamWriter(path);
            if (!rest.Contains(link) && !firstline.Contains("namespace"))
            {  
               
                writer.WriteLine(firstline);
                writer.WriteLine(link);
                writer.WriteLine(rest);
               

                //return "Added required resource reference to file\n\n";

            }
            else
            {
                writer.WriteLine(firstline);
                writer.WriteLine(rest);
                
            }
            writer.Close();

             List<LocText> locList = CheckFile(path);
             
            

            if (locList == null)
            {
                return "Skipping file: " + path;
            }
            int a = 0;
            foreach (var locText in locList)
            {
                if (!locText.Enumerator.Equals(LocText.LocalizationTypeEnum.NotNeeded))
                {
                    a++;
                }
            }
            Console.WriteLine("Localizing file: "+path);
            Console.WriteLine(a + " strings to be localized...");
            

            for (int i = 0; i < locList.Count; i++)
            {
                if (locList[i].Name != "_NOT_NEEDED") 
                {
                        Console.WriteLine("L: Do you want this string to be localized? (y/n)");
                Console.WriteLine(locList[i].Name+" : "+locList[i].Content);
                string answr = "idk";
                if (autoAnswer == 0)
                { 
                    answr = Console.ReadLine().ToLower();
                    if (answr == "autoyes")
                    {
                        autoAnswer = 1;
                    }
                }
              
             
              
                
                if (answr.ToLower() == "y" || answr.ToLower() == "yes"  || autoAnswer == 1)
                {
                 
                    ReplaceLine(path, locList[i]);
                    ResourceAdd(locList[i]);
                  
                   
                }
                }

            }
            //return "Resource reference already present\n\n";
            return "Operation Successful";
        }

        public void ResourceAdd(LocText loc)
        {
            //resource
            string[] lines = File.ReadAllLines(Resource);
            List<string> newLines = new List<string>();
            for (int i = 0; i < lines.Length - 1; i++)
            {
                newLines.Add(lines[i]);
            }
            loc.Name = loc.Name.Replace("&", "&amp;");
            loc.Content = loc.Content.Replace("&", "&amp;");
            loc.Name = loc.Name.Replace("<", "&lt;");
            loc.Name = loc.Name.Replace(">", "&gt;");
           
            loc.Name = loc.Name.Replace("\"", "&quot;");
            loc.Name = loc.Name.Replace("'", "&apos;");
            loc.Content = loc.Content.Replace("<", "&lt;");
            loc.Content = loc.Content.Replace(">", "&gt;");
          
            loc.Content = loc.Content.Replace("\"", "&quot;");
            loc.Content = loc.Content.Replace("'", "&apos;");
            newLines.Add("<data name=\"" + loc.Name + "\" xml:space=\"preserve\">");
            newLines.Add("<value>" + loc.Content + "</value>");
         
            newLines.Add(" </data>");
            newLines.Add("</root>");
            StreamWriter writer = new StreamWriter(Resource);
            for (int i = 0; i < newLines.Count; i++)
            {
                writer.WriteLine(newLines[i]);
            }
            writer.Close();


            //Designer
            List<string> designer = Resource.Split(".").ToList();
            designer.RemoveAt(designer.Count-1);
            designer.Add("Designer.cs");
            string designerpath = string.Join(".",designer).ToString();
            lines = File.ReadAllLines(designerpath);
            if (lines.Length == 0)
            {
                return;
            }
            newLines = new List<string>();
            int index = 0;
            int deleteBrackets = 0;
            while (deleteBrackets < 2)
            {
                if (lines[^(index+1)].Contains("}"))
                {
                    deleteBrackets += 1;
                } 
                index ++;
            }
                
           
            for (int i = 0; i < lines.Length - index; i++)
            {
                
                newLines.Add(lines[i]);
            }
            newLines.Add("      public static string "+loc.Name+" {");
            newLines.Add("            get {");
            newLines.Add("                return ResourceManager.GetString(\""+loc.Name+"\", resourceCulture);");
            newLines.Add("            }");
            newLines.Add("        }");
            newLines.Add("    }");
            newLines.Add("}");
            writer = new StreamWriter(designerpath);
            for (int i = 0; i < newLines.Count; i++)
            {
                writer.WriteLine(newLines[i]);
            }
            writer.Close();
        }
        public void ReplaceLine(string path, LocText loc)
        {
            if (!File.Exists(path))
            {
                return;
            }
          
            string[] lines = File.ReadAllLines(path);
            if (Path.GetExtension(path)==".cs")
            {
                if (loc.Enumerator.Equals(LocText.LocalizationTypeEnum.Parameter))
                {
                   
                    StringBuilder replace = new StringBuilder("String.Format(Resource." + loc.Name);
                    
                    var matches = Regex.Matches(lines[loc.Line - 1], "{[^{}]*}");
                    for (int i = 0; i < matches.Count; i++)
                    {
                        
                        string temp = matches[i].Value;
                       
                        temp = temp.TrimStart(Convert.ToChar("{"));
                        temp = temp.TrimEnd(Convert.ToChar("}"));
                        if (temp.Contains("Resource.") || temp.Contains("Chars.Enter"))
                        {
                            continue;
                        }
                           if(Regex.IsMatch(temp, "\\p{L}")) 
                               replace.Append(","+temp);
                    }

                    replace.Append(")");
                    var makeit= Regex.Matches(lines[loc.Line-1], "{[\\p{L},0-9,.,(,)]+}");
                    int skipped = 0;
                    for (int i = 0; i <makeit.Count;i++)
                    {
                        
                        try
                        {
                        var match = makeit[i + skipped];
                       
                        
                        if ((match as Match).Value == "{Chars.Enter}")
                        {
                            skipped++;
                            i--;
                            continue;
                        }
                        lines[loc.Line - 1] = lines[loc.Line - 1].Replace((match as Match).Value, "{"+i+"}");
                        }
                        catch
                        {
                            break;
                        }
                    }

                    string testText = loc.Content.Replace("\n", "{Chars.Enter}");
                    lines[loc.Line - 1] = lines[loc.Line - 1].Replace("$\"" + testText + "\"", replace.ToString());
                    //Console.WriteLine("$\""+loc.Content+"\"");
                    //Console.WriteLine(lines[loc.Line-1]);
                   
                }
                else
                {
                    lines[loc.Line-1] = lines[loc.Line-1].Replace("\""+loc.Content+ "\"", "$\"{Resource." + loc.Name + "}\"");
                }
              
               
             
                //regReplace = new Regex("$+");
                //lines[loc.Line-1] = regReplace.Replace(lines[loc.Line], "$", 1);
                //lines[loc.Line - 1].Trim(Convert.ToChar("$"));





            }
            else
            {
                Regex regReplace = new Regex(loc.Content);
                lines[loc.Line-1] = regReplace.Replace(lines[loc.Line-1], "{x:Static CC:Resource." + loc.Name + "}", 1);
                //lines[loc.Line - 1] = lines[loc.Line-1].Replace(loc.Content, "{x:Static CC:Resource." + loc.Name + "}");
            }
            
            StreamWriter writer = new StreamWriter(path);
            for (int i = 0; i < lines.Length; i++)
            {
                writer.WriteLine(lines[i]);
            }
            writer.Close();


        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            Localization localization = new Localization();
            localization.Start();

        }
    }
}
