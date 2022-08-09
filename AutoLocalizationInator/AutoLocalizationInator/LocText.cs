using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoLocalizationInator
{
    
    public class LocText
    {
        public LocText(int line, string file, string data)
        {
            Line = line;
            Path1 = file;

            data = data.Replace("\"", String.Empty);
            string fileType = Path.GetExtension(file);
            if (fileType == ".xaml")
            {
                if (data.Contains("="))
                {
 Content = data.Split("=")[1];
                }
                else
                {
                    Content = data.Substring(1,data.Length-3);
                }
               
            }
            else
            {
                Content = data;
            }
            var variablesRemoved = Regex.Replace(Content, @"\{[^{}]+\}", string.Empty).Replace("\"", String.Empty);

                var matches = Regex.Matches(variablesRemoved, "[\\p{L}]");
                if (!matches.Any())
                {
                    Enumerator = LocText.LocalizationTypeEnum.NotNeeded;
                }
                else
                {
                    if (Content.Contains("{"))
                    {
                        Enumerator = LocText.LocalizationTypeEnum.Parameter;
                    }
                    else
                    {
                        Enumerator = LocText.LocalizationTypeEnum.Basic;
                    }
                }
              
                  
                /*
                Enumerator = string.IsNullOrWhiteSpace(variablesRemoved)
                    ? LocText.LocalizationTypeEnum.NotNeeded
                    : LocText.LocalizationTypeEnum.Parameter;
                */
            Name = "App_" + Path1.Split("\\")[^1].Replace(".","_") + "_";
               


                if (Enumerator.Equals(LocalizationTypeEnum.Parameter))
                {
                   
                Content = Content.Replace("{Chars.Enter}","\n");
                    var matches1 = Regex.Matches(Content, @"\{[^{}]+\}");
                   
                    
                    for (int i = 0; i < matches1.Count; i++)
                    {
                        Content =  Content.Replace(matches1[i].Value, ("{" + i + "}"));
                       
                    }
                }



            if (fileType == ".xaml")
            {
                if (Content == "True" || Content == "False" || Regex.IsMatch(Content, @"^\d+$"))
                {
                    Enumerator = LocText.LocalizationTypeEnum.NotNeeded;
                }
                //Name += String.Join("_", data.Split(" ")[0].Split("=").Reverse()); //Name_Content
                if (data.Contains("="))
                {
                    Name += data.Replace("=", "_").Split(" ")[0]; //Content_Name
                }
                else
                {
                    Name += "Content_"+data.Substring(1, data.Length - 3);
                }
              
            }
            else
            {
                Name += Regex.Match(Content.Split(" ")[0].Replace("=","_"), "[\\p{L},_]+");
            }
            
                
              

              
            switch (Enumerator)
            {
                case (LocText.LocalizationTypeEnum.Basic): break;
                case (LocText.LocalizationTypeEnum.Parameter):
                    
                    Name += "_Param";break;
                case (LocText.LocalizationTypeEnum.NotNeeded):
                    Name = "_NOT_NEEDED";break;
            }

            Name = RemoveDiacritics(Name);
            Name = Regex.Replace(Name, "[^A-z_1-9]","");

        }
        string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
        public override string ToString()
        {
            // return Name+";"+Content;
            return Name+" is in file: "+Path1+", on line: "+Line+", is "+Enumerator+", contains: "+Content;
        }

        public enum LocalizationTypeEnum
        {
            Basic,
            Parameter,
            NotNeeded

        }


        public string Name { get; set; }
        public Enum Enumerator;
        private int _line;
        public int Line
        {
            get => _line;
            set
            {
                if (value > 0)
                {
                    _line = value;
                }
            }

        }
        private string _path;
        public string Path1
        {
            get => _path;
            set
            {
                if (File.Exists(value))
                {
                    _path = value;
                }
                else
                {
                    Console.WriteLine("Path is not valid");
                }

                
                   
                
               
            }

        }

        
        public string Content { get; set; }
        
    }
}
