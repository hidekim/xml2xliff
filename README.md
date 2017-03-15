# xml2xliff
Just an example in converting/merging two XML documents into Xliff 2.0 document, based on Xliff.OM (nuget.org)
Matches are made by constructing XPath for both documents recursively, and storing texts into Dictionary<>.
Currently, there is no strict validation for document match.
Limited HTML support included, utilizing HtmlAgilityPack (also nuget.org).
 - Does not handle tags like <b> <i> very well --- TBD
 
