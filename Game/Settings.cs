using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace OpenTkProject.Game
{
	public enum SettingTypes
	{
		ST_Video,
		ST_Sound,
		ST_Game,
	}

	/// <summary>
	/// Holds single setting parameter and value
	/// </summary>
	public class Setting
	{
		public FieldInfo parameter;
		public object value;
	}

	/// <summary>
	/// Holds multiple settings
	/// </summary>
	public class Preset
	{
		public string presetKey;
		public string presetName;
		public SettingTypes presetType;
		public List<Setting> presetSettings = new List<Setting>();

		public Preset(string pkey, string name,SettingTypes type)
		{
			presetKey = pkey;
			presetName = name;
			presetType = type;
		}

		public void SetValue(FieldInfo param, object value)
		{
			Setting newSetting = new Setting();
			newSetting.parameter = param;
			newSetting.value = value;
			presetSettings.Add(newSetting);
		}

		Type GetTypePreset()
		{
			switch (presetType)
			{
				case SettingTypes.ST_Video:
					return typeof(VideoSettings);

				case SettingTypes.ST_Sound:
					return typeof(SoundSettings);

				case SettingTypes.ST_Game:
					return typeof(GameSettings);
			}

			throw new Exception("Unknown preset type");
		}

		public void SetValue(string paramName, object value)
		{
			Type cType = GetTypePreset();

			FieldInfo fi = cType.GetField(paramName);
			if (fi == null)
			{
				throw new Exception(string.Format("Parameter {0} not found for preset type {1}", paramName, presetType.ToString()));
			}
			SetValue(fi, value);
		}

		public void SetAllQualityLevels(QualityLevel newLevel)
		{
			Type cType = GetTypePreset();
			FieldInfo[] fields = cType.GetFields();

			foreach (FieldInfo f in fields)
			{
				if (f.FieldType == typeof(QualityLevel))
				{
					SetValue(f, newLevel);
				}
			}
		}

		public void ApplyToSettings(object obj)
		{
			foreach (Setting s in presetSettings)
			{
				s.parameter.SetValue(obj, s.value);
			}
		}
	}

	public class PresetManager
	{
		public SettingTypes managerType;
		public List<Preset> presets = new List<Preset>();

		public Preset CreateQualityLevel(QualityLevel level)
		{
			string presetTypeName = managerType.ToString();
			presetTypeName = presetTypeName.Substring(presetTypeName.IndexOf('_')+1);

			string presetQualityName = level.ToString();
			presetQualityName = presetQualityName[0] + presetQualityName.ToLower().Substring(1);

			string finalName = string.Format("{0} Quality {1}", presetQualityName, presetTypeName);

			Preset ql = new Preset(presetQualityName.ToLower(), finalName, managerType);
			ql.SetAllQualityLevels(level);
			presets.Add(ql);

			return ql;
		}

		public Preset GetPreset(string pkey)
		{
			foreach (Preset p in presets)
			{
				if (p.presetKey == pkey)
				{
					return p;
				}
			}

			return null;
		}

		public void CreateDefaultQualityLevels()
		{
			CreateQualityLevel(QualityLevel.Low);
			CreateQualityLevel(QualityLevel.Medium);
			CreateQualityLevel(QualityLevel.High);
		}

		public void SavePresets(string path)
		{
			FileStream fs = File.Create(path);
			SavePresets(fs);
			fs.Close();
		}

		public void SavePresets(Stream output)
		{
			XmlWriter xw = GenericMethods.CoolXMLWriter(output);

			xw.WriteStartElement("presets"); // base node

			foreach (Preset p in presets)
			{
				xw.WriteStartElement("preset");

				xw.WriteAttributeString("key", p.presetKey);
				xw.WriteAttributeString("name", p.presetName);
				xw.WriteAttributeString("type", p.presetType.ToString());

				foreach (Setting s in p.presetSettings)
				{
					xw.WriteStartElement("setting");
					xw.WriteAttributeString("param", s.parameter.Name);
					xw.WriteAttributeString("value", s.value.ToString());
					xw.WriteEndElement();
				}

				xw.WriteEndElement();
			}

			xw.WriteEndElement();

			xw.Close();
		}

		/************************************************************************/
		/* We can create different presets for different devices, and get them by their name
		 * Like for ati 3870 we can define a preset for it, which works best and use it
		/************************************************************************/
	}

	public class VideoPresets: PresetManager
	{
		public static VideoPresets Instance = new VideoPresets();

		VideoPresets()
		{
			managerType = SettingTypes.ST_Video;
			Preset low = CreateQualityLevel(QualityLevel.Low);
			low.SetValue("postProcessing", false);
			low.SetValue("ambientOcclusion", false);
			low.SetValue("bloom", false);

			Preset medium = CreateQualityLevel(QualityLevel.Medium);
			medium.SetValue("postProcessing", true);
			medium.SetValue("ambientOcclusion", false);
			medium.SetValue("bloom", true);

			Preset high = CreateQualityLevel(QualityLevel.High);
			high.SetValue("postProcessing", true);
			high.SetValue("ambientOcclusion", true);
			high.SetValue("bloom", true);
		}
	}

	public enum QualityLevel
	{
		Low,
		Medium,
		High,
	}

	public class VideoSettings
	{
		public bool postProcessing;
		public bool ambientOcclusion;
		public bool bloom;

		public QualityLevel shadow;
		public QualityLevel shader;
		public QualityLevel lighting;

		/// <summary>
		/// Added to just showing it can be done :P
		/// </summary>
		public float gamma;
	}

	public class SoundSettings
	{
		public bool eax;
		public float effectsVolume;
		public float musicVolume;
		public float speechVolume;
	}

	public class GameSettings
	{

	}

	public class Settings
	{
		public static Settings Instance = new Settings();

		public VideoSettings video = new VideoSettings();
		public SoundSettings sound = new SoundSettings();
		public GameSettings game = new GameSettings();

		public void SaveSettings(string path)
		{
			try
			{
				FileStream fs = File.Create(path);
				SaveSettings(fs);
				fs.Close();
			}
			catch (System.Exception ex)
			{
				Console.WriteLine("Error occured while saving settings: {0}", ex.Message);
			}
		}

		public void SaveSettings(Stream output)
		{
			XmlWriter xw = GenericMethods.CoolXMLWriter(output);
			XmlSerializer ser = new XmlSerializer(typeof(Settings));
			ser.Serialize(xw, this);
			xw.Close();
		}

		public void LoadSettings(string path)
		{
			try
			{
				FileStream fs = File.OpenRead(path);
				LoadSettings(fs);
				fs.Close();
			}
			catch (System.Exception ex)
			{
				Console.WriteLine("Error occured while loading settings: {0}", ex.Message);	
			}
			
		}

		public void LoadSettings(Stream input)
		{
			XmlReader xr = XmlReader.Create(input);
			XmlSerializer ser = new XmlSerializer(typeof(Settings));
			Settings s = (Settings)ser.Deserialize(xr);
			video = s.video;
			sound = s.sound;
			game = s.game;
			xr.Close();
		}
	}
}
