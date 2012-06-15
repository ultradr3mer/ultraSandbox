using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace OpenTkProject
{
    class FileSeeker : GameObject
    {
        public FileSeeker(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;

            string root = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            List<string> files = GetFileList(root, true);

            foreach (var file in files)
            {
                string relativePath = file.Substring(root.Length + 1);
                string extention = System.IO.Path.GetExtension(file);

                switch (extention)
                {
                    case ".obj":
                        mGameWindow.log("found object file: " + relativePath);
                        mGameWindow.meshLoader.fromObj(relativePath);
                        break;
                    case ".dae":
                        mGameWindow.log("found object file: " + relativePath);
                        mGameWindow.meshLoader.fromCollada(relativePath);
                        break;
                    case ".xmd":
                        mGameWindow.log("found object definition file: " + relativePath);
                        mGameWindow.meshLoader.fromXml(relativePath);
                        break;
                    case ".png":
                        mGameWindow.log("found image file: " + relativePath);
                        mGameWindow.textureLoader.fromPng(relativePath);
                        break;
                    case ".dds":
                        mGameWindow.log("found image file: " + relativePath);
                        mGameWindow.textureLoader.fromDds(relativePath);
                        break;
                    case ".xmf":
                        mGameWindow.log("found material file: " + relativePath);
                        mGameWindow.materialLoader.fromXmlFile(relativePath);
                        break;
                    case ".xsp":
                        mGameWindow.log("found shaderpair file: " + relativePath);
                        mGameWindow.shaderLoader.fromXmlFile(relativePath);
                        break;
                    case ".snip":
                        mGameWindow.log("found shader snipet: " + relativePath);
                        mGameWindow.shaderLoader.loadSnippet(relativePath);
                        break;
                    case ".xtmp":
                        mGameWindow.log("found template file: " + relativePath);
                        mGameWindow.templateLoader.fromXmlFile(relativePath);
                        break;
                    default:
                        break;
                }
            }
        }

        public List<string> GetFileList(string Root, bool SubFolders)
        {
            List<string> FileArray = new List<string>();

            try
            {
                string[] Files = System.IO.Directory.GetFiles(Root);
                string[] Folders = System.IO.Directory.GetDirectories(Root);

                for (int i = 0; i < Files.Length; i++)
                {
                    FileArray.Add(Files[i].ToString());
                }

                if (SubFolders == true)
                {
                    for (int i = 0; i < Folders.Length; i++)
                    {
                        FileArray.AddRange(GetFileList(Folders[i], SubFolders));
                    }
                }
            }
            catch (Exception Ex)
            {
                throw (Ex);
            }
            return FileArray;
        }
    }
}
