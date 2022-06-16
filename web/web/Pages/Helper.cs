namespace web

{
    static class Helper
    {
        public const string searchResult = "searchResult";
        public const string broaderResult = "broaderResult";
        public const string narrowerResult = "narrowerResult";

        public static Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

        static Helper()
        {
            result.Add(searchResult, new List<string>());
            result.Add(broaderResult, new List<string>());
            result.Add(narrowerResult, new List<string>());
        }

        public class AllegroGraphJsonResult
        {
            public List<string> names { get; set; }
            public List<List<string>> values { get; set; }
        }

        public class Term
        {
            public Term()
            {
                RelatedTerm = new List<string>();
                NarrowerTerm = new List<string>();
                BroaderTerm = new List<string>();
                UsedFor = new List<string>();
                Tukutuku = new List<string>();
            }

            public string PrefLabel { get; set; }
            public List<string> Tukutuku { get; set; }

            public string AltLabel { get; set; }
            public List<string> UsedFor { get; set; }

            public string Whakamārama { get; set; }
            public string ScopeNote { get; set; }

            public List<string> RelatedTerm { get; set; }
            public List<string> NarrowerTerm { get; set; }
            public List<string> BroaderTerm { get; set; }
        }

        public class Record
        {
            public Record()
            {
                Link = new List<string>();
            }

            public string RecordId { get; set; }
            public string ISBN { get; set; }
            public string Title { get; set; }

            public List<string> Link { get; set; }
        }
    }
}

