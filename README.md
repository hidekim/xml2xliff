# xml2xliff
Just an example in converting/merging two XML documents into Xliff 2.0 document, based on Xliff.OM (nuget.org). 
Matches are made by constructing XPath for both documents recursively, and storing texts into Dictionary<>.
Limited HTML support included, utilizing HtmlAgilityPack (also nuget.org).

TODO list:
 - Handle some HTML tags properly, such as <b>bold</b> <i>italic</i> tags
 - Abstraction for parsers
 - More comments and document
 - Options to match documents strictly

