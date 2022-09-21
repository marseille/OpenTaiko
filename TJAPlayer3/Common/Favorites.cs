using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TJAPlayer3
{
    internal class Favorites
    {
        public void tFavorites() {
            if (!File.Exists("Favorite.json"))
                tSaveFile();

            tLoadFile();
        }

        #region [Auxiliary methods]

        public void tToggleFavorite(string chartID, int player)
        {
            if (tIsFavorite(chartID, player))
            {
                TJAPlayer3.Skin.sound取消音.t再生する();
                data.favorites[player].Remove(chartID);
            }
            else
            {
                TJAPlayer3.Skin.sound決定音.t再生する();
                data.favorites[player].Add(chartID);
            }
            tSaveFile();
        }

        public bool tIsFavorite(string chartID, int player = 0)
        {
            return data.favorites[player].Contains(chartID);
        }
        

        #endregion

        public class Data
        {
            public HashSet<string>[] favorites = new HashSet<string>[2] { new HashSet<string>(), new HashSet<string>() };
        }

        public Data data = new Data();

        #region [private]

        private void tSaveFile()
        {
            ConfigManager.SaveConfig(data, "Favorite.json");
        }

        private void tLoadFile()
        {
            data = ConfigManager.GetConfig<Data>(@"Favorite.json");
        }

        #endregion
    }

}
