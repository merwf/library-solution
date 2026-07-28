using LibraryConfigUtilities;
using System.Collections.Generic;

namespace Library.Business.Interfaces
{
    public interface ICountrySettingProvider
    {
        List<Country> GetCountrySettings();
    }
}