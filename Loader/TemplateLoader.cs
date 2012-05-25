using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Collections;
using System.Xml;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject
{
    public struct Template
    {
        const int FROM_XML = 0;

        public int type;
        public int identifier;

        public List<string> meshes;
        public List<string> pmeshes;
        public List<string> materials;

        public string pointer;
        public string name;

        public bool isStatic;
        public bool loaded;


        public float positionOffset;
        public int filePosition;
        public bool hasLight;
        public Vector3 lightColor;
        public bool normal;
        public string useType;
        public float volumeRadius;
    }

    public class TemplateLoader : GameObject
    {
        public List<Template> templates = new List<Template> { };
        public Hashtable templateNames = new Hashtable();

        public List<PhysModel> templatePhyModels = new List<PhysModel> { };

        public TemplateLoader(OpenTkProjectWindow gameWindow)
            : base(gameWindow)
        {
        }

        public void fromXmlFile(string pointer)
        {
            string name = pointer.Replace(gameWindow.templateFolder, "");

            Template newTemp = new Template();

            newTemp.type = Material.FROM_XML;
            newTemp.loaded = false;
            newTemp.name = name;
            newTemp.pointer = pointer;
            newTemp.filePosition = 0;

            register(newTemp);
        }


        private void register(Template newTemp)
        {
            newTemp.identifier = templates.Count;
            templates.Add(newTemp);
            templateNames.Add(newTemp.name, newTemp.identifier);
        }

        public void loadTemplates()
        {
            for (int i = 0; i < templates.Count; i++)
            {
                loadTemplate(templates[i]);
            }
        }

        public Template getTemplate(string name)
        {
            int id = (int)templateNames[name];
            return templates[id];
        }


        public Template getTemplate(int id)
        {
            return templates[id];
        }

        public float loadSingleTemplates()
        {
            for (int i = 0; i < templates.Count; i++)
            {
                if (!templates[i].loaded)
                {
                    loadTemplate(templates[i]);
                    return (float)i / (float)templates.Count;
                }
            }
            return 1;
        }

        private void loadTemplate(Template target)
        {
            XmlTextReader reader = new XmlTextReader(target.pointer);

            target.meshes = new List<string> { };
            target.pmeshes = new List<string> { };
            target.materials = new List<string> { };

            target.isStatic = false;
            target.useType = "pmodel";

            while (reader.Read())
            {
                // parsing data in template tag
                if (reader.Name == "template")
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == "name")
                            target.name = reader.Value;

                        if (reader.Name == "type")
                            target.useType = reader.Value;

                    }

                    gameWindow.log("parsing template: " + target.name);
                    reader.MoveToElement();

                    while (reader.Read())
                    {
                        if (reader.Name == "template")
                            break;

                        if (reader.Name == "static")
                            target.isStatic = true;

                        if (reader.Name == "material" && reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "source")
                                {
                                    target.materials.Add(reader.Value);
                                    gameWindow.log("material: " + reader.Value);
                                }
                            }
                        }

                        if (reader.Name == "mesh" && reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "source")
                                {
                                    target.meshes.Add(reader.Value);
                                    gameWindow.log("mesh: " + reader.Value);
                                }
                            }
                        }

                        if (reader.Name == "pmesh" && reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "source")
                                {
                                    target.pmeshes.Add(reader.Value);
                                    gameWindow.log("phys mesh: " + reader.Value);
                                }
                            }
                        }

                        if (reader.Name == "position" && reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "offset")
                                {
                                    target.positionOffset = GenericMethods.FloatFromString(reader.Value);
                                    gameWindow.log("offset: " + reader.Value);
                                }
                            }
                        }

                        if (reader.Name == "volume" && reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "radius")
                                {
                                    target.volumeRadius = GenericMethods.FloatFromString(reader.Value);
                                    gameWindow.log("radius: " + reader.Value);
                                }
                            }
                        }

                        if (reader.Name == "light" && reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                target.hasLight = true;

                                if (reader.Name == "color")
                                {
                                    target.lightColor = GenericMethods.VectorFromString(reader.Value);
                                }
                            }
                        }

                        if (reader.Name == "normal")
                        {
                            target.normal = true;
                        }

                        target.loaded = true;
                        templates[target.identifier] = target;
                    }
                }
            }
        }
    }
}
