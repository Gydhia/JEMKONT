using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace DownBelow.GameData
{
    /// <summary>
    /// To handle game version numbers such as 0.0.1 or 3.4.15
    /// </summary>
    public struct GameVersion : IComparable<GameVersion>
    {
        [DataMember]
        public int major;
        [DataMember]
        public int middle;
        [DataMember]
        public int minor;

        public readonly static GameVersion Zero = new GameVersion(0, 0, 0);

        private static GameVersion _current = GameVersion.Zero;
        public static GameVersion Current
        {
            get
            {
                if (_current == GameVersion.Zero)
                {
                    TryParse(Application.version, out _current);
                }
                return _current;
            }
        }

        public GameVersion(int major, int middle, int minor = 0)
        {
            this.major = major;
            this.middle = middle;
            this.minor = minor;
        }

        public static bool TryParse(string supposedVersion, out GameVersion gameversionValue)
        {
            //We check the integrity of the string
            if (!String.IsNullOrEmpty(supposedVersion))
            {
                //Split by the point, it is this one which delimits our types of versions
                string[] GameVersion = supposedVersion.Split('.');

                //Check of the integrity of the values of the version types, in case they are only 2
                if (GameVersion.Length == 2 && int.TryParse(GameVersion[0], out int major) &&
                    int.TryParse(GameVersion[1], out int middle))
                {
                    gameversionValue = new GameVersion(major, middle, 0);
                    return true;
                }
                //Check of the integrity of the values of the version types, in case they are only 3
                else if (GameVersion.Length == 3 && int.TryParse(GameVersion[0], out int major1) &&
                    int.TryParse(GameVersion[1], out int middle1) && int.TryParse(GameVersion[2], out int minor))
                {
                    gameversionValue = new GameVersion(major1, middle1, minor);
                    return true;
                }
            }

            gameversionValue = GameVersion.Zero;
            return false;
        }

        public override string ToString()
        {
            return this.major + "." + this.middle + "." + this.minor;
        }

        public int CompareTo(GameVersion compareVersion)
        {
            //Create a GameVersion to use the operators created above
            GameVersion initialVersion = new GameVersion(this.major, this.middle, this.minor);
            if (initialVersion > compareVersion)
                return 1;
            else if (initialVersion < compareVersion)
                return -1;
            return 0;
        }

        #region OPERATORS_OVERRIDES
        public static bool operator >(GameVersion version1, GameVersion version2)
        {
            return (version1.major > version2.major)
                || ((version1.major == version2.major) && (version1.middle > version2.middle))
                || ((version1.major == version2.major) && (version1.middle == version2.middle) && (version1.minor > version2.minor));
        }
        public static bool operator <(GameVersion version1, GameVersion version2)
        {
            return (version1.major < version2.major)
                || ((version1.major == version2.major) && (version1.middle < version2.middle))
                || ((version1.major == version2.major) && (version1.middle == version2.middle) && (version1.minor < version2.minor));
        }

        public static bool operator >=(GameVersion version1, GameVersion version2)
        {
            return (version1 > version2) 
                || (version1 == version2);
        }

        public static bool operator <=(GameVersion version1, GameVersion version2)
        {
            return (version1 < version2) 
                || (version1 == version2);
        }

        public static bool operator ==(GameVersion version1, GameVersion version2)
        {
            return (version1.major == version2.major)
                && (version1.middle == version2.middle)
                && (version1.minor == version2.minor);
        }

        public static bool operator !=(GameVersion version1, GameVersion version2)
        {
            return !(version1 == version2);
        }
        #endregion
    }

}
