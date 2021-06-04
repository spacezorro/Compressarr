﻿using Compressarr.Application;
using Compressarr.FFmpeg;
using Compressarr.FFmpeg.Models;
using Compressarr.Filtering.Models;
using Compressarr.Helpers;
using Compressarr.JobProcessing;
using Compressarr.JobProcessing.Models;
using Compressarr.Presets.Models;
using Compressarr.Services.Base;
using Compressarr.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Compressarr.Presets
{
    public class PresetManager : IPresetManager
    {

        private readonly IApplicationService applicationService;
        private readonly IFileService fileService;
        private readonly ILogger<PresetManager> logger;
        private readonly IMediaInfoService mediaInfoService;
        private readonly IProcessManager processManager;
        public PresetManager(IApplicationService applicationService, IFileService fileService, ILogger<PresetManager> logger, IMediaInfoService mediaInfoService, IProcessManager processManager)
        {
            this.applicationService = applicationService;
            this.fileService = fileService;
            this.logger = logger;
            this.mediaInfoService = mediaInfoService;
            this.processManager = processManager;
        }

        public List<string> AudioBitrates
        {
            get
            {
                return new()
                {
                    "8k",
                    "16k",
                    "24k",
                    "32k",
                    "40k",
                    "48k",
                    "64k",
                    "80k",
                    "96k",
                    "112k",
                    "128k",
                    "160k",
                    "192k",
                    "224k",
                    "256k",
                    "320k"
                };
            }
        }

        public SortedSet<Codec> AudioCodecs => Codecs[CodecType.Audio];
        public SortedSet<Encoder> AudioEncoders => Encoders[CodecType.Audio];
        private Dictionary<CodecType, SortedSet<Codec>> Codecs => applicationService.Codecs;
        public SortedSet<ContainerResponse> Containers => applicationService.Containers;
        private Dictionary<CodecType, SortedSet<Encoder>> Encoders => applicationService.Encoders;
        public Dictionary<string, string> LanguageCodes
        {
            get
            {
                return new()
                {
                    { "aar", "Afar" },
                    { "abk", "Abkhazian" },
                    { "ace", "Achinese" },
                    { "ach", "Acoli" },
                    { "ada", "Adangme" },
                    { "ady", "Adyghe; Adygei" },
                    { "afa", "Afro-Asiatic languages" },
                    { "afh", "Afrihili" },
                    { "afr", "Afrikaans" },
                    { "ain", "Ainu" },
                    { "aka", "Akan" },
                    { "akk", "Akkadian" },
                    { "alb", "Albanian (alb)" },
                    { "ale", "Aleut" },
                    { "alg", "Algonquian languages" },
                    { "alt", "Southern Altai" },
                    { "amh", "Amharic" },
                    { "ang", "English, Old (ca.450-1100)" },
                    { "anp", "Angika" },
                    { "apa", "Apache languages" },
                    { "ara", "Arabic" },
                    { "arc", "Official Aramaic (700-300 BCE); Imperial Aramaic (700-300 BCE)" },
                    { "arg", "Aragonese" },
                    { "arm", "Armenian (arm)" },
                    { "arn", "Mapudungun; Mapuche" },
                    { "arp", "Arapaho" },
                    { "art", "Artificial languages" },
                    { "arw", "Arawak" },
                    { "asm", "Assamese" },
                    { "ast", "Asturian; Bable; Leonese; Asturleonese" },
                    { "ath", "Athapascan languages" },
                    { "aus", "Australian languages" },
                    { "ava", "Avaric" },
                    { "ave", "Avestan" },
                    { "awa", "Awadhi" },
                    { "aym", "Aymara" },
                    { "aze", "Azerbaijani" },
                    { "bad", "Banda languages" },
                    { "bai", "Bamileke languages" },
                    { "bak", "Bashkir" },
                    { "bal", "Baluchi" },
                    { "bam", "Bambara" },
                    { "ban", "Balinese" },
                    { "baq", "Basque (baq)" },
                    { "bas", "Basa" },
                    { "bat", "Baltic languages" },
                    { "bej", "Beja; Bedawiyet" },
                    { "bel", "Belarusian" },
                    { "bem", "Bemba" },
                    { "ben", "Bengali" },
                    { "ber", "Berber languages" },
                    { "bho", "Bhojpuri" },
                    { "bih", "Bihari languages" },
                    { "bik", "Bikol" },
                    { "bin", "Bini; Edo" },
                    { "bis", "Bislama" },
                    { "bla", "Siksika" },
                    { "bnt", "Bantu languages" },
                    { "bod", "Tibetan (bod)" },
                    { "bos", "Bosnian" },
                    { "bra", "Braj" },
                    { "bre", "Breton" },
                    { "btk", "Batak languages" },
                    { "bua", "Buriat" },
                    { "bug", "Buginese" },
                    { "bul", "Bulgarian" },
                    { "bur", "Burmese (bur)" },
                    { "byn", "Blin; Bilin" },
                    { "cad", "Caddo" },
                    { "cai", "Central American Indian languages" },
                    { "car", "Galibi Carib" },
                    { "cat", "Catalan; Valencian" },
                    { "cau", "Caucasian languages" },
                    { "ceb", "Cebuano" },
                    { "cel", "Celtic languages" },
                    { "ces", "Czech (ces)" },
                    { "cha", "Chamorro" },
                    { "chb", "Chibcha" },
                    { "che", "Chechen" },
                    { "chg", "Chagatai" },
                    { "chi", "Chinese (chi)" },
                    { "chk", "Chuukese" },
                    { "chm", "Mari" },
                    { "chn", "Chinook jargon" },
                    { "cho", "Choctaw" },
                    { "chp", "Chipewyan; Dene Suline" },
                    { "chr", "Cherokee" },
                    { "chu", "Church Slavic; Old Slavonic; Church Slavonic; Old Bulgarian; Old Church Slavonic" },
                    { "chv", "Chuvash" },
                    { "chy", "Cheyenne" },
                    { "cmc", "Chamic languages" },
                    { "cnr", "Montenegrin" },
                    { "cop", "Coptic" },
                    { "cor", "Cornish" },
                    { "cos", "Corsican" },
                    { "cpe", "Creoles and pidgins, English based" },
                    { "cpf", "Creoles and pidgins, French-based" },
                    { "cpp", "Creoles and pidgins, Portuguese-based" },
                    { "cre", "Cree" },
                    { "crh", "Crimean Tatar; Crimean Turkish" },
                    { "crp", "Creoles and pidgins" },
                    { "csb", "Kashubian" },
                    { "cus", "Cushitic languages" },
                    { "cym", "Welsh (cym)" },
                    { "cze", "Czech (cze)" },
                    { "dak", "Dakota" },
                    { "dan", "Danish" },
                    { "dar", "Dargwa" },
                    { "day", "Land Dayak languages" },
                    { "del", "Delaware" },
                    { "den", "Slave (Athapascan)" },
                    { "deu", "German (deu)" },
                    { "dgr", "Dogrib" },
                    { "din", "Dinka" },
                    { "div", "Divehi; Dhivehi; Maldivian" },
                    { "doi", "Dogri" },
                    { "dra", "Dravidian languages" },
                    { "dsb", "Lower Sorbian" },
                    { "dua", "Duala" },
                    { "dum", "Dutch, Middle (ca.1050-1350)" },
                    { "dut", "Dutch; Flemish (dut)" },
                    { "dyu", "Dyula" },
                    { "dzo", "Dzongkha" },
                    { "efi", "Efik" },
                    { "egy", "Egyptian (Ancient)" },
                    { "eka", "Ekajuk" },
                    { "ell", "Greek, Modern (1453-) (ell)" },
                    { "elx", "Elamite" },
                    { "eng", "English" },
                    { "enm", "English, Middle (1100-1500)" },
                    { "epo", "Esperanto" },
                    { "est", "Estonian" },
                    { "eus", "Basque (eus)" },
                    { "ewe", "Ewe" },
                    { "ewo", "Ewondo" },
                    { "fan", "Fang" },
                    { "fao", "Faroese" },
                    { "fas", "Persian (fas)" },
                    { "fat", "Fanti" },
                    { "fij", "Fijian" },
                    { "fil", "Filipino; Pilipino" },
                    { "fin", "Finnish" },
                    { "fiu", "Finno-Ugrian languages" },
                    { "fon", "Fon" },
                    { "fra", "French (fra)" },
                    { "fre", "French (fre)" },
                    { "frm", "French, Middle(ca.1400 - 1600)" },
                    { "fro", "French, Old (842-ca.1400)" },
                    { "frr", "Northern Frisian" },
                    { "frs", "Eastern Frisian" },
                    { "fry", "Western Frisian" },
                    { "ful", "Fulah" },
                    { "fur", "Friulian" },
                    { "gaa", "Ga" },
                    { "gay", "Gayo" },
                    { "gba", "Gbaya" },
                    { "gem", "Germanic languages" },
                    { "geo", "Georgian (geo)" },
                    { "ger", "German (ger)" },
                    { "gez", "Geez" },
                    { "gil", "Gilbertese" },
                    { "gla", "Gaelic; Scottish Gaelic" },
                    { "gle", "Irish" },
                    { "glg", "Galician" },
                    { "glv", "Manx" },
                    { "gmh", "German, Middle High (ca.1050-1500)" },
                    { "goh", "German, Old High (ca.750-1050)" },
                    { "gon", "Gondi" },
                    { "gor", "Gorontalo" },
                    { "got", "Gothic" },
                    { "grb", "Grebo" },
                    { "grc", "Greek, Ancient (to 1453)" },
                    { "gre", "Greek, Modern (1453-) (gre)" },
                    { "grn", "Guarani" },
                    { "gsw", "Swiss German; Alemannic; Alsatian" },
                    { "guj", "Gujarati" },
                    { "gwi", "Gwich'in" },
                    { "hai", "Haida" },
                    { "hat", "Haitian; Haitian Creole" },
                    { "hau", "Hausa" },
                    { "haw", "Hawaiian" },
                    { "heb", "Hebrew" },
                    { "her", "Herero" },
                    { "hil", "Hiligaynon" },
                    { "him", "Himachali languages; Western Pahari languages" },
                    { "hin", "Hindi" },
                    { "hit", "Hittite" },
                    { "hmn", "Hmong; Mong" },
                    { "hmo", "Hiri Motu" },
                    { "hrv", "Croatian" },
                    { "hsb", "Upper Sorbian" },
                    { "hun", "Hungarian" },
                    { "hup", "Hupa" },
                    { "hye", "Armenian (hye)" },
                    { "iba", "Iban" },
                    { "ibo", "Igbo" },
                    { "ice", "Icelandic (ice)" },
                    { "ido", "Ido" },
                    { "iii", "Sichuan Yi; Nuosu" },
                    { "ijo", "Ijo languages" },
                    { "iku", "Inuktitut" },
                    { "ile", "Interlingue; Occidental" },
                    { "ilo", "Iloko" },
                    { "ina", "Interlingua (International Auxiliary Language Association)" },
                    { "inc", "Indic languages" },
                    { "ind", "Indonesian" },
                    { "ine", "Indo-European languages" },
                    { "inh", "Ingush" },
                    { "ipk", "Inupiaq" },
                    { "ira", "Iranian languages" },
                    { "iro", "Iroquoian languages" },
                    { "isl", "Icelandic (isl)" },
                    { "ita", "Italian" },
                    { "jav", "Javanese" },
                    { "jbo", "Lojban" },
                    { "jpn", "Japanese" },
                    { "jpr", "Judeo-Persian" },
                    { "jrb", "Judeo-Arabic" },
                    { "kaa", "Kara-Kalpak" },
                    { "kab", "Kabyle" },
                    { "kac", "Kachin; Jingpho" },
                    { "kal", "Kalaallisut; Greenlandic" },
                    { "kam", "Kamba" },
                    { "kan", "Kannada" },
                    { "kar", "Karen languages" },
                    { "kas", "Kashmiri" },
                    { "kat", "Georgian (kat)" },
                    { "kau", "Kanuri" },
                    { "kaw", "Kawi" },
                    { "kaz", "Kazakh" },
                    { "kbd", "Kabardian" },
                    { "kha", "Khasi" },
                    { "khi", "Khoisan languages" },
                    { "khm", "Central Khmer" },
                    { "kho", "Khotanese; Sakan" },
                    { "kik", "Kikuyu; Gikuyu" },
                    { "kin", "Kinyarwanda" },
                    { "kir", "Kirghiz; Kyrgyz" },
                    { "kmb", "Kimbundu" },
                    { "kok", "Konkani" },
                    { "kom", "Komi" },
                    { "kon", "Kongo" },
                    { "kor", "Korean" },
                    { "kos", "Kosraean" },
                    { "kpe", "Kpelle" },
                    { "krc", "Karachay-Balkar" },
                    { "krl", "Karelian" },
                    { "kro", "Kru languages" },
                    { "kru", "Kurukh" },
                    { "kua", "Kuanyama; Kwanyama" },
                    { "kum", "Kumyk" },
                    { "kur", "Kurdish" },
                    { "kut", "Kutenai" },
                    { "lad", "Ladino" },
                    { "lah", "Lahnda" },
                    { "lam", "Lamba" },
                    { "lao", "Lao" },
                    { "lat", "Latin" },
                    { "lav", "Latvian" },
                    { "lez", "Lezghian" },
                    { "lim", "Limburgan; Limburger; Limburgish" },
                    { "lin", "Lingala" },
                    { "lit", "Lithuanian" },
                    { "lol", "Mongo" },
                    { "loz", "Lozi" },
                    { "ltz", "Luxembourgish; Letzeburgesch" },
                    { "lua", "Luba-Lulua" },
                    { "lub", "Luba-Katanga" },
                    { "lug", "Ganda" },
                    { "lui", "Luiseno" },
                    { "lun", "Lunda" },
                    { "luo", "Luo (Kenya and Tanzania)" },
                    { "lus", "Lushai" },
                    { "mac", "Macedonian (mac)" },
                    { "mad", "Madurese" },
                    { "mag", "Magahi" },
                    { "mah", "Marshallese" },
                    { "mai", "Maithili" },
                    { "mak", "Makasar" },
                    { "mal", "Malayalam" },
                    { "man", "Mandingo" },
                    { "mao", "Maori (mao)" },
                    { "map", "Austronesian languages" },
                    { "mar", "Marathi" },
                    { "mas", "Masai" },
                    { "may", "Malay (may)" },
                    { "mdf", "Moksha" },
                    { "mdr", "Mandar" },
                    { "men", "Mende" },
                    { "mga", "Irish, Middle (900-1200)" },
                    { "mic", "Mi'kmaq; Micmac" },
                    { "min", "Minangkabau" },
                    { "mis", "Uncoded languages" },
                    { "mkd", "Macedonian (mkd)" },
                    { "mkh", "Mon - Khmer languages" },
                    { "mlg", "Malagasy" },
                    { "mlt", "Maltese" },
                    { "mnc", "Manchu" },
                    { "mni", "Manipuri" },
                    { "mno", "Manobo languages" },
                    { "moh", "Mohawk" },
                    { "mon", "Mongolian" },
                    { "mos", "Mossi" },
                    { "mri", "Maori (mri)" },
                    { "msa", "Malay (msa)" },
                    { "mul", "Multiple languages" },
                    { "mun", "Munda languages" },
                    { "mus", "Creek" },
                    { "mwl", "Mirandese" },
                    { "mwr", "Marwari" },
                    { "mya", "Burmese (mya)" },
                    { "myn", "Mayan languages" },
                    { "myv", "Erzya" },
                    { "nah", "Nahuatl languages" },
                    { "nai", "North American Indian languages" },
                    { "nap", "Neapolitan" },
                    { "nau", "Nauru" },
                    { "nav", "Navajo; Navaho" },
                    { "nbl", "Ndebele, South; South Ndebele" },
                    { "nde", "Ndebele, North; North Ndebele" },
                    { "ndo", "Ndonga" },
                    { "nds", "Low German; Low Saxon; German, Low; Saxon, Low" },
                    { "nep", "Nepali" },
                    { "new", "Nepal Bhasa; Newari" },
                    { "nia", "Nias" },
                    { "nic", "Niger-Kordofanian languages" },
                    { "niu", "Niuean" },
                    { "nld", "Dutch; Flemish (nld)" },
                    { "nno", "Norwegian Nynorsk; Nynorsk, Norwegian" },
                    { "nob", "Bokmål, Norwegian; Norwegian Bokmål" },
                    { "nog", "Nogai" },
                    { "non", "Norse, Old" },
                    { "nor", "Norwegian" },
                    { "nqo", "N'Ko" },
                    { "nso", "Pedi; Sepedi; Northern Sotho" },
                    { "nub", "Nubian languages" },
                    { "nwc", "Classical Newari; Old Newari; Classical Nepal Bhasa" },
                    { "nya", "Chichewa; Chewa; Nyanja" },
                    { "nym", "Nyamwezi" },
                    { "nyn", "Nyankole" },
                    { "nyo", "Nyoro" },
                    { "nzi", "Nzima" },
                    { "oci", "Occitan (post 1500)" },
                    { "oji", "Ojibwa" },
                    { "ori", "Oriya" },
                    { "orm", "Oromo" },
                    { "osa", "Osage" },
                    { "oss", "Ossetian; Ossetic" },
                    { "ota", "Turkish, Ottoman (1500-1928)" },
                    { "oto", "Otomian languages" },
                    { "paa", "Papuan languages" },
                    { "pag", "Pangasinan" },
                    { "pal", "Pahlavi" },
                    { "pam", "Pampanga; Kapampangan" },
                    { "pan", "Panjabi; Punjabi" },
                    { "pap", "Papiamento" },
                    { "pau", "Palauan" },
                    { "peo", "Persian, Old (ca.600-400 B.C.)" },
                    { "per", "Persian (per)" },
                    { "phi", "Philippine languages" },
                    { "phn", "Phoenician" },
                    { "pli", "Pali" },
                    { "pol", "Polish" },
                    { "pon", "Pohnpeian" },
                    { "por", "Portuguese" },
                    { "pra", "Prakrit languages" },
                    { "pro", "Provençal, Old (to 1500);Occitan, Old (to 1500)" },
                    { "pus", "Pushto; Pashto" },
                    { "que", "Quechua" },
                    { "raj", "Rajasthani" },
                    { "rap", "Rapanui" },
                    { "rar", "Rarotongan; Cook Islands Maori" },
                    { "roa", "Romance languages" },
                    { "roh", "Romansh" },
                    { "rom", "Romany" },
                    { "ron", "Romanian; Moldavian; Moldovan (ron)_" },
                    { "rum", "Romanian; Moldavian; Moldovan (rum)" },
                    { "run", "Rundi" },
                    { "rup", "Aromanian; Arumanian; Macedo-Romanian" },
                    { "rus", "Russian" },
                    { "sad", "Sandawe" },
                    { "sag", "Sango" },
                    { "sah", "Yakut" },
                    { "sai", "South American Indian languages" },
                    { "sal", "Salishan languages" },
                    { "sam", "Samaritan Aramaic" },
                    { "san", "Sanskrit" },
                    { "sas", "Sasak" },
                    { "sat", "Santali" },
                    { "scn", "Sicilian" },
                    { "sco", "Scots" },
                    { "sel", "Selkup" },
                    { "sem", "Semitic languages" },
                    { "sga", "Irish, Old (to 900)" },
                    { "sgn", "Sign Languages" },
                    { "shn", "Shan" },
                    { "sid", "Sidamo" },
                    { "sin", "Sinhala; Sinhalese" },
                    { "sio", "Siouan languages" },
                    { "sit", "Sino-Tibetan languages" },
                    { "sla", "Slavic languages" },
                    { "slk", "Slovak (slk)" },
                    { "slo", "Slovak (slo)" },
                    { "slv", "Slovenian" },
                    { "sma", "Southern Sami" },
                    { "sme", "Northern Sami" },
                    { "smi", "Sami languages" },
                    { "smj", "Lule Sami" },
                    { "smn", "Inari Sami" },
                    { "smo", "Samoan" },
                    { "sms", "Skolt Sami" },
                    { "sna", "Shona" },
                    { "snd", "Sindhi" },
                    { "snk", "Soninke" },
                    { "sog", "Sogdian" },
                    { "som", "Somali" },
                    { "son", "Songhai languages" },
                    { "sot", "Sotho, Southern" },
                    { "spa", "Spanish; Castilian" },
                    { "sqi", "Albanian (sqi)" },
                    { "srd", "Sardinian" },
                    { "srn", "Sranan Tongo" },
                    { "srp", "Serbian" },
                    { "srr", "Serer" },
                    { "ssa", "Nilo-Saharan languages" },
                    { "ssw", "Swati" },
                    { "suk", "Sukuma" },
                    { "sun", "Sundanese" },
                    { "sus", "Susu" },
                    { "sux", "Sumerian" },
                    { "swa", "Swahili" },
                    { "swe", "Swedish" },
                    { "syc", "Classical Syriac" },
                    { "syr", "Syriac" },
                    { "tah", "Tahitian" },
                    { "tai", "Tai languages" },
                    { "tam", "Tamil" },
                    { "tat", "Tatar" },
                    { "tel", "Telugu" },
                    { "tem", "Timne" },
                    { "ter", "Tereno" },
                    { "tet", "Tetum" },
                    { "tgk", "Tajik" },
                    { "tgl", "Tagalog" },
                    { "tha", "Thai" },
                    { "tib", "Tibetan (tib)" },
                    { "tig", "Tigre" },
                    { "tir", "Tigrinya" },
                    { "tiv", "Tiv" },
                    { "tkl", "Tokelau" },
                    { "tlh", "Klingon; tlhIngan-Hol" },
                    { "tli", "Tlingit" },
                    { "tmh", "Tamashek" },
                    { "tog", "Tonga (Nyasa)" },
                    { "ton", "Tonga (Tonga Islands)" },
                    { "tpi", "Tok Pisin" },
                    { "tsi", "Tsimshian" },
                    { "tsn", "Tswana" },
                    { "tso", "Tsonga" },
                    { "tuk", "Turkmen" },
                    { "tum", "Tumbuka" },
                    { "tup", "Tupi languages" },
                    { "tur", "Turkish" },
                    { "tut", "Altaic languages" },
                    { "tvl", "Tuvalu" },
                    { "twi", "Twi" },
                    { "tyv", "Tuvinian" },
                    { "udm", "Udmurt" },
                    { "uga", "Ugaritic" },
                    { "uig", "Uighur; Uyghur" },
                    { "ukr", "Ukrainian" },
                    { "umb", "Umbundu" },
                    { "und", "Undetermined" },
                    { "urd", "Urdu" },
                    { "uzb", "Uzbek" },
                    { "vai", "Vai" },
                    { "ven", "Venda" },
                    { "vie", "Vietnamese" },
                    { "vol", "Volapük" },
                    { "vot", "Votic" },
                    { "wak", "Wakashan languages" },
                    { "wal", "Wolaitta; Wolaytta" },
                    { "war", "Waray" },
                    { "was", "Washo" },
                    { "wel", "Welsh (wel)" },
                    { "wen", "Sorbian languages" },
                    { "wln", "Walloon" },
                    { "wol", "Wolof" },
                    { "xal", "Kalmyk; Oirat" },
                    { "xho", "Xhosa" },
                    { "yao", "Yao" },
                    { "yap", "Yapese" },
                    { "yid", "Yiddish" },
                    { "yor", "Yoruba" },
                    { "ypk", "Yupik languages" },
                    { "zap", "Zapotec" },
                    { "zbl", "Blissymbols; Blissymbolics; Bliss" },
                    { "zen", "Zenaga" },
                    { "zgh", "Standard Moroccan Tamazight" },
                    { "zha", "Zhuang; Chuang" },
                    { "zho", "Chinese (zho)" },
                    { "znd", "Zande languages" },
                    { "zul", "Zulu" },
                    { "zun", "Zuni" },
                    { "zxx", "No linguistic content; Not applicable" },
                    { "zza", "Zaza; Dimili; Dimli; Kirdki; Kirmanjki; Zazaki" }
                };
            }
        }

        public List<FilterComparitor> NumberComparitors
        {
            get
            {
                return new()
                {
                    new FilterComparitor("=="),
                    new FilterComparitor("!="),
                    new FilterComparitor("<"),
                    new FilterComparitor(">"),
                    new FilterComparitor("<="),
                    new FilterComparitor(">=")
                };
            }
        }

        public HashSet<FFmpegPreset> Presets => applicationService.Presets;

        public SortedSet<Codec> SubtitleCodecs => Codecs[CodecType.Subtitle];

        public SortedSet<Encoder> SubtitleEncoders => Encoders[CodecType.Subtitle];

        public SortedSet<Codec> VideoCodecs => Codecs[CodecType.Video];

        public SortedSet<Encoder> VideoEncoders => Encoders[CodecType.Video];

        public async Task AddPresetAsync(FFmpegPreset newPreset)
        {
            using (logger.BeginScope("Adding Preset"))
            {
                logger.LogInformation($"Preset Name: {newPreset.Name}");

                if (Presets.Contains(newPreset))
                {
                    logger.LogDebug("Preset already exists, updating.");
                }
                else
                {
                    logger.LogDebug("Adding a new preset.");
                    newPreset.Initialised = true;
                    applicationService.Presets.Add(newPreset);
                }

                await applicationService.SaveAppSetting();
            }
        }

        public async Task DeletePresetAsync(FFmpegPreset preset)
        {

            using (logger.BeginScope("Deleting Preset: {preset}", preset))
            {
                if (Presets.Contains(preset))
                {
                    logger.LogInformation($"Removing");
                    Presets.Remove(preset);
                    applicationService.Presets.Remove(preset);
                }
                else
                {
                    logger.LogWarning($"Preset {preset.Name} not found.");
                }

                await applicationService.SaveAppSetting();
            }
        }

        public async Task<GetArgumentsResult> GetArguments(FFmpegPreset preset, WorkItem wi, CancellationToken token)
        {

            using (logger.BeginScope("Get Arguments"))
            {
                await applicationService.InitialisePresets;

                var mediaInfo = await mediaInfoService.GetMediaInfo(wi.Source, wi.SourceFile);
                var filePath = wi.SourceFile;

                List<string> args = new();

                var frameRate = preset.FrameRate.HasValue ? $" -r {preset.FrameRate}" : "";
                var opArgsStr = string.IsNullOrWhiteSpace(preset.OptionalArguments) ? "" : $" {preset.OptionalArguments.Trim()}";
                var passStr = " -pass %passnum%";

                var audioArguments = string.Empty;

                var hardwareDecoder = preset.HardwareDecoder.Wrap("-hwaccel {0} ");

                var mapAllElse = " -map 0:s? -c:s copy -map 0:t? -map 0:d? -movflags use_metadata_tags";

                logger.LogInformation("Calculating Audio Arguments");

                {
                    var i = 0; //for stream output tracking

                    if (mediaInfo?.streams != null)
                    {
                        foreach (var stream in mediaInfo.AudioStreams)
                        {
                            foreach (var audioPreset in preset.AudioStreamPresets)
                            {
                                var match = audioPreset.Filters.All(f =>
                                    f.Rule switch
                                    {
                                        AudioStreamRule.Any => true,
                                        AudioStreamRule.Codec => f.Matches == f.Values.Contains(stream.codec_name.ToLower()),
                                        AudioStreamRule.Channels => new List<int>() { stream.channels ?? 0 }.AsQueryable().Where($"it{f.NumberComparitor.Operator}{f.ChannelValue}").Any(),
                                        AudioStreamRule.Language => stream.tags?.language == null || f.Matches == f.Values.Contains(stream.tags?.language.ToLower()),
                                        _ => throw new NotImplementedException()
                                    }
                                );

                                if (match)
                                {
                                    var audioStreamMap = $" -map 0:{stream.index} -c:a:{i++}";
                                    audioArguments += audioPreset.Action switch
                                    {
                                        AudioStreamAction.Copy => $"{audioStreamMap} copy",
                                        AudioStreamAction.Delete => "",
                                        AudioStreamAction.DeleteUnlessOnly => preset.AudioStreamPresets.Last() == audioPreset && i == 0 ? $"{audioStreamMap} copy" : "",
                                        AudioStreamAction.Clone => $"{audioStreamMap} copy  -map 0:{stream.index} -c:a:{i++} {audioPreset.Encoder.Name}{(string.IsNullOrWhiteSpace(audioPreset.BitRate) ? "" : $" -b:a:{i} ")}{audioPreset.BitRate}",
                                        AudioStreamAction.Encode => $"{audioStreamMap} {audioPreset.Encoder.Name}{(string.IsNullOrWhiteSpace(audioPreset.BitRate) ? "" : $" -b:a:{i} ")}{audioPreset.BitRate}",
                                        _ => throw new System.NotImplementedException()
                                    };
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new EndOfStreamException("No streams found in this media");
                    }
                }

                logger.LogDebug($"Audio Arguments: {audioArguments}");

                logger.LogInformation("Calculating Video Arguments");
                var videoArguments = string.Empty;

                if (preset.VideoEncoder.IsCopy)
                {
                    videoArguments = " -map 0:v -c:v copy";
                }
                else
                {
                    if (mediaInfo?.streams != null)
                    {
                        var i = 0; //for stream output tracking

                        foreach (var vstream in mediaInfo.VideoStreams)
                        {
                            var videoStreamMap = $" -map 0:{vstream.index} -c:v:{i++}";

                            if (vstream.disposition.attached_pic)
                            {
                                videoArguments += $"{videoStreamMap} copy";
                            }
                            else
                            {
                                if (preset.VideoBitRate.HasValue)
                                {
                                    if (preset.VideoCodecOptions != null)
                                    {
                                        if (preset.VideoCodecOptions.Any(vco => vco.EncoderOption.IncludePass))
                                        {
                                            passStr = string.Empty;
                                        }
                                    }
                                    videoArguments += $"{videoStreamMap} {preset.VideoEncoder.Name}{await GetVideoCodecParams(preset, wi, token)} -b:v {preset.VideoBitRate}k{frameRate}{passStr}";
                                }
                                else
                                {
                                    videoArguments += $"{videoStreamMap} {preset.VideoEncoder.Name}{await GetVideoCodecParams(preset, wi, token)}{frameRate}";
                                }
                            }

                            if (token.IsCancellationRequested) return new(false);
                        }
                    }
                }

                logger.LogDebug($"Video Arguments: {videoArguments}");

                if (preset.VideoBitRate.HasValue)
                {


                    var part1Ending = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "NUL" : @"/dev/null";

                    args.Add($"{hardwareDecoder}-y -i \"{{0}}\" {videoArguments} -an -f null {part1Ending}".Replace("%passnum%", "1"));
                    args.Add($"{hardwareDecoder}-y -i \"{{0}}\" {videoArguments}{opArgsStr}{audioArguments}{mapAllElse} \"{{1}}\"".Replace("%passnum%", "2"));
                }
                else
                {
                    args.Add($"{hardwareDecoder}-y -i \"{{0}}\" {videoArguments}{opArgsStr}{audioArguments}{mapAllElse} \"{{1}}\"");
                }

                if (token.IsCancellationRequested) return new(false);

                return new(args);
            }
        }

        public FFmpegPreset GetPreset(string presetName) => Presets.FirstOrDefault(p => p.Name == presetName);

        public Task<StatusResult> GetStatus()
        {
            return Task.Run(() =>
            {
                return new StatusResult()
                {
                    Status = Presets.Any() ? ServiceStatus.Ready : ServiceStatus.Incomplete,
                    Message = new(Presets.Any() ? "Ready" : "No presets have been defined, you can create some on the <a href=\"/ffmpeg\">FFmpeg</a> page")
                };
            });
        }

        private async Task<string> CalculateBestOptions(FFmpegPreset preset, WorkItem wi, CancellationToken token)
        {
            using (logger.BeginScope("Calculating Best Encoder Options"))
            {
                
                var tempFile = Path.Combine(fileService.TempDir, $"directCopy.{preset.ContainerExtension}");
                var tempEncFile = Path.Combine(fileService.TempDir, $"encFile.{preset.ContainerExtension}");
                var sampleListFile = Path.Combine(fileService.TempDir, $"sampleList.txt");

                if (File.Exists(sampleListFile)) File.Delete(sampleListFile);

                var sampleTime = applicationService.ArgCalcSampleLength.Value.TotalSeconds;
                var videoLength = wi.Duration?.TotalSeconds ?? 3600;

                var samplePitch = videoLength / 4;

                for (int i = 1; i < 4; i++)
                {
                    var sampleFileName = Path.Combine(fileService.TempDir, $"sample{i}.{preset.ContainerExtension}");
                    var temparg = $"-y -ss {samplePitch * i} -t {sampleTime} -i \"{wi.SourceFile}\" -codec copy -an \"{sampleFileName}\"";

                    var sampleEncodeResult = await processManager.EncodeAVideo(null, null, temparg, token);
                    if (!sampleEncodeResult.Success)
                    {
                        throw sampleEncodeResult.Exception ?? new("Not sure - something went wrong with encoding. Check the logs");
                    }
                    File.AppendAllLines(sampleListFile, new List<string>() { $"file '{sampleFileName}'" });
                }

                var concatArg = $"-y -f concat -safe 0 -i {sampleListFile} -c copy {tempFile}";

                var encodeResult = await processManager.EncodeAVideo(null, null, concatArg, token);
                if (!encodeResult.Success)
                {
                    throw encodeResult.Exception ?? new("Not sure - something went wrong with encoding. Check the logs");
                }


                var origSize = new FileInfo(tempFile).Length;

                var hardwareDecoder = preset.HardwareDecoder.Wrap("-hwaccel {0} ");
                var frameRate = preset.FrameRate.HasValue ? $" -r {preset.FrameRate}" : "";

                var result = string.Empty;

                wi.ArgCalcResults = new List<AutoPresetTest>();
                var testSuite = wi.ArgCalcResults;

                wi.Output("Calculating Best Encoder Options");
                if (token.IsCancellationRequested) return string.Empty;

                foreach (var option in preset.VideoCodecOptions.Where(o => o.AutoCalculate).Select(x => x.EncoderOption))
                {
                    if (option == null) throw new InvalidDataException("Options no longer available, please rebuild the FFmpeg Preset");

                    var range = new List<string>();
                    switch (option.Type)
                    {
                        case CodecOptionType.Range:
                        case CodecOptionType.Number:
                            {
                                range = Enumerable.Range(Math.Min(option.AutoTune.Start, option.AutoTune.End), Math.Abs(option.AutoTune.End - option.AutoTune.Start) + 1).OrderBy(x => Math.Abs(x - option.AutoTune.Start)).Select(i => i.ToString()).ToList();
                            }
                            break;
                        case CodecOptionType.Select:
                        case CodecOptionType.String:
                            {
                                range = option.AutoTune.Values;
                            }
                            break;
                    }

                    var SSIMTest = new AutoPresetTest(option.Arg);

                    foreach (var i in range)
                    {
                        var key = i;
                        SSIMTest.AutoPresetResultSet.Add(i.Trim(), null);
                    }

                    wi.ArgCalcResults.Add(SSIMTest);
                }

                wi.Update();
                if (token.IsCancellationRequested) return string.Empty;

                foreach (var test in wi.ArgCalcResults)
                {

                    test.Argument = $" {result} {test.Argument}";

                    foreach (var i in test.AutoPresetResultSet)
                    {
                        test.AutoPresetResultSet[i.Key] = new();
                        test.AutoPresetResultSet[i.Key].Processing = true;
                        wi.Update();

                        var autoTuneStr = $" {test.Argument.Replace("<val>", i.Key)}";
                        var arg = $"{hardwareDecoder}-y -i \"{tempFile}\" -map 0:V -c:V {preset.VideoEncoder.Name}{autoTuneStr} {frameRate}  \"{tempEncFile}\" ";
                        logger.LogInformation($"Trying: {arg}");

                        await processManager.EncodeAVideo(null, (sender, args) =>
                        {
                            test.AutoPresetResultSet[i.Key].EncodingProgress = args.Percent;
                            wi.Update();
                        }, arg, token);

                        if (token.IsCancellationRequested) return string.Empty;

                        var size = new FileInfo(tempEncFile).Length;
                        test.AutoPresetResultSet[i.Key].AddSize(size, origSize);
                        wi.Update();

                        var ssimResult = await processManager.CalculateSSIM(null, (sender, args) =>
                        {
                            test.AutoPresetResultSet[i.Key].SSIMProgress = args.Percent;
                            wi.Update();
                        }, tempFile, tempEncFile, hardwareDecoder, token);

                        if (ssimResult.Success)
                        {
                            test.AutoPresetResultSet[i.Key].SSIM = ssimResult.SSIM;
                            logger.LogDebug($"SSIM: {ssimResult.SSIM}, Size: {size.ToFileSize()} {Math.Round((decimal)size / origSize * 100M, 2).Adorn("%")}");
                        }

                        test.AutoPresetResultSet[i.Key].Processing = false;
                        wi.Update();
                        if (token.IsCancellationRequested) return string.Empty;
                    }

                    logger.LogDebug($"SSIM results: {string.Join("\r\n", test.AutoPresetResultSet.Select(d => $"{d.Key} | {d.Value.SSIM} | {d.Value.Size.ToFileSize()}"))}");

                    KeyValuePair<string, AutoPresetResult> bestVal = default;

                    if (applicationService.AutoCalculationType == AutoCalcType.FirstPastThePost)
                    {
                        bestVal = test.AutoPresetResultSet.Where(x => x.Value != null && x.Value.SSIM >= (applicationService.AutoCalculationPost ?? 99)).OrderBy(x => x.Value.Size).ThenByDescending(x => x.Value.SSIM).FirstOrDefault();
                    }

                    if (bestVal.Equals(default(KeyValuePair<string, AutoPresetResult>)))
                    {
                        bestVal = test.AutoPresetResultSet.Where(x => x.Value != null && x.Value.Size < origSize).OrderByDescending(x => x.Value.SSIM).ThenBy(x => x.Value.Size).FirstOrDefault();
                    };

                    if (bestVal.Equals(default(KeyValuePair<string, AutoPresetResult>)))
                    {
                        bestVal = test.AutoPresetResultSet.OrderBy(x => x.Value.Size).FirstOrDefault();
                    }

                    logger.LogInformation($"Best Val: {bestVal.Value.SSIM}, Size: {bestVal.Value.Size.ToFileSize()} {Math.Round((decimal)bestVal.Value.Size / origSize * 100M, 2).Adorn("%")}");

                    result = $" {test.Argument.Replace("<val>", bestVal.Key.Trim())}";

                    logger.LogInformation($"Best Argument: {result}");

                    wi.Output($"Best Argument: {result}");
                }
                wi.Update();
                return result;
            }
        }
        private async Task<string> GetVideoCodecParams(FFmpegPreset preset, WorkItem wi, CancellationToken token)
        {
            using (logger.BeginScope("Get Video Codec Parameters"))
            {

                var sb = new System.Text.StringBuilder();
                if (preset.VideoCodecOptions != null)
                {
                    foreach (var vco in preset.VideoCodecOptions.Where(x => !string.IsNullOrWhiteSpace(x.Value) && x.AutoCalculate == false))
                    {

                        var param = $" {vco.EncoderOption.Arg.Replace("<val>", vco.Value.Trim())}";

                        if (preset.VideoBitRate.HasValue)
                        {
                            if (vco.EncoderOption.IncludePass)
                            {
                                param += " pass=%passnum%";
                            }

                            if (!vco.EncoderOption.DisabledByVideoBitRate)
                            {
                                sb.Append(param);
                            }
                        }
                        else
                        {
                            sb.Append(param);
                        }
                    }

                    if (preset.VideoCodecOptions.Any(x => x.AutoCalculate))
                    {
                        logger.LogInformation($"Now Calculating Best values");
                        var calVals = await CalculateBestOptions(preset, wi, token);
                        logger.LogDebug($"Calculated: {calVals}");

                        sb.Append(calVals);
                    }
                }

                return sb.ToString();
            }
        }
    }
}