﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class SubtitleEditorProject : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Subtitle Editor Project"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("<SubtitleEditorProject") &&
                xmlAsString.Contains("<subtitle "))
            {
                XmlDocument xml = new XmlDocument();
                xml.XmlResolver = null;
                try
                {
                    xml.LoadXml(xmlAsString);

                    XmlNode div = xml.DocumentElement.SelectSingleNode("subtitles");
                    int numberOfParagraphs = div.ChildNodes.Count;
                    return numberOfParagraphs > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\"?>" + Environment.NewLine +
                "<SubtitleEditorProject version=\"1.0\">" + Environment.NewLine +
                "  <player />" + Environment.NewLine +
                "  <waveform />" + Environment.NewLine +
                "  <styles />" + Environment.NewLine +
                "  <subtitles timing_mode=\"TIME\" edit_timing_mode=\"TIME\" framerate=\"25\">" + Environment.NewLine +
                "  </subtitles>" + Environment.NewLine +
                "  <subtitles-selection />" + Environment.NewLine +
                "</SubtitleEditorProject>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            //          <subtitle duration="2256" effect="" end="124581" layer="0" margin-l="0" margin-r="0" margin-v="0" name="" note="" path="0" start="122325" style="Default" text="The fever hath weakened thee." translation="" />
            XmlNode div = xml.DocumentElement.SelectSingleNode("subtitles");
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("subtitle");

                XmlAttribute duration = xml.CreateAttribute("duration");
                duration.InnerText = p.Duration.TotalMilliseconds.ToString();
                paragraph.Attributes.Append(duration);

                XmlAttribute effect = xml.CreateAttribute("effect");
                effect.InnerText = string.Empty;
                paragraph.Attributes.Append(effect);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = p.EndTime.TotalMilliseconds.ToString();
                paragraph.Attributes.Append(end);

                XmlAttribute layer = xml.CreateAttribute("layer");
                layer.InnerText = "0";
                paragraph.Attributes.Append(layer);

                XmlAttribute marginL = xml.CreateAttribute("margin-l");
                marginL.InnerText = "0";
                paragraph.Attributes.Append(marginL);

                XmlAttribute marginR = xml.CreateAttribute("margin-r");
                marginR.InnerText = "0";
                paragraph.Attributes.Append(marginR);

                XmlAttribute marginV = xml.CreateAttribute("margin-v");
                marginV.InnerText = "0";
                paragraph.Attributes.Append(marginV);

                XmlAttribute name = xml.CreateAttribute("name");
                name.InnerText = string.Empty;
                paragraph.Attributes.Append(name);

                XmlAttribute note = xml.CreateAttribute("note");
                note.InnerText = string.Empty;
                paragraph.Attributes.Append(note);

                XmlAttribute path = xml.CreateAttribute("path");
                path.InnerText = "0";
                paragraph.Attributes.Append(path);

                XmlAttribute start = xml.CreateAttribute("start");
                start.InnerText = p.StartTime.TotalMilliseconds.ToString();
                paragraph.Attributes.Append(start);

                XmlAttribute style = xml.CreateAttribute("style");
                style.InnerText = "Default";
                paragraph.Attributes.Append(style);

                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                XmlAttribute textNode = xml.CreateAttribute("text");
                textNode.InnerText = text;
                paragraph.Attributes.Append(textNode);

                XmlAttribute translation = xml.CreateAttribute("translation");
                translation.InnerText = string.Empty;
                paragraph.Attributes.Append(translation);

                div.AppendChild(paragraph);
                no++;
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            XmlDocument xml = new XmlDocument();
            xml.XmlResolver = null;
            xml.LoadXml(sb.ToString().Trim());

            XmlNode div = xml.DocumentElement.SelectSingleNode("subtitles");
            foreach (XmlNode node in div.ChildNodes)
            {
                try
                {
                    //<subtitle duration="2256" effect="" end="124581" layer="0" margin-l="0" margin-r="0" margin-v="0" name="" note="" path="0" start="122325" style="Default" text="The fever hath weakened thee." translation="" />
                    Paragraph p = new Paragraph();
                    p.StartTime.TotalMilliseconds = int.Parse(node.Attributes["start"].Value);
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + int.Parse(node.Attributes["duration"].Value);
                    p.Text = node.Attributes["text"].Value;

                    subtitle.Paragraphs.Add(p);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

    }
}
