﻿using System;
using System.Collections;
using System.Text;
using System.IO;

namespace ls
{
    public interface IMetaReader
    {
        object Read(StringReader inStream);
    }

    public class MetaStringReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume leading quote
            StringBuilder theStringBuilder = new StringBuilder();
            while(true)
            {
                int theChar = inStream.Peek();
                if ((char)theChar == '"' || theChar == -1)
                    break;

                theStringBuilder.Append((char)inStream.Read());
            }
            inStream.Read(); // consume trailing quote

            return theStringBuilder.ToString();
        }
    }

    public class MetaListReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume leading paren
            ArrayList theList = new ArrayList();
            while (true)
            {
                int theChar = inStream.Peek();
                if ((char)theChar == ')' || theChar == -1)
                    break;

                theList.Add(Reader.Read(inStream));

                while (char.IsWhiteSpace((char)inStream.Peek()))
                    inStream.Read();
            }
            inStream.Read(); // consume trailing paren

            return theList;
        }
    }

    public class MetaQuoteReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume quote
            ArrayList theList = new ArrayList();
            theList.Add(new Symbol("quote"));
            theList.Add(Reader.Read(inStream));
            return theList;
        }
    }

    public class Reader
    {
        static IMetaReader[] MetaReaders = new IMetaReader[256];
        static Reader()
        {
            MetaReaders['"'] = new MetaStringReader();
            MetaReaders['('] = new MetaListReader();
            MetaReaders['\''] = new MetaQuoteReader();
        }
        
        public static object ReadNumber(StringReader inStream)
        {
            StringBuilder theStringBuilder = new StringBuilder();
            while (true)
            {
                int theChar = inStream.Peek();
                if (char.IsWhiteSpace((char)theChar) || theChar == -1 || (char)theChar == ')')
                    break;

                theStringBuilder.Append((char)inStream.Read());
            }

            // TODO: Decide what type of number, and use the apropriate parser. Also something like "123a" could be an error.
            return int.Parse(theStringBuilder.ToString());
        }

        public static object ReadSymbol(StringReader inStream)
        {
            StringBuilder theStringBuilder = new StringBuilder();
            while (true)
            {
                int theChar = inStream.Peek();
                if (char.IsWhiteSpace((char)theChar) || theChar == -1 || (char)theChar == ')')
                    break;

                theStringBuilder.Append((char)inStream.Read());
            }
            return new Symbol(theStringBuilder.ToString());
        }

        public static object Read(string inString)
        {
            return Read(new StringReader(inString));
        }

        public static object Read(StringReader inStream)
        {
            while (char.IsWhiteSpace((char)inStream.Peek()))
                inStream.Read();

            if (-1 == inStream.Peek()) // EOF
                return null;

            char theChar = (char)inStream.Peek();
            if (char.IsDigit(theChar))
                return ReadNumber(inStream);

            if (MetaReaders[theChar]!=null)
                return MetaReaders[theChar].Read(inStream);

            return ReadSymbol(inStream);
        }
    }
}