using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using static System.Console;
using System.Text;
using System.Security.AccessControl;

namespace assoc {
    static class Program {
        static void Main (string[ ] args) {
            /*
             *  when -help or -? = help 
             * 
             *  when -a = add
             *  
             *  args[1] = application extension
             *  args[2] = application path
             *  args[3] = application name for the registry
             *  
             *  additional args:
             *      -i "path"           Image for the file
             *      -fd "description"   Description for the filetype
             *      -pd "description"   Description for the programtype
             *      -t "type"           Content Type
             *      
             *  when -r = remove
             *  args[1] = application extension
             *  args[2] = application id
             */

            if (args.Length < 1 || args[0] == "-help" || args[0] == "-?") {
                PrintHelp( );
                ReadLine( );
                return;
            }

            if (args[0] == "-a" && args.Length > 3) {
                string extension = args[1];
                string application = args[2];
                string id = args[3] + extension;
                Dictionary<string, string> additionals = new Dictionary<string, string>( );
                for (int i = 4; i < args.Length; i += 2) {
                    additionals.Add(args[i].ToLower( ), args[i + 1]);
                }
                AddExtension(extension, application, id,
                    additionals.ContainsKey("-t") ? additionals["-t"] : null,
                    additionals.ContainsKey("-i") ? additionals["-i"] : null,
                    additionals.ContainsKey("-fd") ? additionals["-fd"] : null,
                    additionals.ContainsKey("-pd") ? additionals["-pd"] : null);
            } else if (args[0] == "-r" && args.Length > 2) {
                RemoveExtension(args[1], args[2]);
            }
        }

        private static void PrintHelp ( ) {
            WriteLine("\t-help\t\t\tshows help");
            WriteLine("\t-a \"extension\" \"path\" \"name\" [-i] [-d] [-t]\tcreates an file extension assosiation");
            WriteLine("\t-r \"extension\" \"name\" \tremoves an file extension assosiation");
        }

        private static void AddExtension (string extension, string applicationpath, string id, string type = null, string icon = null, string typedescription = null, string programdescription = null) {
            if (Registry.ClassesRoot.OpenSubKey(extension) != null) {
                Registry.ClassesRoot.DeleteSubKeyTree(extension);
            }

            if (Registry.ClassesRoot.OpenSubKey(id) != null) {
                Registry.ClassesRoot.DeleteSubKeyTree(id);
            }

            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(extension)) {
                key.SetValue("", id);

                using (RegistryKey subkey = key.CreateSubKey("OpenWithProgids")) {
                    subkey.SetValue(id, "");
                }

                if (typedescription != null) {
                    key.SetValue("", typedescription);
                }
                if (type != null) {
                    key.SetValue("Content Type", type);
                }
                if (icon != null) {
                    key.CreateSubKey("DefaultIcon").SetValue("", icon);
                } else {
                    key.CreateSubKey("DefaultIcon").SetValue("", applicationpath);
                }
            }

            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(id)) {
                if(programdescription != null) {
                    key.SetValue("", programdescription);
                }
                key.CreateSubKey(@"shell\open\command").SetValue("", "\"" + applicationpath + "\"" + " \"%1\"");
                key.CreateSubKey("DefaultIcon").SetValue("", applicationpath);
            }

            WriteLine("Done!");
        }

        private static void RemoveExtension(string extension, string id) {
            Registry.ClassesRoot.DeleteSubKeyTree(extension, false);
            Registry.ClassesRoot.DeleteSubKeyTree(id + extension, false);
            WriteLine("Done!");
        }
    }
}
