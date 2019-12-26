using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Spotlight
{
    struct Response
    {
        public string name;
        public string path;
        public string fext;
        public string size;
        public Icon icon;

        public Type type;

        public enum Type
        {
            Command,
            File
        }
    }

    class Parser
    {
        readonly string ConfigFile = Path.Combine(Directory.GetCurrentDirectory(), @"config.xml");
        readonly XElement Config;

        internal Parser()
        {
            if (File.Exists(ConfigFile))
                Config = XElement.Load(ConfigFile);
            else
            {
                Config = new XElement("Config", new XElement("commands"));
                Config.Save(ConfigFile);
            }
        }

        internal bool Invoke(Response response, bool asAdmin)
        {
            switch (response.type)
            {
                case Response.Type.Command:
                    return InvokeCommand(response.name, asAdmin);
            }
            return false;
        }

        private bool InvokeCommand(string name, bool asAdmin)
        {
            XElement aliases = Config.Element("commands");
            XElement alias = aliases.XPathSelectElement(string.Format(@"//alias[@short=""{0}""]", name));
            if (alias != null)
                name = alias.Value;

            try
            {
                if (asAdmin)
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = name;
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
                    proc.Start();
                }
                else
                    Process.Start(name);
                return true;
            }
            catch (Exception ex) when (
                ex is FileNotFoundException
                || ex is System.ComponentModel.Win32Exception
            )
            {
                return false;
            }
        }

        internal List<Response> Search(string text)
        {
            // autocomplete
            // run commands
            // use windows search

            return WindowsSearch(text);
        }

        private List<Response> WindowsSearch(string text)
        {
            List<Response> ret = new List<Response>();
            string query = @"SELECT TOP 10 System.ItemNameDisplay, System.ItemPathDisplay, System.ItemType, System.Size, System.Search.Rank FROM SystemIndex WHERE System.ItemName LIKE '%{0}%'";
            using (OleDbConnection conn = new OleDbConnection(@"Provider=Search.CollatorDSO;Extended Properties=""Application=Windows"""))
            {
                conn.Open();
                OleDbCommand command = new OleDbCommand(query, conn);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Icon icon = Icon.ExtractAssociatedIcon(reader[1].ToString());
                        ret.Add(new Response
                        {
                            name = reader[0].ToString(),
                            path = reader[1].ToString(),
                            fext = reader[2].ToString(),
                            size = reader[3].ToString(),
                            icon = icon,
                            type = Response.Type.File
                        });
                    }
                }

            }
            return ret;
        }
    }
}
