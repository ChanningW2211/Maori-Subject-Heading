namespace web

{
	public class Model
	{
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
            }

            public string PrefLabel { get; set; }
            public string Tukutuku { get; set; }

            public string AltLabel { get; set; }
            public List<string> UsedFor { get; set; }

            public string Whakamārama { get; set; }
            public string ScopeNote { get; set; }

            public List<string> RelatedTerm { get; set; }
            public List<string> NarrowerTerm { get; set; }
            public List<string> BroaderTerm { get; set; }
        }
    }
}

