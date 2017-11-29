using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DK8;
//using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Globalization;
using System.IO.IsolatedStorage;

namespace ProcrastinatingSquirrel
{
	class Profile
	{
		public static Profile Instance;

		public bool HasSaves = false;
		public List<string> Saves = new List<string>();
		public string CurrentSaveName = "";
		public bool IsLoaded = false;
		public bool MusicOn = true;
		public bool SoundsOn = true;
		public bool FullscreenOn = false;

		public Profile()
		{
			Instance = this;

			// Load profile
			try
			{
                //StorageDevice device = CFrameData.Instance.StorageDevice;
                //IAsyncResult result = device.BeginOpenContainer("Profile", null, null);
                //result.AsyncWaitHandle.WaitOne();
                //StorageContainer container = device.EndOpenContainer(result);
                //result.AsyncWaitHandle.Close();

                IsolatedStorageFile container = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

                // Load saves
                string filename = "saves.dat";
				if (!container.FileExists(filename))
				{
					HasSaves = false;
				}
				else
				{
					BinaryReader fic_in = new BinaryReader(container.OpenFile(filename, System.IO.FileMode.Open));
					while (fic_in.PeekChar() > 0)
					{
						Saves.Add(fic_in.ReadString());
					}
					if (Saves.Count() == 0) HasSaves = false;
					else HasSaves = true;
					fic_in.Close();
				}

				// Load player profile, so we can get the recent save and launch it right away
				filename = "profile.dat";
				if (container.FileExists(filename))
				{
					BinaryReader fic_in = new BinaryReader(container.OpenFile(filename, System.IO.FileMode.Open));
					string recentSave = fic_in.ReadString();
					if (Saves.Contains(recentSave))
					{
						CurrentSaveName = recentSave;
					}
					else
					{
						// Just pick the last one (Most recent). but this shouldn't have happened
						if (Saves.Count() > 0)
						{
							CurrentSaveName = Saves.Last();
						}
						else
						{
							// No saves in the profile, continue with launching the game normally I guess
						}
					}
					try
					{
						MusicOn = fic_in.ReadBoolean();
						SoundsOn = fic_in.ReadBoolean();
                        FullscreenOn = fic_in.ReadBoolean();
					}
					catch
					{
						MusicOn = true;
						SoundsOn = true;
                        FullscreenOn = false;
                    }
					fic_in.Close();
				}

				container.Dispose();
			}
			catch (Exception ex)
			{
				if (IsExitingGame) return; // Ignore
				CFrameData.Instance.WhoTheHellRemoveStorageDevice();
			}

			IsLoaded = true;
		}

		public bool IsExitingGame = false;

		public void CreateNewSave()
		{
			string saveName = DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo);
			saveName = saveName.Replace('\\', '_');
			saveName = saveName.Replace('/', '_');
			saveName = saveName.Replace('.', '_');
			saveName = saveName.Replace(' ', '_');
			saveName = saveName.Replace(':', '_');

			CurrentSaveName = saveName;

			// Save the new save + profile with it selected
			try
			{
				//StorageDevice device = CFrameData.Instance.StorageDevice;
				//IAsyncResult result = device.BeginOpenContainer("Profile", null, null);
				//result.AsyncWaitHandle.WaitOne();
				//StorageContainer container = device.EndOpenContainer(result);
				//result.AsyncWaitHandle.Close();

                IsolatedStorageFile container = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

                string filename = "saves.dat";
				BinaryWriter fic_out;
				if (!container.FileExists(filename))
				{
					fic_out = new BinaryWriter(container.CreateFile(filename));
				}
				else
				{
					fic_out = new BinaryWriter(container.OpenFile(filename, FileMode.Append));
				}
				fic_out.Write(CurrentSaveName);
				Saves.Add(CurrentSaveName);
				fic_out.Close();

				filename = "profile.dat";
				fic_out = new BinaryWriter(container.OpenFile(filename, FileMode.Create));
				fic_out.Write(CurrentSaveName);
				fic_out.Write(MusicOn);
				fic_out.Write(SoundsOn);
				fic_out.Write(FullscreenOn);
				fic_out.Close();

				container.Dispose();
			}
			catch
			{
				CFrameData.Instance.WhoTheHellRemoveStorageDevice();
			}

			IsLoaded = true;
		}

		public void DeleteSave(string saveName)
		{
			// Check first if this chunk is on disk.
			try
			{
				//StorageDevice device = CFrameData.Instance.StorageDevice;
				//IAsyncResult result = device.BeginOpenContainer("Chunks_" + saveName, null, null);
				//result.AsyncWaitHandle.WaitOne();
				//StorageContainer container = device.EndOpenContainer(result);
				//result.AsyncWaitHandle.Close();

                IsolatedStorageFile container = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

                string[] allFiles = container.GetFileNames("*_" + saveName + ".sav");
				foreach (string filename in allFiles)
				{
					container.DeleteFile(filename);
				}
				container.Dispose();

				// Remove the save from the save array
				Saves.Remove(saveName);

				// Save it again
				//result = device.BeginOpenContainer("Profile", null, null);
				//result.AsyncWaitHandle.WaitOne();
				//container = device.EndOpenContainer(result);
				//result.AsyncWaitHandle.Close();
				string savesDat = "saves.dat";
				BinaryWriter fic_out = new BinaryWriter(container.OpenFile(savesDat, FileMode.Create));
				foreach (string save in Saves)
				{
					fic_out.Write(save);
				}
				fic_out.Close();

				if (Saves.Count() == 0)
				{
					CreateNewSave();
					container.Dispose();
					return;
				}

				// Select the last one, save that to profile
				if (saveName == CurrentSaveName) // This shall never be the case
				{
					CurrentSaveName = Saves.Last();
					string profileDat = "profile.dat";
					fic_out = new BinaryWriter(container.OpenFile(profileDat, FileMode.Create));
					fic_out.Write(CurrentSaveName);
					fic_out.Write(MusicOn);
					fic_out.Write(SoundsOn);
					fic_out.Write(FullscreenOn);
					fic_out.Close();
				}

				container.Dispose();
			}
			catch
			{
				CFrameData.Instance.WhoTheHellRemoveStorageDevice();
			}
		}

		internal void Dispose()
		{
			Instance = null;
		}

		internal void SetToDefaultWorld()
		{
			try
			{
				//StorageDevice device = CFrameData.Instance.StorageDevice;
				//IAsyncResult result = device.BeginOpenContainer("Profile", null, null);
				//result.AsyncWaitHandle.WaitOne();
				//StorageContainer container = device.EndOpenContainer(result);
				//result.AsyncWaitHandle.Close();

                IsolatedStorageFile container = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

                string filename = "profile.dat";
				BinaryWriter fic_out;
				fic_out = new BinaryWriter(container.OpenFile(filename, FileMode.Create));
				fic_out.Write(CurrentSaveName);
				fic_out.Write(MusicOn);
				fic_out.Write(SoundsOn);
				fic_out.Write(FullscreenOn);
				fic_out.Close();

				container.Dispose();
			}
			catch
			{
				CFrameData.Instance.WhoTheHellRemoveStorageDevice();
			}
		}
	}
}
