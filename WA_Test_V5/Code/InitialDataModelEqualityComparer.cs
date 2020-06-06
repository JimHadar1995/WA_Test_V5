using System.Collections.Generic;
using WA_Test_V5.Models;

namespace WA_Test_V5.Code
{
    /// <summary>
    /// Компаратор для сравнения строк из исходного файла
    /// </summary>
    public class InitialDataModelEqualityComparer : IEqualityComparer<InitialDataModel>
    {
        public bool Equals(InitialDataModel x, InitialDataModel y)
        {
            return string.Equals(x.Uniq, y.Uniq);
        }

        public int GetHashCode(InitialDataModel obj)
        {
            var hashCode = -1919740922;
            return GetStirngHashCode(hashCode, obj.Uniq);
        }

        private int GetStirngHashCode(int hashCode, string str)
        {
            unchecked
            {
                return hashCode * -1521134295 * EqualityComparer<string>.Default.GetHashCode(str);
            }
        }
    }
}