using Library.Business.Interfaces;
using LibraryConfigUtilities;
using System.Collections.Generic;

namespace Library.Business.Concrete
{
    public class CountrySettingProvider : ICountrySettingProvider
    {
        public List<Country> GetCountrySettings()
        {
            // LibrarySetting altyapısını bu tek provider arkasında izole ediyoruz.
            return new LibrarySetting().LibrarySettingList;
        }
    }
}