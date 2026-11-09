using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones.Catalog
{
    /// <summary>
    /// Містить довідкові дані телефонної нумерації та перевіряє
    /// допустимість поєднання міжнародного коду країни
    /// й довжини національної частини.
    ///
    /// Перевірка враховує всі записи зі спільним міжнародним кодом.
    ///
    /// (Contains telephone-numbering reference data and checks
    /// whether a country calling code and national number length
    /// form a supported combination.
    ///
    /// The check considers all entries sharing the same calling code.)
    /// </summary>
    internal static class PhoneNumberingPlanCatalog
    {
        /// <summary>
        /// Зберігає довідкові записи за назвами країн і територій.
        /// Кожен запис містить телефонні коди та допустимі довжини
        /// національної частини.
        ///
        /// Кілька записів можуть містити однаковий міжнародний код.
        ///
        /// (Stores reference entries keyed by country and territory names.
        /// Each entry contains calling codes and allowed national number lengths.
        ///
        /// Multiple entries may contain the same international calling code.)
        /// </summary>
        private static IReadOnlyDictionary<string, PhoneInfo> Catalog = new Dictionary<string, PhoneInfo>()
        {
            {
                "USA", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths =[10]
                }
            },

            {
                "Guam", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Northern Mariana Islands", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "American Samoa", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "United States Virgin Islands", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Puerto Rico", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Canada", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Anguilla", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Antigua and Barbuda", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Commonwealth of the Bahamas", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Barbados", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Bermuda Islands", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Cayman Islands", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Dominica", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Dominican Republic", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Grenada", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Jamaica", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Montserrat", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Federation of Saint Kitts and Nevis", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Saint Lucia", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Saint Vincent and the Grenadines", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Republic of Trinidad and Tobago", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "the Turks and Caicos Islands", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Egypt", new PhoneInfo
                {
                    CountryCodes = ["+20"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "South Sudan", new PhoneInfo
                {
                    CountryCodes = ["+211"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Morocco", new PhoneInfo
                {
                    CountryCodes = ["+212"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Western Sahara", new PhoneInfo
                {
                    CountryCodes = ["+212"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Algeria", new PhoneInfo
                {
                    CountryCodes = ["+213"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Tunisia", new PhoneInfo
                {
                    CountryCodes = ["+216"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Libya", new PhoneInfo
                {
                    CountryCodes = ["+218"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Republic of The Gambia", new PhoneInfo
                {
                    CountryCodes = ["+220"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Senegal", new PhoneInfo
                {
                    CountryCodes = ["+221"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Mauritania", new PhoneInfo
                {
                    CountryCodes = ["+222"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Mali", new PhoneInfo
                {
                    CountryCodes = ["+223"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Guinea", new PhoneInfo
                {
                    CountryCodes = ["+224"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Ivory Coast", new PhoneInfo
                {
                    CountryCodes = ["+225"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Burkina Faso", new PhoneInfo
                {
                    CountryCodes = ["+226"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Niger", new PhoneInfo
                {
                    CountryCodes = ["+227"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Togo", new PhoneInfo
                {
                    CountryCodes = ["+228"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Benin", new PhoneInfo
                {
                    CountryCodes = ["+229"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Republic of Mauritius", new PhoneInfo
                {
                    CountryCodes = ["+230"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Liberia", new PhoneInfo
                {
                    CountryCodes = ["+231"],
                    NationalNumberLengths = [7, 9]
                }
            },

            {
                "Republic of Sierra Leone", new PhoneInfo
                {
                    CountryCodes = ["+232"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Republic of Ghana", new PhoneInfo
                {
                    CountryCodes = ["+233"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Federal Republic of Nigeria", new PhoneInfo
                {
                    CountryCodes = ["+234"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Chad", new PhoneInfo
                {
                    CountryCodes = ["+235"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Central African Republic", new PhoneInfo
                {
                    CountryCodes = ["+236"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Cameroon", new PhoneInfo
                {
                    CountryCodes = ["+237"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Cape Verde", new PhoneInfo
                {
                    CountryCodes = ["+238"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "São Tomé and Príncipe", new PhoneInfo
                {
                    CountryCodes = ["+239"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Equatorial Guinea", new PhoneInfo
                {
                    CountryCodes = ["+240"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Gabon", new PhoneInfo
                {
                    CountryCodes = ["+241"],
                    NationalNumberLengths = [7, 8]
                }
            },

            {
                "Republic of the Congo", new PhoneInfo
                {
                    CountryCodes = ["+242"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Democratic Republic of the Congo", new PhoneInfo
                {
                    CountryCodes = ["+243"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Angola", new PhoneInfo
                {
                    CountryCodes = ["+244"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Guinea-Bissau", new PhoneInfo
                {
                    CountryCodes = ["+245"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Diego Garcia", new PhoneInfo
                {
                    CountryCodes = ["+246"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Ascension Island", new PhoneInfo
                {
                    CountryCodes = ["+247"],
                    NationalNumberLengths = [5]
                }
            },

            {
                "Seychelles", new PhoneInfo
                {
                    CountryCodes = ["+248"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Sudan", new PhoneInfo
                {
                    CountryCodes = ["+249"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Rwanda", new PhoneInfo
                {
                    CountryCodes = ["+250"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Ethiopia", new PhoneInfo
                {
                    CountryCodes = ["+251"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Somalia", new PhoneInfo
                {
                    CountryCodes = ["+252"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Djibouti", new PhoneInfo
                {
                    CountryCodes = ["+253"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Kenya", new PhoneInfo
                {
                    CountryCodes = ["+254"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Tanzania", new PhoneInfo
                {
                    CountryCodes = ["+255"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Zanzibar", new PhoneInfo
                {
                    CountryCodes = ["+255"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Uganda", new PhoneInfo
                {
                    CountryCodes = ["+256"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Burundi", new PhoneInfo
                {
                    CountryCodes = ["+257"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Mozambique", new PhoneInfo
                {
                    CountryCodes = ["+258"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Zambia", new PhoneInfo
                {
                    CountryCodes = ["+260"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Madagascar", new PhoneInfo
                {
                    CountryCodes = ["+261"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Reunion", new PhoneInfo
                {
                    CountryCodes = ["+262"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Mayotte", new PhoneInfo
                {
                    CountryCodes = ["+262"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Zimbabwe", new PhoneInfo
                {
                    CountryCodes = ["+263"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Namibia", new PhoneInfo
                {
                    CountryCodes = ["+264"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Malawi", new PhoneInfo
                {
                    CountryCodes = ["+265"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Lesotho", new PhoneInfo
                {
                    CountryCodes = ["+266"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Botswana", new PhoneInfo
                {
                    CountryCodes = ["+267"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Eswatini", new PhoneInfo
                {
                    CountryCodes = ["+268"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Comoros", new PhoneInfo
                {
                    CountryCodes = ["+269"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "South Africa", new PhoneInfo
                {
                    CountryCodes = ["+27"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Saint Helena, Ascension", new PhoneInfo
                {
                    CountryCodes = ["+290"],
                    NationalNumberLengths = [5]
                }
            },

            {
                "Tristan da Cunha", new PhoneInfo
                {
                    CountryCodes = ["+290"],
                    NationalNumberLengths = [4]
                }
            },

            {
                "Eritrea", new PhoneInfo
                {
                    CountryCodes = ["+291"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Aruba", new PhoneInfo
                {
                    CountryCodes = ["+297"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Faroe Islands", new PhoneInfo
                {
                    CountryCodes = ["+298"],
                    NationalNumberLengths = [6]
                }
            },

            {
                "Greenland", new PhoneInfo
                {
                    CountryCodes = ["+299"],
                    NationalNumberLengths = [6]
                }
            },

            {
                "Greece", new PhoneInfo
                {
                    CountryCodes = ["+30"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Netherlands", new PhoneInfo
                {
                    CountryCodes = ["+31"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Belgium", new PhoneInfo
                {
                    CountryCodes = ["+32"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "France", new PhoneInfo
                {
                    CountryCodes = ["+33"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Spain", new PhoneInfo
                {
                    CountryCodes = ["+34"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Gibraltar", new PhoneInfo
                {
                    CountryCodes = ["+350"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Portugal", new PhoneInfo
                {
                    CountryCodes = ["+351"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Luxembourg", new PhoneInfo
                {
                    CountryCodes = ["+352"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Ireland", new PhoneInfo
                {
                    CountryCodes = ["+353"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Iceland", new PhoneInfo
                {
                    CountryCodes = ["+354"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Albania", new PhoneInfo
                {
                    CountryCodes = ["+355"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Malta", new PhoneInfo
                {
                    CountryCodes = ["+356"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Cyprus", new PhoneInfo
                {
                    CountryCodes = ["+357"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Finland", new PhoneInfo
                {
                    CountryCodes = ["+358"],
                    NationalNumberLengths = [6, 7, 8, 9, 10]
                }
            },

            {
                "Bulgaria", new PhoneInfo
                {
                    CountryCodes = ["+359"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Hungary", new PhoneInfo
                {
                    CountryCodes = ["+36"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Lithuania", new PhoneInfo
                {
                    CountryCodes = ["+370"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Latvia", new PhoneInfo
                {
                    CountryCodes = ["+371"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Estonia", new PhoneInfo
                {
                    CountryCodes = ["+372"],
                    NationalNumberLengths = [7, 8]
                }
            },

            {
                "Moldova", new PhoneInfo
                {
                    CountryCodes = ["+373"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Armenia", new PhoneInfo
                {
                    CountryCodes = ["+374"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Nagorno-Karabakh", new PhoneInfo
                {
                    CountryCodes = ["+374"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Belarus", new PhoneInfo
                {
                    CountryCodes = ["+375"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Andorra", new PhoneInfo
                {
                    CountryCodes = ["+376"],
                    NationalNumberLengths = [6]
                }
            },

            {
                "Monaco", new PhoneInfo
                {
                    CountryCodes = ["+377"],
                    NationalNumberLengths = [8, 9]
                }
            },

            {
                "Kosovo (Monaco code)", new PhoneInfo
                {
                    CountryCodes = ["+377"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "San Marino", new PhoneInfo
                {
                    CountryCodes = ["+378"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Ukraine", new PhoneInfo
                {
                    CountryCodes = ["+380"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Serbia", new PhoneInfo
                {
                    CountryCodes = ["+381"],
                    NationalNumberLengths = [8, 9, 10]
                }
            },

            {
                "Kosovo (Serbia code)", new PhoneInfo
                {
                    CountryCodes = ["+381"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Montenegro", new PhoneInfo
                {
                    CountryCodes = ["+382"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Kosovo", new PhoneInfo
                {
                    CountryCodes = ["+383"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Croatia", new PhoneInfo
                {
                    CountryCodes = ["+385"],
                    NationalNumberLengths = [8, 9]
                }
            },

            {
                "Slovenia", new PhoneInfo
                {
                    CountryCodes = ["+386"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Kosovo (Slovenia code)", new PhoneInfo
                {
                    CountryCodes = ["+386"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Bosnia and Herzegovina", new PhoneInfo
                {
                    CountryCodes = ["+387"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "North Macedonia", new PhoneInfo
                {
                    CountryCodes = ["+389"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Italy", new PhoneInfo
                {
                    CountryCodes = ["+39"],
                    NationalNumberLengths = [9, 10]
                }
            },

            {
                "Vatican City", new PhoneInfo
                {
                    CountryCodes = ["+39"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Romania", new PhoneInfo
                {
                    CountryCodes = ["+40"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Switzerland", new PhoneInfo
                {
                    CountryCodes = ["+41"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Czech Republic", new PhoneInfo
                {
                    CountryCodes = ["+420"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Slovakia", new PhoneInfo
                {
                    CountryCodes = ["+421"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Liechtenstein", new PhoneInfo
                {
                    CountryCodes = ["+423"],
                    NationalNumberLengths = [7, 9]
                }
            },

            {
                "Austria", new PhoneInfo
                {
                    CountryCodes = ["+43"],
                    NationalNumberLengths = [10, 11, 12, 13]
                }
            },

            {
                "United Kingdom", new PhoneInfo
                {
                    CountryCodes = ["+44"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Guernsey", new PhoneInfo
                {
                    CountryCodes = ["+44"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Jersey", new PhoneInfo
                {
                    CountryCodes = ["+44"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Isle of Man", new PhoneInfo
                {
                    CountryCodes = ["+44"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Denmark", new PhoneInfo
                {
                    CountryCodes = ["+45"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Sweden", new PhoneInfo
                {
                    CountryCodes = ["+46"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Norway", new PhoneInfo
                {
                    CountryCodes = ["+47"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Poland", new PhoneInfo
                {
                    CountryCodes = ["+48"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Germany", new PhoneInfo
                {
                    CountryCodes = ["+49"],
                    NationalNumberLengths = [10, 11]
                }
            },

            {
                "Falkland Islands", new PhoneInfo
                {
                    CountryCodes = ["+500"],
                    NationalNumberLengths = [5]
                }
            },

            {
                "Belize", new PhoneInfo
                {
                    CountryCodes = ["+501"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Guatemala", new PhoneInfo
                {
                    CountryCodes = ["+502"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "El Salvador", new PhoneInfo
                {
                    CountryCodes = ["+503"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Honduras", new PhoneInfo
                {
                    CountryCodes = ["+504"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Nicaragua", new PhoneInfo
                {
                    CountryCodes = ["+505"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Costa Rica", new PhoneInfo
                {
                    CountryCodes = ["+506"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Panama", new PhoneInfo
                {
                    CountryCodes = ["+507"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Saint Pierre and Miquelon", new PhoneInfo
                {
                    CountryCodes = ["+508"],
                    NationalNumberLengths = [6]
                }
            },

            {
                "Haiti", new PhoneInfo
                {
                    CountryCodes = ["+509"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Peru", new PhoneInfo
                {
                    CountryCodes = ["+51"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Mexico", new PhoneInfo
                {
                    CountryCodes = ["+52"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Cuba", new PhoneInfo
                {
                    CountryCodes = ["+53"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Argentina", new PhoneInfo
                {
                    CountryCodes = ["+54"],
                    NationalNumberLengths = [11]
                }
            },

            {
                "Brazil", new PhoneInfo
                {
                    CountryCodes = ["+55"],
                    NationalNumberLengths = [11]
                }
            },

            {
                "Chile", new PhoneInfo
                {
                    CountryCodes = ["+56"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Colombia", new PhoneInfo
                {
                    CountryCodes = ["+57"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Venezuela", new PhoneInfo
                {
                    CountryCodes = ["+58"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Guadeloupe", new PhoneInfo
                {
                    CountryCodes = ["+590"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Bolivia", new PhoneInfo
                {
                    CountryCodes = ["+591"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Guyana", new PhoneInfo
                {
                    CountryCodes = ["+592"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Ecuador", new PhoneInfo
                {
                    CountryCodes = ["+593"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "French Guiana", new PhoneInfo
                {
                    CountryCodes = ["+594"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Paraguay", new PhoneInfo
                {
                    CountryCodes = ["+595"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Martinique", new PhoneInfo
                {
                    CountryCodes = ["+596"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Suriname", new PhoneInfo
                {
                    CountryCodes = ["+597"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Uruguay", new PhoneInfo
                {
                    CountryCodes = ["+598"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Sint Eustatius", new PhoneInfo
                {
                    CountryCodes = ["+599"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Saba", new PhoneInfo
                {
                    CountryCodes = ["+599"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Sint Maarten", new PhoneInfo
                {
                    CountryCodes = ["+1"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Bonaire", new PhoneInfo
                {
                    CountryCodes = ["+599"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Curacao", new PhoneInfo
                {
                    CountryCodes = ["+599"],
                    NationalNumberLengths = [7, 8]
                }
            },

            {
                "Malaysia", new PhoneInfo
                {
                    CountryCodes = ["+60"],
                    NationalNumberLengths = [9, 10]
                }
            },

            {
                "Australia", new PhoneInfo
                {
                    CountryCodes = ["+61"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Cocos (Keeling) Islands", new PhoneInfo
                {
                    CountryCodes = ["+61"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Christmas Island", new PhoneInfo
                {
                    CountryCodes = ["+61"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Indonesia", new PhoneInfo
                {
                    CountryCodes = ["+62"],
                    NationalNumberLengths = [9, 10, 11, 12]
                }
            },

            {
                "Philippines", new PhoneInfo
                {
                    CountryCodes = ["+63"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "New Zealand", new PhoneInfo
                {
                    CountryCodes = ["+64"],
                    NationalNumberLengths = [8, 9, 10]
                }
            },

            {
                "Singapore", new PhoneInfo
                {
                    CountryCodes = ["+65"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Thailand", new PhoneInfo
                {
                    CountryCodes = ["+66"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "East Timor", new PhoneInfo
                {
                    CountryCodes = ["+670"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Australian Antarctic Territory", new PhoneInfo
                {
                    CountryCodes = ["+672"],
                    NationalNumberLengths = [6]
                }
            },

            {
                "Norfolk Island", new PhoneInfo
                {
                    CountryCodes = ["+672"],
                    NationalNumberLengths = [6]
                }
            },

            {
                "Brunei", new PhoneInfo
                {
                    CountryCodes = ["+673"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Nauru", new PhoneInfo
                {
                    CountryCodes = ["+674"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Papua New Guinea", new PhoneInfo
                {
                    CountryCodes = ["+675"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Tonga", new PhoneInfo
                {
                    CountryCodes = ["+676"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Solomon Islands", new PhoneInfo
                {
                    CountryCodes = ["+677"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Vanuatu", new PhoneInfo
                {
                    CountryCodes = ["+678"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Fiji", new PhoneInfo
                {
                    CountryCodes = ["+679"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Palau", new PhoneInfo
                {
                    CountryCodes = ["+680"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Wallis and Futuna", new PhoneInfo
                {
                    CountryCodes = ["+681"],
                    NationalNumberLengths = [6]
                }
            },

            {
                "Cook Islands", new PhoneInfo
                {
                    CountryCodes = ["+682"],
                    NationalNumberLengths = [5]
                }
            },

            {
                "Niue", new PhoneInfo
                {
                    CountryCodes = ["+683"],
                    NationalNumberLengths = [4]
                }
            },

            {
                "Samoa", new PhoneInfo
                {
                    CountryCodes = ["+685"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Kiribati", new PhoneInfo
                {
                    CountryCodes = ["+686"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "New Caledonia", new PhoneInfo
                {
                    CountryCodes = ["+687"],
                    NationalNumberLengths = [6]
                }
            },

            {
                "Tuvalu", new PhoneInfo
                {
                    CountryCodes = ["+688"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "French Polynesia", new PhoneInfo
                {
                    CountryCodes = ["+689"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Tokelau", new PhoneInfo
                {
                    CountryCodes = ["+690"],
                    NationalNumberLengths = [4]
                }
            },

            {
                "Federated States of Micronesia", new PhoneInfo
                {
                    CountryCodes = ["+691"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Marshall Islands", new PhoneInfo
                {
                    CountryCodes = ["+692"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Japan", new PhoneInfo
                {
                    CountryCodes = ["+81"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "South Korea", new PhoneInfo
                {
                    CountryCodes = ["+82"],
                    NationalNumberLengths = [9, 10]
                }
            },

            {
                "Vietnam", new PhoneInfo
                {
                    CountryCodes = ["+84"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "North Korea", new PhoneInfo
                {
                    CountryCodes = ["+850"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Hong Kong", new PhoneInfo
                {
                    CountryCodes = ["+852"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Macau", new PhoneInfo
                {
                    CountryCodes = ["+853"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Cambodia", new PhoneInfo
                {
                    CountryCodes = ["+855"],
                    NationalNumberLengths = [8, 9]
                }
            },

            {
                "Laos", new PhoneInfo
                {
                    CountryCodes = ["+856"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "China", new PhoneInfo
                {
                    CountryCodes = ["+86"],
                    NationalNumberLengths = [11]
                }
            },

            {
                "Bangladesh", new PhoneInfo
                {
                    CountryCodes = ["+880"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Taiwan", new PhoneInfo
                {
                    CountryCodes = ["+886"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Kazakhstan", new PhoneInfo
                {
                    CountryCodes = ["+7"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Russia", new PhoneInfo
                {
                    CountryCodes = ["+7"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Abkhazia", new PhoneInfo
                {
                    CountryCodes = ["+7"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "South Ossetia", new PhoneInfo
                {
                    CountryCodes = ["+7"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Turkey", new PhoneInfo
                {
                    CountryCodes = ["+90"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Northern Cyprus", new PhoneInfo
                {
                    CountryCodes = ["+90"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "India", new PhoneInfo
                {
                    CountryCodes = ["+91"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Pakistan", new PhoneInfo
                {
                    CountryCodes = ["+92"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Afghanistan", new PhoneInfo
                {
                    CountryCodes = ["+93"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Sri Lanka", new PhoneInfo
                {
                    CountryCodes = ["+94"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Myanmar", new PhoneInfo
                {
                    CountryCodes = ["+95"],
                    NationalNumberLengths = [7, 8, 9, 10]
                }
            },

            {
                "Maldives", new PhoneInfo
                {
                    CountryCodes = ["+960"],
                    NationalNumberLengths = [7]
                }
            },

            {
                "Lebanon", new PhoneInfo
                {
                    CountryCodes = ["+961"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Jordan", new PhoneInfo
                {
                    CountryCodes = ["+962"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Syria", new PhoneInfo
                {
                    CountryCodes = ["+963"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Iraq", new PhoneInfo
                {
                    CountryCodes = ["+964"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Kuwait", new PhoneInfo
                {
                    CountryCodes = ["+965"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Saudi Arabia", new PhoneInfo
                {
                    CountryCodes = ["+966"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Yemen", new PhoneInfo
                {
                    CountryCodes = ["+967"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Oman", new PhoneInfo
                {
                    CountryCodes = ["+968"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Palestine", new PhoneInfo
                {
                    CountryCodes = ["+970"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "United Arab Emirates", new PhoneInfo
                {
                    CountryCodes = ["+971"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Israel", new PhoneInfo
                {
                    CountryCodes = ["+972"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Bahrain", new PhoneInfo
                {
                    CountryCodes = ["+973"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Qatar", new PhoneInfo
                {
                    CountryCodes = ["+974"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Bhutan", new PhoneInfo
                {
                    CountryCodes = ["+975"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Mongolia", new PhoneInfo
                {
                    CountryCodes = ["+976"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Nepal", new PhoneInfo
                {
                    CountryCodes = ["+977"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Iran", new PhoneInfo
                {
                    CountryCodes = ["+98"],
                    NationalNumberLengths = [10]
                }
            },

            {
                "Tajikistan", new PhoneInfo
                {
                    CountryCodes = ["+992"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Turkmenistan", new PhoneInfo
                {
                    CountryCodes = ["+993"],
                    NationalNumberLengths = [8]
                }
            },

            {
                "Azerbaijan", new PhoneInfo
                {
                    CountryCodes = ["+994"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Georgia", new PhoneInfo
                {
                    CountryCodes = ["+995"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Abkhazia (Georgia plan)", new PhoneInfo
                {
                    CountryCodes = ["+995"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Kyrgyzstan", new PhoneInfo
                {
                    CountryCodes = ["+996"],
                    NationalNumberLengths = [9]
                }
            },

            {
                "Uzbekistan", new PhoneInfo
                {
                    CountryCodes = ["+998"],
                    NationalNumberLengths = [9]
                }
            }
        };

        /// <summary>
        /// Перевіряє поєднання міжнародного коду країни
        /// та довжини національного номера.
        ///
        /// Спочатку знаходить усі записи з переданим кодом,
        /// потім об’єднує їхні допустимі довжини та визначає
        /// конкретний підсумок перевірки.
        ///
        /// Успіх означає відповідність коду й довжини даним каталогу.
        ///
        /// (Checks the combination of a country calling code
        /// and national number length.
        ///
        /// First finds all entries with the supplied calling code,
        /// then combines their allowed lengths and determines
        /// the specific check outcome.
        ///
        /// Success indicates that the calling code and length
        /// match the catalog data.)
        /// </summary>
        /// <param name="countryCode">
        /// Міжнародний код країни в канонічному форматі.
        ///
        /// (The country calling code in canonical format.)
        /// </param>
        /// <param name="lengthNumber">
        /// Кількість цифр національної частини без міжнародного коду країни.
        ///
        /// (The number of digits in the national part,
        /// excluding the country calling code.)
        /// </param>
        /// <returns>
        /// Результат перевірки з ознакою успішності
        /// та конкретним статусом.
        ///
        /// (The check result containing a success indicator
        /// and a specific status.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Для переданого коду знайдено записи,
        /// але їхній об’єднаний набір допустимих довжин порожній.
        ///
        /// (Entries exist for the supplied calling code,
        /// but their combined set of allowed lengths is empty.)
        /// </exception>
        internal static PhoneNumberingPlanCheckResult CheckNumberingPlan(
            string countryCode,
            int lengthNumber)
        {
            var matchingPhones = Catalog.Values
                .Where(info => info.CountryCodes.Contains(countryCode))
                .ToList();

            if (!matchingPhones.Any())
            {
                return new PhoneNumberingPlanCheckResult(PhoneNumberingPlanCheckStatus.UnknownCountryCode);
            }

            var validLengths = matchingPhones
                .SelectMany(info => info.NationalNumberLengths)
                .ToHashSet();

            if (validLengths.Count == 0)
            {
                throw DomainInvariantViolationException.BrokenState<Phone>(
                    "The phone numbering plan catalog contains entries for the specified country calling code, " +
                    "but their combined set of allowed national number lengths is empty.",
                    new Dictionary<string, object?>
                    {
                        ["CatalogName"] = nameof(PhoneNumberingPlanCatalog),
                        ["CountryCode"] = countryCode,
                        ["MatchingEntriesCount"] = matchingPhones.Count,
                        ["FieldName"] = nameof(PhoneInfo.NationalNumberLengths),
                        ["ActualCount"] = validLengths.Count
                    },
                    OperationType.Compare
                );
            }

            int minLength = validLengths.Min();
            int maxLength = validLengths.Max();

            if (validLengths.Contains(lengthNumber))
            {
                return new PhoneNumberingPlanCheckResult(PhoneNumberingPlanCheckStatus.Valid);
            }

            if (lengthNumber < minLength)
            {
                return new PhoneNumberingPlanCheckResult(PhoneNumberingPlanCheckStatus.NationalNumberTooShort);
            }

            if (lengthNumber > maxLength)
            {
                return new PhoneNumberingPlanCheckResult(PhoneNumberingPlanCheckStatus.NationalNumberTooLong);
            }

            return new PhoneNumberingPlanCheckResult(PhoneNumberingPlanCheckStatus.UnsupportedNationalNumberLength);
        }
    }
}
