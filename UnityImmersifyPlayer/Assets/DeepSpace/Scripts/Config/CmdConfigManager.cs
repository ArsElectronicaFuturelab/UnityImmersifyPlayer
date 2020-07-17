using UnityEngine;
using System.Collections;

namespace DeepSpace
{
	// Parses config values from the command line arguments.
	// A call to his Unity Application might look like this:
	// app.exe -mode=WALL -udpAddress=192.168.0.1 -udpPort=1111
	public class CmdConfigManager : MonoBehaviour
	{
		#region singleton
		//Here is a private reference only this class can access
		private static CmdConfigManager _instance;

		//This is the public reference that other classes will use
		public static CmdConfigManager Instance
		{
			get
			{
				//If _instance hasn't been set yet, we grab it from the scene!
				//This will only happen the first time this reference is used.
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType<CmdConfigManager>();
				}

				if (_instance == null)
				{
					Debug.LogWarning("ConfigManager component accessed but not found in scene!");
				}
				else
				{
					_instance.ParseConfiguration();
				}
				return _instance;
			}
		}
		#endregion

		private bool _configParsed = false;

		public enum AppType
		{
			WALL = 0,
			FLOOR = 1
		}

		public AppType applicationType = AppType.WALL;
		public bool invertStereo = false;

		private void Awake()
		{
			ParseConfiguration();
		}

		private void ParseConfiguration()
		{
			// Only parse the config once.
			if (_configParsed)
			{
				return;
			}
			_configParsed = true;

			// Just some preliminary command line parsing...
			// Use this as an example for your own command line arguments.
			string[] args = System.Environment.GetCommandLineArgs();
			char[] splitChar = { '=' };
			foreach (string argument in args)
			{
				if (argument.Contains("="))
				{
					string[] splitArgument = argument.Split(splitChar);

					string key = splitArgument[0];
					string value = splitArgument[1];

					// If there has been used an equal sign in the argument -> Add this to the value:
					if (splitArgument.Length > 2)
					{
						for (int ii = 2; ii < splitArgument.Length; ++ii)
						{
							value += splitArgument[ii];
						}
					}

					ParseArgument(key, value);
				}
			}

			FinishedParsingArguments();
		}

		// This will be called for each argument passed on from the commandline:
		protected virtual void ParseArgument(string key, string value)
		{
			// Make key and value lower case, so that it is not case sensitive.
			key = key.ToLower();
			value = value.ToLower();

			if (key.Equals("-mode"))
			{
				if (value == "wall")
				{
					applicationType = AppType.WALL;
				}
				else if (value == "floor")
				{
					applicationType = AppType.FLOOR;
				}
			}
			else if(key.Equals("-invertstereo"))
			{
				bool boolVal;
				if(ValueToBool(value, out boolVal))
				{
					invertStereo = boolVal;
				}
			}
		}

		protected virtual void FinishedParsingArguments()
		{
			// Derive from this class and override this method if you need a callback after all parameters have been parsed.
		}

		public bool ValueToInt(string val, out int intVal, bool logException = true)
		{
			bool success = false;
			intVal = 0;
			try
			{
				intVal = System.Convert.ToInt32(val);
				success = true;
			}
			catch (System.Exception e)
			{
				if (logException)
				{
					Debug.LogException(e);
				}
			}

			return success;
		}

		public bool ValueToBool(string value, out bool boolVal, bool logException = true)
		{
			bool success = false;
			boolVal = false;
			try
			{
				boolVal = System.Convert.ToBoolean(value);
				success = true;
			}
			catch (System.FormatException formatException)
			{
				// Could not convert value to a bool
				int checkInt = 0;
				if(ValueToInt(value, out checkInt, false))
				{
					// Converting int to bool:
					boolVal = checkInt != 0;
					success = true;
				}
				else
				{
					if(logException)
					{
						Debug.LogException(formatException);
					}
				}
			}

			return success;
		}

		public bool ValueToFloat(string val, out float floatVal, bool logException = true)
		{
			bool success = false;
			floatVal = 0f;
			try
			{
				floatVal = System.Convert.ToSingle(val, new System.Globalization.CultureInfo("en-US")); // valid float: "12.34"  
				success = true;
			}
			catch (System.Exception e)
			{
				if (logException)
				{
					Debug.LogException(e);
				}
			}

			return success;
		}
	}
}