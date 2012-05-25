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
                if (extention == ".obj")
                {
                    mGameWindow.log("found object file: " + relativePath);
                    mGameWindow.meshLoader.fromObj(relativePath);
                }
                if (extention == ".png")
                {
                    mGameWindow.log("found image file: " + relativePath);
                    mGameWindow.textureLoader.fromFile(relativePath,true);
                }
                if (extention == ".xmf")
                {
                    mGameWindow.log("found material file: " + relativePath);
                    mGameWindow.materialLoader.fromXmlFile(relativePath);
                }
                if (extention == ".xsp")
                {
                    mGameWindow.log("found shaderpair file: " + relativePath);
                    mGameWindow.shaderLoader.fromXmlFile(relativePath);
                }
                if (extention == ".xtmp")
                {
                    mGameWindow.log("found template file: " + relativePath);
                    mGameWindow.templateLoader.fromXmlFile(relativePath);
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
